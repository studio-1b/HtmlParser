using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Threading;
using System.IO;

using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;
using CrawlerCommon.HtmlPathTest;

namespace CrawlerHpathUI
{
    public partial class ExtractGrid : Form
    {
        public ExtractGrid()
        {
            InitializeComponent();
            this.Mode = DropMode.Multi;
        }

        new public Form1 ParentForm { get; set; }
        public string Filename { get; set; }
        public enum DropMode { Single, Multi, MultiAndOverwrite }
        public DropMode Mode { get; set; }

        private void ExtractGrid_Load(object sender, EventArgs e)
        {
            richTextBox1.AllowDrop = true;
            richTextBox1.DragDrop += new DragEventHandler(richTextBox1_DragDrop);
            richTextBox1.DragEnter += new DragEventHandler(richTextBox1_DragEnter);

            var col = new DataGridViewTextBoxColumn();
            dataGridView1.Columns.Add(col);
            dataGridView1.Rows.Add();
        }

        void richTextBox1_DragEnter(object sender, DragEventArgs e)
        {
            e.Effect = DragDropEffects.Move;
        }

        class TextRange
        {
            public TextRange(int start, int length)
            {
                this.Start = start;
                this.Length = length;
            }
            public int Start;
            public int Length;
            public int End()
            {
                return this.Start + this.Length - 1;
            }
        }
        Dictionary<Token, TextRange> _dropped = new Dictionary<Token, TextRange>();
        void richTextBox1_DragDrop(object sender, DragEventArgs e)
        {
            // MessageBox.Show("NotImplementedYet");
            RichTextBox txt = (RichTextBox)sender;

            //if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode", false))
            if (e.Data.GetDataPresent(typeof(TreeNode)))
            {
                //Point pt = ((TreeView)sender).PointToClient(new Point(e.X, e.Y));
                //TreeNode DestinationNode = ((TreeView)sender).GetNodeAt(pt);
                //NewNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
                var node = (TreeNode)e.Data.GetData(typeof(TreeNode));
                if (node.Tag is Token)
                {
                    Token casted = (Token)node.Tag;
                    string expr = reverseTraverse(casted);
                    if (_dropped.ContainsKey(casted))
                        return;
                    _dropped.Add(casted, new TextRange(txt.Text.Length, expr.Length));
                    
                    if (txt.Text.Length > 0)
                        txt.Text += "\n" + expr;
                    else
                        txt.Text = expr;
                }
                else
                {
                    var obj = node.Tag;
                }
            }
        }

        string reverseTraverse(Token startingAt)
        {
            if(_dropped.Count==0 || this.Mode == DropMode.Single)
                return reverseTraverse(startingAt, null);

            Token gcd = GCD(startingAt, _dropped.LastOrDefault<KeyValuePair<Token, TextRange>>().Key);

            return reverseTraverse(startingAt, gcd);
        }
        Dictionary<Token, TextRange> _cache = new Dictionary<Token, TextRange> ();
        string reverseTraverse(Token startingAt, Token gcd)
        {
            //try to ascertain the best test to identify the node
            string test = startingAt.ActualSymbol;
            if (startingAt is Tag)
                test = ((Tag)startingAt).GetExpectedTagString();
            else if (startingAt is Literal)
                test = "LITERAL";
            else if (startingAt is Closure)
                test = "CLOSURE";

            //put in index
            string subtest = string.Empty;
            if (gcd!=null && startingAt.ParentNode == gcd)
            {
                var casted = (Node)startingAt.ParentNode;
                var num = casted.ChildElements.TakeWhile<Token>(s => s != startingAt).Count<Token>(s=>s.GetType()==startingAt.GetType());
                subtest = string.Format("[{0}]", num);
                test += subtest;
            }

            if (startingAt.ParentNode == null)
            {
                return test;
            }
            else
                return reverseTraverse(startingAt.ParentNode, gcd) + "/" + test;
        }


        Token GCD(Token a, Token b)
        {
            if (a == null || b == null)
                return null;
            if (a == b) return a;

            while (a != null)
            {
                var bb= b;
                while (bb != null)
                {
                    bb = bb.ParentNode;
                    if (a == bb)
                        return a;
                }
                a = a.ParentNode;
            }

            throw new InvalidOperationException("Not in same tree");
        }

        class container { public string[] data; }
        delegate void PassParameter(object state);
        private void extractHereToolStripMenuItem_Click(object sender, EventArgs e)
        {
            dataGridView1.Columns.Clear();
            dataGridView1.Rows.Clear();
            string all = richTextBox1.Text;

            // Add columns
            List<HtmlPath> tests = new List<HtmlPath>();
            foreach (var expr in nextExpr(all))
                if(!string.IsNullOrWhiteSpace(expr))
                {
                    var col = new DataGridViewTextBoxColumn();
                    col.HeaderText = expr;
                    dataGridView1.Columns.Add(col);
                    HtmlPath parser = new HtmlPath();
                    parser.Parse(expr);
                    tests.Add(parser);
                }

            //populate data
            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                List<HtmlPath> casted = (List<HtmlPath>)state;
                int cols = casted.Count;
                string[] row = new string[cols];
                var list = this.ParentForm.LoadedTags;
                //if (list[0] is Tag)
                //    AttribToken.ExpandAttribChildren((Tag)list[0]);
                //else
                //    throw new InvalidOperationException("First tag in list expected to be root");
                foreach (var token in list)
                    for (int i = 0; i < cols; i++)
                    {
                        var test = tests[i];
                        if (test.IsMatch(token))
                        {
                            row[i] = test.GetValue(token);
                            if (i == cols - 1)
                            {
                                dataGridView1.BeginInvoke(new PassParameter(delegate(object param) { dataGridView1.Rows.Add(((container)param).data); }), new container() { data = row });
                                row = new string[cols];
                            }
                        }
                    }
            }, tests);
        }

        IEnumerable<string> nextExpr(string whole)
        {
            int pos = 0;
            int end = whole.IndexOf('\n', pos+1);
            while (end > 0)
            {
                yield return whole.Substring(pos, end - pos); //exclude lf
                pos = end+1;
                end = whole.IndexOf('\n', pos);
            }
            yield return whole.Substring(pos, whole.Length - pos);
        }



        private void singleRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = true;
            this.multiRecordToolStripMenuItem.Checked = false;
            this.Mode = DropMode.Single;
        }

        private void multiRecordToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ((ToolStripMenuItem)sender).Checked = true;
            this.singleRecordToolStripMenuItem.Checked = false;
            this.Mode = DropMode.Single;
        }



        #region File Operations
        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Create an instance of the open file dialog box.
            using (OpenFileDialog openFileDialog1 = new OpenFileDialog())
            {
                // Set filter options and filter index.
                openFileDialog1.Title = "Open File";
                openFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
                openFileDialog1.FilterIndex = 1;

                openFileDialog1.Multiselect = false;

                // Call the ShowDialog method to show the dialog box.
                var userClickedOK = openFileDialog1.ShowDialog();

                // Process input if the user clicked OK.
                if (userClickedOK == DialogResult.OK)
                    try
                    {
                        // Open the selected file to read.
                        string name = openFileDialog1.FileName;
                        string text = File.ReadAllText(name);
                        richTextBox1.Text = text;
                        this.Filename = name;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem opening file");
                    }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Displays a SaveFileDialog so the user can save the Image
            // assigned to Button2.
            if (!string.IsNullOrWhiteSpace(this.Filename))
            {
                string name = this.Filename;
                string text = richTextBox1.Text;
                try
                {
                    File.WriteAllText(name, text);
                    return;
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Problem saving file.  Please select a new location");
                }
            }
            
            using (SaveFileDialog saveFileDialog1 = new SaveFileDialog())
            {
                saveFileDialog1.Filter = "Text Files (.txt)|*.txt|All Files (*.*)|*.*";
                saveFileDialog1.Title = "Save File";
                var userClickedOK = saveFileDialog1.ShowDialog();

                // If the file name is not an empty string open it for saving.
                if (userClickedOK == DialogResult.OK && !string.IsNullOrWhiteSpace(saveFileDialog1.FileName))
                {
                    string name = saveFileDialog1.FileName;
                    string text = richTextBox1.Text;
                    try
                    {
                        File.WriteAllText(name, text);
                        this.Filename = name;
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Problem saving file");
                    }
                }
            }

        }
        #endregion File Operations


        Token _lastselection = null;
        bool _multiselection = false;
        private void richTextBox1_KeyPress(object sender, KeyPressEventArgs e)
        {
            RichTextBox ctrl = (RichTextBox)sender;
            string text = ctrl.Text;
            int pos = ctrl.SelectionStart;
            int thru = ctrl.SelectionLength;
            int end = pos + thru;

            //nothing selected, so more than 1 cannot be possible
            if (thru == 0)
                return;

            bool found = false;
            foreach (var item in _dropped)
                if ((pos <= item.Value.Start && item.Value.Start <= end)
                    || (pos <= item.Value.End() && item.Value.End() <= end))
                    if(!found)
                        found = true;
                    else
                    {
                        _multiselection = true;
                        return;
                    }

            _multiselection = false;
        }

        private void richTextBox1_KeyUp(object sender, KeyEventArgs e)
        {
            RichTextBox ctrl = (RichTextBox)sender;
            string text = ctrl.Text;
            int pos = ctrl.SelectionStart;
            int thru = ctrl.SelectionLength;
            // does not support selection overwrite for now... its just triple the code
            foreach (var item in _dropped)
                if (pos >= item.Value.Start && pos <= item.Value.End())
                {
                    _lastselection = item.Key;
                    Token key = item.Key;
                    if (pos >= text.Length) pos = text.Length - 1;
                    int start = text.LastIndexOf('\n', pos);
                    int end = text.IndexOf('\n', pos) - 1;
                    if (start < 0) start = 0;
                    if (end >= text.Length) end = text.Length - 1;
                    if (item.Value.Start != start || item.Value.End() != end)
                    {
                        _dropped[key].Start = start;
                        _dropped[key].Length = end - start + 1;
                    }
                    return;
                }

            _multiselection = false;
        }



    }
}

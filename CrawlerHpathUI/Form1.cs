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
using System.Net;

using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;
using CrawlerCommon.HtmlPathTest;


namespace CrawlerHpathUI
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public readonly List<Token> LoadedTags = new List<Token>();

        private void button1_Click(object sender, EventArgs e)
        {
            string filename = textBox1.Text;
            if (!File.Exists(filename) && !filename.StartsWith("http://") && !filename.StartsWith("https://"))
            {
                MessageBox.Show("Cannot find " + filename + " in " + Environment.CurrentDirectory);
                return;
            }

            LoadedTags.Clear();
            treeView1.Nodes.Clear();
            toolStripStatusLabel1.Text = "...";
            toolStripProgressBar1.Value = 0;

            // build the helper classes that do the parsing
            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = true, ShowWorkMode = false };
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };

            // load file, or http:// on separate thread
            Thread work = new Thread(delegate()
            {
                try
                {
                    
                    //break the text into separate html tags and literals
                    if (filename.StartsWith("http://") || filename.StartsWith("https://"))
                        using (Stream data = MakeRequest(filename))
                        {
                            if (data == null)
                            {
                                MessageBox.Show("Unable to retreive data from " + filename);
                                return;
                            }

                            statusStrip1.Invoke(new MethodInvoker(delegate() { toolStripProgressBar1.Maximum = (int)data.Length; }));
                            tokenizer.Load(data);
                        }
                    else
                        using (FileStream fs = File.OpenRead(filename))
                        {
                            //load tokens
                            statusStrip1.Invoke(new MethodInvoker(delegate() { toolStripProgressBar1.Maximum = (int)fs.Length; }));
                            tokenizer.Load(fs);
                        }

                    // take the strings, and identify which tags the strings represent, and build parent-child relationship
                    Dictionary<Token, TreeNode> map = new Dictionary<Token, TreeNode>();
                    Dictionary<TreeNode, TreeNode> task = new Dictionary<TreeNode, TreeNode>();
                    //treeView1.Invoke(new MethodInvoker(delegate() { treeView1.BeginUpdate(); }));
                    foreach (var item in tokenizer.Tokens.BuildTree(builder))
                    {
                        statusStrip1.BeginInvoke(new MethodInvoker(delegate() { toolStripProgressBar1.Value += item.ActualSymbol.Length; }));

                        if (treeView1.Nodes.Count == 0)
                        {
                            TreeNode root = new TreeNode(item.ParentNode.GetType().Name + " " + item.ParentNode.ActualSymbol);
                            root.Tag = item.ParentNode; //treenode --> Token established
                            map.Add(item.ParentNode, root); //Token --> treenode established
                            treeView1.Invoke(new MethodInvoker(delegate()
                            {
                                treeView1.Nodes.Add(root);
                            }));
                        }

                        TreeNode node = new TreeNode(item.GetType().Name + " " + item.ActualSymbol);
                        node.Tag = item; //treenode --> Token established
                        map.Add(item, node); //Token --> treenode established
                        task.Add(node, map[item.ParentNode]);
                        //treeView1.Invoke(new MethodInvoker(delegate()
                        //        {
                        //            map[item.ParentNode].Nodes.Add(node); //actually finds the treenode corresponding to the parent, and adds the new node as child
                        //        }));
                        if (item is Tag)
                            foreach (var att in ((Tag)item).Attrib)
                            {
                                TreeNode attnode = new TreeNode(att.GetType().Name + " [" + att.Name + "] [" + att.Value + "]");
                                task.Add(attnode, node);
                        //        treeView1.BeginInvoke(new MethodInvoker(delegate()
                        //        {
                        //            node.Nodes.Add(attnode);
                        //        }));
                            }
                        if (task.Count > 50)
                        {
                            var sent = task;
                            task = new Dictionary<TreeNode, TreeNode>();
                            treeView1.BeginInvoke(new MethodInvoker(delegate()
                            {
                                treeView1.BeginUpdate();
                                foreach (var pair in sent)
                                    pair.Value.Nodes.Add(pair.Key);
                                treeView1.EndUpdate(); 
                            }));
                        }

                        LoadedTags.Add(item);
                    }
                    treeView1.Invoke(new MethodInvoker(delegate() {
                        treeView1.BeginUpdate();
                        foreach (var pair in task)
                            pair.Value.Nodes.Add(pair.Key);
                        treeView1.EndUpdate(); 
                        task.Clear();
                    }));
                    Config.Singleton.LastOpened = filename;

                }
                finally
                {
                    this.BeginInvoke(new Action(delegate()
                    {
                        treeView1.BeginUpdate();
                        treeView1.ExpandAll();
                        treeView1.EndUpdate(); 
                        textBox1.BackColor = Color.White;
                        toolStripProgressBar1.Value = toolStripProgressBar1.Maximum;
                        toolStripStatusLabel1.Text = string.Format("{1} items in {0} sec(s)", builder.RunTime/10d, builder.Count);
                        toolStripStatusLabel2.Text = "Double-click on any node, to open HPath editor";
                    }));
                }
            }) { IsBackground = true };

            // start the loading process
            textBox1.BackColor = Color.Yellow;
            work.Start();

        }


        public static Stream MakeRequest(string requestUrl)
        {
            try
            {
                HttpWebRequest request = (HttpWebRequest)WebRequest.Create(requestUrl);
                Stream result;

                // returned values are returned as a stream, then read into a string
                // String lsResponse = string.Empty;
                using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
                {
                    if (response.StatusCode != HttpStatusCode.OK)
                        return null;

                    result = new MemoryStream();
                    response.GetResponseStream().CopyTo(result);
                    result.Position = 0;
                    return result;
                }
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        int _searchHighlight = 0;
        string _searchedFor = null;
        List<TreeNode> _found = null;
        private void button2_Click(object sender, EventArgs e)
        {
            string term = textBox2.Text;

            if (treeView1.Nodes.Count == 0)
                return;

            //build search list
            if (_searchedFor != term)
            {
                if (_found != null)
                    resetSearch(_found);
                _found = null;

                // load file, or http:// on separate thread
                Thread work = new Thread(delegate()
                {
                    try
                    {
                        List<TreeNode> newfind = new List<TreeNode>();
                        searchFor(term, newfind, treeView1.Nodes[0]);
                        _found = newfind;
                        if (newfind.Count > 0)
                        {
                            Config.Singleton.LastSearch = term;

                            _searchHighlight = 0;
                            treeView1.Invoke(new MethodInvoker(delegate() { 
                                treeView1.SelectedNode = newfind[0];
                                toolStripStatusLabel1.Text = string.Format("{0} items found", newfind.Count);
                            }));
                        }
                        else
                            MessageBox.Show("Nothing found for [" + term + "]");
                    }
                    finally
                    {
                        treeView1.BeginInvoke(new MethodInvoker(delegate() { textBox2.BackColor = Color.White; }));
                        _searchedFor = term;
                    }
                });
                textBox2.BackColor = Color.Beige;
                work.Start();
            }

            //goto next item
            if (_found != null)
            {
                _searchHighlight++;
                if (_searchHighlight >= _found.Count)
                    _searchHighlight = 0;
                treeView1.SelectedNode = _found[_searchHighlight];
                toolStripStatusLabel1.Text = string.Format("{0}/{1}", _searchHighlight + 1, _found.Count);
            }
        }

        void searchFor(string term, List<TreeNode> foundlist, TreeNode starting)
        {
            List<TreeNode> list = new List<TreeNode>();
            if (starting.Text.ToLower().Contains(term.ToLower()))
            {
                foundlist.Add(starting);
                treeView1.BeginInvoke(new MethodInvoker(delegate() { starting.BackColor = Color.LightSteelBlue; }));
            }

            foreach (TreeNode item in starting.Nodes)
                searchFor(term, foundlist, item);
        }

        void resetSearch(List<TreeNode> foundlist)
        {
            treeView1.BeginInvoke(new MethodInvoker(delegate() {
                foreach (var item in foundlist)
                    item.BackColor = Color.Transparent;
            }));
        }

        private void textBox1_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button1_Click(sender, e);
        }

        private void textBox2_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
                button2_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            // goto next item
            if (_found != null)
            {
                _searchHighlight--;
                if (_searchHighlight < 0)
                    _searchHighlight = _found.Count-1;
                treeView1.SelectedNode = _found[_searchHighlight];
                toolStripStatusLabel1.Text = string.Format("{0}/{1}", _searchHighlight + 1, _found.Count);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.textBox1.Text = Config.Singleton.LastOpened;
            this.textBox2.Text = Config.Singleton.LastSearch;
        }

        ExtractGrid grid = null;
        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            if (grid == null)
            {
                grid = new ExtractGrid();
                grid.ParentForm = this;
                grid.Disposed += delegate(object sender2, EventArgs e2)
                {
                    this.grid = null;
                    toolStripStatusLabel2.Text = "Double click any node, to re-open ExtractGrid";
                };

                grid.StartPosition = FormStartPosition.Manual;
                grid.Location = new Point(this.Location.X + this.Size.Width, this.Location.Y);
                grid.Left = this.Left + this.Width;
                grid.Top = this.Top;

                //grid.Location = new Point(0, 0);
                //grid.Left = 0;
                //grid.Top = 0;
                toolStripStatusLabel2.Text = "Drag and drop any HTML element from tree, to ExtractGrid's Textbox";

                grid.Show();
            }
        }

        private void treeView1_ItemDrag(object sender, ItemDragEventArgs e)
        {
            DoDragDrop(e.Item, DragDropEffects.Move);
        }

    }
}

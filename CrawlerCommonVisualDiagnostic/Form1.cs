using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

using System.Net;
using System.IO;

using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;

using CrawlerCommon.HtmlPathTest;



namespace CrawlerCommonVisualDiagnostic
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }
        
        private void Form1_Load(object sender, EventArgs e)
        {
            textBox1.Text = "playerstats.html";
        }

        private void button1_Click(object sender, EventArgs e)
        {
            /*
            string failedfore = "a771f34b-1473-4aa0-91b0-9de66527e9bb";
            ReadonlyStringSegmentation test = new ReadonlyStringSegmentation(failedfore, "-", "a", "b");
            string[] split = failedfore.Split('-', 'a', 'b');
            for(int i=0;i<split.Length; i++)
                MessageBox.Show(split[i] + "..." + test[i]);
            */

            //Check readonly string segment
            string output = string.Empty;
            using (System.Threading.AutoResetEvent signal = new System.Threading.AutoResetEvent(false))
            {
                System.Threading.Thread work = new System.Threading.Thread(delegate()
                {
                    for (int i = 0; i < 100; i++)
                    {
                        string sample = Guid.NewGuid().ToString();
                        string[] control = sample.Split(new string[] { "-", "a", "b" }, StringSplitOptions.None);
                        ReadonlyStringSegmentation segmentation = new ReadonlyStringSegmentation(sample, "-", "a", "b");

                        if (segmentation.Count == control.Length) output += "[OK count]"; else output = "[ X count]";
                        for (int j = 0; j < segmentation.Count; j++)
                            if (segmentation[j] == control[j]) output += "[OK " + control[j] + "]"; else output = "[ X " + segmentation[j] + " != " + control[j] + "]";
                        output += sample + "\n";
                        //listBox1.Items.Add(output);
                    }
                    signal.Set();
                }) { IsBackground = true };
                work.Start();
                while(!signal.WaitOne(10));
                textBox2.Text = output.Split('\n').Length + " guids tested " + (output.Split('X').Length-1) + " errors found, cases= " + output;
            }

            string filename = textBox1.Text;
            if(!File.Exists(filename))
            {
                MessageBox.Show("Cannot find " + filename + " in " + Environment.CurrentDirectory);
                return;
            }

            //load file
            using (FileStream fs = File.OpenRead(filename))
            using(StreamReader sr = new StreamReader(fs))
            {
                richTextBox1.Text = sr.ReadToEnd();
            }

            //give the html a stream
            MemoryStream ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(richTextBox1.Text));

            //load tokens
            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = true, ShowWorkMode = false };
            tokenizer.Load(ms);
            //foreach (var item in tokenizer.Tokens)
              //  listBox1.Items.Add(item);
            listBox1.DataSource = tokenizer.Tokens;

            //load parse tree
            treeView1.Nodes.Clear();
            Dictionary<Token,TreeNode> map = new Dictionary<Token,TreeNode>();
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            var HPath = new CrawlerCommon.HtmlPath();
            HPath.Parse(string.Empty);//fill in manually with tag you want to find (in case of emergency and u want to know why something went boom)
            HPath.Match += delegate(object sdr, MatchEventArgs<Token> ea)
            {
                if (!treeView1.InvokeRequired)
                {
                    map[ea.SelectedValue].BackColor = Color.Red;
                }
                else
                    treeView1.BeginInvoke(new Action(delegate()
                    {
                        map[ea.SelectedValue].BackColor = Color.Red;
                    }));
            };
            
            foreach (var item in tokenizer.Tokens.BuildTree(builder))
            {
                if(treeView1.Nodes.Count==0)
                {
                    TreeNode root = new TreeNode(item.ParentNode.GetType().Name + " " + item.ParentNode.ActualSymbol);
                    root.Tag = item.ParentNode; //treenode --> Token established
                    map.Add(item.ParentNode, root); //Token --> treenode established
                    treeView1.Nodes.Add(root);
                } 

                TreeNode node = new TreeNode(item.GetType().Name + " " + item.ActualSymbol);
                node.Tag = item; //treenode --> Token established
                map.Add(item, node); //Token --> treenode established
                map[item.ParentNode].Nodes.Add(node);
                if(item is Tag)
                    foreach (var att in ((Tag)item).Attrib)
                    {
                        TreeNode attnode = new TreeNode(att.GetType().Name + " [" + att.Name + "] [" + att.Value + "]");
                        node.Nodes.Add(attnode);
                    }

                if (HPath.IsMatch(item))
                    node.BackColor = Color.Red;
                //HPath.BeginMatch(item);
            }
            this.BeginInvoke(new Action(delegate()
            {
                treeView1.ExpandAll();
            }));

            MessageBox.Show(CachedStringSegmentation.Requests + " " + CachedStringSegmentation.Hits);

            StringBuilder sb = new StringBuilder();
            foreach (var item in builder.TagStats.OrderBy<System.Collections.Generic.KeyValuePair<string,int>, int>(s=>s.Value))
            {
                sb.AppendLine(item.Key + "..." +item.Value);
            }
            MessageBox.Show(sb.ToString());
            MessageBox.Show(builder.RunTime.ToString());
            MessageBox.Show(((float)builder.RunTime / (float)builder.Count).ToString());

            sb = new StringBuilder();
            foreach (var item in builder.Unknown)
            {
                sb.AppendLine(item.Key + "..." + item.Value);
            }
            MessageBox.Show(sb.ToString());
        }


        private void button2_Click(object sender, EventArgs e)
        {
            textBox1.Text = "playerstats.html";
            button1_Click(sender, e);
        }

        private void button3_Click(object sender, EventArgs e)
        {
            textBox1.Text = "C-Historical_Prices.htm";
            button1_Click(sender, e);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            textBox1.Text = "2013120900_2013_REG14_cowboys@bears.html";
            button1_Click(sender, e);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            treeView1.Nodes.Clear();
            TreeNode location = new TreeNode() { Text = "Root" };

            HtmlPath hpath = new HtmlPath();
            textBox1.Text = "div//tabletag/tr[td=\"SSS\" and (@class='sort1' or @class='sort2') and position>=1]/td[0]/a";
            /*
            foreach(var item in hpath.parseTestStructure(textBox1.Text))
                sb.AppendLine(item.ToString());
            sb.AppendLine();
            foreach (var part in textBox1.Text.Segment(hpath.parseTestStructure(textBox1.Text)).Reverse<string>())
            if(part!=null)
            {
                if (treeView1.Nodes.Count == 0)
                {
                    treeView1.Nodes.Add(location);
                }
                else
                {
                    TreeNode temp = new TreeNode() { Text = "ParentTest" };
                    location.Nodes.Add(temp);
                    location = temp;
                }

                sb.AppendLine(part);
                int counter=0;
                var headtail = part.Segment(hpath.parseIndividualElement(part));

                TreeNode elementTest = new TreeNode() { Text = headtail[0] };
                location.Nodes.Add(elementTest);
                
                int OP_FOUND=1;
                int mode = 0;
                foreach (var where in headtail)
                {
                    sb.AppendLine("    " + where);

                    Stack<object> stack = new Stack<object>();
                    //Stack<object> instr = new Stack<object>();
                    if(counter>0)
                        foreach (var clause in where.Segment(hpath.parseCriteriaElement(where)))
                        {
                            sb.AppendLine("        " + clause);

                            if (clause == "(")
                            {
                                if (stack.Count > 0 && stack.Peek() is TreeNode && ((TreeNode)stack.Peek()).Text=="AND")
                                {
                                    TreeNode test = new TreeNode() { Text = "Grouping" };
                                    ((TreeNode)stack.Peek()).Nodes.Add(test);
                                    stack.Push(test);
                                }
                                else if (stack.Count == 0)
                                {
                                    TreeNode test = new TreeNode() { Text = "Grouping" };
                                    stack.Push(test);
                                }
                            }
                            else if (clause == ")")
                                while (!(stack.Peek() is TreeNode && ((TreeNode)stack.Pop()).Text=="Grouping")) ;
                            else if (clause == "AND" || clause == "OR")
                            {
                                if (stack.Count > 0 && stack.Peek() is TreeNode)
                                    if (((TreeNode)stack.Peek()).Text == "Grouping")
                                    {
                                        TreeNode test = new TreeNode() { Text = "AND" };
                                        ((TreeNode)stack.Peek()).Nodes.Add(test);
                                        stack.Pop();
                                        stack.Push(test);
                                    }
                                    else if (((TreeNode)stack.Peek()).Text == "AND")
                                    {
                                        TreeNode test = new TreeNode() { Text = "Grouping" };
                                        ((TreeNode)stack.Peek()).Nodes.Add(test);
                                        stack.Push(test);
                                    }
                            }
                            else if ((new string[] { "<", ">", "<=", ">=", "=", "<>" }).Contains<string>(clause))
                            {
                                mode = OP_FOUND;
                                //instr.Push(clause);
                            }
                            else
                            {
                                //instr.Push(clause);
                            }
                        }

                    counter++;
                }
            }
            sb.AppendLine();
            */
            richTextBox1.Text = sb.ToString();
        }
        



        /// <summary>
        /// getTestForParentIsPlayerRows() /Td[@CLASS="sort1"]/A/Literal
        /// </summary>
        /// <returns></returns>
        private BaseTestLinkage getTestForPlayerName()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    getTestForParentIsPlayerRows()
                                },
                                new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="sort1" },
                                new ElementPositionTest() { CompareValue=0 }
                            }
                        }
                    } 
                };
            /*
            top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    new ParentTest() {

                                    }
                                }
                            }
                        }
                    } 
                };
            */
            return top;
        }

        /// <summary>
        /// getTestForParentIsPlayerRows() /Td[@CLASS="sort1" and position()=index]/Literal
        /// </summary>
        /// <returns></returns>
        private BaseTestLinkage getTestForColumn(int index) {

            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Td>() {
                            getTestForParentIsPlayerRows()
                        },
                        new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="sort1" },
                        new ElementPositionTest() { CompareValue=index }
                    } 
                };

            
            return top;
        }


        static BaseTestLinkage madeTestForParentIsPlayerRows;
        /// <summary>
        /// getTestForPlayerRows() /...
        /// </summary>
        /// <returns></returns>
        private BaseTestLinkage getTestForParentIsPlayerRows()
        {
            if (madeTestForParentIsPlayerRows == null)
            {
                BaseTestLinkage top = new ParentTest() {
                                getTestForPlayerRows()
                };
                madeTestForParentIsPlayerRows = top;
            }

            return madeTestForParentIsPlayerRows;
        }

        ITokenTest madeTestForPlayerRows;
        /// <summary>
        /// //TableTag/Tr[1]/Td/TableTag/Tr[position()>2]
        /// </summary>
        /// <returns></returns>
        private ITokenTest getTestForPlayerRows()
        {
            if (madeTestForPlayerRows == null)
            {
                var eureka = new ElementTest<TableTag>(); //the top most node/tag

                ITokenTest top = new CacheTestResult(
                                new ElementTest<Tr>() {
                                    new ParentTest(){
                                        new OrTest() {
                                            new ElementTest<TableTag>() {
                                                new ParentTest() {
                                                    new ElementTest<Td>() {
                                                        new ParentTest() {
                                                            new ElementTest<Tr>() {
                                                                new ParentTest() {
                                                                    eureka,
                                                                    new ElementPositionTest<TableTag>() { CompareValue=5 }
                                                                }
                                                            }
                                                        }
                                                    }
                                                } 
                                            },
                                            new ElementTest<TBody>() {
                                                new ParentTest() {
                                                    new ElementTest<TableTag>() {
                                                        new ParentTest() {
                                                            new ElementTest<Td>() {
                                                                new ParentTest() {
                                                                    new ElementTest<Tr>() {
                                                                        new ParentTest() {
                                                                            new ElementTest<TBody>() {
                                                                                new ParentTest() {
                                                                                    eureka,
                                                                                    new ElementPositionTest<TableTag>() { CompareValue=5 }
                                                                                }
                                                                            }
                                                                        }
                                                                    }
                                                                }
                                                            }
                                                        } 
                                                    }
                                                }
                                            }
                                        }
                                    },
                                    new ElementPositionComparisonTest() { CompareValue=1, Operator=1 },
                                    //new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="navstats" },
                                });

                madeTestForPlayerRows = top;
            }

            return madeTestForPlayerRows;
        }
        /// <summary>
        /// //TableTag/Tr/Td/TableTag/Tr[@class=tablehdr]/td[0]
        /// </summary>
        /// <returns></returns>
        private BaseTestLinkage getTestForStatsType()
        {

                var eureka = new ElementTest<TableTag>(); //the top most node/tag
                eureka.Match += new MatchHandler(delegate(object sender, MatchEventArgs<Token> e) {
                    Console.WriteLine("Stats"+e.SelectedValue);
                });

                BaseTestLinkage top =
                    new ElementTest<Literal>() {
                        new ParentTest(){
                        new ElementTest<Td>() {
                            new ElementPositionTest() { CompareValue=1 },
                            new ParentTest(){
                                new ElementTest<Tr>() {
                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tablehdr" },
                                    
                                    new ParentTest(){
                                        new ElementTest<TableTag>() {
                                            new ParentTest() {
                                                new ElementTest<Td>() {
                                                    new ParentTest() {
                                                        new ElementTest<Tr>() {
                                                            new ParentTest() {
                                                                eureka,
                                                            }
                                                        }
                                                    }
                                                }
                                            } 
                                        }
                                    },
                                    //new ElementPositionComparisonTest() { CompareValue=0, Operator=1 }
                                }
                            },
                            
                        }
                        }
                    };


            return top;
        }




        private void button6_Click(object sender, EventArgs e)
        {
            string filename = textBox1.Text;
            if (!File.Exists(filename))
            {
                MessageBox.Show("Cannot find " + filename + " in " + Environment.CurrentDirectory);
                return;
            }

            //load file
            using (FileStream fs = File.OpenRead(filename))
            using (StreamReader sr = new StreamReader(fs))
            {
                richTextBox1.Text = sr.ReadToEnd();
            }

            //give the html a stream
            MemoryStream ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(richTextBox1.Text));
            //ms = new MemoryStream(System.Text.Encoding.ASCII.GetBytes("<iframe style=\"height:24px !important;\" id=\"facebook-share\" src=\"http://www.facebook.com/plugins/like.php?app_id=152185691527861&amp;href=http://www.spotrac.com/nfl/injured-reserve/&amp;send=false&amp;layout=button_count&amp;width=75&amp;show_faces=false&amp;action=like&amp;colorscheme=light&amp;font&amp;height=24\" ></iframe>  \t\t\t\t\t\t\t\t</li> \t\t\t\t\t\t\t</ul> \t\t\t\t\t\t</nav> \t\t\t\t\t\t<!-- end:menu -->"));
            

            //load tokens
            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = true, ShowWorkMode = false };
            tokenizer.Load(ms);
            //foreach (var item in tokenizer.Tokens)
            //  listBox1.Items.Add(item);
            listBox1.DataSource = tokenizer.Tokens;

            //create instruction on how to color tree, after the node has been matched
            treeView1.Nodes.Clear();
            Dictionary<Token, TreeNode> map = new Dictionary<Token, TreeNode>();
            /*
            var HPath = new CrawlerCommon.HtmlPath();
            HPath.Parse(string.Empty);
            HPath.Match += delegate(object sdr, MatchEventArgs<Token> ea)
            {
                if (!treeView1.InvokeRequired)
                {
                    map[ea.SelectedValue].BackColor = Color.Red;
                }
                else
                    treeView1.BeginInvoke(new Action(delegate()
                    {
                        map[ea.SelectedValue].BackColor = Color.Red;
                    }));
            };*/

            // creating new next to fill HTML... I'll have the other fields anyway
            StringBuilder table = new StringBuilder();
            string statstype = null;
            string name=null;
            string team = null;
            string games=null;
            string pcomp, patt, pyard, ptd, pint;
            string ruatt, ruyard, rutd;
            string rvtarget, rvrec, rvyd, rvtd;
            // create an hpath to identify "protocol"... or the td pattern that identifies QB, RB, WR column headings
            // create an hpath to identidy "version" or is protocols have changed... TBD... if so, generate a warning to prevent gibberish from flooding your database
            var typePath = new CrawlerCommon.HtmlPath();
            typePath.SetTest(getTestForStatsType());
            
            var rowPath = new CrawlerCommon.HtmlPath();
            rowPath.SetTest(new ElementTest<Closure>() { getTestForParentIsPlayerRows()});
            rowPath.Match += delegate(object sdr, MatchEventArgs<Token> ea)
            {
                table.AppendLine(String.Format("{0:20} | {1:4} | {2:3}", name, team, games));
            };

            var namePath = new CrawlerCommon.HtmlPath();
            namePath.SetTest(getTestForPlayerName());
            namePath.Match += delegate(object sdr, MatchEventArgs<Token> ea)
            {
                name = ea.SelectedValue.ActualSymbol;
            };

            var teamPath = new CrawlerCommon.HtmlPath();
            teamPath.SetTest(getTestForColumn(1));
            teamPath.Match += delegate(object sdr, MatchEventArgs<Token> ea)
            {
                team = ea.SelectedValue.ActualSymbol;
            };
            var gamesPath = new CrawlerCommon.HtmlPath();
            gamesPath.SetTest(getTestForColumn(2));
            gamesPath.Match += delegate(object sdr, MatchEventArgs<Token> ea)
            {
                games = ea.SelectedValue.ActualSymbol;
            };
            bool isInjuredStart = false;
            var injuredStartPath = getTestForInjuredPlayerSectionStart();
            var injuredNamePath = getTestForInjuredPlayerName();
            var injuredPosPath = getTestForInjuredPlayerPos();
            var injuredTeamPath = getTestForInjuredPlayerTeam();
            var injuredCategoryPath = getTestForInjuredPlayerCategory();
            var injuredEndPath = new CrawlerCommon.HtmlPath();
            injuredEndPath.SetTest(new ElementTest<Closure>() { getTestForInjuredPlayerRow() });

            ElementTest tmpTest =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Div>() {
                                },
                            }
                        }
                    },
                    new ElementComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, CompareValue="Next        Page", Operator=0 }
                };
            HtmlPath tmpPath = new HtmlPath();
            tmpPath.SetTest(tmpTest);


            var teamSchedulePath = getTestForScheduledTeam();
            HtmlPath[] versusPath = new HtmlPath[17];
            for (int i = 0; i < 17; i++)
                versusPath[i] = getTestForScheduledVersus(i);
            var teamEndPath = getTestForScheduleRowEnd();
            

            TreeNode root = null;
            var builder = new BuilderStats(new IndexedBuilder()) { StatsOn = true };
            foreach (var item in tokenizer.Tokens.BuildTree(builder))
            {
                
                if (treeView1.Nodes.Count == 0 && root==null)
                {
                    root = new TreeNode(item.ParentNode.GetType().Name + " " + item.ParentNode.ActualSymbol);
                    root.Tag = item.ParentNode; //treenode --> Token established
                    map.Add(item.ParentNode, root); //Token --> treenode established
                    //treeView1.Nodes.Add(root);
                }

                TreeNode node = new TreeNode(item.GetType().Name + " " + item.ActualSymbol);
                node.Tag = item; //treenode --> Token established
                map.Add(item, node); //Token --> treenode established
                map[item.ParentNode].Nodes.Add(node);
                if (item is Tag)
                    foreach (var att in ((Tag)item).Attrib)
                    {
                        TreeNode attnode = new TreeNode(att.GetType().Name + " [" + att.Name + "] [" + att.Value + "]");
                        node.Nodes.Add(attnode);
                    }

                //if (HPath.IsMatch(item))
                //    node.BackColor = Color.Red;
                //HPath.BeginMatch(item);


                if (typePath.IsMatch(item)) statstype = item.ActualSymbol;
                if (namePath.IsMatch(item)) name = item.ActualSymbol;
                if (teamPath.IsMatch(item)) team = item.ActualSymbol;
                if (gamesPath.IsMatch(item)) games = item.ActualSymbol;
                if (rowPath.IsMatch(item))
                {
                    map[item].BackColor = Color.Red;
                    table.AppendLine(String.Format("{0:20} | {1:4} | {2:3} | {3}", name, team, games, statstype));
                }
                if (tmpPath.IsMatch(item))
                    if(item.ActualSymbol.StartsWith("Next"))
                {
                    map[item].BackColor = Color.Green;
                    table.AppendLine("Next Page detected");
                }

                if (isInjuredStart)
                {
                    if (injuredNamePath.IsMatch(item))
                        map[item].BackColor = Color.HotPink;
                    if (injuredPosPath.IsMatch(item))
                        map[item].BackColor = Color.Honeydew;
                    if (injuredTeamPath.IsMatch(item))
                        map[item].BackColor = Color.GreenYellow;
                    if (injuredCategoryPath.IsMatch(item))
                        map[item].BackColor = Color.Gold;
                    if (injuredEndPath.IsMatch(item))
                        map[item].BackColor = Color.Firebrick;
                } else if (injuredStartPath.IsMatch(item))
                    isInjuredStart = true ;

                if (teamSchedulePath.IsMatch(item))
                    map[item].BackColor = Color.HotPink;
                
                for (int i = 0; i < 17; i++)
                    if (versusPath[i].IsMatch(item))
                        map[item].BackColor = Color.Firebrick;

                if (teamEndPath.IsMatch(item))
                    map[item].BackColor = Color.Khaki;
            }
            this.BeginInvoke(new Action(delegate()
            {
                treeView1.BeginUpdate();
                treeView1.Nodes.Add(root);
                treeView1.ExpandAll();
                treeView1.EndUpdate();
            }));
            richTextBox1.Text = table.ToString();

            MessageBox.Show(CachedStringSegmentation.Requests + " " + CachedStringSegmentation.Hits);

            StringBuilder sb = new StringBuilder();
            foreach (var item in builder.TagStats.OrderBy<System.Collections.Generic.KeyValuePair<string, int>, int>(s => s.Value))
            {
                sb.AppendLine(item.Key + "..." + item.Value);
            }
            MessageBox.Show(sb.ToString());
            MessageBox.Show(builder.RunTime.ToString());
            MessageBox.Show(((float)builder.RunTime / (float)builder.Count).ToString());

            sb = new StringBuilder();
            foreach (var item in builder.Unknown)
            {
                sb.AppendLine(item.Key + "..." + item.Value);
            }
            MessageBox.Show(sb.ToString());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
        }

        /// <summary>
        /// let's see how the parser decoded the next
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void listBox1_DoubleClick(object sender, EventArgs e)
        {
            string x = (string)listBox1.SelectedItem;
            byte[] y = Encoding.Default.GetBytes(x);
            string z = BitConverter.ToString(y);
            MessageBox.Show(z);
        }

        /// <summary>
        /// let's see where in the dom the node is 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void treeView1_DoubleClick(object sender, EventArgs e)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine();
            TreeNode item = treeView1.SelectedNode;
            while (item!=null) {
                sb.AppendLine(item.Text);
                item = item.Parent;
            }

            richTextBox1.Text += sb.ToString();
        }

        private void button7_Click(object sender, EventArgs e)
        {
            //http://www.spotrac.com/nfl/injured-reserve/
            string url = textBox1.Text;
            if (url.StartsWith("http://"))
            {
                WebRequest request = WebRequest.Create(url);
                string filename = "webfile.htm";

                // Get the response.
                int size=1024;
                byte[] buffer = new byte[size];
                using (WebResponse response = request.GetResponse())
                {
                    // Get the stream containing content returned by the server.
                    using (Stream dataStream = response.GetResponseStream())
                    using (FileStream fs = new FileStream(filename, FileMode.Create))
                    {
                        int length = dataStream.Read(buffer, 0, size);
                        while (length > 0)
                        {
                            fs.Write(buffer, 0, length);
                            length = dataStream.Read(buffer, 0, size);
                        }
                    }
                }

                textBox1.Text = filename;
            }
            else
                textBox1.Text = "webfile.htm";
            button6_Click(sender, e);
        }

        static private HtmlPath getTestForInjuredPlayerTeam()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Td>() {
                            getTestForInjuredPlayerRow(),
                            new ElementPositionTest<Td>() { CompareValue=1 }
                        }
                    }
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerPos()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Td>() {
                            getTestForInjuredPlayerRow(),
                            new ElementPositionTest<Td>() { CompareValue=2 }
                        }
                    }
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerName()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    getTestForInjuredPlayerRow(),
                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                }
                            }
                        }
                    } 
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerCategory()
        {
            ElementTest top =
                new ElementTest<Literal>() { 
                    new ParentTest() {
                        new ElementTest<Span>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    getTestForInjuredPlayerRow(),
                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                }
                            },
                            new ElementPositionTest<Span>() { CompareValue=1 }
                        }
                    } 
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForInjuredPlayerID()
        {
            ElementTest top =
                        new ElementTest<Hyperlink>() {
                            new ParentTest() {
                                new ElementTest<Td>() {
                                    getTestForInjuredPlayerRow(),
                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private ITokenTest getTestForInjuredPlayerRow()
        {
            ITokenTest top =
                                    new ParentTest() {
                                        new ElementTest<Tr>() {
                                            new ParentTest() {
                                                new ElementTest<TBody>() {
                                                    new ParentTest() {
                                                        new ElementTest<TableTag>() {
                                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="datatable" },
                                                            new ParentTest() {
                                                                new ElementTest<Div>() {
                                                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="teams" },
                                                                }
                                                            }
                                                        }
                                                    },
                                                }
                                            }
                                        } 
                                    };

            return top;
        }
        static private HtmlPath getTestForInjuredPlayerSectionStart()
        {
            ITokenTest top =
                new ElementTest<Literal>() {
                    new ElementComparisonTest<string>() {convert = AttribComparisonTest<string>.PassThru, Operator=0, CompareValue="Reserve/Injured List By Player" },
                    new ParentTest() {
                        new ElementTest<BaseH>() {
                            new ParentTest() {
                                new ElementTest<UnknownTag>() {
                                    new AttribContainsTest() { AttributeName="CLASS", CompareValue="team-header" },
                                    new ParentTest() {
                                        new ElementTest<Div>() {
                                            new AttribContainsTest() { AttributeName="CLASS", CompareValue="teams" },
                                        }
                                    }
                                }
                            }
                        }
                    }
                };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }



        private HtmlPath getTestForScheduledVersus(int wk)
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new OrTest {
                                    new ElementTest<Hyperlink>() {
                                        new ParentTest() {
                                            new ElementTest<Td>() {
                                                getTestForScheduleRow(),
                                                new ElementPositionTest<Td>() { CompareValue=wk+1 }
                                            }
                                        }
                                    },
                                    new ElementTest<Td>() {
                                        getTestForScheduleRow(),
                                        new ElementPositionTest<Td>() { CompareValue=wk+1 }
                                    }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        private HtmlPath getTestForScheduledTeam()
        {
            ITokenTest top =
                        new ElementTest<Literal>() {
                            new ParentTest() {
                                new ElementTest<Hyperlink>() {
                                    new ParentTest() {
                                        new ElementTest<Bold>() {
                                            new ParentTest() {
                                                new ElementTest<Td>() {
                                                    getTestForScheduleRow(),
                                                    new ElementPositionTest<Td>() { CompareValue=0 }
                                                }
                                            }
                                        } 
                                    }
                                }
                            }
                        };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private HtmlPath getTestForScheduleRowEnd()
        {
            ITokenTest top = new ElementTest<Closure>() {
                                    getTestForScheduleRow()
                            };

            HtmlPath hpath = new HtmlPath();
            hpath.SetTest(top);
            return hpath;
        }
        static private ITokenTest getTestForScheduleRow()
        {
            ITokenTest top = new CacheTestResult(
                                    new ParentTest() {
                                        new ElementTest<Tr>() {
                                            new ElementPositionComparisonTest<Tr>() { Operator=1, CompareValue=1 },
                                            new ParentTest() {
                                                new ElementTest<TableTag>() {
                                                    new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="tablehead" },
                                                    new ParentTest() {
                                                        new ElementTest<Div>() {
                                                            new AttribComparisonTest<string>() { convert = AttribComparisonTest<string>.PassThru, AttributeName="CLASS", Operator=0, CompareValue="mod-content" },
                                                        }
                                                    }
                                                }
                                            },
                                        }
                                    });

            return top;
        }


    }
}

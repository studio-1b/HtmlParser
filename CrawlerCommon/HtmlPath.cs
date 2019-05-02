using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

using CrawlerCommon.TagDef.StrictXHTML;
using CrawlerCommon.HtmlPathTest;

namespace CrawlerCommon
{
    public class HtmlPath
    {
        public HtmlPath()
        {
        }
        public HtmlPath(string expression)
        {
            this.Parse(expression);
        }
        public HtmlPath(string expression, matchHandler eventdelegate): this(expression)
        {
            this.Match += eventdelegate;
        }

        public string Namespace { get; set;}
        

        public void SetTest(ITokenTest top)
        {
            this.topLevelTest = top;
        }

        public void Parse(ReadonlyStringSegmentation path)
        {
            this.Parse(path.Value);
        }
        public void Parse(string htmlpath)
        {
            ITokenTest test = this.parseHPath(htmlpath);
            this.topLevelTest = test;
        }


        string _expression = null;
        ITokenTest topLevelTest = null;
        public ITokenTest Copy
        {
            get
            {
                if (this.topLevelTest == null || this._expression == null)
                    return null;

                HtmlPath parser = new HtmlPath();
                parser.Parse(this._expression);

                var clone = parser.topLevelTest; //private variable within separate instance
                return clone;
            }
        }
        
        public bool IsMatch(Token node)
        {
            if (topLevelTest == null)
                throw new InvalidOperationException("No query");

            bool result = topLevelTest.IsMatch(node);

            return result;
        }
        public void BeginMatch(Token node)
        {
            if (topLevelTest == null)
                throw new InvalidOperationException("No query");

            if (this.Match != null)
            {
                bool result = topLevelTest.IsMatch(node);
                if (result)
                    this.Match(this, new MatchEventArgs<Token>(node));
            }
        }

        public delegate void matchHandler(object sender, MatchEventArgs<Token> e);
        public event matchHandler Match;



        /// <summary>
        /// Custom XPATH like expression
        /// Rules: Each character is traversal of parsing tree
        ///          table/tr
        ///        When a reset is warranted, it checks that the prev position in parsing tree is valid expression
        ///          table '/' triggers check of t-a-b-l-e node
        ///        if no leaf with test, it fails
        ///        otherwise it gets put on stack, depending on rules of the test in the leaf
        ///          if parenttest, it will pop test on stack, add the test to itself, and add parenttest to stack
        ///          if elementtest, it will pop test on stack, add the test to iteself and add elementtest to stack
        ///          etc.
        ///          if or, it needs to put -or- test marker to stack, that way, then 2nd test completes, it pops 3 times
        /// Reset back to top of parsing tree when
        ///        1. terminating character is encountered, forcing check of current parsing traveral is valid
        ///           - Is prev node a valid keyword?  If not - FAIL
        ///        2. end of branch reached, forcing reset (end of branch should always be valid keyword), but start of next is not forced
        ///        3. current character is not found in current next children, and current character is NOT terminating character - FAIL
        /// </summary>
        /// <returns></returns>
        ITokenTest parseHPath(string expr)
        {
            /*  literal/td/tr[1]/[table/td/tr/table[5]|tbody/table/td/tr/tbody/table[5]]
             
                is how the object test tree is created w the object of interest on left side (or top of tree), 
                but xpath puts object of interest on right side of expression.  
                    plus tree expressions are part of node conditional expressions
                    but for us, a tree represents either a or/and condition on anynumber of nodes
             
                [table[5]/tr/td/table|table[5]/tbody/tr/td/table/tbody]/tr[1]/td/literal
             
                we will parse a xpath-like expression for familarity of user
             */

            // pretend this is a cisc-like instruction byte stream, instructions can alter prev instructions
            // instruction1, instruction 2 popstack alter pushonstack, instruction 3 popstack alter pushonstack 
            Stack<ITokenTest> stack = new Stack<ITokenTest>();
            RecursiveDictionary<Type> tagIndex = getXpathExprIndexTree();
            RecursiveDictionary<Type> prev = null;
            //bool isPushed = false;
            int notided = 0;
            int len = expr.Length;
            for (int i = 0; i < len; i++)
            {
                var ch = expr[i];
                var found = tagIndex.Traverse(ch);

                if (found != null)
                {
                    // rule 2
                    if (found.Count == 0)
                    {
                        // no need to check if current has leaf, by definition it has to
                        var test = createTest(stack, found);
                        
                        if (test is IExprHandler)
                        {
                            var casted = (IExprHandler)test;
                            casted.ParseExpr(expr, notided, i - notided - 1, found.Operator);
                        }

                        System.Diagnostics.Debug.WriteLine("Expression [{0}]", expr.Substring(notided, i - notided+1),0);
                        notided = i + 1;
                        tagIndex.ResetTraversal();
                    }

                    // rule 1
                    else if (found.IsTerminationChar && notided != i)  //may have been reset by rule3
                    {
                        // still need to check if current traversal is valid keyword
                        if (prev.Leaf == null)
                            throw new ArgumentException("Invalid keyword");
                        var test = createTest(stack, prev);
                        
                        if (test is IExprHandler)
                        {
                            var casted = (IExprHandler)test;
                            casted.ParseExpr(expr, notided, i - notided - 1, prev.Operator);
                        }
                        System.Diagnostics.Debug.WriteLine("Expression [{0}]", expr.Substring(notided, i - notided+1),0);
                        notided = i;
                        tagIndex.ResetTraversal();
                        found = tagIndex.Traverse(ch);
                    }
                }
                else
                    // rule3
                    if (prev != null)
                    {
                        if (prev.Leaf != null)
                        {
                            // see if you can build this if-tree in as part of ???Test interface
                            var test = createTest(stack, prev);
                            
                            if (test is IExprHandler)
                            {
                                var casted = (IExprHandler)test;
                                casted.ParseExpr(expr, notided, i - notided, prev.Operator);
                            }
                            System.Diagnostics.Debug.WriteLine("Expression [{0}]", expr.Substring(notided, i - notided), 0);
                            notided = i;
                            tagIndex.ResetTraversal();
                            found = tagIndex.Traverse(ch);
                        }
                        else
                            throw new ArgumentException("expression not found " + expr.Substring(notided, i - notided));
                    }
                prev = found;
            }

            if (notided != len)
                if (prev.Leaf == null)
                    throw new ArgumentException("expression not found " + expr.Substring(notided));

                // process last expression
                //should not be parent test, either it ends wo /, or implicit . added
                else //if (typeof(BaseTestLinkage).IsAssignableFrom(prev.Leaf))
                {
                    var test = createTest(stack, prev);
                    if (test is IExprHandler)
                    {
                        var casted = (IExprHandler)test;
                        casted.ParseExpr(expr, notided, len - notided - 1, prev.Operator);
                    }
                }

            ITokenTest result = stack.Pop();
            while (stack.Count > 0)
            {
                throw new ArgumentException("stack should be empty");
            }
            tagIndex.ResetTraversal();


            // return new CacheTestResult(test); // add this optimization in, if you can figure out which parent has a lot of children, so the parent call fails a lot.
            return result;
        }

        /// <summary>
        /// creates the test indicated by the current node in parsing tree, and pushes it on the stack
        /// </summary>
        /// <param name="stack"></param>
        /// <param name="node"></param>
        ITokenTest createTest(Stack<ITokenTest> stack, RecursiveDictionary<Type> node)
        {
            ITokenTest result = null;
            if (stack.Count > 0)
            {
                var instr = stack.Pop();
                var clone = (ITokenTest)Activator.CreateInstance(node.Leaf); //clone too
                if(clone is AttribNodeTest) //should be last guy, and ignore parent semantics (/preceding the @)
                {
                    var casted = (AttribNodeTest)clone;
                    var throwaway = (ParentTest)instr;
                    foreach (var keep in throwaway)
                    {
                        casted.SubTest = keep;
                        stack.Push(casted);
                        break;
                    }
                }
                else if (clone is BaseTestLinkage)
                {
                    //if new test is tree-able, then make what's on stack a child
                    var casted = (BaseTestLinkage)clone;
                    casted.Add(instr); //order doesn't matter, if it checks the current node is root, or the element
                    //here it checks the elementtest first, then if it's root
                    stack.Push(casted);
                }
                // else vice versa
                else if (instr is BaseTestLinkage)
                {
                    var casted = (BaseTestLinkage)instr;
                    casted.Add(clone); //order doesn't matter, if it checks the current node is root, or the element
                    //here it checks the elementtest first, then if it's root
                    stack.Push(casted);
                }
                // not a supported expression
                else
                    throw new InvalidOperationException("There is something very wrong with the exception.  a test does not apply to an element (tree-able)");
                result = clone;
            }
            else
            {
                var clone = (ITokenTest)Activator.CreateInstance(node.Leaf);
                stack.Push(clone); //clone this, instead
                result = clone;
            }

            if (result is IConverterNeeded<int>)
            {
                var casted = (IConverterNeeded<int>)result;
                casted.convert = Convert.ToInt32;
            }
            if (result is IConverterNeeded<string>)
            {
                var casted = (IConverterNeeded<string>)result;
                casted.convert = IConverter.PassThru;
            }


            return result;
        }


        StringBuilder ToStringBuilder(ITokenTest test)
        {
            return ToStringBuilder(test, 0, null);
        }
        StringBuilder ToStringBuilder(ITokenTest test, int level, StringBuilder sb)
        {
            if (sb == null)
                sb = new StringBuilder();
            for (int i = 0; i < level; i++)
                sb.Append("\t");

            Type type = test.GetType();
            Type[] generic = type.GetGenericArguments();
            sb.AppendLine(type.Name + (generic.Length > 0 ? generic[0].Name : string.Empty));
            if (test is BaseTestLinkage)
                foreach (var item in (BaseTestLinkage)test)
                {
                    ToStringBuilder(item, level + 1, sb);
                }


            return sb;
        }



        #region support classes
        class RecursiveDictionary<Q> : Dictionary<char, RecursiveDictionary<Q>>
        {
            public Q Leaf;
            public string Operator;
            public bool IsTerminationChar = false;

            public Q Find(string expr)
            {
                RecursiveDictionary<Q> top = this;
                foreach (var ch in expr)
                    if (top.ContainsKey(ch))
                        top = top[ch];
                    else
                        return default(Q);

                return top.Leaf;
            }

            private RecursiveDictionary<Q> step;
            public void ResetTraversal()
            {
                step = this;
            }
            public RecursiveDictionary<Q> Traverse(char ch)
            {
                if (step == null)
                    step = this; // we can put this in constructor instead later, so we arent re-running code that only has significance during construction

                // RecursiveDictionary<Q> top = step;
                // if (step.ContainsKey(ch))
                //     step = step[ch];
                // else
                //     return null;
                step = step.NextNode(ch);

                return step;
            }

            virtual public RecursiveDictionary<Q> NextNode(char ch)
            {
                if (this.ContainsKey(ch))
                    return this[ch];
                else
                    return null;
            }
        }


        class LoopedTraversalNode<Q> : RecursiveDictionary<Q>
        {
            public List<char> ValidCharButNoTraversal = new List<char>();
            public List<char> InvalidChar = new List<char>();
            public override RecursiveDictionary<Q> NextNode(char ch)
            {
                if (this.ContainsKey(ch))
                    return this[ch];
                else
                {
                    if (InvalidChar != null) //dominant property, ignore valid char if used
                    {
                        if (InvalidChar.Contains(ch))
                            return null;
                        else
                            return this;
                    }
                    else
                    {
                        if (ValidCharButNoTraversal.Contains(ch))
                            return this; //now you have to plan for no movement
                        else
                            return null;
                    }
                }
            }
        }
        #endregion support classes

        #region test expression parsing tree
        static RecursiveDictionary<Type> _parsingtree = null;
        static RecursiveDictionary<Type> getXpathExprIndexTree()
        {
            if (_parsingtree == null)
                _parsingtree = buildXpathExprIndexTree();

            //clone the root, so no state is passed between invocations
            //the tree root ISNT stateless
            RecursiveDictionary<Type> tree = new RecursiveDictionary<Type>();
            
            foreach (var item in _parsingtree)
                tree.Add(item.Key, item.Value);

            //shallow clone only b/c only root is stateful.  the branches are not
            return tree; 
        }
        static RecursiveDictionary<Type> buildXpathExprIndexTree()
        {
            DefaultBuilder taglist = new DefaultBuilder();

            // include the parent test
            RecursiveDictionary<Type> tree = new RecursiveDictionary<Type>()
            {
                {'/', new RecursiveDictionary<Type>() { Leaf = typeof(ParentTest) } }
            };
            tree['/'].IsTerminationChar = true;


            tree.Add('L', new RecursiveDictionary<Type>()
            {
                {'I', new RecursiveDictionary<Type>()
                    {{'T', new RecursiveDictionary<Type>()
                        {{'E', new RecursiveDictionary<Type>()
                            {{'R', new RecursiveDictionary<Type>()
                                {{'A', new RecursiveDictionary<Type>()
                                    {{'L', new RecursiveDictionary<Type>() { Leaf = typeof(ElementTest<Literal>)} }}
                                }}
                            }}
                        }}
                    }}
                }
            });
            tree.Add('C', new RecursiveDictionary<Type>()
            {
                {'L', new RecursiveDictionary<Type>()
                    {{'O', new RecursiveDictionary<Type>()
                        {{'S', new RecursiveDictionary<Type>()
                            {{'U', new RecursiveDictionary<Type>()
                                {{'R', new RecursiveDictionary<Type>()
                                    {{'E', new RecursiveDictionary<Type>() { Leaf = typeof(ElementTest<Closure>)} }}
                                }}
                            }}
                        }}
                    }}
                }
            });

            int diff = 'A' - 'a';
            var notattrib = new List<char>() { ' ', '"', '=', '<', '>' };
            tree.Add('@', new RecursiveDictionary<Type>());
            for (char ch = 'A'; ch <= 'Z'; ch++)
            {
                tree['@'].Add(ch, new LoopedTraversalNode<Type>() { Leaf = typeof(AttribNodeTest), InvalidChar = notattrib }); //expected to be last in expresion
                tree['@'][ch].Add(';', new RecursiveDictionary<Type>()); // not expected to ever see
                char lwr = (char)((int)ch - diff);
                tree['@'].Add(lwr, new LoopedTraversalNode<Type>() { Leaf = typeof(AttribNodeTest), InvalidChar = notattrib }); //expected to be last in expresion
                tree['@'][lwr].Add(';', new RecursiveDictionary<Type>()); // not expected to ever see
            }

            //add in the parsing tree for each tag identifier, which each to a map to ElementTest
            foreach (var tag in taglist.SupportedTags())
                if (tag != null)
                {
                    RecursiveDictionary<Type> reset = tree;
                    string keyword = tag.GetExpectedTagString();
                    if (keyword == null)
                        continue;
                    foreach (var ch in keyword)
                    {
                        if (!reset.ContainsKey(ch))
                            reset.Add(ch, new RecursiveDictionary<Type>());
                        reset = reset[ch];
                    }
                    Type genericized = typeof(ElementTest<>).MakeGenericType(tag.GetType());
                    reset.Leaf = genericized;

                    // reset.Leaf = new ElementTest<Tr>(); // really put the tag your looking for here
                }


            tree.Add('.', new RecursiveDictionary<Type>() { Leaf = typeof(ElementTest) });

            tree.Add('[', new RecursiveDictionary<Type>() { 
                {'@', new LoopedTraversalNode<Type>() },
                {'<', new RecursiveDictionary<Type>() },
                {'>', new RecursiveDictionary<Type>() },
                {'!', new RecursiveDictionary<Type>() }
            });
            tree['[']['@'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribTest) });
            tree['[']['@'].Add('=', new RecursiveDictionary<Type>()); //A1) attrib = value, waiting for " or 0-9
            tree['[']['@'].Add('<', new RecursiveDictionary<Type>()); //A2) attrib < value, waiting for " or 0-9
            tree['[']['@'].Add('>', new RecursiveDictionary<Type>()); //A3) attrib > value, waiting for " or 0-9.
            tree['[']['@'].Add('!', new RecursiveDictionary<Type>());
            tree['[']['@']['<'].Add('=', new RecursiveDictionary<Type>()); //A4) attrib <=, waiting for " or 0-9
            tree['[']['@']['>'].Add('=', new RecursiveDictionary<Type>()); //A5) attrib >=, waiting for " or 0-9
            tree['[']['@']['!'].Add('=', new RecursiveDictionary<Type>()); //A6) attrib !=, waiting for " or 0-9
            var numericchar = new List<char>() { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9' };
            foreach (var num in numericchar)
            {
                //finish a1
                tree['[']['@']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = "=" });
                //finish a2
                tree['[']['@']['<'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['<'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = "<" });
                //finish a3
                tree['[']['@']['>'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['>'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = ">" });
                //finish a4
                tree['[']['@']['<']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['<']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = "<=" });
                //finish a5
                tree['[']['@']['>']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['>']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = ">=" });
                //finish a6
                tree['[']['@']['!']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['@']['!']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<int>), Operator = "!=" });
            }

            //attrib = string
            tree['[']['@']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = "=" });
            //attrib <string
            tree['[']['@']['<'].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['<']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = "<" });
            //attrib >string
            tree['[']['@']['>'].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['>']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = ">" });
            //attrib >=string
            tree['[']['@']['>']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['>']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = ">=" });
            //attrib <=string
            tree['[']['@']['<']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['<']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = "<=" });
            //attrib !=string
            tree['[']['@']['!']['='].Add('"', new LoopedTraversalNode<Type>() { { '\"', new RecursiveDictionary<Type>() } });
            tree['[']['@']['!']['=']['"']['"'].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribComparisonTest<string>), Operator = "!=" });


            tree['['].IsTerminationChar = true;
            tree['[']['<'].Add('=', new RecursiveDictionary<Type>());
            tree['[']['>'].Add('=', new RecursiveDictionary<Type>());
            tree['[']['!'].Add('=', new RecursiveDictionary<Type>());
            foreach (var num in numericchar)
            {
                tree['['].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['['][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionTest) });

                //no operator, implcitly means equals

                tree['[']['<'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['<'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = "<" });

                tree['[']['>'].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['>'][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = ">" });

                tree['[']['<']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['<']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = "<=" });

                tree['[']['>']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['>']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = ">=" });

                tree['[']['!']['='].Add(num, new LoopedTraversalNode<Type>() { ValidCharButNoTraversal = numericchar });
                tree['[']['!']['='][num].Add(']', new RecursiveDictionary<Type>() { Leaf = typeof(ElementPositionComparisonTest), Operator = "!=" });
            }
            tree['[']['@'].Add(' ', new RecursiveDictionary<Type>()
                                {{'C', new RecursiveDictionary<Type>()
                                    {{'O', new RecursiveDictionary<Type>()
                                        {{'N', new RecursiveDictionary<Type>()
                                            {{'T', new RecursiveDictionary<Type>()
                                                {{'A', new RecursiveDictionary<Type>()
                                                    {{'I', new RecursiveDictionary<Type>()
                                                        {{'N', new RecursiveDictionary<Type>()
                                                            {{'S', new RecursiveDictionary<Type>()
                                                                {{' ', new RecursiveDictionary<Type>()
                                                                    {{'\"', new LoopedTraversalNode<Type>()
                                                                        {{'\"', new RecursiveDictionary<Type>()
                                                                            {{']', new RecursiveDictionary<Type>() { Leaf = typeof(AttribContainsTest)} }}
                                                                        }}
                                                                    }}
                                                                }}
                                                            }}
                                                        }}
                                                    }}
                                                }}
                                            }}
                                        }}
                                    }}
                                }});

            Type ortype = typeof(OrTest);
            tree.Add('(', new RecursiveDictionary<Type>() { Leaf = ortype });
            tree['('].IsTerminationChar = true;
            tree.Add(')', new RecursiveDictionary<Type>() { Leaf = ortype });
            tree[')'].IsTerminationChar = true;
            tree.Add('|', new RecursiveDictionary<Type>() { Leaf = ortype });
            tree['|'].IsTerminationChar = true;


            // attribute test [@name]
            // attribute test [@name="value"]
            // attribute test [@name!=0]
            // attribute test [@name contains "value"]
            // position test  [0], only if element test above
            // position test  [<0], [>=0], [!=0]
            // or test        (...|...)

            // any tag        .
            // any token      *

            // and test       element[0][name=""]
            // is different from xpath

            // xpath has ./@attrib to extract attribute.  None here for now
            // no "//" until I can figure it out what it means
            // only "table/tr/td/literal" or "/table/tr/td/literal"

            // override Traverse(char) for 
            //                              [@                  for ] to terminate
            //                                                  for = to upgrade to numeric test
            //                                                      for ] to terminate
            //                                                      for " to upgrade to string test
            //                                                          for "] to terminate
            //                              [@name!=            for ![0-9] to terminate
            //                              [@name contains "   for "] to terminate
            //                              [>=                 for only numbers and ] to terminate
            //                              [0                  for only numbers and ] to terminate

            return tree;
        }
        #endregion test expression parsing tree


        /*
        nodename 	Selects all nodes with the name "nodename"
        / 	Selects from the root node
        // 	Selects nodes in the document from the current node that match the selection no matter where they are
        . 	Selects the current node
        .. 	Selects the parent of the current node
        @ 	Selects attributes
            bookstore 	Selects all nodes with the name "bookstore"
        /bookstore 	Selects the root element bookstore

        Note: If the path starts with a slash ( / ) it always represents an absolute path to an element!
        bookstore/book 	Selects all book elements that are children of bookstore
        //book 	Selects all book elements no matter where they are in the document
        bookstore//book 	Selects all book elements that are descendant of the bookstore element, no matter where they are under the bookstore element
        //@lang 	Selects all attributes that are named lang
        /bookstore/book[1] 	Selects the first book element that is the child of the bookstore element.

        Note: IE5 and later has implemented that [0] should be the first node, but according to the W3C standard it should have been [1]!!
        /bookstore/book[-1] 	Selects the last book element that is the child of the bookstore element
        /bookstore/book[-2] 	Selects the last but one book element that is the child of the bookstore element
        /bookstore/book[position()<3] 	Selects the first two book elements that are children of the bookstore element
        /bookstore/book[position()>-3] 	Selects the last two book elements that are children of the bookstore element
        //title[@lang] 	Selects all the title elements that have an attribute named lang
        //title[@lang='eng'] 	Selects all the title elements that have an attribute named lang with a value of 'eng'
        /bookstore/book[price>35.00] 	Selects all the book elements of the bookstore element that have a price element with a value greater than 35.00
        /bookstore/book[price>35.00]/title 	Selects all the title elements of the book elements of the bookstore element that have a price element with a value greater than 35.00
            * 	Matches any element node
        @* 	Matches any attribute node
        node() 	Matches any node of any kind
            /bookstore/* 	Selects all the child nodes of the bookstore element
        //* 	Selects all elements in the document
        //title[@*] 	Selects all title elements which have any attribute
        */

        /// Parse a single test in 3 parts (value or value factory, operator, value or value factory)
        /// [abc] --> "abc", Empty, Empty
        /// [abc>1] --> "abc", ">", "1"
        /// [position()=-1] --> "position()", "=", "-1"
        /// [@attrib] --> "@attrib", Empty, Empty
        /// [@attrib="abc"] --> "@attrib", "=", ""abc""

        /*
        nodename 	Selects all nodes with the name "nodename"
            / 	Selects from the root node
            // 	Selects nodes in the document from the current node that match the selection no matter where they are
            . 	Selects the current node
            .. 	Selects the parent of the current node
            @ 	Selects attributes
                bookstore 	Selects all nodes with the name "bookstore"
            /bookstore 	Selects the root element bookstore

            Note: If the path starts with a slash ( / ) it always represents an absolute path to an element!
            bookstore/book 	Selects all book elements that are children of bookstore
            //book 	Selects all book elements no matter where they are in the document
            bookstore//book 	Selects all book elements that are descendant of the bookstore element, no matter where they are under the bookstore element
            //@lang 	Selects all attributes that are named lang
            /bookstore/book[1] 	Selects the first book element that is the child of the bookstore element.

            Note: IE5 and later has implemented that [0] should be the first node, but according to the W3C standard it should have been [1]!!
            /bookstore/book[last()] 	Selects the last book element that is the child of the bookstore element
            /bookstore/book[last()-1] 	Selects the last but one book element that is the child of the bookstore element
            /bookstore/book[position()<3] 	Selects the first two book elements that are children of the bookstore element
            //title[@lang] 	Selects all the title elements that have an attribute named lang
            //title[@lang='eng'] 	Selects all the title elements that have an attribute named lang with a value of 'eng'
            /bookstore/book[price>35.00] 	Selects all the book elements of the bookstore element that have a price element with a value greater than 35.00
            /bookstore/book[price>35.00]/title 	Selects all the title elements of the book elements of the bookstore element that have a price element with a value greater than 35.00
                * 	Matches any element node
            @* 	Matches any attribute node
            node() 	Matches any node of any kind
                /bookstore/* 	Selects all the child nodes of the bookstore element
            //* 	Selects all elements in the document
            //title[@*] 	Selects all title elements which have any attribute
           */

        static Dictionary<string, string> ParsingDiagnosticList = new Dictionary<string, string>()
        {
            //get literal value of single element
            {"//a[.]", "<html><head><title>YYY</title></head><body>ZZZ<a href='XXX'>test</a></body></html>"},
            //get node by type
            {"title", "<html><head><title>test</title></head><body><h1>ZZZ</h1><P><span class='css'>AAA</span></body></html>"},
            //get node out of list by comparing begin position
            {"/html/body/ul/li[position()>1]", "<html><head><script>var x=1;</script></head><body><ul><li>EEE<li>FFF<li>test<ul></body></html>"},
            //get ONE node out of list by begin position
            {"tabletag/tr/td[1]", "<html><head><style>.Classname { display:none }</style></head><body>CCC<table><tr><td>BBB</td><td>test</td></tr></table>DDD</body></html>"},
            //get ONE node out of list by end position
            {"//formtag/select/option[-1]", "<html><body><form action='post'><select name='choices'><option>1<option>2<option>3<option>4<option>5<option>6<option>test</select></form></html>"},
            //filter element by attribute value
            {"span[id='name']", ""},
            //filter parent by attribute value
            {"div[id='ivy']//a", ""},
            //filter element by aggregate condition
            {"//formtag/select/input[@type='radio' and value>10]", ""},
            //filter element by string methods
            {"span[substring(0,3)='ABC']", ""},
            //filter element by children
            {"A[img=abc.gif]", ""},
            //filter multiple parents
            {"tabletag/tr[td=\"SSS\"]/td[0]/a", "<html><body>CCC<table><tr class='header'><td>test</td><td>SSS</td></tr><tr><td>BBB</td><td>QQQ</td></tr></table>DDD</body></html>"}
        };



        public string GetValue(Token token)
        {
            if (this.topLevelTest is AttribNodeTest)
            {
                AttribNodeTest casted = (AttribNodeTest)this.topLevelTest;
                var name = casted.AttributeName.ToUpper();
                var tag = (Tag)token;
                var attrib = tag.Attrib.SingleOrDefault<TagAttribute>(s => s.Name.ToUpper() == name);
                return attrib.Value;
            }
            else
                return token.ActualSymbol;
        }
    }


}

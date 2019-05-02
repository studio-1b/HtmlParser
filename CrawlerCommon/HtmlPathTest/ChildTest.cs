using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    /*
     /bookstore/book[price]
     */
    public class ChildTest: BaseTestLinkage
    {
        #region ITokenTest
        override public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (!(token is Node)) return false;
            Node node = (Node)token;
            if(node.ChildElements.Count==0) return false;

            //if (!node.IsClosureTraversed)
            //    throw new TagNotClosedException();

            foreach(var child in node.ChildElements)
                foreach (var test in this.ChildSubTest)
                    if (!test.IsMatch(node))
                        return false;

            if(this.Match!=null)
                this.Match(this, new MatchEventArgs<Token>(node));
            
            return true;
        }

        override public event MatchHandler Match;
        #endregion ITokenTest

        public List<ITokenTest> ChildSubTest { get { return this.TestList; } }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    /*
     bookstore/book
     */
    public class ParentTest : BaseTestLinkage
    {
        #region ITokenTest
        override public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (token.ParentNode == null) return false;
            token = token.ParentNode;

            foreach (var test in this.ParentSubTest)
                if (!test.IsMatch(token))
                    return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }

        override public event MatchHandler Match;
        #endregion ITokenTest

        public List<ITokenTest> ParentSubTest { get { return this.TestList; }}
    }
}

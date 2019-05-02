using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    /*
     //book
     */
    public class RootTest : ITokenTest
    {
        #region ITokenTest
        public bool IsMatch(Token node)
        {
            if (node == null) return false;
            if (!(node.ParentNode is Document)) return false;
            if (node.ParentNode.ParentNode != null) return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(node));

            return true;
        }

        public event MatchHandler Match;
        #endregion ITokenTest
    }
}

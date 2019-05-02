using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    public class OrTest : BaseTestLinkage
    {
        //public Type Expected = null;

        #region ITokenTest
        override public bool IsMatch(Token token)
        {
            if (token == null) return false;
            //if (!this.Expected.IsAssignableFrom(token.GetType())) return false;

            foreach (var test in this.SubTest)
                if (test.IsMatch(token))
                {
                    if (this.Match != null)
                        this.Match(this, new MatchEventArgs<Token>(token));

                    return true;
                }

            return false;
        }

        override public event MatchHandler Match;

        public List<ITokenTest> SubTest { get { return this.TestList; } }
        #endregion ITokenTest
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    public class NotTest : ITokenTest
    {
        //public Type Expected = null;
        public NotTest(ITokenTest test)
        {
            this.BaseTest = test;
        }
        public ITokenTest BaseTest { get; private set; }

        #region ITokenTest
        public bool IsMatch(Token token)
        {
            if (token == null) return false;
            //if (!this.Expected.IsAssignableFrom(token.GetType())) return false;

            if (!this.BaseTest.IsMatch(token))
            {
                if (this.Match != null)
                    this.Match(this, new MatchEventArgs<Token>(token));

                return true;
            }

            return false;
        }

        public event MatchHandler Match;

        #endregion ITokenTest
    }
}

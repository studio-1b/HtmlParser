using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    /* Identifies if node matches type, or has attribute (no implementation to check attribute itself matches rules)
     "*"
     @*
     node()
     bookstore
     */
    public class ElementTest<T> : ElementTest where T : Token
    {
        public ElementTest() 
        {
            this.Expected = typeof(T);
        }

    }

    public class ElementTest : BaseTestLinkage
    {
        public Type Expected = null;

        #region ITokenTest
        override public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (!this.Expected.IsAssignableFrom(token.GetType())) return false;

            foreach (var test in this.SubTest)
                if (!test.IsMatch(token))
                    return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }

        override public event MatchHandler Match;

        public List<ITokenTest> SubTest { get { return this.TestList; } }
        #endregion ITokenTest
    }
}

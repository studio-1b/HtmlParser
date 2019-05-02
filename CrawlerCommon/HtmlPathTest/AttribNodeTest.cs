using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    public class AttribNodeTest : AttribTest
    {
        

        override public bool IsMatch(Token token)
        {
            var b = base.IsMatch(token);
            if (!b)
                return false;

            if(this.SubTest!=null)
                b = this.SubTest.IsMatch(token);
            
            return b;
        }

        public ITokenTest SubTest { get; set; }

        #region IExprHandler
        override public void ParseExpr(string expr)
        {
            this.ParseExpr(expr, 0, expr.Length, null);
        }
        /// <summary>
        /// Gets attribute namefor Test, from string [@name]
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        override public void ParseExpr(string expr, int startIndex, int length, string operatorHint)
        {
            var at = expr.IndexOf('@', startIndex);
            if (at < 0)
                throw new ArgumentException("expression cannot be parsed as a AttribContainsTest");
            
            this.AttributeName = expr.Substring(at+1).Trim();
        }
        #endregion IExprHandler
    }
}

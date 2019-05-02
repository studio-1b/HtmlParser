using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    public class ElementPositionTest<T> : ElementPositionComparisonTest<T>, IExprHandler where T : Token
    {
        public ElementPositionTest()
        {
            this.Operator = 0;
        }

        /*
         * /bookstore/book[1]
            /bookstore/book[last()]
            /bookstore/book[last()-1]
         */
        #region ITokenTest

        #endregion ITokenTest


        #region IExprHandler
        override public void ParseExpr(string expr)
        {
            ParseExpr(expr, 0, expr.Length, null);
        }
        /// <summary>
        /// Gets comparison value for Test, from string [2]
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        override public void ParseExpr(string expr, int startIndex, int length, string operatorHint)
        {
            //no operator supported in this test

            var start = expr.IndexOf('[', startIndex)+1;
            var end = expr.IndexOf(']', start);
            if (start < 0 || end < 0)
                throw new ArgumentException("expression cannot be parsed as a ElementPositionTest");
            var value = expr.Substring(start, end-start).Trim();
            this.CompareValue = int.Parse(value);
        }
        #endregion IExprHandler

    }

    public class ElementPositionTest : ElementPositionTest<Token>
    {
        
    }
}

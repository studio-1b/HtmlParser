using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    /*
    //title[@lang='eng']
    */
    public class AttribContainsTest : ITokenTest, IExprHandler
    {
        #region ITokenTest
        public string CompareValue;
        public string AttributeName;

        public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (!(token is Tag)) return false;
            Tag tag = (Tag)token;

            TagAttribute search = tag.Attrib.FirstOrDefault<TagAttribute>(s => s.Name.ToUpper() == AttributeName.ToUpper());
            if (search == null) return false;

            bool result = search.Value.Contains(this.CompareValue);
            if (!result) return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }

        public event MatchHandler Match;
        #endregion ITokenTest





        #region IExprHandler
        public void ParseExpr(string expr)
        {
            this.ParseExpr(expr, 0, expr.Length, " contains ");
        }
        /// <summary>
        /// Gets attribute name, and comparison value for Test, from string [@name contains \"waldo\"]
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        public void ParseExpr(string expr, int startIndex, int length, string operatorHint)
        {
            var at = expr.IndexOf('@', startIndex);
            var eq = expr.IndexOf(" CONTAINS ", at);
            if (at < 0 || eq < 0)
                throw new ArgumentException("expression cannot be parsed as a AttribContainsTest");
            this.AttributeName = expr.Substring(at + 1, eq - at - 1).Trim();
            
            //contains is a string operator only
            var open = expr.IndexOf("\"", eq+9) + 1;
            var close = expr.IndexOf("\"", open);
            if (open < 0 || close < 0)
                throw new ArgumentException("expression cannot be parsed as a AttribContainsTest");
            this.CompareValue = expr.Substring(open, close - open);
            
        }
        #endregion IExprHandler
    }
}

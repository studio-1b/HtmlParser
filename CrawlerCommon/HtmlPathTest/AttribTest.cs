using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    /*  ... the two tests are the same b/c current implementation does not return attributes, only elements
        //title[@lang]
        //@lang
     */
    public class AttribTest : ITokenTest, IExprHandler
    {

        #region ITokenTest
        public string AttributeName;

        virtual public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (!(token is Tag)) return false;
            Tag tag = (Tag)token;

            TagAttribute search = tag.Attrib.FirstOrDefault<TagAttribute>(s => s.Name.ToUpper() == AttributeName.ToUpper());
            if (search == null) return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }

        public event MatchHandler Match;
        #endregion ITokenTest





        #region IExprHandler
        virtual public void ParseExpr(string expr)
        {
            this.ParseExpr(expr, 0, expr.Length, null);
        }
        /// <summary>
        /// Gets attribute namefor Test, from string [@name]
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        virtual public void ParseExpr(string expr, int startIndex, int length, string operatorHint)
        {
            var at = expr.IndexOf('@', startIndex);
            var eq = expr.IndexOf(']', at);
            if (at < 0 || eq < 0)
                throw new ArgumentException("expression cannot be parsed as a AttribTest");
            this.AttributeName = expr.Substring(at + 1, eq - at - 1).Trim();
        }
        #endregion IExprHandler
    }
}

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
    public class AttribComparisonTest<T> : ITokenTest, IConverterNeeded<T>, IExprHandler
    {

        #region ITokenTest
        public T CompareValue;
        public int Operator;
        public string AttributeName;

        public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (!(token is Tag)) return false;
            Tag tag = (Tag)token;

            TagAttribute search = tag.Attrib.FirstOrDefault<TagAttribute>(s => s.Name.ToUpper() == AttributeName.ToUpper());
            if (search == null) return false;

            T sourceValue = convert(search.Value);
            int result = Comparer<T>.Default.Compare(sourceValue, this.CompareValue);
            if (result != Operator) return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }
        public Converter<string, T> convert {get;set;}

        public event MatchHandler Match;
        #endregion ITokenTest



        public static readonly int EQ = 0;
        public static readonly int LT = -1;
        public static readonly int GT = 1;
        public static readonly int NE = -3;
        public static readonly int LE = -2;
        public static readonly int GE = 2;



        #region IExprHandler
        static Dictionary<string, int> _operators = new Dictionary<string, int>() { { ">=", GE }, { "<=", LE }, { "!=", NE }, { "=", EQ }, { ">", GT }, { "<", LT } };
        public void ParseExpr(string expr)
        {
            foreach (var op in _operators)
            {
                var eq = expr.IndexOf(op.Key);
                if (eq >= 0)
                {
                    this.ParseExpr(expr, 0, expr.Length, op.Key);
                    break;
                }
            }
        }

        /// <summary>
        /// Gets attribute name, operator, and comparison value for Test, from string [@name=13] or [@name=\"\"]
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        public void ParseExpr(string expr, int startIndex, int length, string operatorHint)
        {
            this.Operator = _operators[operatorHint];

            var at = expr.IndexOf('@', startIndex);
            var eq = expr.IndexOf(operatorHint, at);
            if (at < 0 || eq < 0)
                throw new ArgumentException("expression cannot be parsed as a AttribComparisonTest");
            this.AttributeName = expr.Substring(at + 1, eq - at - 1).Trim();
            if (typeof(T) == typeof(string))
            {
                var open = expr.IndexOf("\"", eq + operatorHint.Length)+1;
                var close = expr.IndexOf("\"", open);
                if (open < 0 || close < 0)
                    throw new ArgumentException("expression cannot be parsed as a AttribComparisonTest");
                
                this.CompareValue = this.convert(expr.Substring(open, close - open));
            }
            else
            {
                var open = eq + operatorHint.Length;
                var close = expr.IndexOf("]", open);
                if (open < 0 || close < 0)
                    throw new ArgumentException("expression cannot be parsed as a AttribComparisonTest");
                this.CompareValue = this.convert(expr.Substring(open, close - open));
            }
            var equals = expr.IndexOf('=', at);
        }
        #endregion IExprHandler


        static public string PassThru(string dummy)
        {
            return dummy;
        }

    }
}

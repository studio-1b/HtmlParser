using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    public class ElementPositionComparisonTest<T> : ITokenTest, IExprHandler where T:Token
    {
        ///bookstore/book[position()<3]
        #region ITokenTest
        public int CompareValue;
        public int Operator;

        public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (token.ParentNode == null) return false;
            if (!(token is Node)) return false;
            Node node = (Node)token;
            Node parent = (Node)token.ParentNode;

            int sourceValue = parent.ChildElements.Where<Token>(s=>s is T).TakeWhile<Token>(s => s != token).Count<Token>();
            int result = Comparer<int>.Default.Compare(sourceValue, this.CompareValue);
            if (this.Operator==NE && result == 0) return false; //Not equals
            if (this.Operator == LE || this.Operator == GE)
                if (this.Operator * result < 0) return false;
            if (result != Operator) return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }

        public event MatchHandler Match;
        #endregion ITokenTest


        public static readonly int EQ=0;
        public static readonly int LT = -1;
        public static readonly int GT = 1;
        public static readonly int NE = -3;
        public static readonly int LE = -2;
        public static readonly int GE = 2;


        #region IExprHandler
        static Dictionary<string, int> _operators = new Dictionary<string, int>() { { ">=", GE }, { "<=", LE }, { "!=", NE }, { "=", EQ }, { ">", GT }, { "<", LT } };
        virtual public void ParseExpr(string expr)
        {
            foreach (var op in _operators)
            {
                var eq = expr.IndexOf(op.Key);
                if (eq >= 0)
                {
                    ParseExpr(expr, 0, expr.Length, op.Key);
                    break;
                }
            }
        }
        /// <summary>
        /// Gets operator, and comparison value for Test, from string [!=13] or [<10]
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        virtual public void ParseExpr(string expr, int startIndex, int length, string operatorHint)
        {
            this.Operator = _operators[operatorHint];

            var eq = expr.IndexOf(operatorHint, startIndex) + operatorHint.Length;
            var end = expr.IndexOf(']', eq);
            if (end < 0 || eq < 0)
                throw new ArgumentException("expression cannot be parsed as a ElementPositionComparisonTest");
            this.CompareValue = int.Parse(expr.Substring(eq, end-eq));
        }
        #endregion IExprHandler

    }
    public class ElementPositionComparisonTest : ElementPositionComparisonTest<Token>
    {
        /*
        ///bookstore/book[position()<3]
        #region ITokenTest
        public int CompareValue;
        public int Operator;

        public bool IsMatch(Token token)
        {
            if (token == null) return false;
            if (token.ParentNode == null) return false;
            if (!(token is Node)) return false;
            Node node = (Node)token;
            Node parent = (Node)token.ParentNode;

            int sourceValue = parent.ChildElements.TakeWhile<Token>(s => s != token).Count<Token>();
            int result = Comparer<int>.Default.Compare(sourceValue, this.CompareValue);
            if (result != Operator) return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }

        public event MatchHandler Match;
        #endregion ITokenTest
        */
    }

}

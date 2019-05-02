using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    /*
    /bookstore/book[price>35.00]
    /bookstore/book[price>35.00]/title
    */

    public enum ComparisonOperator{
        LessThan = -1, Equals=0, GreaterThan = 1
    }
    public class ElementComparisonTest<T> : ITokenTest //where T : struct
    {
        
        public T CompareValue;
        public int Operator;

        #region ITokenTest
        public bool IsMatch(Token token)
        {
            if (token == null) return false;
            //if (!(token is Node)) return false; //nodes have child elements
            //Node node = (Node)token;

            //Token search = node.ChildElements.FirstOrDefault<Token>(s => s is Literal);
            //if (search == null) return false;

            T sourceValue = convert(token.ActualSymbol);
            int result = Comparer<T>.Default.Compare(sourceValue, this.CompareValue);
            if (result != Operator) return false;

            if (this.Match != null)
                this.Match(this, new MatchEventArgs<Token>(token));

            return true;
        }

        public event MatchHandler Match;

        public Converter<string, T> convert;
        #endregion ITokenTest
    }
}

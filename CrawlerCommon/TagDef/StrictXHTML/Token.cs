using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    abstract public class Token
    {
        public Token(Token parent, string symbolString)
        {
            _parent = parent;
            this.ActualSymbol = symbolString;
        }


        protected Token _parent = null;
        public Token ParentNode
        {
            get
            {
                return _parent;
            }
        }

        public int OpenIndex;
        public string ActualSymbol { get; private set; }

        abstract public Token LikeIdentify(string value, ref Node parentContext);

        public void AssignParent(Token value)
        {
            _parent = value;
        }
    }


}

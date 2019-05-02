using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    public class Literal : Token
    {
        public Literal(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            Token element = new Literal(parentContext, value);
            parentContext.ChildElements.Add(element);
            return element;
        }
    }
}

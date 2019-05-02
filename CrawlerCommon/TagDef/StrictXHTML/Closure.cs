using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    public class Closure : Tag
    {
        public Closure(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            return null;
        }
    }
    public class ImplicitClosure : Closure
    {
        public ImplicitClosure(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            return null;
        }
    }
}

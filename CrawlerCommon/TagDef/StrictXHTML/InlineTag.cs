using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    abstract public class InlineTag : Tag
    {
        public InlineTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }
    }
}

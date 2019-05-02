using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    abstract public class BlockTag : Tag
    {
        public BlockTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    abstract public class Node : Token
    {
        public Node(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        public readonly TokenCollection ChildElements = new TokenCollection();

        public bool IsClosureTraversed = false;
    }
}

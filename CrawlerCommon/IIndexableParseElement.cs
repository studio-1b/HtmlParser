using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon
{
    public interface IIndexableParseElement
    {
        List<string> Traversal { get; }
    }
}

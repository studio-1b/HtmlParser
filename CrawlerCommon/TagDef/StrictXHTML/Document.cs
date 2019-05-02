using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    /// <summary>
    /// This is root Node for Builder (not Html), in case html has errors and 2 html docs.
    /// </summary>
    public class Document : Node
    {
        public Document()
            : base(null, string.Empty)
        { }

        /// <summary>
        /// Not implemented.  Pseudo-tag object never has string representation to parse.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            throw new NotImplementedException();
        }
    }

}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    public class CommentTag : Token, IIndexableParseElement
    {
        public CommentTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            value = value.Trim();
            if (value.StartsWith("<!--") && value.EndsWith("-->"))
            {
                Token element = new CommentTag(parentContext, value);
                parentContext.ChildElements.Add(element);
                return element;
            }
            else return null;
        }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<!--" }; } }
        #endregion IIndexableParseElement
    }

}

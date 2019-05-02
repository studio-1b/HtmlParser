using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    public class ObjectTag : InlineTag, IIndexableParseElement
    {
        public ObjectTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "OBJECT"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new ObjectTag(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Param : InlineTag, IIndexableParseElement
    {
        public Param(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "PARAM"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Param(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

}

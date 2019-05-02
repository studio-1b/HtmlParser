using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    public class Html : BlockTag, IIndexableParseElement
    {
        public Html(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "HTML"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Html(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Head : BlockTag, IIndexableParseElement
    {
        public Head(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "HEAD"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Head(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement

    }


    public class Title : InlineTag, IIndexableParseElement
    {
        public Title(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TITLE"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Title(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement

    }
    public class Style : InlineTag, IIndexableParseElement
    {
        public Style(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "STYLE"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Style(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement

    }

    public class Body : BlockTag, IIndexableParseElement
    {
        public Body(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "BODY"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Body(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }



}

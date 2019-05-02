using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    abstract public class PresentationalTag : InlineTag
    {
        public PresentationalTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

    }

    public class Bold : PresentationalTag, IIndexableParseElement
    {
        public Bold(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "B"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Bold(parentContext, value); }


        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Italic : PresentationalTag, IIndexableParseElement
    {
        public Italic(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "I"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Italic(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class TeleType : PresentationalTag, IIndexableParseElement
    {
        public TeleType(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TT"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new TeleType(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class SubScript : PresentationalTag, IIndexableParseElement
    {
        public SubScript(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "SUB"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new SubScript(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class SuperScript : PresentationalTag, IIndexableParseElement
    {
        public SuperScript(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "SUP"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new SuperScript(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Font : PresentationalTag, IIndexableParseElement
    {
        public Font(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "FONT"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Font(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Big : PresentationalTag, IIndexableParseElement
    {
        public Big(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "BIG"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Big(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Small : PresentationalTag, IIndexableParseElement
    {
        public Small(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "SMALL"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Small(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class Hr : NilTag, IIndexableParseElement
    {
        public Hr(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "HR"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Hr(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    public class TableTag : BlockTag, IIndexableParseElement
    {
        public TableTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TABLE"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new TableTag(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Tr : BlockTag, IIndexableParseElement
    {
        public Tr(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TR"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Tr(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Td : BlockTag, IIndexableParseElement
    {
        public Td(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TD"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Td(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Th : BlockTag, IIndexableParseElement
    {
        public Th(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TH"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Th(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class TBody : BlockTag, IIndexableParseElement
    {
        public TBody(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TBody"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new TBody(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class THead : BlockTag, IIndexableParseElement
    {
        public THead(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "THead"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new THead(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class TFoot : BlockTag, IIndexableParseElement
    {
        public TFoot(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TFoot"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new TFoot(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Col : BlockTag, IIndexableParseElement
    {
        public Col(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "Col"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Col(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }


    public class ColGroup : BlockTag, IIndexableParseElement
    {
        public ColGroup(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "ColGroup"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new ColGroup(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Caption : BlockTag, IIndexableParseElement
    {
        public Caption(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "CAPTION"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Caption(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

}

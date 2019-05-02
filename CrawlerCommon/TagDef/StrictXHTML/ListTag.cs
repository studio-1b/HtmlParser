using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    abstract public class ListTag : BlockTag
    {
        public ListTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }
    }

    public class UnorderedList : BlockTag, IIndexableParseElement
    {
        public UnorderedList(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "UL"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new UnorderedList(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class OrderedList : BlockTag, IIndexableParseElement
    {
        public OrderedList(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "OL"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new OrderedList(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class ListItem : InlineTag, IIndexableParseElement
    {
        public ListItem(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "LI"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new ListItem(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }


    public class DefinitionList : BlockTag, IIndexableParseElement
    {
        public DefinitionList(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "DL"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new DefinitionList(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }


}

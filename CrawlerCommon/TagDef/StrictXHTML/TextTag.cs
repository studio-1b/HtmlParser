using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    public class Span : InlineTag, IIndexableParseElement
    {
        public Span(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "SPAN"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Span(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Paragraph : InlineTag, IIndexableParseElement
    {
        public Paragraph(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "P"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Paragraph(parentContext, value); }


        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class BaseH : InlineTag, IIndexableParseElement
    {
        public BaseH(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "H"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new BaseH(parentContext, value); }

        override protected Token LikeIdentify(string value, string expected, ref Node parentContext, callerTagFactory factory)
        {
            TagType tagType = parseTagType(value); //cursory look
            if (tagType != TagType.Not)
                try
                {
                    ReadonlyStringSegmentation parts = value.Segment(parseTag);
                    string name = parts[0].ToUpper();
                    if ((name != "H1")
                        && (name != "H2")
                        && (name != "H3")
                        && (name != "H4")
                        && (name != "H5")
                        && (name != "H6")) return null; //name has to match expected
                }
                catch (HtmlParseException ex) { return null; } //cursory passed, but unable to parse tag for name

            //return the correct tag type
            if ((tagType == TagType.Open) || (tagType == TagType.Nil))
            {
                Token element = factory(parentContext, value);
                if (element is Tag)
                    ((Tag)element).IsNil = (tagType == TagType.Nil);

                parentContext.ChildElements.Add(element);
                if (element is Node)
                    parentContext = (Node)element;

                System.Diagnostics.Debug.Assert(deferredTagName != null, "Problem getting tag name from " + value);
                return element;
            }
            else if (tagType == TagType.Close)
            {
                if (!(parentContext is Tag))
                    System.Diagnostics.Debug.Assert(parentContext is Tag, "Only tags should have the potential to be closed");

                Tag element = new Closure(parentContext, value);
                parentContext.ChildElements.Add(element);
                Tag newParent = findMatchingOpen((Closure)element, (Tag)parentContext);
                if (newParent != null)
                    parentContext = newParent;

                if (parentContext == null)
                    System.Diagnostics.Debug.Assert(parentContext != null, "The stack should never be empty.  There should be at least the document");

                System.Diagnostics.Debug.Assert(deferredTagName != null, "Problem getting tag name from " + value);

                return element;
            }
            else return null; //Not a tag
        }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<H1", "</H1", "<H2", "</H2", "<H3", "</H3", "<H4", "</H4", "<H5", "</H5", "<H6", "</H6" }; } }
        #endregion IIndexableParseElement
    }

    public class Strong : InlineTag, IIndexableParseElement
    {
        public Strong(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "STRONG"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Strong(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Pre : InlineTag, IIndexableParseElement
    {
        public Pre(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "PRE"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Pre(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement

    }
    public class Br : NilTag, IIndexableParseElement
    {
        public Br(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "BR"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Br(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    public class BlockQuote : InlineTag, IIndexableParseElement
    {
        public BlockQuote(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "BLOCKQUOTE"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new BlockQuote(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
    /*
    samp
    var
    cite
    q
    code
    ins
    del
    dfn
    kbd
    acronym
    address
    bdo
    em
    abbr
    */

}

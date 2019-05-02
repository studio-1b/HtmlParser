using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    abstract public class NilTag : InlineTag
    {
        public NilTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        /// <summary>
        /// NilTag's default behavior is to assume they have no children, so they are never put into stack.
        /// 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="expected"></param>
        /// <param name="parentContext"></param>
        /// <param name="factory"></param>
        /// <returns></returns>
        override protected Token LikeIdentify(string value, string expected, ref Node parentContext, callerTagFactory factory)
        {
            TagType tagType = parseTagType(value);
            if (tagType != TagType.Not)
                try
                {
                    ReadonlyStringSegmentation parts = value.Segment(parseTag);
                    string name = parts[0];
                    if (name.ToUpper() != expected.ToUpper()) return null; //name has to match expected
                }
                catch (HtmlParseException ex) { return null; } //cursory passed, but unable to parse tag for name

            if ((tagType == TagType.Open) || (tagType == TagType.Nil))
            {
                Token element = factory(parentContext, value);
                parentContext.ChildElements.Add(element);
                return element;
            }
            else if (tagType == TagType.Close)
            {
                Tag element = new Closure(parentContext, value);
                parentContext.ChildElements.Add(element);
                return element;
            }
            return null;
        }
    }

    public class Link : NilTag, IIndexableParseElement
    {
        public Link(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "LINK"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Link(parentContext, value) { IsNil = true }; }

        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
    }

    public class Meta : NilTag, IIndexableParseElement
    {
        public Meta(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "META"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Meta(parentContext, value) { IsNil = true }; }

        public List<string> Traversal { get { return new List<string>() { "<"+EXPECTED_TAG_NAME, "</"+EXPECTED_TAG_NAME }; } }
    }
    
}

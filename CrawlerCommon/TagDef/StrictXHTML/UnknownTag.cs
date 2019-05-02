using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using CrawlerCommon;

    public class UnknownTag : Tag
    {
        public UnknownTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        /// <summary>
        /// If value is tag formatted, add Tag instance to parentContext, assign as new parent, return new instance, 
        /// ie. <abc>, </abc>, <!abc>, <abc >, <abc />, <abc a=2>  but not <, > or <>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            Node copy = parentContext;
            return this.LikeIdentify(value, null, ref parentContext, delegate() { return new UnknownTag(copy, value); });
        }
        protected delegate Token callerTagFactory();
        virtual protected Token LikeIdentify(string value, string expected, ref Node parentContext, callerTagFactory factory)
        {
            TagType tagType = parseTagType(value);

            //no name validation, but still needs to be tag formatted

            if ((tagType == TagType.Open) || (tagType == TagType.Nil))
            {
                Token element = factory();
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
                //parseAttribute(element.Attrib, value);
                //this.parseTagName(value, true);
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
    }
}

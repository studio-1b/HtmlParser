using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    public class Declaration : InlineTag, IIndexableParseElement
    {
        public Declaration(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            if (isEnclosedByTag(value))
                if (isOpenTag(value, false))
                    if (value[1]=='!')
                    {
                        Token element = new Declaration(parentContext, value);
                        parentContext.ChildElements.Add(element);
                        return element;
                    }
            return null;
        }

        override protected void parseAttribute(AttributeCollection list, string value)
        {
            return; //no attributes supported yet

            ReadonlyStringSegmentation segments = new ReadonlyStringSegmentation(value, Tag.parseTag); //(value, " ", "/>",">"); //value.Split(' ');
            for (int i = 1; i < segments.Count; i+=2)
                if (segments.GetLazyLoadSegment(i).Length > 0)
                {
                    list.Add(new TagAttribute(segments.GetLazyLoadSegment(i), segments.GetLazyLoadSegment(i+1)));
                }
        }


        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<!DOC" }; } }
        #endregion IIndexableParseElement
    }
}

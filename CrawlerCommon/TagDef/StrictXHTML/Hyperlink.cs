using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    public class Hyperlink : InlineTag, IIndexableParseElement
    {
        public Hyperlink(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "A"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Hyperlink(parentContext, value); }


        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement

        public string Href {
            get {
                var value = this.Attrib.SingleOrDefault<TagAttribute>(s=>s.Name.ToLower()=="href");
                return value==null ? null : value.Value;
            }
        }
    }

}

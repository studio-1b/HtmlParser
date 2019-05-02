using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon.HtmlPathTest
{
    public class AttribToken : Token
    {
        static public void ExpandAttribChildren(Tag root)
        {
            var runbefore = root.ChildElements.Exists(s => s.GetType() == typeof(AttribToken));
            if (runbefore)
                return;

            foreach (Tag item in root.ChildElements)
                ExpandAttribChildren(item);

            var list = root.Attrib;
            int len = list.Count;
            for (int i = 0; i < len; i++)
            {
                var attribnode = new AttribToken(root, i, list[i].Value);
                root.ChildElements.Add(attribnode);
            }
        }


        public AttribToken(Tag parent, int rank, string symbolString)
            : base(parent, symbolString)
        {
            var pair = parent.Attrib[rank];
            this.Name = pair.Name;
        }

        public string Name { get; private set; }
        

        /// <summary>
        /// If value is tag formatted, add Tag instance to parentContext, assign as new parent, return new instance, 
        /// ie. <abc>, </abc>, <!abc>, <abc >, <abc />, <abc a=2>  but not <, > or <>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            return null; //Not supported.  it is not separate token.  It is part of another
        }
    }
}

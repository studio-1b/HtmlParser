using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    public class TagAttribute
    {
        public TagAttribute(string name, string value)
        {
            this._name = name;
            this._value = value;
            this._unparsed = string.Empty;
        }
        public TagAttribute(ReadonlyStringSegmentation.StringSegment nameFactory, ReadonlyStringSegmentation.StringSegment valueFactory)
        {
            this._nameSegment = nameFactory;
            this._valueSegment = valueFactory;
        }

        string _name;
        string _value;
        string _unparsed;
        ReadonlyStringSegmentation.StringSegment _nameSegment;
        ReadonlyStringSegmentation.StringSegment _valueSegment;

        public string Name { get { if (_unparsed == null) parseAttribute(); return this._name; } }
        public string Value { get { if (_unparsed == null) parseAttribute(); return this._value; } }

        void parseAttribute()
        {
            if (this._unparsed == null)
            {
                this._name = this._nameSegment.SegmentValue;
                this._value = this._valueSegment.SegmentValue;
                this._unparsed = string.Empty;
            }
            /*
            int index=this._unparsed.IndexOf('=');
            if (index < 0)
            { this._name = this._unparsed; }
            else
            {
                this._name = this._unparsed.Substring(0,index-1);
                string value = this._unparsed.Substring(index + 1).Trim();
                //if quotes, then remove
                if (value[0] == '\"' && value[value.Length - 1] == '\"')
                    value = value.Substring(1, value.Length - 1);
                else if (value[0] == '\'' && value[value.Length - 1] == '\'')
                    value = value.Substring(1, value.Length - 1);
                this._value = value;
            }*/
        }
    }
}

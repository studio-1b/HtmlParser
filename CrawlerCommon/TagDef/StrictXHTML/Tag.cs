using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CrawlerCommon;

namespace CrawlerCommon.TagDef.StrictXHTML
{
    abstract public class Tag : Node
    {
        public Tag(Token parent, string symbolString)
            : base(parent, symbolString)
        {
            TagType tagType = parseTagType(symbolString);
            parseAttribute(this.Attrib, this.ActualSymbol);
            this.parseTagName(this.ActualSymbol, tagType==TagType.Close);
        }


        public bool IsNil;
        public int CloseIndex;
        public readonly AttributeCollection Attrib = new AttributeCollection();
        public string TagName { get { return deferredTagName.SegmentValue; } }
        protected ReadonlyStringSegmentation.StringSegment deferredTagName;

        /// <summary>
        /// If value is tag formatted, add Tag instance to parentContext, assign as new parent, return new instance, 
        /// ie. <abc>, </abc>, <!abc>, <abc >, <abc />, <abc a=2>  but not <, > or <>
        /// </summary>
        /// <param name="value"></param>
        /// <param name="parentContext"></param>
        /// <returns></returns>
        override public Token LikeIdentify(string value, ref Node parentContext)
        {
            if (this.EXPECTED_TAG_NAME == null)
                throw new InvalidProgramException("Faulty code detected.  " + this.GetType().FullName + " needs to override EXPECTED_TAG_NAME, OR-- not use and override LikeIdentify(string, ref Node)");

            Node copy = parentContext;
            return this.LikeIdentify(value, this.EXPECTED_TAG_NAME, ref parentContext, this.InstanceFactory );
        }
        protected virtual string EXPECTED_TAG_NAME { get { return null; } }
        protected virtual Token InstanceFactory(Node parentContext, string value) { return null; }

        protected delegate Token callerTagFactory(Node parentContext, string value);
        virtual protected Token LikeIdentify(string value, string expected, ref Node parentContext, callerTagFactory factory)
        {
            TagType tagType = parseTagType(value); //cursory look
            if (tagType != TagType.Not)
                try
                {
                    ReadonlyStringSegmentation parts = value.Segment(parseTag);
                    string name = parts[0];
                    if (name.ToUpper() != expected.ToUpper()) return null; //name has to match expected
                }
                catch(HtmlParseException ex) { return null; } //cursory passed, but unable to parse tag for name
            
            //return the correct tag type
            if ((tagType == TagType.Open) || (tagType == TagType.Nil))
            {
                Token element = factory(parentContext, value);
                if (element is Tag)
                    ((Tag)element).IsNil = (tagType == TagType.Nil);

                parentContext.ChildElements.Add(element);
                if (element is Node)
                    parentContext = (Node)element;

                System.Diagnostics.Debug.Assert(deferredTagName != null, "Problem getting tag name from "  + value);
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
                if (newParent!=null)
                    parentContext = newParent;

                if (parentContext == null)
                    System.Diagnostics.Debug.Assert(parentContext != null, "The stack should never be empty.  There should be at least the document");

                System.Diagnostics.Debug.Assert(deferredTagName != null, "Problem getting tag name from " + value);

                return element;
            }
            else return null; //Not a tag
        }

        protected enum TagType
        {
            Not, Open, Close, Nil
        }

        virtual protected TagType parseTagType(string value)
        {
            if (isEnclosedByTag(value))
            {

                if (isOpenTag(value, false))
                {
                    if (isNilTag(value, false))
                        return TagType.Open;
                    else
                        return TagType.Nil;
                }
                else if (isCloseTag(value, false))
                    return TagType.Close;

                return TagType.Not; //it look like a tag, but isn't
            }
            else
                return TagType.Not;
        }

        /// <summary>
        /// formatted <_...> with no space as 1st char in enclosed brackets
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        virtual protected bool isEnclosedByTag(string value)
        {
            int lastIndex = value.Length - 1;
            if (lastIndex > 1 && value[0] == '<' && value[lastIndex] == '>' && value[1] != ' ')
                return true;
            else
                return false;
        }

        /// <summary>
        /// <...> or <!...> (with no </...> allowed) 
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        virtual public bool isOpenTag(string value)
        {
            return isOpenTag(value, true);
        }
        virtual protected bool isOpenTag(string value, bool validate)
        {
            if (validate)
                if (!isEnclosedByTag(value))
                    return false;

            if (value[1] == '/')
                return false;

            if (value[1] == '!')
                return hasTagName(value, 2);
            else
                return hasTagName(value, 1);
        }

        /// <summary>
        /// checks that 1st character of tagname is alpha
        /// </summary>
        /// <param name="value"></param>
        /// <param name="start"></param>
        /// <returns></returns>
        virtual protected bool hasTagName(string value, int start)
        {
            if ("ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz".Contains(value[start]))
                return true;
            else
                return false;
        }
        /// <summary>
        /// </...> only
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        virtual public bool isCloseTag(string value)
        {
            return isCloseTag(value, true);
        }
        virtual protected bool isCloseTag(string value, bool validate)
        {
            if (validate)
                if (isOpenTag(value))
                    return false;

            return hasTagName(value, 2);
        }

        /// <summary>
        /// <.../> only
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool isNilTag(string value)
        {
            return isNilTag(value, true);
        }
        virtual protected bool isNilTag(string value, bool validate)
        {
            if (validate)
                if (!isOpenTag(value, validate))
                    return false;

            int lastIndex = value.Length - 1;
            if (value[lastIndex - 1] == '/')
                return true;
            else
                return true;
        }

        virtual protected void parseTagName(string value, bool closeTag)
        {
            ReadonlyStringSegmentation segments = value.Segment(parseTag);  //new ReadonlyStringSegmentation(value, "<", " ", "/", ">");
            // [#0]<[#1]_[#2...]/[#n-1]>[#n]
            // [#0]<[#1]/[#2...]_[#n-1]>[#n]
            /*
            if(closeTag)
                this.deferredTagName = segments.GetLazyLoadSegment(2);
            else
                this.deferredTagName = segments.GetLazyLoadSegment(1);
             */
            this.deferredTagName = segments.GetLazyLoadSegment(0);
        }

        /// <summary>
        /// <tag attrib1 attrib2=value attrib3="value", attrib4='value'>
        /// <tag attrib1 attrib2=value attrib3="value", attrib4='value'/>
        /// </tag attrib1 attrib2=value attrib3="value", attrib4='value'>
        /// all produces: attrib1=value, attrib2=value, attrib3=value
        /// </summary>
        /// <param name="list"></param>
        /// <param name="value"></param>
        virtual protected void parseAttribute(AttributeCollection list, string value)
        {
            ReadonlyStringSegmentation segments = value.Segment(parseTag); //new ReadonlyStringSegmentation(value, Tag.parseTag); //(value, " ", "/>",">"); //value.Split(' ');
            for (int i = 1; i < segments.Count; i += 2)
                if (segments.GetLazyLoadSegment(i).Length > 0)
                {
                    list.Add(new TagAttribute(segments.GetLazyLoadSegment(i), segments.GetLazyLoadSegment(i + 1)));
                }
        }


        enum tagParseMode
        {
            Initial,
            OpenFound,
            CloseIndFound,
            NameMode,
            WhiteAfterName,
            AttribMode,
            WhiteAfterAttrib,
            EqualsFound,
            WhiteAfterEquals,
            ValueFound,
            SingleQuoteFound,
            DoubleQuoteFound,
            SingleQuoteClose,
            DoubleQuoteClose,
            NilFound,
            CloseFound
        }
        protected static string NAME_CHAR_LIST=    "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz";
        protected static string ATTRIB_CHAR_LIST = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz1234567890";
        protected static IEnumerable<int> parseTag(IEnumerable<char> charStream)
        {
            bool closeindfound=false;
            int counter = 0;
            int length = 0;
            int start = 0;
            tagParseMode mode = tagParseMode.Initial;
            tagParseMode prevmode = tagParseMode.Initial;
            foreach (var ch in charStream)
            {
                counter++;
                //once name is defined, tag can end at any time, except when in quotes
                if (mode >= tagParseMode.NameMode && mode != tagParseMode.SingleQuoteFound && mode != tagParseMode.DoubleQuoteFound) 
                {
                    if (ch == '/')
                    {
                        if (closeindfound)
                            throw new HtmlParseException("bad tag" + charStream);
                        mode = tagParseMode.NilFound;
                    }
                    else if (ch == '>')
                    {
                        mode = tagParseMode.CloseFound;
                    }
                    if ((mode == tagParseMode.NilFound || mode == tagParseMode.CloseFound)) 
                    {
                        if (length > 0)
                        {
                            yield return length;
                            length = 0;
                            
                            if (prevmode == tagParseMode.AttribMode
                                || prevmode == tagParseMode.WhiteAfterAttrib
                                || prevmode == tagParseMode.EqualsFound
                                || prevmode == tagParseMode.WhiteAfterEquals)
                                yield return 0; //prev modes occur after attribute key encountered, but tag ended before value.  insert 0 to create pair.
                        }
                        
                        if (prevmode == tagParseMode.WhiteAfterAttrib
                            || prevmode == tagParseMode.EqualsFound
                            || prevmode == tagParseMode.WhiteAfterEquals)
                            yield return 0; //prev modes occur after attribute key encountered, but tag ended before value.  insert 0 to create pair.

                        continue;
                    }
                }
                
                switch (mode)
                {
                    case tagParseMode.Initial:
                        if (ch == '<') mode = tagParseMode.OpenFound;
                        yield return -1;
                        break;
                    case tagParseMode.OpenFound:
                        if (ch == '/' || ch == '!')
                        {
                            if (ch == '/') closeindfound = true;
                            mode = tagParseMode.CloseIndFound;
                            yield return -1;
                        }
                        else if (NAME_CHAR_LIST.Contains(ch)) { mode = tagParseMode.NameMode; length++; }
                        else throw new HtmlParseException("bad tag " + charStream);
                        break;
                    case tagParseMode.CloseIndFound:
                        if (NAME_CHAR_LIST.Contains(ch)) { mode = tagParseMode.NameMode; length++; }
                        else throw new HtmlParseException("bad tag" + charStream);
                        break;
                    case tagParseMode.NameMode:
                        if (ch == ' ')
                        {
                            mode = tagParseMode.WhiteAfterName;
                            if (length > 0) yield return length;
                            length = 0;
                            yield return -1;
                        }
                        else length++;
                        break;
                    case tagParseMode.WhiteAfterName:
                        if (ch == ' ') yield return -1;
                        else
                        {
                            mode = tagParseMode.AttribMode;
                            length++;
                        }
                        break;
                    case tagParseMode.AttribMode:
                        if (ch == ' ') 
                        {
                            mode = tagParseMode.WhiteAfterAttrib;
                            if (length > 0) yield return length;
                            length = 0;
                            yield return -1;
                        }
                        else if (ch == '=')
                        {
                            mode = tagParseMode.EqualsFound;
                            if (length > 0) yield return length;
                            length = 0;
                            yield return -1;
                        }
                        else length++; 
                        break;
                    case tagParseMode.WhiteAfterAttrib:
                        if (ch == ' ') yield return -1;
                        else if (ch == '=')
                        {
                            mode = tagParseMode.EqualsFound;
                            if(length>0) yield return length;
                            length = 0;
                            yield return -1;
                        }
                        else {
                            yield return 0; //attrib should be in pairs, always
                            mode = tagParseMode.AttribMode;
                            length++;
                        };
                        break;
                    case tagParseMode.EqualsFound:
                        if (ch == ' ') { mode = tagParseMode.WhiteAfterEquals; yield return -1; }
                        else if (ch == '\'') { mode = tagParseMode.SingleQuoteFound; yield return -1; }
                        else if (ch == '"') { mode = tagParseMode.DoubleQuoteFound; yield return -1; }
                        else 
                        {
                            mode = tagParseMode.ValueFound;
                            length++;
                        }
                        break;
                    case tagParseMode.WhiteAfterEquals:
                        if (ch == ' ') yield return -1;
                        else if (ch == '\'') { mode = tagParseMode.SingleQuoteFound; yield return -1; }
                        else if (ch == '"') { mode = tagParseMode.DoubleQuoteFound; yield return -1; }
                        else
                        {
                            mode = tagParseMode.ValueFound;
                            length++;
                        }
                        break;
                    case tagParseMode.ValueFound:
                        if (ch == ' ')
                        {
                            mode = tagParseMode.WhiteAfterName;
                            if (length > 0) yield return length;
                            length = 0;
                            yield return -1;
                        }
                        else length++;
                        break;
                    case tagParseMode.SingleQuoteFound:
                        if (ch == '\'')
                        {
                            mode = tagParseMode.SingleQuoteClose;
                            yield return length;
                            length = 0;
                            yield return -1;
                        }
                        else length++;
                        break;
                    case tagParseMode.DoubleQuoteFound:
                        if (ch == '"')
                        {
                            mode = tagParseMode.DoubleQuoteClose;
                            yield return length;
                            length = 0;
                            yield return -1;
                        }
                        else length++;
                        break;
                    case tagParseMode.SingleQuoteClose:
                    case tagParseMode.DoubleQuoteClose:
                        if (ch == ' ')
                        {
                            mode = tagParseMode.WhiteAfterName;
                            yield return -1;
                        }
                        else if (ch != '\t' && ch != '/' && ch != '=' && ch != '"' && ch != '\'') // *tab, line feed, form feed, space, *solidus, greater than sign, *quotation mark, *apostrophe and *equals sign 
                        { //ESPN's site has some odd HTML.  I thought code would fall back and call this an unknown tag, but it didn't.  It might be easier just handling this odd case(see end, there is no space between close quote and next attribute) than figuring out how to get parser to skip to the close tag.  http://games.espn.go.com/ffl/leaguerosters?leagueId=51111 "<a onclick=\"\"href=\"/ffl/leagueoffice?leagueId=51111&seasonId=2015\">"
                            //should be same as case [tagParseMode.WhiteAfterName], since this case has no space between the attribute name/value pairs
                            //...still dont understand why LikeIdentify didnt just call it a UnknownTag (meaning it didnt parse out the internals).  The earlier tokenizer obviously was able to identify this as a token.
                            //skipping [tagParseMode.WhiteAfterName] state, straight into [tagParseMode.AttribMode] state
                            //http://stackoverflow.com/questions/925994/what-characters-are-allowed-in-an-html-attribute-name
                            mode = tagParseMode.AttribMode;
                            length++;
                        }
                        else throw new HtmlParseException("bad tag" + charStream);
                        break;
                    case tagParseMode.NilFound:
                        if (ch == '>')
                        {
                            mode = tagParseMode.CloseFound;
                            yield return -1;
                        }
                        else throw new HtmlParseException("bad tag" + charStream);
                        break;
                    case tagParseMode.CloseFound:
                        break;
                }
                prevmode = mode;
            }
        }

        protected static Tag findMatchingOpen(Closure close, Tag stack)
        {
            //Tag output = null;
            while (stack.TagName != close.TagName)
                if (stack.ParentNode is Tag)
                    stack = (Tag)stack.ParentNode;
                else
                    return null;

            if (stack == null) return null;

            if (stack.TagName.ToUpper() == close.TagName.ToUpper())
                if (stack.ParentNode is Tag)
                    stack = (Tag)stack.ParentNode;
                //else
                //    stack = stack.ParentNode;  Need to fix this

            return stack;
        }

        public string GetExpectedTagString()
        {
            return this.EXPECTED_TAG_NAME;
        }
    }

    public class HtmlParseException : Exception
    {
        public HtmlParseException() : base() { }
        public HtmlParseException(string message) : base(message) { }
    }
}

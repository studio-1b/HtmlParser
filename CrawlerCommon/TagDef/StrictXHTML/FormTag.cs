using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.TagDef.StrictXHTML
{

    public class FormTag : BlockTag, IIndexableParseElement
    {
        public FormTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "FORM"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new FormTag(parentContext, value); }

        public readonly InputCollection Input = new InputCollection();

        public Page Submit()
        {
            throw new NotImplementedException();
        }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }



    public class InputCollection : Dictionary<string, Input>
    { }

    public class Input : NilTag, IIndexableParseElement
    {
        public Input(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "INPUT"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Input(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class TextArea : BlockTag, IIndexableParseElement
    {
        public TextArea(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "TEXTAREA"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new TextArea(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class SelectTag : BlockTag, IIndexableParseElement
    {
        public SelectTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "SELECT"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new SelectTag(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }


    public class OptionTag : BlockTag, IIndexableParseElement
    {
        public OptionTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "OPTION"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new OptionTag(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }


    public class OptGroup : BlockTag, IIndexableParseElement
    {
        public OptGroup(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "OPTGROUP"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new OptGroup(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class ButtonTag : BlockTag, IIndexableParseElement
    {
        public ButtonTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "BUTTON"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new ButtonTag(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class LabelTag : BlockTag, IIndexableParseElement
    {
        public LabelTag(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "LABEL"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new LabelTag(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Fieldset : BlockTag, IIndexableParseElement
    {
        public Fieldset(Token parent, string symbolString)
            : base(parent, symbolString)
        { }


        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "FIELDSET"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Fieldset(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }

    public class Legend : BlockTag, IIndexableParseElement
    {
        public Legend(Token parent, string symbolString)
            : base(parent, symbolString)
        { }

        public string Name;
        public string Value;

        //LikeIdentify(string, ref Node) from ancestor, uses values below, from this object
        override protected string EXPECTED_TAG_NAME { get { return "LEGEND"; } }
        override protected Token InstanceFactory(Node parentContext, string value) { return new Legend(parentContext, value); }

        #region IIndexableParseElement
        public List<string> Traversal { get { return new List<string>() { "<" + EXPECTED_TAG_NAME, "</" + EXPECTED_TAG_NAME }; } }
        #endregion IIndexableParseElement
    }
}

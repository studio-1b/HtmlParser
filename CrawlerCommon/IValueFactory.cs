using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon
{
    public interface IValueFactory
    {
        string Representative { get; set; }
        Type GetOutputType();
    }
    public interface IValueFactory<T> : IValueFactory
    {
        T Output { get; }
    }

    public class QuotedText : IValueFactory
    {
        public string Representative { get; set; }
        public Type GetOutputType()
        {
            return typeof(string);
        }
    }
}

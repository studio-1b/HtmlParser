using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.HtmlPathTest
{
    public interface IConverterNeeded<T>
    {
        Converter<string, T> convert {get;set;}

    }

    public static class IConverter
    {
        static public string PassThru(string dummy)
        {
            return dummy;
        }
    }
}

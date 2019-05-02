using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon.HtmlPathTest
{
    public interface IExprHandler
    {
        /// <summary>
        /// Parses the string for the arguments for the instance
        /// </summary>
        /// <param name="expr"></param>
        void ParseExpr(string expr);
        
        /// <summary>
        /// Parses the string for the arguments for the instance
        /// </summary>
        /// <param name="expr"></param>
        /// <param name="startIndex"></param>
        /// <param name="length"></param>
        void ParseExpr(string expr, int startIndex, int length, string operatorHint);
    }
}

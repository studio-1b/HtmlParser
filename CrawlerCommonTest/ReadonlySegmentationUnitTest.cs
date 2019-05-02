using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.Diagnostics;
using CrawlerCommon;

namespace CrawlerCommonTest
{
    [TestClass]
    public class ReadonlySegmentationUnitTest
    {
        [TestMethod]
        public void EquivalantToStringSplitTest()
        {
            StringBuilder output = new StringBuilder();

            for (int i = 0; i < 100; i++)
            {
                string sample = Guid.NewGuid().ToString();
                string[] expected = sample.Split(new string[] { "-", "a", "b" }, StringSplitOptions.None);
                ReadonlyStringSegmentation actual = new ReadonlyStringSegmentation(sample, "-", "a", "b");

                if (actual.Count == expected.Length) 
                    output.Append("[OK count]"); 
                else 
                    output.Append("[ X count]");
                
                for (int j = 0; j < actual.Count; j++)
                    if (actual[j] == expected[j]) 
                        output.Append( "[OK " + expected[j] + "]"); 
                    else 
                        output.Append( "[ X " + actual[j] + " != " + expected[j] + "]");

                output.AppendLine(sample); ;
                //listBox1.Items.Add(output);
            }

            string result = output.ToString();
            int num = result.Split('\n').Length;
            int error = result.Split(new string[] {"[ X "},StringSplitOptions.None).Length - 1;
            string msg = num + " guids tested " + error + " errors found, cases= " + result;
            Debug.WriteLine(msg);
        }
    }
}

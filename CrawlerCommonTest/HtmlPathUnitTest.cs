using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;


using CrawlerCommon;
using CrawlerCommon.TagDef.StrictXHTML;
using CrawlerCommon.HtmlPathTest;
using System.Diagnostics;


namespace CrawlerCommonTest
{
    [TestClass]
    public class HtmlPathUnitTest
    {

        [TestMethod]
        public void RandomElementExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);

            // Negative testing
            var tag2 = getRandomTag();
            while (tag2.GetExpectedTagString() == tag.GetExpectedTagString())
                tag2 = getRandomTag();

            var different = tag2.GetExpectedTagString();
            string wrong = string.Format("{0}", different);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "Different tag test shouldve failed");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }

        [TestMethod]
        public void RandomParentExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);

            var tag2 = getRandomTag();
            var randomTestType2 = typeof(ElementTest<>).MakeGenericType(tag2.GetType());
            BaseTestLinkage parent = (BaseTestLinkage)Activator.CreateInstance(randomTestType2);

            expected.Add(new ParentTest() {
                parent
            });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}/{1}", tag2.GetExpectedTagString(), tag.GetExpectedTagString());

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);


            // Negative testing
            var tag3 = getRandomTag();
            while (tag3.GetExpectedTagString() == tag2.GetExpectedTagString())
                tag3 = getRandomTag();

            var different = tag3.GetExpectedTagString();
            string wrong = string.Format("{0}/{1}", tag3.GetExpectedTagString(), tag.GetExpectedTagString());

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "Wrong parent tag shouldve failed");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }

        [TestMethod]
        public void SimplePositionExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new ElementPositionTest() { CompareValue = 0 });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[0]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);


            // Negative testing
            string wrong = string.Format("{0}[12]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "12 position shouldve failed.");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }

        [TestMethod]
        public void PositionGreaterThanExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new ElementPositionComparisonTest() { Operator = ElementPositionComparisonTest.GT, CompareValue = 0 });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[>0]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);


            // Negative testing
            string wrong = string.Format("{0}[<0]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "Less than, should have failed");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }

        [TestMethod]
        public void PositionNotEqualThanExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new ElementPositionComparisonTest() { Operator = ElementPositionComparisonTest.NE, CompareValue = 0 });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[!=0]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);


            // Negative testing
            string wrong = string.Format("{0}[!=1]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "Position of one, should have failed.");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }


        [TestMethod]
        public void PositionLessThanExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new ElementPositionComparisonTest() { Operator = ElementPositionComparisonTest.LE, CompareValue = 100 });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[<=100]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);

            // Negative testing
            string wrong = string.Format("{0}[>=100]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "Greater than and equal, should have failed.");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }


        [TestMethod]
        public void SimpleAttribExistExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new AttribTest() { AttributeName = "id" });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[@id]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);

            // Negative testing
            string wrong = string.Format("{0}[@name]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "attribute name, should have failed.");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }

        [TestMethod]
        public void AttribCompareIntExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new AttribComparisonTest<int>() { AttributeName = "rank", CompareValue = 6 });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[@rank=6]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);


            // Negative testing
            string wrong = string.Format("{0}[@rank=2]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "attribute value=2, should have failed.");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }


        [TestMethod]
        public void AttribCompareStringExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new AttribComparisonTest<string>() { AttributeName = "hello", CompareValue = "world" });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[@hello=\"world\"]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);


            // Negative testing
            string wrong = string.Format("{0}[@rank!=\"world\"]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "operator !=, should have failed.");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }

        [TestMethod]
        public void AttribContainsStringExprTest()
        {
            var tag = getRandomTag();
            var randomTestType = typeof(ElementTest<>).MakeGenericType(tag.GetType());
            BaseTestLinkage expected = (BaseTestLinkage)Activator.CreateInstance(randomTestType);
            expected.Add(new AttribContainsTest() { AttributeName = "message", CompareValue = "world" });

            var str = tag.GetExpectedTagString();
            string expr = string.Format("{0}[@message CONTAINS \"world\"]", str);

            standardTest(expected, expr);
            Debug.WriteLine("OK! {0}", expr, 0);


            // Negative testing
            string wrong = string.Format("{0}[@message CONTAINS \"earth\"]", str);

            var negative = negativeTest(expected, wrong);
            Assert.IsFalse(negative, "earth, should have failed.");
            Debug.WriteLine("Fail expected! {0}", expr, 0);
        }

        //------------------------------------
        static HtmlPath hpathparser = new HtmlPath();
        static bool standardTest(ITokenTest expected, string expr)
        {
            hpathparser.Parse(expr);
            var actual = hpathparser.Copy;
            Assert.IsNotNull(actual, "Expression did not result in complete parsing");

            var compare = fullTraversalCompare(expected, actual);
            Assert.IsTrue(compare, "Not the same");

            return compare;
        }
        static bool negativeTest(ITokenTest expected, string expr)
        {
            hpathparser.Parse(expr);
            var actual = hpathparser.Copy;
            //var actual = Html2Tsv.Program.ParseXPath(expr);
            Assert.IsNotNull(actual, "Expression did not result in complete parsing");

            var compare = fullTraversalEquals(expected, actual);
            Assert.IsFalse(compare, "Not the same");

            return compare;
        }

        static Random rnd = new Random();
        static List<Tag> tags = (new DefaultBuilder()).SupportedTags();

        static Tag getRandomTag()
        {
            var len = tags.Count;
            int next = rnd.Next(len);
            while (tags[next].GetExpectedTagString() == null)
                next = rnd.Next(len);
            return tags[next];
        }


        static bool fullTraversalCompare(ITokenTest a, ITokenTest b)
        {
            if (a.GetType() != b.GetType())
                return false;

            var expected = a;
            var actual = b;
            if (expected is AttribTest)
                Assert.AreEqual(((AttribTest)expected).AttributeName, ((AttribTest)actual).AttributeName);

            if (expected is AttribComparisonTest<int>)
            {
                Assert.AreEqual(((AttribComparisonTest<int>)expected).Operator, (((AttribComparisonTest<int>)actual).Operator));
                Assert.AreEqual(((AttribComparisonTest<int>)expected).CompareValue, (((AttribComparisonTest<int>)actual).CompareValue));
            }
            else if (expected is AttribComparisonTest<string>)
            {
                Assert.AreEqual(((AttribComparisonTest<string>)expected).Operator, (((AttribComparisonTest<string>)actual).Operator));
                Assert.AreEqual(((AttribComparisonTest<string>)expected).CompareValue, (((AttribComparisonTest<string>)actual).CompareValue));
            }
            if (expected is AttribContainsTest)
            {
                Assert.AreEqual(((AttribContainsTest)expected).CompareValue, (((AttribContainsTest)actual).CompareValue));
            }

            if (expected is ElementPositionTest)
            {
                Assert.AreEqual(((ElementPositionTest)expected).CompareValue, (((ElementPositionTest)actual).CompareValue));
            }
            if (expected is ElementPositionComparisonTest)
            {
                Assert.AreEqual(((ElementPositionComparisonTest)expected).Operator, (((ElementPositionComparisonTest)actual).Operator));
                Assert.AreEqual(((ElementPositionComparisonTest)expected).CompareValue, (((ElementPositionComparisonTest)actual).CompareValue));
            }


            if (a is BaseTestLinkage) // then b is BaseLinkaageType bc of above if
            {
                var casteda = ((BaseTestLinkage)a).ToArray<ITokenTest>();
                var castedb = ((BaseTestLinkage)b).ToArray<ITokenTest>();
                if (casteda.Length != castedb.Length)
                    return false;
                int len = casteda.Length;
                for (int i = 0; i < len; i++)
                {
                    if (!fullTraversalCompare(casteda[i], castedb[i]))
                        return false;
                }
            }

            return true;
        }

        static bool fullTraversalEquals(ITokenTest a, ITokenTest b)
        {
            if (a.GetType() != b.GetType())
                return false;

            var expected = a;
            var actual = b;
            if (expected is AttribTest)
                if (((AttribTest)expected).AttributeName != ((AttribTest)actual).AttributeName)
                    return false;

            if (expected is AttribComparisonTest<int>)
            {
                if (((AttribComparisonTest<int>)expected).Operator != (((AttribComparisonTest<int>)actual).Operator))
                    return false;
                if (((AttribComparisonTest<int>)expected).CompareValue != (((AttribComparisonTest<int>)actual).CompareValue))
                    return false;
            }
            else if (expected is AttribComparisonTest<string>)
            {
                if (((AttribComparisonTest<string>)expected).Operator != (((AttribComparisonTest<string>)actual).Operator))
                    return false;
                if (((AttribComparisonTest<string>)expected).CompareValue != (((AttribComparisonTest<string>)actual).CompareValue))
                    return false;
            }
            if (expected is AttribContainsTest)
            {
                if (((AttribContainsTest)expected).CompareValue != (((AttribContainsTest)actual).CompareValue))
                    return false;
            }

            if (expected is ElementPositionTest)
            {
                if (((ElementPositionTest)expected).CompareValue != (((ElementPositionTest)actual).CompareValue))
                    return false;
            }
            if (expected is ElementPositionComparisonTest)
            {
                if (((ElementPositionComparisonTest)expected).Operator != (((ElementPositionComparisonTest)actual).Operator))
                    return false;
                if (((ElementPositionComparisonTest)expected).CompareValue != (((ElementPositionComparisonTest)actual).CompareValue))
                    return false;
            }


            if (a is BaseTestLinkage) // then b is BaseLinkaageType bc of above if
            {
                var casteda = ((BaseTestLinkage)a).ToArray<ITokenTest>();
                var castedb = ((BaseTestLinkage)b).ToArray<ITokenTest>();
                if (casteda.Length != castedb.Length)
                    return false;
                int len = casteda.Length;
                for (int i = 0; i < len; i++)
                {
                    if (!fullTraversalEquals(casteda[i], castedb[i]))
                        return false;
                }
            }

            return true;
        }

    }
}

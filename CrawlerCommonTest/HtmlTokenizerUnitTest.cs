using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using CrawlerCommon;

namespace CrawlerCommonTest
{
    [TestClass]
    public class HtmlTokenizerUnitTest
    {
        List<string> badhtmlurl = new List<string>() { };

        [TestMethod]
        public void BadHtmlLoadTest()
        {
            foreach(var url in badhtmlurl)
            {
                var esc = Path.GetInvalidFileNameChars();
                string filename = url;
                foreach (var bad in esc)
                    filename = filename.Replace(bad, '_');
                filename += ".html";

                HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = false, ShowWorkMode = false };

                Stream html = null;
                if (File.Exists(filename))
                    html = new MemoryStream(File.ReadAllBytes(filename));
                else
                    html = getRemoteHtml(url, filename);

                var decoded = (new StreamReader(html)).ReadToEnd().Replace("\r", " ").Replace("\n", " ");
                html.Position = 0;
                tokenizer.Load(html);


                var actual = tokenizer.Tokens;
                // should be No errors
                Assert.AreNotEqual(0, actual.Count, "Expecting valid html to be returned from remote source");

                //making sure each token returned in list, exists in original stream
                //doesnt ensure vice versa, or everything in original stream is in token list
                //  hoping another test checks that enough.
                //  just checking that html from internet can be successfully parsed
                int location = 0;
                foreach (var item in actual)
                {
                    int next = decoded.IndexOf(item, location);
                    Assert.IsTrue(next > 0, "Cannot find token [" + item + "]");
                    location = next + item.Length;
                }

                html.Dispose();
            }
        }

        [TestMethod]
        public void RealWorldHtmlLoadTest()
        {
            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = false, ShowWorkMode = false };
            Stream html = getRandomHtml();
            
            
            var decoded = (new StreamReader(html)).ReadToEnd().Replace("\r"," ").Replace("\n"," ");
            html.Position = 0;
            tokenizer.Load(html);
            

            var actual = tokenizer.Tokens;
            // should be No errors
            Assert.AreNotEqual(0, actual.Count, "Expecting valid html to be returned from remote source");

            //making sure each token returned in list, exists in original stream
            //doesnt ensure vice versa, or everything in original stream is in token list
            //  hoping another test checks that enough.
            //  just checking that html from internet can be successfully parsed
            int location = 0;
            foreach (var item in actual)
            {
                int next = decoded.IndexOf(item, location);
                Assert.IsTrue(next > 0, "Cannot find token [" +  item + "]");
                location = next+item.Length;
            }

            html.Dispose();
        }

        [TestMethod]
        public void LoadUrlTest()
        {
            bool abort = false;
            bool ready = false;
            ThreadPool.QueueUserWorkItem(delegate(object state)
            {
                // Create an instance of the TcpListener class.
                TcpListener tcpListener = null;
                IPAddress ipAddress = Dns.GetHostEntry("localhost").AddressList[0];
                try
                {
                    // Set the listener on the local IP address 
                    // and specify the port.
                    tcpListener = new TcpListener(ipAddress, 8081);
                    tcpListener.Start();
                    ready = true;
                }
                catch
                {
                    abort = true;
                }
                

                // Create a TCP socket. 
                // If you ran this server on the desktop, you could use 
                // Socket socket = tcpListener.AcceptSocket() 
                // for greater flexibility.
                TcpClient tcpClient = tcpListener.AcceptTcpClient();
                // Read the data stream from the client. 
                byte[] bytes = new byte[256];
                using (NetworkStream stream = tcpClient.GetStream())
                {
                    stream.Read(bytes, 0, bytes.Length);
                    var status = string.Format("HTTP/1.0 200 OK{0}Date: Thu, 08 Sep 2016 19:51:58 GMT{0}{0}","\r\n");
                    var response = ASCIIEncoding.ASCII.GetBytes(status);
                    stream.Write(response, 0, response.Length);
                    using( Stream html = getHardcodedHtml())
                        html.CopyTo(stream);
                    stream.WriteByte(10);
                }
                Thread.Sleep(100);
                abort = true;
            }, null);

            while (!ready && !abort)
                Thread.Sleep(10);

            //while (1 == 1)
            //    Thread.Sleep(100);

            if (abort)
                throw new InvalidOperationException("server was aborted");

            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = false, ShowWorkMode = false };
            string url="http://localhost:8081/";
            tokenizer.Load(url);

            var actual = tokenizer.Tokens;
            var expected = getHardcodedToken();

            int lena = actual.Count;
            int lene = expected.Count;

            for (int i = 0; i < lena; i++)
                Assert.AreEqual(expected[i], actual[i], "Didnt get the same tokens as was put in");
        }

        [TestMethod]
        public void ValidHtmlLoadTest()
        {
            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = false, ShowWorkMode = false };
            Stream html = getHardcodedHtml();
            tokenizer.Load(html);
            var actual = tokenizer.Tokens;
            var expected = getHardcodedToken();

            int lena = actual.Count;
            int lene = expected.Count;

            for (int i = 0; i < lena; i++)
                Assert.AreEqual(expected[i], actual[i], "Didnt get the same tokens as was put in");

            html.Dispose();
        }

        [TestMethod]
        public void NullLoadTest()
        {
            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = false, ShowWorkMode = false };
            Stream html = null;
            try
            {
                tokenizer.Load(html);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex, "Exception expected");
            }

            var actual = tokenizer.Tokens;
            Assert.IsNotNull(actual, "There should be nothing parsed");
            Assert.AreEqual(0, actual.Count, "Empty expected");

        }

        [TestMethod]
        public void EmptyLoadTest()
        {
            HtmlTokenizer tokenizer = new HtmlTokenizer() { TrimWhitespace = true, DebugMode = false, ShowWorkMode = false };
            Stream html = new MemoryStream();
            try
            {
                tokenizer.Load(html);
            }
            catch (Exception ex)
            {
                Assert.IsNotNull(ex, "Exception expected");
            }
            var actual = tokenizer.Tokens;
            Assert.IsNotNull(actual, "There should be nothing parsed");
            Assert.AreEqual(0, actual.Count, "Empty expected");

            html.Dispose();
        }





        static Stream getRemoteHtml(string url, string filename)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            Stream result;

            // returned values are returned as a stream, then read into a string
            // String lsResponse = string.Empty;
            using (HttpWebResponse response = (HttpWebResponse)request.GetResponse())
            {
                if (response.StatusCode != HttpStatusCode.OK)
                    throw new Exception();

                result = new MemoryStream();
                response.GetResponseStream().CopyTo(result);
                result.Position = 0;

                using (FileStream fs = new FileStream(filename, FileMode.Create))
                    result.CopyTo(fs);
                result.Position = 0;

                return result;
            }


        }
        static Stream getRandomHtml()
        {
            string url="https://www.google.com/search?btnI=I'm+Feeling+Lucky&q=sample+html";
            string FILENAME="html.txt";
            try
            {
                Stream result = getRemoteHtml(url, FILENAME);

                return result;
            } catch {
                try
                {
                    var contents = File.ReadAllBytes(FILENAME);
                    Stream fromfile = new MemoryStream(contents);
                    return fromfile;
                }
                catch
                {
                    return getHardcodedHtml();
                }
            }
        }
        static Stream getHardcodedHtml()
        {
            var sample = string.Join(" ", getHardcodedToken());
            Stream hardcoded = new MemoryStream(ASCIIEncoding.ASCII.GetBytes(sample));
            hardcoded.Position = 0;
            return hardcoded;

        }
        static List<string> getHardcodedToken()
        {
            var sample = new List<string>() { "<html>","<head>","<title>","Test data","</title>","</head>","<body>","<p>","<h1>","Where the rubber meets the road","</h1>","<p>","<b>","Bolded here.","</b>","Just gibberish","</body>","</html>"};
            return sample;
        }
    }
}

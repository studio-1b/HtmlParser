using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.IO;


namespace CrawlerCommon
{
    public class HtmlTokenizer
    {
        public bool ShowWorkMode { get; set; }
        public bool DebugMode { get; set; }
        public bool TrimWhitespace { get; set; }

        List<string> _response = null;
        public Dictionary<string, string> Load(string url)
        {
            /*
            Dictionary<string, string> headers = new Dictionary<string, string>();

            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url);

            // Get the response.
            using (WebResponse response = request.GetResponse())
            {
                foreach (var item in response.Headers.AllKeys)
                    headers.Add(item, response.Headers[item]);

                // Get the stream containing content returned by the server.
                using (Stream dataStream = response.GetResponseStream())
                    this.Load(dataStream);
            }
            */
            var headers = this.Load(url, null);

            return headers;
        }

        public Dictionary<string, string> Load(string url, string filename)
        {
            Dictionary<string, string> headers = new Dictionary<string, string>();

            // Create a request using a URL that can receive a post. 
            WebRequest request = WebRequest.Create(url);

            // Get the response.
            using (WebResponse response = request.GetResponse())
            {
                foreach (var item in response.Headers.AllKeys)
                    headers.Add(item, response.Headers[item]);

                // Get the stream containing content returned by the server.
                if(filename==null)
                    using (Stream dataStream = response.GetResponseStream())
                        this.Load(dataStream);
                else
                    using (Stream dataStream = response.GetResponseStream())
                    {
                        using (FileStream fs = File.Create(filename))
                            dataStream.CopyTo(fs);
                        using (FileStream fs = File.OpenRead(filename))
                            this.Load(fs);
                    }
            }

            return headers;
        }

        public void Load(FileInfo file)
        {
            using (FileStream fs = File.OpenRead(file.FullName))
                this.Load(fs);
        }

        public void Load(Stream dataStream)
        {
            //List<char> all = new List<char>();
            string all = string.Empty;
            if (this.DebugMode)
            {
                all = (new StreamReader(dataStream)).ReadToEnd();
                dataStream = new MemoryStream(System.Text.Encoding.ASCII.GetBytes(all));
            }

            char[] buffer = new char[1024];
            int buffersize = buffer.Length;
            int seek = 0;
            List<char> workspace = new List<char>();

            //parsing state
            int assumecomment = -1;
            bool iscommentchar = false;
            bool assumetag = false;
            bool indquote = false;
            bool insquote = false;

            //reset tokenizer object state
            _response = new List<string>();
            
            // Open the stream using a StreamReader for easy access.
            int retreived = 0;
            using (StreamReader reader = new StreamReader(dataStream))
                // Read the content.
                while (tee<int>(reader.Read(buffer, 0, buffersize), ref retreived) != 0)
                {
                    seek += buffersize;

                    char c = ' ';
                    int tagstart = -1;
                    int txtstart = -1;
                    for (int i = 0; i < retreived; i++)
                    {
                        iscommentchar = false;
                        c = buffer[i];
                        //if (this.DebugMode)
                            //all.Add(c);

                        if (assumetag && workspace.Count == 1 && !(char.IsLetter(c) || c == '/' || c == '!'))
                            assumetag = false;

                        if (assumecomment == 0)
                        {
                            workspace.Add(c);
                            int end = workspace.Count - 1;

                            if (workspace[end - 2] == '-' && workspace[end - 1] == '-' && workspace[end] == '>')
                            {
                                string s = workspace.ConvertToString(this.TrimWhitespace);
                                if (!string.IsNullOrEmpty(s))
                                    _response.Add(s);
                                workspace.Clear();

                                assumetag = false;
                                assumecomment = -1;
                            }
                        }
                        else
                            switch (c)
                            {
                                case '<':
                                    if (!indquote && !insquote)
                                    {
                                        assumetag = true;
                                        assumecomment = 3;
                                        iscommentchar= true;

                                        string s = workspace.ConvertToString(this.TrimWhitespace);
                                        if (!string.IsNullOrEmpty(s))
                                            AddAndTrigger(this._response, s, this.TokenIdentified);
                                        workspace.Clear();
                                    }
                                    workspace.Add(c);
                                    break;
                                case '>':
                                    workspace.Add(c);
                                    if (!indquote && !insquote)
                                        if (workspace[0] == '<' && assumetag)
                                        {
                                            string s = workspace.ConvertToString(this.TrimWhitespace);
                                            if (!string.IsNullOrEmpty(s))
                                                AddAndTrigger(this._response, s, this.TokenIdentified);
                                            workspace.Clear();

                                            assumetag = false;
                                        }
                                    break;
                                case '\'':
                                    {
                                        if (assumetag && !indquote)
                                            insquote = !insquote;
                                        workspace.Add(c);
                                    }
                                    break;
                                case '"':
                                    {
                                        if (assumetag && !insquote)
                                            indquote = !indquote;
                                        workspace.Add(c);
                                    }
                                    break;
                                case '!':
                                    if (assumecomment == 3)
                                    {
                                        assumecomment = 2;
                                        iscommentchar = true;
                                    }
                                    workspace.Add(c);
                                    break;
                                case '-':
                                    if (assumecomment < 3 && assumecomment > 0)
                                    {
                                        assumecomment--;
                                        iscommentchar = true;
                                    }
                                    workspace.Add(c);
                                    break;
                                default:
                                    workspace.Add(c);
                                    break;
                            }

                        if (!iscommentchar && assumecomment > 0)
                            assumecomment = -1;
                        /*
                        if (this.ShowWorkMode && this.DebugMode)
                        {
                            Console.WriteLine("");
                            Console.WriteLine("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV");
                            Console.WriteLine("VVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVVV");
                            int maxcol = all.Count >70 ? 70 : all.Count;
                            //Console.Clear();
                            Console.ForegroundColor = ConsoleColor.Blue; Console.Write("Last70: "); Console.ResetColor();
                            foreach(var item in all.Skip<char>(all.Count-maxcol).Take<char>(maxcol))
                                Console.Write(item);
                            Console.WriteLine();

                            Console.ForegroundColor = ConsoleColor.Blue; Console.Write("Workspace: "); Console.ResetColor();
                            foreach (var item in workspace)
                                Console.Write(item);
                            Console.WriteLine();

                            Console.ForegroundColor = ConsoleColor.Blue; Console.Write("Last5Token: "); Console.ResetColor();
                            if (_response.Count > 1)
                                foreach (var item in _response.Reverse<string>().Take<string>(5))
                                {
                                    if(Console.BackgroundColor==ConsoleColor.DarkGray)
                                        Console.ResetColor();
                                    else
                                        Console.BackgroundColor  = ConsoleColor.DarkGray;
                                    Console.WriteLine(item);
                                }
                            Console.ResetColor();

                            //if(Console.ReadKey(true).Key != ConsoleKey.Enter)
                                //System.Threading.Thread.Sleep(250);
                            //else
                            Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                            Console.WriteLine("AAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAAA");
                            Console.WriteLine("");
                            //Console.ReadLine();
                        }*/
                    }
                }

            if (this.DebugMode)
            //using (Stream dataStream = response.GetResponseStream())
            //using (StreamReader reader = new StreamReader(dataStream))
            {
                string orig = (new string(all.ToArray())).Replace("\n"," ").Replace("\r"," ");
                int end = 0;
                int next = 0;
                string last = null;
                Console.Write("Count = ");
                Console.WriteLine(this.Tokens.Count);
                foreach (var item in this.Tokens)
                {
                    next = orig.IndexOf(item, end);
                    if (next == -1)
                    {
                        Console.WriteLine("----------------------------------------------------");
                        Console.WriteLine("No ");
                        Console.ForegroundColor = ConsoleColor.Blue; Console.WriteLine(item); Console.ResetColor();
                        Console.WriteLine(" after Pos ");
                        Console.ForegroundColor = ConsoleColor.Blue; Console.WriteLine(end); Console.ResetColor();
                        Console.WriteLine(", or after the fragment: ");
                        Console.ForegroundColor = ConsoleColor.Red; 
                        Console.WriteLine(orig.Substring(end - (end > 40 ? 40 : 0), (end > 40 ? 40 : 0)));
                        Console.ResetColor();
                        Console.ReadLine();
                        break;
                    }

                    if (next != end)
                        if (!string.IsNullOrWhiteSpace(orig.Substring(end, next - end).Trim()))
                        {
                            Console.WriteLine("----------------------------------------------------");
                            Console.WriteLine("-----------Missing Token-----------");
                            Console.WriteLine(last + " ...");
                            Console.WriteLine(orig.Substring(end, next - end));
                            Console.WriteLine("... " + item);
                            Console.ReadLine();
                        }

                    last = item;
                    end = next + item.Length;
                }
            }

        }

        static private T tee<T>(T value, ref T store) {
            store = value;
            return value;
        }

        public List<string> Tokens
        {
            get
            {
                if(this.TrimWhitespace)
                    return _response.Where<string>(s=>!string.IsNullOrWhiteSpace(s)).ToList();
                else
                    return _response;
            }
        }
        public int FindIndexOf(string sub)
        {
            sub = sub.ToLower();
            return _response.TakeWhile<string>(s => !s.ToLower().Contains(sub)).Count<string>();
        }
        /*
        private string toString(List<char> chars)
        {
            if (this.TrimWhitespace)
                return (new string(chars.ToArray())).Replace("\0", string.Empty).Replace("\r", string.Empty).Replace("\n", string.Empty).Trim();
            else
                return new string(chars.ToArray()).Replace("\0", string.Empty);
        }
        */

        public delegate void TokenIdentifiedEventhander(object sender, StringTokenEventArgs e);
        public event TokenIdentifiedEventhander TokenIdentified;

        
        public void AddAndTrigger(List<string> list, string item, TokenIdentifiedEventhander eventHandler)
        {
            list.Add(item);
            if (eventHandler != null)
                eventHandler(this, new StringTokenEventArgs() { TokenString = item });
        }
    }

    public class StringTokenEventArgs : EventArgs
    {
        public string TokenString;
    }

    static public class CharList
    {
        static public string ConvertToString(this List<char> chars, bool trimWhitespace)
        {
            /*
            if (trimWhitespace)
                return (new string(chars.Where<char>(s=> s!='\0' && s!='\r' && s!='\n').ToArray())).Trim();
            else
                return (new string(chars.Where<char>(s => s != '\0').ToArray())).Trim();
             */
            if (trimWhitespace)
                return (new string(chars.Where<char>(s => s != '\0').Replace<char>('\r', ' ').Replace<char>('\n', ' ').ToArray())).Trim();
            else
                return (new string(chars.Where<char>(s => s != '\0').ToArray())).Trim();
        }

        static public IEnumerable<T> Replace<T>(this IEnumerable<T> list, T find, T replace)
        {
            foreach(var item in list)
                if(item.Equals(find))
                    yield return replace;
                else
                    yield return item;
        }
        static public IEnumerable<T> Replace<T>(this IEnumerable<T> list, Func<T, T> selector)
        {
            foreach (var item in list)
                yield return selector(item);
        }
    }
}

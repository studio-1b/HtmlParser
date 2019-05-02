using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CrawlerCommon
{
    public class ReadonlyStringSegmentation : IList<string>, ICollection<string>, IEnumerable<string>, System.Collections.IEnumerable
    {
        #region operator overrides
        /*
        public static bool operator !=(ReadonlyStringSegmentation a, ReadonlyStringSegmentation b)
        {
            if (a == null) return false;
            if (b == null) return false;

            return a.Equals(b);
        }
        public static bool operator ==(ReadonlyStringSegmentation a, ReadonlyStringSegmentation b)
        {
            if (a == null) return false;
            if (b == null) return false;
            return !a.Equals(b);
        }
        */
        #endregion operator overrides

        #region private classes for this class
        public class StringSegment
        {
            internal StringSegment(string value, int offset, int length, int endPad)
            {
                if (value == null)
                    throw new ArgumentNullException(value);
                System.Diagnostics.Debug.Assert(value != null, "Should never try to split a null");

                this.Value = value;
                this.Start = offset;
                this.Length = length;
                this.segmentValue = null;
                this.EndPad = endPad;
            }

            private string Value;
            private string segmentValue;

            public int Start;
            public int Length;
            public int EndPad;

            public string SegmentValue { 
                get 
                {
                    if (segmentValue == null)
                        segmentValue = this.Value.Substring(this.Start, this.Length);
                    
                    return segmentValue;
                } 
            }

            public IEnumerable<char> GetChar()
            {
                return this.Value.Skip<char>(this.Start).Take<char>(this.Length);
            }
        }
        #endregion private classes for this class
        
        #region IEnumerator
        public class SegmentValueEnumerator : IEnumerator<string>
        {
            internal SegmentValueEnumerator(ReadonlyStringSegmentation source)
            {
                this._source = new StringSegmentEnumerator(source);
            }
            private StringSegmentEnumerator _source;

            public string Current { get { return this._source.Current.SegmentValue; }}
            public bool MoveNext()
            {
                return this._source.MoveNext();
            }
            public void Reset() { this._source.Reset(); }
            object System.Collections.IEnumerator.Current { get { throw new NotSupportedException(); } }

            public void Dispose() { }
        }
        public class StringSegmentEnumerator : IEnumerator<StringSegment>
        {
            internal StringSegmentEnumerator(ReadonlyStringSegmentation source)
            {
                this._source = source;
            }
            private ReadonlyStringSegmentation _source;
            private int index=-1;

            public StringSegment Current { get { return _source.GetLazyLoadSegment(this.index); } }
            public bool MoveNext()
            {
                if (this.index >= this._source.Count-1)
                    return false;

                this.index++;
                return true;
            }
            public void Reset() { index = 0; }

            object System.Collections.IEnumerator.Current { get { return _source.GetLazyLoadSegment(this.index); } }

            public void Dispose() { }
        }
        #endregion IEnumerator

        public delegate IEnumerable<int> parserDelegate(IEnumerable<char> charStream);
        /// <summary>
        /// Accepts list of fixed width lengths.  Negative integer values are skipped fields.  index given for each non-negative number.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offsetList"></param>
        public ReadonlyStringSegmentation(string value, parserDelegate parser)
            : this(value, parser(value))
        { }

        /// <summary>
        /// Accepts list of fixed width lengths.  Negative integer values are skipped fields.  index given for each non-negative number.
        /// </summary>
        /// <param name="value"></param>
        /// <param name="offsetList"></param>
        public ReadonlyStringSegmentation(string value, IEnumerable<int> offsetList)
        {
            if (value == null)
                throw new ArgumentNullException(value);

            int start=0;
            int index = 0;
            foreach (var item in offsetList)
                if (item < 0)
                {
                    if (this.offsets.Count > 0)
                        this.offsets[this.offsets.Count - 1].EndPad += -item; //add to prev endpad, if there was prev segment
                    start += -item; //move onto next field
                }
                else
                {
                    StringSegment segment = new StringSegment(value, start, item, 0);
                    this.offsets.Add(segment);
                    start += item;
                    index++;
                }

            //if (this.offsets.Count % 2!=1)
            //    System.Diagnostics.Debug.Assert(this.offsets.Count % 2 == 1, "This application expects odd # of param");
        }
        /// <summary>
        /// Accepts list of separators for delimited string
        /// </summary>
        /// <param name="value"></param>
        /// <param name="initialSeparator"></param>
        /// <param name="separatorValue"></param>
        public ReadonlyStringSegmentation(string value, string initialSeparator, params string[] separatorValue)
        {
            if (value == null)
                throw new ArgumentNullException(value);

            if (separatorValue.Length == 0)
                this.Separator = new string[1] { initialSeparator };
            else
            {
                string[] temp = new string[separatorValue.Length + 1];
                Array.Copy(separatorValue, 0, temp, 1, separatorValue.Length);
                temp[0] = initialSeparator;
                this.Separator = temp;
            }
            this.Value = value;

            //find next occurrance of separator and place it in StringSegment. assign the value of terminate variable, if no next occurrence found.
            int last = 0;
            StringSegment terminate = new StringSegment(string.Empty, -1, 0, 0);
            StringSegment next = this.Separator.Select<string, StringSegment>(s => new StringSegment(value, last, value.IndexOf(s) - last, s.Length))
                .Where<StringSegment>(s => s.Length >= 0)
                .DefaultIfEmpty<StringSegment>(terminate)
                .OrderBy<StringSegment, int>(s => s.Length)
                .FirstOrDefault<StringSegment>();

            //find all occurences of separators and save the segments
            while (next.Start != terminate.Start)
            {
                this.offsets.Add(next);

                last = next.Start + next.Length + next.EndPad;
                if (last < value.Length)
                    next = this.Separator.Select<string, StringSegment>(s => new StringSegment(value, last, value.IndexOf(s, last) - last, s.Length))
                            .Where<StringSegment>(s => s.Length >= 0)
                            .DefaultIfEmpty<StringSegment>(terminate)
                            .OrderBy<StringSegment, int>(s => s.Length)
                            .FirstOrDefault<StringSegment>();
                else
                    next = terminate;
            }

            next = new StringSegment(value, last, value.Length - last, 0);
            this.offsets.Add(next);
        }
        public string Value { get; private set; }

        #region IList
        public string[] Separator { get; private set; }
        List<StringSegment> offsets = new List<StringSegment>();
        public string this[int index]
        {
            get
            {
                return this.GetLazyLoadSegment(index).SegmentValue;
            }
            set
            {
                throw new NotSupportedException();
            }
        }
        public StringSegment GetLazyLoadSegment(int index)
        {
            if (index >= offsets.Count)
                throw new IndexOutOfRangeException();

            return this.offsets[index];
        }
        public int Count { get { return offsets.Count; } }
        public int IndexOf(string value) { throw new NotSupportedException(); }
        public bool IsReadOnly { get { return true; } }
        public bool Contains(string value) { throw new NotSupportedException(); }
        public void CopyTo(string[] dest, int start) {
            for (int i = 0; i < this.offsets.Count; i++)
                if(start + i<dest.Length)
                    dest[start + i] = this[i];
        }

        public bool Remove(string index) { throw new NotSupportedException(); }
        public void RemoveAt(int index) { throw new NotSupportedException(); }
        public void Insert(string index) { throw new NotSupportedException(); }
        public void Insert(int at, string index) { throw new NotSupportedException(); }
        public void Clear() { throw new NotSupportedException(); }
        public void Add(string index) { throw new NotSupportedException(); }
        #endregion IList


        #region IEnumerable
        public IEnumerator<string> GetEnumerator()
        {
            return new SegmentValueEnumerator(this);
        }
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return null;
        }
        #endregion IEnumerable

        private bool isArrayElementsSame(string[] a, string[] b)
        {
            return a.Zip<string, string, bool>(b, (m,n) => m == n).All(s=>s);
        }
        public bool Equals(ReadonlyStringSegmentation obj) 
        {
            return (obj.Value == this.Value) && isArrayElementsSame(this.Separator, obj.Separator);
        }
        public override bool Equals(object obj) 
        { 
            if(obj is string)
                return (obj==this.Value);

            if (obj is ReadonlyStringSegmentation)
                return this.Equals((ReadonlyStringSegmentation)obj);

            return false;
        }

        /*
        public override int GetHashCode()
        {
            return 0;
        }*/
    }

    static public class CachedStringSegmentation
    {
        struct CatValPar {
            public string Category;
            public string Value;
            public object Param;
        }
        static CachedStringSegmentation()
        {
            System.Threading.Thread main=System.Threading.Thread.CurrentThread;
            System.Threading.Thread cleanup = new System.Threading.Thread(delegate() {
                int wait = 1000;
                while (main.Join(wait))
                {
                    List<CatValPar> r = new List<CatValPar>();

                    foreach(var category in cache)
                        foreach(var value in category.Value)
                            foreach (var param in value.Value)
                                if (!param.Value.IsAlive)
                                    r.Add(new CatValPar() { Category=category.Key, Value=value.Key, Param=param.Key} );
                    lock(cache)
                        foreach (var item in r)
                        {
                            cache[item.Category][item.Value].Remove(item.Param);
                            if (cache[item.Category][item.Value].Count == 0)
                                cache[item.Category].Remove(item.Value);
                            if (cache[item.Category].Count == 0)
                                cache.Remove(item.Category);
                        }

                    if (r.Count == 0) wait = 1000; else wait = 100;
                }
            }) { IsBackground=true };
            cleanup.Start();
        }

        /// <summary>
        /// type, value, [parameters] --> ReadonlyStringSegmentation 
        /// </summary>
        static readonly Dictionary<string, Dictionary<string, Dictionary<object, WeakReference>>> cache = new Dictionary<string, Dictionary<string, Dictionary<object, WeakReference>>>();
        static public ReadonlyStringSegmentation Segment(this string value, ReadonlyStringSegmentation.parserDelegate parser)
        {
            const string DELEGATE_SECTION = "delegate";
            ReadonlyStringSegmentation cacheItem = getFromCache(DELEGATE_SECTION, value, parser);
            if (cacheItem == null)
            {
                cacheItem = new ReadonlyStringSegmentation(value, parser);
                setCache(DELEGATE_SECTION, value, parser, cacheItem);
            }
            return cacheItem;
        }
        static public ReadonlyStringSegmentation Segment(this string value, IEnumerable<int> offsetList)
        {
            const string DELEGATE_SECTION = "IEnumerable";
            ReadonlyStringSegmentation cacheItem = getFromCache(DELEGATE_SECTION, value, offsetList);
            if (cacheItem == null)
            {
                cacheItem = new ReadonlyStringSegmentation(value, offsetList);
                setCache(DELEGATE_SECTION, value, offsetList, cacheItem);
            }
            return cacheItem;
        }
        static public ReadonlyStringSegmentation Segment(this string value, string initialSeparator, params string[] separatorValue)
        {
            const string DELEGATE_SECTION = "delimited";
            string rep = initialSeparator + "\n" + string.Join("\n", separatorValue);
            ReadonlyStringSegmentation cacheItem = getFromCache(DELEGATE_SECTION, value, rep);
            if (cacheItem == null)
            {
                cacheItem = new ReadonlyStringSegmentation(value, rep);
                setCache(DELEGATE_SECTION, value, rep, cacheItem);
            }
            return cacheItem;
        }
        static public int Requests { get; private set; }
        static public int Hits { get; private set; }
        static ReadonlyStringSegmentation getFromCache(string category, string value, object param) {
            Requests++;
            if (!cache.ContainsKey(category))
                lock(cache)
                    if (!cache.ContainsKey(category))
                        cache.Add(category, new Dictionary<string, Dictionary<object, WeakReference>>() { {value, new Dictionary<object, WeakReference>() }} );
            if (!cache[category].ContainsKey(value))
                lock (cache)
                    if (!cache[category].ContainsKey(value))
                        cache[category].Add(value, new Dictionary<object, WeakReference>());
            WeakReference temp=null;
            if (cache[category][value].ContainsKey(param))
                lock (cache)
                    if (cache[category][value].ContainsKey(param)) // just in case the key gets removed by another thread
                        temp=cache[category][value][param]; 
            if(temp!=null)
                if (temp.IsAlive)
                    if (temp.Target != null)
                        if (temp.Target is ReadonlyStringSegmentation)
                        {
                            Hits++;
                            return (ReadonlyStringSegmentation)temp.Target;
                        }
            return null;
        }
        static void setCache(string category, string value, object param, ReadonlyStringSegmentation segment)
        {
            if (cache[category][value].ContainsKey(param))
                lock (cache)
                    if (cache[category][value].ContainsKey(param))
                    {
                        cache[category][value].Remove(param);
                        cache[category][value].Add(param, new WeakReference(segment));
                    } else
                        cache[category][value].Add(param, new WeakReference(segment));
            else
                lock (cache)
                    if (!cache[category][value].ContainsKey(param))
                        cache[category][value].Add(param, new WeakReference(segment));
                    else
                    {
                        cache[category][value].Remove(param);
                        cache[category][value].Add(param, new WeakReference(segment));
                    }
        }

        /*
        static public ReadonlyStringSegmentation Resegment(this ReadonlyStringSegmentation.StringSegment value, string initialSeparator, params string[] separatorValue)
        {
            const string DELEGATE_SECTION = "reseg";
            string rep = initialSeparator + "\n" + string.Join("\n", separatorValue);
            ReadonlyStringSegmentation cacheItem = getFromCache(DELEGATE_SECTION, rep, value); //last 2 reversed bc it doesnt matter
            if (cacheItem == null)
            {
                cacheItem = new ReadonlyStringSegmentation(value, rep);
                setCache(DELEGATE_SECTION, value, rep, cacheItem);
            }
            return cacheItem;
        }*/
    }
}

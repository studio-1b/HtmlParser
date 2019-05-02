using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using CrawlerCommon.TagDef.StrictXHTML;

namespace CrawlerCommon
{
    public interface ITokenTest
    {
        bool IsMatch(Token node);
        event MatchHandler Match;

    }
    public delegate void MatchHandler(object sender, MatchEventArgs<Token> e);
    public class MatchEventArgs<T> : EventArgs
    {
        public MatchEventArgs(T passingValue)
        {
            this.SelectedValue = passingValue;
        }

        public T SelectedValue { get; private set; }
    }

    public abstract class BaseTestLinkage : ITokenTest, ICollection<ITokenTest>
    {
        #region ITokenTest
        public abstract bool IsMatch(Token token);

        public abstract event MatchHandler Match;

        protected readonly List<ITokenTest> TestList = new List<ITokenTest>();
        #endregion ITokenTest



        #region ICollection
        public void CopyTo(ITokenTest[] array, int index)
        {
            int end = this.Count < (array.Length -index) ? (this.Count) : (array.Length -index);
            for (int i = index; i < index + end; i++)
                array[i] = this.TestList[i];
        }

        public int Count
        {
            get { return this.TestList.Count; }
        }

        public bool IsSynchronized
        {
            get { return false; }
        }

        public object SyncRoot
        {
            get { return this.TestList; }
        }

        public IEnumerator<ITokenTest> GetEnumerator()
        {
            return this.TestList.GetEnumerator();
        }

        public void Add(ITokenTest item)
        {
            this.TestList.Add(item);
        }

        public void Clear()
        {
            this.TestList.Clear();
        }

        public bool Contains(ITokenTest item)
        {
            return this.TestList.Contains(item);
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(ITokenTest item)
        {
            return this.TestList.Remove(item);
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }
        #endregion ICollection
    }

    public class CacheTestResult : ITokenTest
    {
        public CacheTestResult(ITokenTest instance)
        {
            this.test = instance;
        }
        ITokenTest test = null;
        Dictionary<Token, bool> cache = new Dictionary<Token, bool>();

        #region ITokenTest
        public bool IsMatch(Token token)
        {
            if (!cache.ContainsKey(token))
            {
                bool result = test.IsMatch(token);
                cache.Add(token, result);
                return result;
            }

            bool cacheresult = cache[token];

            if (cacheresult)
                if (this.cacheMatch!=null)
                    this.cacheMatch(this, new MatchEventArgs<Token>(token));

            return cacheresult;
        }

        private event MatchHandler cacheMatch;
        public event MatchHandler Match { 
            add
            {
                test.Match += value;
                this.cacheMatch += value;
            }
            remove
            {
                test.Match -= value;
                this.cacheMatch -= value;
            }
        }
        #endregion ITokenTest
    }
}

using System;
using System.Collections;

namespace NUnit.TestUtilities
{
    public class SimpleObjectList : IList
    {
        private IList inner;

        public SimpleObjectList(IList contents)
        {
            Initialize(contents);
        }

        public SimpleObjectList(params object[] contents)
        {
            Initialize(contents);
        }

        private void Initialize(IList contents)
        {
#if CLR_1_1
            this.inner = new System.Collections.ArrayList();
#else
            this.inner = new System.Collections.Generic.List<object>();
#endif
            foreach (object o in contents)
                this.inner.Add(o);
        }

        #region IList Members

        public int Add(object value)
        {
            return inner.Add(value);
        }

        public void Clear()
        {
            inner.Clear();
        }

        public bool Contains(object value)
        {
            return inner.Contains(value);
        }

        public int IndexOf(object value)
        {
            return inner.IndexOf(value);
        }

        public void Insert(int index, object value)
        {
            inner.Insert(index, value);
        }

        public bool IsFixedSize
        {
            get { return inner.IsFixedSize; }
        }

        public bool IsReadOnly
        {
            get { return inner.IsReadOnly; }
        }

        public void Remove(object value)
        {
            inner.Remove(value);
        }

        public void RemoveAt(int index)
        {
            inner.RemoveAt(index);
        }

        public object this[int index]
        {
            get { return inner[index]; }
            set { inner[index] = value; }
        }

        #endregion

        #region ICollection Members

        public void CopyTo(Array array, int index)
        {
            inner.CopyTo(array, index);
        }

        public int Count
        {
            get { return inner.Count; }
        }

        public bool IsSynchronized
        {
            get { return inner.IsSynchronized; }
        }

        public object SyncRoot
        {
            get { return inner.SyncRoot; }
        }

        #endregion

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            return inner.GetEnumerator();
        }

        #endregion
    }
}

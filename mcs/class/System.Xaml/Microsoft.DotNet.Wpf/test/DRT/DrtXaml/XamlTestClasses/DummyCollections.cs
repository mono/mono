using System;
using System.Collections.Generic;
using System.Collections;
using System.Windows.Markup;
using System.Xaml;

namespace Test.Elements
{
    public class IDictionaryExplicitImpl : IDictionary<string, string>
    {
        #region IDictionary<string,string> Members

        void IDictionary<string, string>.Add(string key, string value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, string>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, string>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary<string, string>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, string>.TryGetValue(string key, out string value)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, string>.Values
        {
            get { throw new NotImplementedException(); }
        }

        string IDictionary<string, string>.this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,string>> Members

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, string>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<string, string>>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,string>> Members

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class DictionaryPlusExplicitImpl : Dictionary<string, string>, IDictionary<int, int>
    {

        #region IDictionary<int,int> Members

        void IDictionary<int, int>.Add(int key, int value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<int, int>.ContainsKey(int key)
        {
            throw new NotImplementedException();
        }

        ICollection<int> IDictionary<int, int>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary<int, int>.Remove(int key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<int, int>.TryGetValue(int key, out int value)
        {
            throw new NotImplementedException();
        }

        ICollection<int> IDictionary<int, int>.Values
        {
            get { throw new NotImplementedException(); }
        }

        int IDictionary<int, int>.this[int key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<int,int>> Members

        void ICollection<KeyValuePair<int, int>>.Add(KeyValuePair<int, int> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<int, int>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<int, int>>.Contains(KeyValuePair<int, int> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<int, int>>.CopyTo(KeyValuePair<int, int>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<int, int>>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<int, int>>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<int, int>>.Remove(KeyValuePair<int, int> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<int,int>> Members

        IEnumerator<KeyValuePair<int, int>> IEnumerable<KeyValuePair<int, int>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class DictionaryIEnumerablePlusAdd : IEnumerable
    {
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(double key, double val)
        {
        }
    }

    public class DictionaryGetEnumeratorPlusAdd
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(double key, double val)
        {
        }
    }

    public class DictionaryPrivateGetEnumeratorPlusAdd
    {
        private IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(double key, double val)
        {
        }
    }

    public class DictionaryAmbiguousInterface : IDictionary<string, string>, IDictionary<int, int>
    {
        #region IDictionary<string,string> Members

        void IDictionary<string, string>.Add(string key, string value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, string>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, string>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary<string, string>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, string>.TryGetValue(string key, out string value)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, string>.Values
        {
            get { throw new NotImplementedException(); }
        }

        string IDictionary<string, string>.this[string key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<string,string>> Members

        void ICollection<KeyValuePair<string, string>>.Add(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, string>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, string>>.Contains(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, string>>.CopyTo(KeyValuePair<string, string>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<string, string>>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<string, string>>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<string, string>>.Remove(KeyValuePair<string, string> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<string,string>> Members

        IEnumerator<KeyValuePair<string, string>> IEnumerable<KeyValuePair<string, string>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDictionary<int,int> Members

        void IDictionary<int, int>.Add(int key, int value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<int, int>.ContainsKey(int key)
        {
            throw new NotImplementedException();
        }

        ICollection<int> IDictionary<int, int>.Keys
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary<int, int>.Remove(int key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<int, int>.TryGetValue(int key, out int value)
        {
            throw new NotImplementedException();
        }

        ICollection<int> IDictionary<int, int>.Values
        {
            get { throw new NotImplementedException(); }
        }

        int IDictionary<int, int>.this[int key]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection<KeyValuePair<int,int>> Members

        void ICollection<KeyValuePair<int, int>>.Add(KeyValuePair<int, int> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<int, int>>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<int, int>>.Contains(KeyValuePair<int, int> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<int, int>>.CopyTo(KeyValuePair<int, int>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<KeyValuePair<int, int>>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<int, int>>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<KeyValuePair<int, int>>.Remove(KeyValuePair<int, int> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<KeyValuePair<int,int>> Members

        IEnumerator<KeyValuePair<int, int>> IEnumerable<KeyValuePair<int, int>>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class DictionaryAmbiguousMethods : IEnumerable
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(string key, string val)
        {
        }

        public void Add(int key, int val)
        {
        }
    }

    public class GetEnumPlusAdd
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(Element item)
        {
        }
    }

    [ContentProperty("Element")]
    public class ElementWrapper
    {
        public Element Element { get; set; }
    }

    [ContentProperty("Kid")]
    public class KidWrapper
    {
        public Kid Kid { get; set; }
    }

    [ContentWrapper(typeof(ElementWrapper))]
    [ContentWrapper(typeof(KidWrapper))]
    public class EnumPlusContentWrapper : IEnumerable
    {
        public void Add(object item)
        {
        }

        public void Add(Element item)
        {
        }

        public void Add(Kid item)
        {
        }

        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }

    public class ExplicitIList : IList
    {

        #region IList Members

        int IList.Add(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Clear()
        {
            throw new NotImplementedException();
        }

        bool IList.Contains(object value)
        {
            throw new NotImplementedException();
        }

        int IList.IndexOf(object value)
        {
            throw new NotImplementedException();
        }

        void IList.Insert(int index, object value)
        {
            throw new NotImplementedException();
        }

        bool IList.IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        bool IList.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        void IList.Remove(object value)
        {
            throw new NotImplementedException();
        }

        void IList.RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        object IList.this[int index]
        {
            get
            {
                throw new NotImplementedException();
            }
            set
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region ICollection Members

        void ICollection.CopyTo(Array array, int index)
        {
            throw new NotImplementedException();
        }

        int ICollection.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection.IsSynchronized
        {
            get { throw new NotImplementedException(); }
        }

        object ICollection.SyncRoot
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ExplicitICollectionOfString : ICollection<string>
    {

        #region ICollection<string> Members

        void ICollection<string>.Add(string item)
        {
            throw new NotImplementedException();
        }

        void ICollection<string>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<string>.Contains(string item)
        {
            throw new NotImplementedException();
        }

        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<string>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<string>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<string>.Remove(string item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<string> Members

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class ImplicitAndExplicitCollection : ICollection<string>, ICollection<int>
    {
        #region ICollection<string> Members

        public void Add(string item)
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(string item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(string[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        public bool Remove(string item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<string> Members

        public IEnumerator<string> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollection<int> Members

        void ICollection<int>.Add(int item)
        {
            throw new NotImplementedException();
        }

        void ICollection<int>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<int>.Contains(int item)
        {
            throw new NotImplementedException();
        }

        void ICollection<int>.CopyTo(int[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<int>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<int>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<int>.Remove(int item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<int> Members

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class AmbiguousCollectionInterface : ICollection<string>, ICollection<int>
    {
        #region ICollection<string> Members

        void ICollection<string>.Add(string item)
        {
            throw new NotImplementedException();
        }

        void ICollection<string>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<string>.Contains(string item)
        {
            throw new NotImplementedException();
        }

        void ICollection<string>.CopyTo(string[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<string>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<string>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<string>.Remove(string item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<string> Members

        IEnumerator<string> IEnumerable<string>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region ICollection<int> Members

        void ICollection<int>.Add(int item)
        {
            throw new NotImplementedException();
        }

        void ICollection<int>.Clear()
        {
            throw new NotImplementedException();
        }

        bool ICollection<int>.Contains(int item)
        {
            throw new NotImplementedException();
        }

        void ICollection<int>.CopyTo(int[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        int ICollection<int>.Count
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<int>.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        bool ICollection<int>.Remove(int item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IEnumerable<int> Members

        IEnumerator<int> IEnumerable<int>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        #endregion
    }

    public class AmbiguousCollectionMethods
    {
        public IEnumerator GetEnumerator()
        {
            throw new NotImplementedException();
        }

        public void Add(string s)
        {
        }

        public void Add(int i)
        {
        }
    }

    public class CollectionWithTwoParameterAdd : List<string>
    {
        public void Add(string s, int y)
        {
        }
    }
}

using System;
using System.Collections.Generic;
using System.Collections;
using System.Globalization;
using System.Windows.Markup;
using System.Xaml;
using System.ComponentModel;

namespace Test.Elements
{
    [ContentProperty(@"Content")]
    public class MyContentType
    {
        public string[] Content { get; set; }
    }

    public class DoubleCollection: List<Double>
    {
        public string Flavor { get; set; }
    }

    public class Kid
    {
        public string Name { get; set; }
    }

    public class SuperKid : Kid
    {
        public string Title { get; set; }
    }

    public class KidList : List<Kid>
    {
    }

    [ContentProperty("RwKids")]
    public class RwParentWithCP
    {
        public RwParentWithCP()
        {
            RwKids = new KidList();
        }

        public KidList RwKids { get; set; }
    }

    public class RwParentWithOutCP
    {
        public RwParentWithOutCP()
        {
            RwKids = new KidList();
        }

        public KidList RwKids { get; set; }
    }

    [ContentProperty("RoKids")]
    public class RoParentWithCP
    {
        KidList _kids;

        public RoParentWithCP()
        {
            _kids = new KidList();
        }

        public KidList RoKids
        {
            get { return _kids; }
        }
    }

    public class RoParentWithOutCP
    {
        KidList _kids;

        public RoParentWithOutCP()
        {
            _kids = new KidList();
        }

        public KidList RoKids
        {
            get { return _kids; }
        }
    }

    [ContentProperty("RwNullKids")]
    public class RwNullParentWithCP
    {
        public KidList RwNullKids { get; set; }
    }

    public class RwNullParentWithOutCP
    {
        public KidList RwNullKids {get; set; }
    }

    public class Resource
    {
        public string SomeData { get; set; }
    }

    // This is here while x:String is unnameable
    public class ResourceWithNameableKey: Resource
    {
        public StringKey MyKey { get; set; }
        public StringKey SomeData2 { get; set; }
    }

    // This abomination is only here while x:String is unnameable
    [ContentProperty("Key")]
    public class StringKey
    {
        public string Key { get; set; }
        public override string ToString()
        {
            return Key;
        }
    }

    [DictionaryKeyProperty("MyKey")]
    public class ResourceWithImplictKey : Resource
    {
        public object MyKey { get; set; }
    }

    [RuntimeNameProperty("MyName")]
    public class ResourceWithName : Resource
    {
        public string MyName { get; set; }
    }

    [DictionaryKeyProperty("RandomKey")]
    public class ResourceWithRoImplictKey : Resource
    {
        static Random _random;
        int _key = 0;

        static ResourceWithRoImplictKey()
        {
            _random = new Random();
        }

        public string RandomKey
        {
            get
            {
                if (_key == 0)
                {
                    do
                    {
                        _key = _random.Next();
                    } while (_key == 0);
                }
                return _key.ToString();
            }
        }
    }

    public class RoDictionaryProvider
    {
        Dictionary<string, object> _dict = new Dictionary<string, object>();

        public Dictionary<string, object> Dict
        {
            get { return _dict; }
        }
    }

    public class RWGenericDictionaryProvider<K, V>
    {
        private IDictionary<K, V> dict = new Dictionary<K, V>();

        public IDictionary<K, V> Dict
        {
            get
            {
                return dict;
            }
        }
    }

    public class RWDerivedGenericDictionaryProvider : RWGenericDictionaryProvider<int, string>
    {
    }

    [ContentProperty("ObjectList")]
    public class ObjectListHolder
    {
        List<object> _objectList = new List<object>();
        List<string> _stringList = new List<string>();

        public List<object> ObjectList
        {
            get { return _objectList; }
        }

        public List<string> StringList
        {
            get { return _stringList; }
        }

        public object Tag { get; set; }
    }

    public class DictionaryPlusAddExtraKeys : Dictionary<string, Element>
    {
        public void Add(int idx, Element elm)
        {
            this.Add(idx.ToString(), elm);
        }
        public void Add(double idx, Element elm)
        {
            this.Add(idx.ToString(), elm);
        }
    }

    public class DictionaryPlusAddExtraValues : Dictionary<string, string>
    {
        public void Add(string key, int val)
        {
            this.Add(key, val.ToString());
        }
        public void Add(string key, double val)
        {
            this.Add(key, val.ToString());
        }
    }

    public class ElementDictionaryHolder: Element
    {
        private Dictionary<string, Element> _dict;

        public ElementDictionaryHolder()
        {
            _dict = new Dictionary<string, Element>();
        }

        public Dictionary<string, Element> ElementDictionary
        { 
            get { return _dict; }
            set { _dict = value; }
        }
    }

    public class ElementCollection : ICollection<Element>
    {
        #region ICollection<Element> Members

        List<Element> _list;
        public ElementCollection()
        {
            _list = new List<Element>();
        }

        public void Add(Element item)
        {
            _list.Add(item);
        }

        public void Clear()
        {
            _list.Clear();
        }

        public bool Contains(Element item)
        {
            return _list.Contains(item);
        }

        public void CopyTo(Element[] array, int arrayIndex)
        {
            _list.CopyTo(array, arrayIndex);
        }

        public int Count
        {
            get { return _list.Count; }
        }

        public bool IsReadOnly
        {
            get { return false; }
        }

        public bool Remove(Element item)
        {
            return _list.Remove(item);
        }

        #endregion

        #region IEnumerable<Element> Members

        public IEnumerator<Element> GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _list.GetEnumerator();
        }

        #endregion
    }

    [ContentProperty("Content")]
    public class ElementCollectionHolder : Element
    {
        ElementCollection _content;

        public ElementCollectionHolder()
        {
            _content = new ElementCollection();
        }

        public ElementCollection Content
        {
            get { return _content; }
        }

        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class EnumPlusAdd : IEnumerable
    {
        private List<object> _list;

        public EnumPlusAdd()
        {
            _list = new List<object>();
        }

        public void Add(Element item)
        {
            _list.Add(item);
        }

        #region IEnumerable Members

        public IEnumerator GetEnumerator()
        {
            foreach (object o in _list)
            {
                yield return o;
            }
        }

        #endregion
    }

    public class EnumPlusAddHolder : Element
    {
        EnumPlusAdd _content;

        public EnumPlusAddHolder()
        {
            _content = new EnumPlusAdd();
        }

        public EnumPlusAdd Content
        {
            get { return _content; }
        }

        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class BuggyHashCodeResource: Resource
    {
        public int Value { get; set; }

        public override int GetHashCode()
        {
            return (SomeData==null) ? 0 : SomeData.GetHashCode() + Value;
        }
    }

    [WhitespaceSignificantCollection]
    public class ListOfStrings : List<string>
    {
        public string Title { get; set; }
        public string Text { get; set; }
    }

    public class AnimalInfo
    {
        string name;
        int number;

        public string Name
        {
            get { return name; }
            set { name = value; }
        }

        public int Number
        {
            get { return number; }
            set { number = value; }
        }
    }

    [System.Windows.Markup.ContentPropertyAttribute("ContentProperty")]
    public class TypeConverterContentProperty
    {
        public AnimalList ContentProperty
        { get; set; }
    }

    [TypeConverter(typeof(AnimalListConverter))]
    public class AnimalList : List<AnimalInfo>
    {
    }
}

namespace Test.Collections
{
    public class Dictionaries<K, V>
    {
        public IDictionary IDictionary { get; set; }

        public IDictionary<K, V> GenericIDictionary { get; set; }

        public Dictionary<K, V> Dictionary { get; set; }

        public MyDictionary<K, V> MyDictionary { get; set; }

        public CustomConvertingDictionary CustomConvertingDictionary { get; set; }

        public Dictionaries()
        {
            IDictionary = new Dictionary<K, V>();
            GenericIDictionary = new Dictionary<K, V>();
            Dictionary = new Dictionary<K, V>();
            MyDictionary = new MyDictionary<K, V>();
            CustomConvertingDictionary = new CustomConvertingDictionary();
        }
    }

    public class MyDictionary<K, V> : Dictionary<K, V>
    {
    }

    public class CustomConvertingDictionary : Dictionary<int, string>, IDictionary
    {
        void IDictionary.Add(object key, object value)
        {
            int intKey = int.Parse((string)key, NumberStyles.HexNumber);
            Add(intKey, (string)value);
        }

        void IDictionary.Clear()
        {
            throw new NotImplementedException();
        }

        bool IDictionary.Contains(object key)
        {
            throw new NotImplementedException();
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        bool IDictionary.IsFixedSize
        {
            get { throw new NotImplementedException(); }
        }

        bool IDictionary.IsReadOnly
        {
            get { throw new NotImplementedException(); }
        }

        ICollection IDictionary.Keys
        {
            get { throw new NotImplementedException(); }
        }

        void IDictionary.Remove(object key)
        {
            throw new NotImplementedException();
        }

        ICollection IDictionary.Values
        {
            get { throw new NotImplementedException(); }
        }

        object IDictionary.this[object key]
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
    }

    internal class InternalList<T> : List<T>
    {
    }
}
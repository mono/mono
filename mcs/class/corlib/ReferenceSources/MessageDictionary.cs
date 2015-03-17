using System.Collections;
using System.Diagnostics.Contracts;

namespace System.Runtime.Remoting.Messaging
{
    //+================================================================================
    //
    // Synopsis:   Abstract class to help present a dictionary view of an object
    //
    //-================================================================================
    internal abstract class MessageDictionary : IDictionary
    {
        internal String[] _keys;
        internal IDictionary  _dict;

        internal MessageDictionary(String[] keys, IDictionary idict)
        {
            _keys = keys;
            _dict = idict;
        }        

        internal bool HasUserData()
        {
            // used by message smuggler to determine if there is any custom user
            //   data in the dictionary
            if ((_dict != null) && (_dict.Count > 0))
                return true;
            else
                return false;
        }

        // used by message smuggler, so that it doesn't have to iterate
        //   through special keys
        internal IDictionary InternalDictionary
        {
            get { return _dict; }
        }
        

        internal abstract Object GetMessageValue(int i);

        [System.Security.SecurityCritical]
        internal abstract void SetSpecialKey(int keyNum, Object value);

        public virtual bool IsReadOnly { get { return false; } }
        public virtual bool IsSynchronized { get { return false; } }
        public virtual bool IsFixedSize { get { return false; } }
        
        public virtual Object SyncRoot { get { return this; } }
        

        public virtual bool Contains(Object key)
        {
            if (ContainsSpecialKey(key))
            {
                return true;
            }
            else if (_dict != null)
            {
                return _dict.Contains(key);
            }
            return false;
        }

        protected virtual bool ContainsSpecialKey(Object key)
        {
            if (!(key is System.String))
            {
                return false;
            }
            String skey = (String) key;
            for (int i = 0 ; i < _keys.Length; i++)
            {
                if (skey.Equals(_keys[i]))
                {
                    return true;
                }
            }
            return false;
        }

        public virtual void CopyTo(Array array, int index)
        {
            for (int i=0; i<_keys.Length; i++)
            {
                array.SetValue(GetMessageValue(i), index+i);
            }

            if (_dict != null)
            {
                _dict.CopyTo(array, index+_keys.Length);
            }
        }

        public virtual Object this[Object key]
        {
            get
            {
                System.String skey = key as System.String;
                if (null != skey)
                {
                    for (int i=0; i<_keys.Length; i++)
                    {
                        if (skey.Equals(_keys[i]))
                        {
                            return GetMessageValue(i);
                        }
                    }
                    if (_dict != null)
                    {
                        return _dict[key];
                    }
                }
                return null;
            }
            [System.Security.SecuritySafeCritical] // TODO: review - implements transparent public method
            set
            {
                if (ContainsSpecialKey(key))
                {
                    if (key.Equals(MonoMethodMessage.UriKey))
                    {
                        SetSpecialKey(0,value);
                    }
                    else if (key.Equals(MonoMethodMessage.CallContextKey))
                    {
                        SetSpecialKey(1,value);
                    }                    
                    else
                    {
                        throw new ArgumentException(
                            Environment.GetResourceString(
                                "Argument_InvalidKey"));
                    }
                }
                else
                {
                    if (_dict == null)
                    {
                        _dict = new Hashtable();
                    }
                    _dict[key] = value;
                }

            }
        }

        IDictionaryEnumerator IDictionary.GetEnumerator()
        {
            return new MessageDictionaryEnumerator(this, _dict);
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }


        public virtual void Add(Object key, Object value)
        {
            if (ContainsSpecialKey(key))
            {
                throw new ArgumentException(
                    Environment.GetResourceString(
                        "Argument_InvalidKey"));
            } 
            else
            {
                if (_dict == null)
                {
                    // no need to interlock, message object not guaranteed to
                    // be thread-safe.
                    _dict = new Hashtable();
                }
                _dict.Add(key, value);
            }
        }

        public virtual void Clear()
        {
            // Remove all the entries from the hash table
            if (null != _dict)
            {
                _dict.Clear();
            }
        }

        public virtual void Remove(Object key)
        {
            if (ContainsSpecialKey(key) || (_dict == null))
            {
                throw new ArgumentException(
                    Environment.GetResourceString(
                        "Argument_InvalidKey"));
            } 
            else
            {
                _dict.Remove(key);
            }
        }

        public virtual ICollection Keys
        {
            get
            {

                int len = _keys.Length;
                ICollection c = (_dict != null) ? _dict.Keys : null;
                if (c != null)
                {
                    len += c.Count;
                }

                ArrayList l = new ArrayList(len);
                for (int i = 0; i<_keys.Length; i++)
                {
                    l.Add(_keys[i]);
                }

                if (c != null)
                {
                    l.AddRange(c);
                }

                return l;
            }
        }

        public virtual ICollection Values
        {
            get
            {
                int len = _keys.Length;
                ICollection c = (_dict != null) ? _dict.Keys : null;
                if (c != null)
                {
                    len += c.Count;
                }

                ArrayList l = new ArrayList(len);

                for (int i = 0; i<_keys.Length; i++)
                {
                    l.Add(GetMessageValue(i));
                }

                if (c != null)
                {
                    l.AddRange(c);
                }
                return l;
            }
        }

        public virtual int Count
        {
            get
            {
                if (_dict != null)
                {
                    return _dict.Count+_keys.Length;
                }
                else
                {
                    return _keys.Length;
                }
            }
        }

    }

    //+================================================================================
    //
    // Synopsis:   Dictionary enumerator for helper class
    //
    //-================================================================================
    internal class MessageDictionaryEnumerator : IDictionaryEnumerator
    {
        private int i=-1;
        private IDictionaryEnumerator _enumHash;
        private MessageDictionary    _md;


        public MessageDictionaryEnumerator(MessageDictionary md, IDictionary hashtable)
        {
            _md = md;
            if (hashtable != null)
            {
                _enumHash = hashtable.GetEnumerator();
            }
            else
            {
                _enumHash = null;
            }
        }
        // Returns the key of the current element of the enumeration. The returned
        // value is undefined before the first call to GetNext and following
        // a call to GetNext that returned false. Multiple calls to
        // GetKey with no intervening calls to GetNext will return
        // the same object.
        //
        public Object Key {
            get {
                if (i < 0)
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_InternalState"));
                }
                if (i < _md._keys.Length)
                {
                    return _md._keys[i];
                }
                else
                {
                    Contract.Assert(_enumHash != null,"_enumHash != null");
                    return _enumHash.Key;
                }
            }
        }

        // Returns the value of the current element of the enumeration. The
        // returned value is undefined before the first call to GetNext and
        // following a call to GetNext that returned false. Multiple calls
        // to GetValue with no intervening calls to GetNext will
        // return the same object.
        //
        public Object Value {
            get {
                if (i < 0)
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_InternalState"));
                }

                if (i < _md._keys.Length)
                {
                    return _md.GetMessageValue(i);
                }
                else
                {
                    Contract.Assert(_enumHash != null,"_enumHash != null");
                    return _enumHash.Value;
                }
            }
        }

        // Advances the enumerator to the next element of the enumeration and
        // returns a boolean indicating whether an element is available. Upon
        // creation, an enumerator is conceptually positioned before the first
        // element of the enumeration, and the first call to GetNext brings
        // the first element of the enumeration into view.
        //
        public bool MoveNext()
        {
            if (i == -2)
            {
                throw new InvalidOperationException(
                    Environment.GetResourceString(
                        "InvalidOperation_InternalState"));
            }
            i++;
            if (i < _md._keys.Length)
            {
                return true;
            }
            else
            {
                if (_enumHash != null && _enumHash.MoveNext())
                {
                    return true;
                }
                else
                {
                    i = -2;
                    return false;
                }
            }
        }

        // Returns the current element of the enumeration. The returned value is
        // undefined before the first call to MoveNext and following a call
        // to MoveNext that returned false. Multiple calls to
        // Current with no intervening calls to MoveNext will return
        // the same object.
        //
        public Object Current {
            get {
                return Entry;
            }
        }

        public DictionaryEntry Entry {
            get {
                return new DictionaryEntry(Key, Value);
            }
        }

        // Resets the enumerator, positioning it before the first element.  If an
        // Enumerator doesn't support Reset, a NotSupportedException is
        // thrown.
        public void Reset()
        {
            i = -1;
            if (_enumHash != null)
            {
                _enumHash.Reset();
            }
        }
    }
}
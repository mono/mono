// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.
// See the LICENSE file in the project root for more information.

namespace System.Xaml
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.Specialized;
    using System.Xaml.MS.Impl;
    using System.Windows.Markup;

    //
    // The implementation for this class is taken directly from the source of NameScope, including the use
    // of HybridDictionary to match the performance semantics of 3.0 for the time being 
    // Note that the IEnumerable<T> uses KeyValuePair<string, object>
    // This means that we need to create KeyValuePairs on the fly
    // The other option would be to just use IEnumerable (or change the HybridDictionary to Dictionary<K,V>)
    // but I opted for generic usability for now since this shouldn't be a common hot path.
    internal class NameScopeDictionary : INameScopeDictionary
    {
        private HybridDictionary _nameMap;
        private INameScope _underlyingNameScope;
        private FrugalObjectList<string> _names;
        
        public NameScopeDictionary()
        {
        }

        public NameScopeDictionary(INameScope underlyingNameScope)
        {
            if (underlyingNameScope == null)
            {
                throw new ArgumentNullException("underlyingNameScope");
            }

            _names = new FrugalObjectList<string>();
            _underlyingNameScope = underlyingNameScope;
        }

        public void RegisterName(string name, object scopedElement)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (scopedElement == null)
                throw new ArgumentNullException("scopedElement");

            if (name == String.Empty)
                throw new ArgumentException(SR.Get(SRID.NameScopeNameNotEmptyString));

            if (!NameValidationHelper.IsValidIdentifierName(name))
            {
                throw new ArgumentException(SR.Get(SRID.NameScopeInvalidIdentifierName, name));
            }

            if (_underlyingNameScope != null)
            {
                _names.Add(name);
                _underlyingNameScope.RegisterName(name, scopedElement);
            }
            else
            {
                if (_nameMap == null)
                {
                    _nameMap = new HybridDictionary();
                    _nameMap[name] = scopedElement;
                }
                else
                {
                    object nameContext = _nameMap[name];

                    if (nameContext == null)
                    {
                        _nameMap[name] = scopedElement;
                    }
                    else if (scopedElement != nameContext)
                    {
                        throw new ArgumentException(SR.Get(SRID.NameScopeDuplicateNamesNotAllowed, name));
                    }
                }
            }
        }

        public void UnregisterName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name == String.Empty)
                throw new ArgumentException(SR.Get(SRID.NameScopeNameNotEmptyString));

            if (_underlyingNameScope != null)
            {
                _underlyingNameScope.UnregisterName(name);
                _names.Remove(name);
            }
            else
            {
                if (_nameMap != null && _nameMap[name] != null)
                {
                    _nameMap.Remove(name);
                }
                else
                {
                    throw new ArgumentException(SR.Get(SRID.NameScopeNameNotFound, name));
                }
            }
        }

        public object FindName(string name)
        {
            if (name == null)
                throw new ArgumentNullException("name");

            if (name == String.Empty)
                throw new ArgumentException(SR.Get(SRID.NameScopeNameNotEmptyString));

            if (_underlyingNameScope != null)
            {
                return _underlyingNameScope.FindName(name);
            }
            else
            {
                if (_nameMap == null)
                {
                    return null;
                }
                return _nameMap[name];
            }
        }

        internal INameScope UnderlyingNameScope { get { return _underlyingNameScope; } }

        class Enumerator : IEnumerator<KeyValuePair<string, object>>
        {
            int index;
            IDictionaryEnumerator dictionaryEnumerator;
            HybridDictionary _nameMap;
            INameScope _underlyingNameScope;
            FrugalObjectList<string> _names;
            
            public Enumerator(NameScopeDictionary nameScopeDictionary)
            {
                _nameMap = nameScopeDictionary._nameMap;
                _underlyingNameScope = nameScopeDictionary._underlyingNameScope;
                _names = nameScopeDictionary._names;

                if (_underlyingNameScope != null)
                {
                    index = -1;
                }
                else
                {
                    if (_nameMap != null)
                    {
                        dictionaryEnumerator = _nameMap.GetEnumerator();
                    }
                }
            }

            public void Dispose()
            {
                GC.SuppressFinalize(this);
            }

            public KeyValuePair<string, object> Current
            {
                get
                {
                    if (_underlyingNameScope != null)
                    {
                        string name = _names[index];
                        return new KeyValuePair<string,object>(name, _underlyingNameScope.FindName(name));
                    }
                    else
                    {
                        if (_nameMap != null)
                        {
                            return new KeyValuePair<string, object>((string)dictionaryEnumerator.Key, dictionaryEnumerator.Value);
                        }

                        return default(KeyValuePair<string, object>);
                    }
                }
            }

            public bool MoveNext()
            {
                if (_underlyingNameScope != null)
                {
                    if (index == _names.Count - 1)
                    {
                        return false;
                    }

                    index++;
                    return true;
                }
                else
                {
                    if (_nameMap != null)
                    {
                        return dictionaryEnumerator.MoveNext();
                    }

                    return false;
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return this.Current;
                }
            }

            void IEnumerator.Reset()
            {
                if (_underlyingNameScope != null)
                {
                    index = -1;
                }
                else
                {
                    dictionaryEnumerator.Reset();
                }
            }
        }

        IEnumerator<KeyValuePair<string, object>> GetEnumerator()
        {
            return new Enumerator(this);
        }

        #region IEnumerable methods
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
        #endregion

        #region IEnumerable<KeyValuePair<string, object> methods
        IEnumerator<KeyValuePair<string, object>> IEnumerable<KeyValuePair<string, object>>.GetEnumerator()
        {
            return new Enumerator(this);
        }
        #endregion

        #region ICollection<KeyValuePair<string, object> methods
        int ICollection<KeyValuePair<string, object>>.Count
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        bool ICollection<KeyValuePair<string, object>>.IsReadOnly
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        void ICollection<KeyValuePair<string, object>>.Clear()
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.CopyTo(KeyValuePair<string, object>[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Remove(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        void ICollection<KeyValuePair<string, object>>.Add(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        bool ICollection<KeyValuePair<string, object>>.Contains(KeyValuePair<string, object> item)
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IDictionary<string, object> methods
        object IDictionary<string, object>.this[string key]
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

        void IDictionary<string, object>.Add(string key, object value)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.ContainsKey(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.Remove(string key)
        {
            throw new NotImplementedException();
        }

        bool IDictionary<string, object>.TryGetValue(string key, out object value)
        {
            throw new NotImplementedException();
        }

        ICollection<string> IDictionary<string, object>.Keys
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        ICollection<object> IDictionary<string, object>.Values
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        #endregion
    }
}

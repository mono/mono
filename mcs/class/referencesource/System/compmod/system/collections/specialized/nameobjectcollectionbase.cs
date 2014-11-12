//------------------------------------------------------------------------------
// <copyright file="NameObjectCollectionBase.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Ordered String/Object collection of name/value pairs with support for null key
 *
 * This class is intended to be used as a base class
 * 
 * Copyright (c) 2000 Microsoft Corporation
 */

namespace System.Collections.Specialized {

    using Microsoft.Win32;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Runtime.Serialization;
    using System.Security.Permissions;
    
    
    /// <devdoc>
    /// <para>Provides the <see langword='abstract '/>base class for a sorted collection of associated <see cref='System.String' qualify='true'/> keys 
    ///    and <see cref='System.Object' qualify='true'/> values that can be accessed either with the hash code of
    ///    the key or with the index.</para>
    /// </devdoc>
    [Serializable()]
    public abstract class NameObjectCollectionBase : ICollection, ISerializable, IDeserializationCallback {
        // const names used for serialization
        private const String ReadOnlyName = "ReadOnly";
        private const String CountName = "Count";
        private const String ComparerName = "Comparer";
        private const String HashCodeProviderName = "HashProvider";
        private const String KeysName = "Keys";
        private const String ValuesName = "Values";
        private const String KeyComparerName = "KeyComparer";
        private const String VersionName = "Version";        

        private bool _readOnly = false;
        private ArrayList _entriesArray;                
        private IEqualityComparer _keyComparer;
        private volatile Hashtable _entriesTable;
        private volatile NameObjectEntry _nullKeyEntry;
        private KeysCollection _keys;
        private SerializationInfo _serializationInfo;
        private int _version;
        [NonSerialized]
        private Object _syncRoot;

        private static StringComparer defaultComparer = StringComparer.InvariantCultureIgnoreCase;

        /// <devdoc>
        /// <para> Creates an empty <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance with the default initial capacity and using the default case-insensitive hash
        ///    code provider and the default case-insensitive comparer.</para>
        /// </devdoc>
        protected NameObjectCollectionBase() :this(defaultComparer) {
        }
    
        protected NameObjectCollectionBase(IEqualityComparer equalityComparer) 
        {
           _keyComparer = (equalityComparer == null) ? defaultComparer : equalityComparer;            
           Reset();
        }

        protected NameObjectCollectionBase(Int32 capacity, IEqualityComparer equalityComparer) : this (equalityComparer)
        {
            Reset(capacity);            
        }


        /// <devdoc>
        /// <para>Creates an empty <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance with 
        ///    the default initial capacity and using the specified case-insensitive hash code provider and the
        ///    specified case-insensitive comparer.</para>
        /// </devdoc>
        
#pragma warning disable 618
        [Obsolete("Please use NameObjectCollectionBase(IEqualityComparer) instead.")]
        protected NameObjectCollectionBase(IHashCodeProvider hashProvider, IComparer comparer) {
            _keyComparer = new CompatibleComparer( comparer, hashProvider); 
            Reset();
        }
#pragma warning restore 618
    
        /// <devdoc>
        /// <para>Creates an empty <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance with the specified 
        ///    initial capacity and using the specified case-insensitive hash code provider
        ///    and the specified case-insensitive comparer.</para>
        /// </devdoc>
#pragma warning disable 618
        [Obsolete("Please use NameObjectCollectionBase(Int32, IEqualityComparer) instead.")]
        protected NameObjectCollectionBase(int capacity, IHashCodeProvider hashProvider, IComparer comparer) {
            _keyComparer = new CompatibleComparer( comparer, hashProvider); 
            Reset(capacity);
        }
#pragma warning restore 618
    
        /// <devdoc>
        /// <para>Creates an empty <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance with the specified 
        ///    initial capacity and using the default case-insensitive hash code provider
        ///    and the default case-insensitive comparer.</para>
        /// </devdoc>
        protected NameObjectCollectionBase(int capacity) {
            _keyComparer = StringComparer.InvariantCultureIgnoreCase;
            Reset(capacity);
        }

        // Allow internal extenders to avoid creating the hashtable/arraylist.
        internal NameObjectCollectionBase(DBNull dummy)
        {
        }

        //
        // Serialization support
        //

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        protected NameObjectCollectionBase(SerializationInfo info, StreamingContext context) {
            _serializationInfo = info; 
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags=SecurityPermissionFlag.SerializationFormatter)] 		
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            if (info == null)
                throw new ArgumentNullException("info");

            info.AddValue(ReadOnlyName, _readOnly);
            
#pragma warning disable 618
            // Maintain backward serialization compatibility if new APIs are not used.
            if( _keyComparer == defaultComparer) {
                info.AddValue(HashCodeProviderName, CompatibleComparer.DefaultHashCodeProvider, typeof(IHashCodeProvider));
                info.AddValue(ComparerName, CompatibleComparer.DefaultComparer, typeof(IComparer));
            }
            else if(_keyComparer == null) {
                info.AddValue(HashCodeProviderName, null, typeof(IHashCodeProvider));
                info.AddValue(ComparerName, null, typeof(IComparer));
            }
            else if(_keyComparer is CompatibleComparer) {
                CompatibleComparer c = (CompatibleComparer)_keyComparer;
                info.AddValue(HashCodeProviderName, c.HashCodeProvider, typeof(IHashCodeProvider));
                info.AddValue(ComparerName, c.Comparer, typeof(IComparer));
            }
            else {
                info.AddValue(KeyComparerName, _keyComparer, typeof(IEqualityComparer));
            }
#pragma warning restore 618

            int count = _entriesArray.Count;
            info.AddValue(CountName, count);

            String[] keys = new String[count];
            Object[] values = new Object[count];

            for (int i = 0; i < count; i++) {
                NameObjectEntry entry = (NameObjectEntry)_entriesArray[i];
                keys[i] = entry.Key;
                values[i] = entry.Value;
            }

            info.AddValue(KeysName, keys, typeof(String[]));
            info.AddValue(ValuesName, values, typeof(Object[]));
            info.AddValue(VersionName, _version);
        }

        /// <devdoc>
        ///    <para>[To be supplied.]</para>
        /// </devdoc>
        public virtual void OnDeserialization(Object sender) {
            if (_keyComparer != null) {
                return;//Somebody had a dependency on this hashtable and fixed us up before the ObjectManager got to it.
            }

            if (_serializationInfo == null)
                throw new SerializationException();

            SerializationInfo info = _serializationInfo;
            _serializationInfo = null;
            
            bool readOnly = false; 
            int count = 0;
            String[] keys = null;
            Object[] values = null;
#pragma warning disable 618
            IHashCodeProvider hashProvider = null;
#pragma warning restore 618
            IComparer comparer = null;
            bool hasVersion = false;
            int serializedVersion = 0;
            
            SerializationInfoEnumerator enumerator = info.GetEnumerator();
            while( enumerator.MoveNext()) 
            {
                switch( enumerator.Name) 
                {
                    case ReadOnlyName:
                        readOnly = info.GetBoolean(ReadOnlyName);;
                        break;
                    case HashCodeProviderName:
#pragma warning disable 618
                        hashProvider = (IHashCodeProvider)info.GetValue(HashCodeProviderName, typeof(IHashCodeProvider));;
#pragma warning restore 618
                        break;
                    case ComparerName:
                        comparer = (IComparer)info.GetValue(ComparerName, typeof(IComparer));
                        break;
                    case KeyComparerName: 
                        _keyComparer = (IEqualityComparer)info.GetValue(KeyComparerName, typeof(IEqualityComparer));
                        break;
                    case CountName:
                        count = info.GetInt32(CountName);
                        break;
                    case KeysName:
                        keys = (String[])info.GetValue(KeysName, typeof(String[]));
                        break;
                    case ValuesName:
                        values = (Object[])info.GetValue(ValuesName, typeof(Object[]));
                        break;
                    case VersionName:
                        hasVersion = true;
                        serializedVersion = info.GetInt32(VersionName);
                        break;                    
                }
            }

            if( _keyComparer == null) {
                if(comparer == null || hashProvider == null) 
                {
                    throw new SerializationException();
                }
                else {
                    // create a new key comparer for V1 Object    
                    _keyComparer = new CompatibleComparer(comparer, hashProvider);
                }
            }
            
            if ( keys == null || values == null)
                throw new SerializationException();

            Reset(count);

            for (int i = 0; i < count; i++)
                BaseAdd(keys[i], values[i]);
    
            _readOnly = readOnly;  // after collection populated
            if(hasVersion) {
                _version = serializedVersion;
            }
        }

        //
        // Private helpers
        //
    
        private void Reset() {
            _entriesArray = new ArrayList();
            _entriesTable = new Hashtable(_keyComparer);
            _nullKeyEntry = null;
            _version++;
        }
    
        private void Reset(int capacity) {
            _entriesArray = new ArrayList(capacity);
            _entriesTable = new Hashtable(capacity, _keyComparer);
            _nullKeyEntry = null;
            _version++;
        }
    
        private NameObjectEntry FindEntry(String key) {
            if (key != null)
                return (NameObjectEntry)_entriesTable[key];
            else
                return _nullKeyEntry;
        }
            
        internal IEqualityComparer Comparer
        {
            get
            {
                return _keyComparer;
            }
            set
            {
                _keyComparer = value;
            }

        }

    
        /// <devdoc>
        /// <para>Gets or sets a value indicating whether the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance is read-only.</para>
        /// </devdoc>
        protected bool IsReadOnly {
            get { return _readOnly; }
            set { _readOnly = value; }
        }
    
        /// <devdoc>
        /// <para>Gets a value indicating whether the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance contains entries whose 
        ///    keys are not <see langword='null'/>.</para>
        /// </devdoc>
        protected bool BaseHasKeys() {
            return (_entriesTable.Count > 0);  // any entries with keys?
        }
    
        //
        // Methods to add / remove entries
        //
    
        /// <devdoc>
        ///    <para>Adds an entry with the specified key and value into the 
        ///    <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected void BaseAdd(String name, Object value) {
            if (_readOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));
    
            NameObjectEntry entry = new NameObjectEntry(name, value);
    
            // insert entry into hashtable
            if (name != null) {
                if (_entriesTable[name] == null)
                    _entriesTable.Add(name, entry);
            }
            else { // null key -- special case -- hashtable doesn't like null keys
                if (_nullKeyEntry == null)
                    _nullKeyEntry = entry;
            }
    
            // add entry to the list
            _entriesArray.Add(entry);

            _version++;
        }
    
        /// <devdoc>
        ///    <para>Removes the entries with the specified key from the 
        ///    <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected void BaseRemove(String name) {
            if (_readOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));
    
            if (name != null) {
                // remove from hashtable
                _entriesTable.Remove(name);
    
                // remove from array
                for (int i = _entriesArray.Count-1; i >= 0; i--) {
                    if (_keyComparer.Equals(name, BaseGetKey(i)))
                        _entriesArray.RemoveAt(i);
                }
            }
            else { // null key -- special case
                // null out special 'null key' entry
                _nullKeyEntry = null;
    
                // remove from array
                for (int i = _entriesArray.Count-1; i >= 0; i--) {
                    if (BaseGetKey(i) == null)
                        _entriesArray.RemoveAt(i);
                }
            }

            _version++;
        }
    
        /// <devdoc>
        ///    <para> Removes the entry at the specified index of the 
        ///    <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected void BaseRemoveAt(int index) {
            if (_readOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));
    
            String key = BaseGetKey(index);
    
            if (key != null) {
                // remove from hashtable
                _entriesTable.Remove(key);
            } 
            else { // null key -- special case
                // null out special 'null key' entry
                _nullKeyEntry = null;
            }
    
            // remove from array
            _entriesArray.RemoveAt(index);

            _version++;
        }
    
        /// <devdoc>
        /// <para>Removes all entries from the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected void BaseClear() {
            if (_readOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));
    
            Reset();
        }
    
        //
        // Access by name
        //
    
        /// <devdoc>
        ///    <para>Gets the value of the first entry with the specified key from 
        ///       the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected Object BaseGet(String name) {
            NameObjectEntry e = FindEntry(name);
            return (e != null) ? e.Value : null;
        }
    
        /// <devdoc>
        /// <para>Sets the value of the first entry with the specified key in the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> 
        /// instance, if found; otherwise, adds an entry with the specified key and value
        /// into the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/>
        /// instance.</para>
        /// </devdoc>
        protected void BaseSet(String name, Object value) {
            if (_readOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));
    
            NameObjectEntry entry = FindEntry(name);
            if (entry != null) {
                entry.Value = value;
                _version++;
            }
            else {
                BaseAdd(name, value);
            }
        }
    
        //
        // Access by index
        //
    
        /// <devdoc>
        ///    <para>Gets the value of the entry at the specified index of 
        ///       the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected Object BaseGet(int index) {
            NameObjectEntry entry = (NameObjectEntry)_entriesArray[index];
            return entry.Value;
        }
    
        /// <devdoc>
        ///    <para>Gets the key of the entry at the specified index of the 
        ///    <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> 
        ///    instance.</para>
        /// </devdoc>
        protected String BaseGetKey(int index) {
            NameObjectEntry entry = (NameObjectEntry)_entriesArray[index];
            return entry.Key;
        }
    
        /// <devdoc>
        ///    <para>Sets the value of the entry at the specified index of 
        ///       the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected void BaseSet(int index, Object value) {
            if (_readOnly)
                throw new NotSupportedException(SR.GetString(SR.CollectionReadOnly));

            NameObjectEntry entry = (NameObjectEntry)_entriesArray[index];
            entry.Value = value;
            _version++;
        }
    
        //
        // ICollection implementation
        //
    
        /// <devdoc>
        /// <para>Returns an enumerator that can iterate through the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/>.</para>
        /// </devdoc>
        public virtual IEnumerator GetEnumerator() {
            return new NameObjectKeysEnumerator(this);
        }
    
        /// <devdoc>
        /// <para>Gets the number of key-and-value pairs in the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        public virtual int Count {
            get {
                return _entriesArray.Count;
            }
        }
            
        void ICollection.CopyTo(Array array, int index) {
            if (array==null) {
                throw new ArgumentNullException("array");
            }

            if (array.Rank != 1) {
                throw new ArgumentException(SR.GetString(SR.Arg_MultiRank));
            }

            if (index < 0) {
                throw new ArgumentOutOfRangeException("index",SR.GetString(SR.IndexOutOfRange, index.ToString(CultureInfo.CurrentCulture)) );
            }

            if (array.Length - index < _entriesArray.Count) {
                throw new ArgumentException(SR.GetString(SR.Arg_InsufficientSpace));
            }

            for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                array.SetValue(e.Current, index++);
        }
    
        Object ICollection.SyncRoot {
            get { 
                if( _syncRoot == null) {
                    System.Threading.Interlocked.CompareExchange(ref _syncRoot, new Object(), null);    
                }
                return _syncRoot; 
            }
        }
        
        bool ICollection.IsSynchronized {
            get { return false; }
        }
    
        //
        //  Helper methods to get arrays of keys and values
        //
    
        /// <devdoc>
        /// <para>Returns a <see cref='System.String' qualify='true'/> array containing all the keys in the 
        /// <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected String[] BaseGetAllKeys() {
            int n = _entriesArray.Count;
            String[] allKeys = new String[n];
    
            for (int i = 0; i < n; i++)
                allKeys[i] = BaseGetKey(i);
    
            return allKeys;
        }
    
        /// <devdoc>
        /// <para>Returns an <see cref='System.Object' qualify='true'/> array containing all the values in the 
        /// <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected Object[] BaseGetAllValues() {
            int n = _entriesArray.Count;
            Object[] allValues = new Object[n];
    
            for (int i = 0; i < n; i++)
                allValues[i] = BaseGet(i);
    
            return allValues;
        }
    
        /// <devdoc>
        ///    <para>Returns an array of the specified type containing 
        ///       all the values in the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        protected object[] BaseGetAllValues(Type type) {
            int n = _entriesArray.Count;
            if (type == null) {
                throw new ArgumentNullException("type"); 
            }
            object[] allValues = (object[]) SecurityUtils.ArrayCreateInstance(type, n);
    
            for (int i = 0; i < n; i++) {
                allValues[i] = BaseGet(i);
            }
    
            return allValues;
        }
    
        //
        // Keys propetry
        //
    
        /// <devdoc>
        /// <para>Returns a <see cref='System.Collections.Specialized.NameObjectCollectionBase.KeysCollection'/> instance containing 
        ///    all the keys in the <see cref='System.Collections.Specialized.NameObjectCollectionBase'/> instance.</para>
        /// </devdoc>
        public virtual KeysCollection Keys {
            get {
                if (_keys == null)
                    _keys = new KeysCollection(this);
                return _keys;
            }
        }
    
        //
        // Simple entry class to allow substitution of values and indexed access to keys
        //
    
        internal class NameObjectEntry {
    
            internal NameObjectEntry(String name, Object value) {
                Key = name;
                Value = value;
            }
    
            internal String Key;
            internal Object Value;
        }
    
        //
        // Enumerator over keys of NameObjectCollection
        //
    
        [Serializable()]
        internal class NameObjectKeysEnumerator : IEnumerator {
            private int _pos;
            private NameObjectCollectionBase _coll;
            private int _version;

            internal NameObjectKeysEnumerator(NameObjectCollectionBase coll) {
                _coll = coll;
                _version = _coll._version;
                _pos = -1;
            }
    
            public bool MoveNext() {
                if ( _version != _coll._version)
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumFailedVersion));
                
                if( _pos < _coll.Count - 1) {
                    _pos++;
                    return true;
                }
                else {
                    _pos = _coll.Count;
                    return false;
                }

            }
    
            public void Reset() {
                if (_version != _coll._version)
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumFailedVersion));
                _pos = -1;
            }
    
            public Object Current {
                get {
                    if(_pos >= 0 && _pos < _coll.Count) {
                        return _coll.BaseGetKey(_pos);                        
                    }
                    else {
                        throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumOpCantHappen));
                    }
                }
            }
        }
    
        //
        // Keys collection
        //
    
        /// <devdoc>
        /// <para>Represents a collection of the <see cref='System.String' qualify='true'/> keys of a collection.</para>
        /// </devdoc>
        [Serializable()]
        public class KeysCollection : ICollection {
    
            private NameObjectCollectionBase _coll;
    
            internal KeysCollection(NameObjectCollectionBase coll) {
                _coll = coll;
            }
    
            // Indexed access
    
            /// <devdoc>
            ///    <para> Gets the key at the specified index of the collection.</para>
            /// </devdoc>
            public virtual String Get(int index) {
                return _coll.BaseGetKey(index);
            }
    
            /// <devdoc>
            ///    <para>Represents the entry at the specified index of the collection.</para>
            /// </devdoc>
            public String this[int index] {
                get {
                    return Get(index);
                }
            }
    
            // ICollection implementation
    
            /// <devdoc>
            ///    <para>Returns an enumerator that can iterate through the 
            ///    <see cref='System.Collections.Specialized.NameObjectCollectionBase.KeysCollection'/>.</para>
            /// </devdoc>
            public IEnumerator GetEnumerator() {
                return new NameObjectKeysEnumerator(_coll);
            }
    
            /// <devdoc>
            /// <para>Gets the number of keys in the <see cref='System.Collections.Specialized.NameObjectCollectionBase.KeysCollection'/>.</para>
            /// </devdoc>
            public int Count {
                get {
                    return _coll.Count;
                }
            }
    
            void ICollection.CopyTo(Array array, int index) {
                if (array==null) {
                    throw new ArgumentNullException("array");
                }

                if (array.Rank != 1) {
                    throw new ArgumentException(SR.GetString(SR.Arg_MultiRank));
                }

                if (index < 0) {
                    throw new ArgumentOutOfRangeException("index",SR.GetString(SR.IndexOutOfRange, index.ToString(CultureInfo.CurrentCulture)) );
                }

                if (array.Length - index < _coll.Count) {
                    throw new ArgumentException(SR.GetString(SR.Arg_InsufficientSpace));
                }

                for (IEnumerator e = this.GetEnumerator(); e.MoveNext();)
                    array.SetValue(e.Current, index++);
            }
    
            Object ICollection.SyncRoot {
                get { return ((ICollection)_coll).SyncRoot; }
            }
    
             
            bool ICollection.IsSynchronized {
                get { return false; }
            }
        }
    }

    [Serializable()]        
    internal class CompatibleComparer: IEqualityComparer  {
        IComparer _comparer;
        static volatile IComparer defaultComparer;
        // Needed for compatability
#pragma warning disable 618
        IHashCodeProvider _hcp;

        static volatile IHashCodeProvider defaultHashProvider;

        internal CompatibleComparer(IComparer comparer, IHashCodeProvider hashCodeProvider) {
            _comparer = comparer;
            _hcp = hashCodeProvider;
        }
#pragma warning restore 618

        public new bool Equals(Object a, Object b) {
            if (a == b) return true;
            if (a == null || b == null) return false;
            
            // We must call Compare or CompareTo method
            // to make sure everything is fine, but the 
            // guideline is that Equals should never throw.
            // So we need to ignore ArgumentException (note this
            // is the exception we should get if two objects are not
            // comparable.)

            try {
                if (_comparer != null)
                    return (_comparer.Compare(a,b) == 0);

                IComparable ia = a as IComparable;
                if (ia != null)
                    return (ia.CompareTo(b) ==0);
            }
            catch(ArgumentException) {
                return false;
            }

            return a.Equals(b);
        }

        public int GetHashCode(Object obj) {
            if( obj == null) {
                throw new ArgumentNullException("obj");
            }

            if (_hcp != null)
                return _hcp.GetHashCode(obj);
            return obj.GetHashCode();
        }

        public IComparer Comparer {
            get {
                return _comparer;
            }
        }

#pragma warning disable 618
        public IHashCodeProvider HashCodeProvider {
            get {
                return _hcp;
            }
        }
#pragma warning restore 618

        public static IComparer DefaultComparer {
            get {
                if( defaultComparer == null) {
                    defaultComparer = new CaseInsensitiveComparer(CultureInfo.InvariantCulture);
                }
                return defaultComparer;
            }
        }

#pragma warning disable 618
        public static IHashCodeProvider DefaultHashCodeProvider {
            get {
                if( defaultHashProvider == null) {
                    defaultHashProvider = new CaseInsensitiveHashCodeProvider(CultureInfo.InvariantCulture);
                }
                return defaultHashProvider;
            }
        }
#pragma warning restore 618
    }
}

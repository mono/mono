namespace System.Collections.Generic {
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;
       
#if !FEATURE_NETCORE
    [Serializable]
#endif
    [DebuggerTypeProxy(typeof(System_DictionaryDebugView<,>))]
    [DebuggerDisplay("Count = {Count}")]        
    public class SortedDictionary<TKey, TValue> : IDictionary<TKey, TValue>, IDictionary {
#if !FEATURE_NETCORE
        [NonSerialized]
#endif
        private KeyCollection keys;

#if !FEATURE_NETCORE
        [NonSerialized]
#endif
        private ValueCollection values;

        private TreeSet<KeyValuePair<TKey, TValue>> _set;

        public SortedDictionary() : this((IComparer<TKey>)null) {
        }

        public SortedDictionary(IDictionary<TKey,TValue> dictionary) : this( dictionary, null) {
        }

        public SortedDictionary(IDictionary<TKey,TValue> dictionary, IComparer<TKey> comparer) {
            if( dictionary == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
            }

            _set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(comparer));

            foreach(KeyValuePair<TKey, TValue> pair in dictionary) {
                _set.Add(pair);
            }            
        }

        public SortedDictionary(IComparer<TKey> comparer) {
            _set = new TreeSet<KeyValuePair<TKey, TValue>>(new KeyValuePairComparer(comparer));
        }

        void ICollection<KeyValuePair<TKey,TValue>>.Add(KeyValuePair<TKey,TValue> keyValuePair) {
            _set.Add(keyValuePair);
        }

        bool ICollection<KeyValuePair<TKey,TValue>>.Contains(KeyValuePair<TKey,TValue> keyValuePair) {        
            TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(keyValuePair);
            if ( node == null) {
                return false;
            }

            if( keyValuePair.Value == null) {
                return node.Item.Value == null;
            }
            else {
                return EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value);
            }            
        }

        bool ICollection<KeyValuePair<TKey,TValue>>.Remove(KeyValuePair<TKey,TValue> keyValuePair) {
            TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(keyValuePair);
            if ( node == null) {
                return false;
            }

            if( EqualityComparer<TValue>.Default.Equals(node.Item.Value, keyValuePair.Value)) {
                _set.Remove(keyValuePair);
                return true;
            }
            return false;
        }

        bool ICollection<KeyValuePair<TKey,TValue>>.IsReadOnly {
            get {
                return false;
            }
        }

        public TValue this[TKey key] {
            get {
                if ( key == null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);                    
                }

                TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
                if ( node == null) {
                    ThrowHelper.ThrowKeyNotFoundException();                    
                }

                return node.Item.Value;
            }
            set {
                if( key == null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
                }
            
                TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));
                if ( node == null) {
                    _set.Add(new KeyValuePair<TKey, TValue>(key, value));                        
                } else {
                    node.Item = new KeyValuePair<TKey, TValue>( node.Item.Key, value);
                    _set.UpdateVersion();
                }
            }
        }

        public int Count {
            get {
                return _set.Count;    
            }
        }

        public IComparer<TKey> Comparer {
            get {
                return ((KeyValuePairComparer)_set.Comparer).keyComparer;
            }
        }

        public KeyCollection Keys {
            get {
                if (keys == null) keys = new KeyCollection(this);
                return keys;
            }
        }

        ICollection<TKey> IDictionary<TKey, TValue>.Keys {
            get {                
                return Keys;
            }
        }

        public ValueCollection Values {
            get {
                if (values == null) values = new ValueCollection(this);
                return values;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values {
            get {                
                return Values;
            }
        }
        
        public void Add(TKey key, TValue value) {
            if( key == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
            _set.Add(new KeyValuePair<TKey, TValue>(key, value));
        }

        public void Clear() {
            _set.Clear();
        }

        public bool ContainsKey(TKey key) {
            if( key == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
        
            return _set.Contains(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        public bool ContainsValue(TValue value) {
            bool found = false;
            if( value == null) {
                _set.InOrderTreeWalk( delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) { 
                                     if(node.Item.Value == null) {
                                        found = true;
                                        return false;  // stop the walk
                                     }
                                     return true;
                                });
                
            }
            else {
                EqualityComparer<TValue> valueComparer = EqualityComparer<TValue>.Default;
                _set.InOrderTreeWalk( delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node) { 
                                     if(valueComparer.Equals(node.Item.Value, value)) {
                                        found = true;
                                        return false;  // stop the walk
                                     }
                                     return true;
                                });
            }
            return found;
        }

        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int index) {
            _set.CopyTo(array, index);
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        IEnumerator<KeyValuePair<TKey,TValue>> IEnumerable<KeyValuePair<TKey,TValue>>.GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        public bool Remove(TKey key) {
            if( key == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
        
            return _set.Remove(new KeyValuePair<TKey, TValue>(key, default(TValue)));
        }

        public bool TryGetValue( TKey key, out TValue value) {
            if( key == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);
            }
        
            TreeSet<KeyValuePair<TKey, TValue>>.Node node = _set.FindNode(new KeyValuePair<TKey, TValue>(key, default(TValue)));                
            if ( node == null) {
                value = default(TValue);                    
                return false;
            }
            value = node.Item.Value;
            return true;
        }

        void ICollection.CopyTo(Array array, int index) {
            ((ICollection)_set).CopyTo(array, index);
        }

        bool IDictionary.IsFixedSize {
            get { return false; }
        }

        bool IDictionary.IsReadOnly {
            get { return false; }
        }

        ICollection IDictionary.Keys {
            get { return (ICollection)Keys; }
        }
    
        ICollection IDictionary.Values {
            get { return (ICollection)Values; }
        }
    
        object IDictionary.this[object key] {
            get { 
                if( IsCompatibleKey(key)) {               
                    TValue value;
                    if( TryGetValue((TKey)key, out value)) {
                        return value;
                    }
                }
                
                return null;
            }
            set { 
                if (key == null)
                {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);                          
                }

                ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

                try {
                    TKey tempKey = (TKey)key;
                    try {
                        this[tempKey] = (TValue)value; 
                    }
                    catch (InvalidCastException) { 
                        ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));   
                    }
                }
                catch (InvalidCastException) { 
                    ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
                }
            }
        }

        void IDictionary.Add(object key, object value) {
            if (key == null)
            {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);                          
            }

            ThrowHelper.IfNullAndNullsAreIllegalThenThrow<TValue>(value, ExceptionArgument.value);

            try {
                TKey tempKey = (TKey)key;

                try {
                    Add(tempKey, (TValue)value);
                }
                catch (InvalidCastException) { 
                    ThrowHelper.ThrowWrongValueTypeArgumentException(value, typeof(TValue));   
                }
            }
            catch (InvalidCastException) { 
                ThrowHelper.ThrowWrongKeyTypeArgumentException(key, typeof(TKey));
            }
        }
    
        bool IDictionary.Contains(object key) {
            if(IsCompatibleKey(key)) {
                return ContainsKey((TKey)key);
            }
            return false;
        }

        private static bool IsCompatibleKey(object key) {
            if( key == null) {
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.key);                
            }
            
            return (key is TKey); 
        }

        IDictionaryEnumerator IDictionary.GetEnumerator() {
            return new Enumerator(this, Enumerator.DictEntry);
        }
    
        void IDictionary.Remove(object key) {
            if(IsCompatibleKey(key)) 
            {
                Remove((TKey)key);
            }
        }

        bool ICollection.IsSynchronized {
            get { return false; }
        }

        object ICollection.SyncRoot { 
            get { return ((ICollection)_set).SyncRoot; }
        }
        
        IEnumerator IEnumerable.GetEnumerator() {
            return new Enumerator(this, Enumerator.KeyValuePair);
        }

        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
        public struct Enumerator : IEnumerator<KeyValuePair<TKey,TValue>>, IDictionaryEnumerator {
            private TreeSet<KeyValuePair<TKey, TValue>>.Enumerator treeEnum; 
            private int getEnumeratorRetType;  // What should Enumerator.Current return?
            
            internal const int KeyValuePair = 1;
            internal const int DictEntry = 2;

            internal Enumerator(SortedDictionary<TKey, TValue> dictionary, int getEnumeratorRetType) {
                treeEnum = dictionary._set.GetEnumerator();
                this.getEnumeratorRetType = getEnumeratorRetType;
            }

            public bool MoveNext() {
                return treeEnum.MoveNext();
            }

            public void Dispose() {
                treeEnum.Dispose();
            }

            public KeyValuePair<TKey, TValue> Current {
                get {
                    return treeEnum.Current;
                }
            }

            internal bool NotStartedOrEnded {
                get {
                    return treeEnum.NotStartedOrEnded;
                }
            }

            internal void Reset() {
                treeEnum.Reset();
            }

            
            void IEnumerator.Reset() {
                treeEnum.Reset();
            }

            object IEnumerator.Current {
                get {
                    if( NotStartedOrEnded) {
                         ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);                        
                    }

                    if (getEnumeratorRetType == DictEntry) {
                        return new DictionaryEntry(Current.Key, Current.Value);
                    } else {
                        return new KeyValuePair<TKey, TValue>(Current.Key, Current.Value);			  
                    }		

                }
            }

            object IDictionaryEnumerator.Key {
                get {
                    if(NotStartedOrEnded) {
                         ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);                        
                    }
                    
                    return Current.Key;
                }
            }

            object IDictionaryEnumerator.Value {
                get {
                    if(NotStartedOrEnded) {
                         ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);                        
                    }
                    
                    return Current.Value;
                }
            }

            DictionaryEntry IDictionaryEnumerator.Entry {
                get {
                    if(NotStartedOrEnded) {
                         ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);                        
                    }

                    return new DictionaryEntry(Current.Key, Current.Value);
                }
            }
        }

        [DebuggerTypeProxy(typeof(System_DictionaryKeyCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
#if !FEATURE_NETCORE
        [Serializable]
#endif
        public sealed class KeyCollection: ICollection<TKey>, ICollection {
            private SortedDictionary<TKey,TValue> dictionary;

            public KeyCollection(SortedDictionary<TKey,TValue> dictionary) {
                if (dictionary == null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator() {
                return new Enumerator(dictionary);
            }
            
            IEnumerator<TKey> IEnumerable<TKey>.GetEnumerator() {
                return new Enumerator(dictionary);
            }
            
            IEnumerator IEnumerable.GetEnumerator() {                
                return new Enumerator(dictionary);                
            }

            public void CopyTo(TKey[] array, int index) {
                if (array == null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (index < 0) {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
                }

                if (array.Length - index < Count) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                dictionary._set.InOrderTreeWalk( delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node){ array[index++] = node.Item.Key; return true;});
            }
            
            void ICollection.CopyTo(Array array, int index) {
                if (array==null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (array.Rank != 1) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                }

                if( array.GetLowerBound(0) != 0 ) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                }

                if (index < 0 ) {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < dictionary.Count) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                
                TKey[] keys = array as TKey[];
                if (keys != null) {
                    CopyTo(keys, index);
                }
                else {
                    object[] objects = (object[])array;
                    if (objects == null) {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                                         
                    try {
                        dictionary._set.InOrderTreeWalk( delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node){ objects[index++] = node.Item.Key; return true;});
                    }                    
                    catch(ArrayTypeMismatchException) {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                }
            }

            public int Count {
                get { return dictionary.Count;}
            }

            bool ICollection<TKey>.IsReadOnly {
                get { return true;}
            }

            void ICollection<TKey>.Add(TKey item){
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            void ICollection<TKey>.Clear(){
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
            }

            bool ICollection<TKey>.Contains(TKey item){
                return dictionary.ContainsKey(item);
            }

            bool ICollection<TKey>.Remove(TKey item){
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_KeyCollectionSet);
                return false;
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            Object ICollection.SyncRoot { 
                get { return ((ICollection)dictionary).SyncRoot; }
            }

            [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
            public struct Enumerator : IEnumerator<TKey>, IEnumerator {
                private SortedDictionary<TKey, TValue>.Enumerator dictEnum;

                internal Enumerator(SortedDictionary<TKey, TValue> dictionary) {
                    dictEnum = dictionary.GetEnumerator();
                }

                public void Dispose() {
                    dictEnum.Dispose();
                }

                public bool MoveNext() {
                    return dictEnum.MoveNext();
                }

                public TKey Current {
                    get {                        
                        return dictEnum.Current.Key;
                    }
                }
                
                object IEnumerator.Current {
                    get {
                        if( dictEnum.NotStartedOrEnded) {
                             ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);                        
                        }
                        
                        return Current;
                    }
                }

                void IEnumerator.Reset() {
                    dictEnum.Reset();
                }

            }                        
        }

        [DebuggerTypeProxy(typeof(System_DictionaryValueCollectionDebugView<,>))]
        [DebuggerDisplay("Count = {Count}")]
#if !FEATURE_NETCORE
        [Serializable]
#endif
        public sealed class ValueCollection: ICollection<TValue>, ICollection {
            private SortedDictionary<TKey,TValue> dictionary;

            public ValueCollection(SortedDictionary<TKey,TValue> dictionary) {
                if (dictionary == null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.dictionary);
                }
                this.dictionary = dictionary;
            }

            public Enumerator GetEnumerator() {
                return new Enumerator(dictionary);
            }

            IEnumerator<TValue> IEnumerable<TValue>.GetEnumerator() {
                return new Enumerator(dictionary);
            }
            
            IEnumerator IEnumerable.GetEnumerator() {                
                return new Enumerator(dictionary);                
            }

            public void CopyTo(TValue[] array, int index) {
                if (array == null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (index < 0 ) {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.index);
                }

                if (array.Length - index < Count) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }

                dictionary._set.InOrderTreeWalk( delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node){ array[index++] = node.Item.Value; return true;});
            }

            void ICollection.CopyTo(Array array, int index) {
                if (array==null) {
                    ThrowHelper.ThrowArgumentNullException(ExceptionArgument.array);
                }

                if (array.Rank != 1) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_RankMultiDimNotSupported);
                }

                if( array.GetLowerBound(0) != 0 ) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_NonZeroLowerBound);
                }

                if (index < 0) {
                    ThrowHelper.ThrowArgumentOutOfRangeException(ExceptionArgument.arrayIndex, ExceptionResource.ArgumentOutOfRange_NeedNonNegNum);
                }

                if (array.Length - index < dictionary.Count) {
                    ThrowHelper.ThrowArgumentException(ExceptionResource.Arg_ArrayPlusOffTooSmall);
                }
                
                TValue[] values = array as TValue[];
                if (values != null) {
                    CopyTo(values, index);
                }
                else {
                    object[] objects = (object[])array;
                    if (objects == null) {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                                         
                    try {
                        dictionary._set.InOrderTreeWalk( delegate(TreeSet<KeyValuePair<TKey, TValue>>.Node node){ objects[index++] = node.Item.Value; return true;});
                    }                    
                    catch(ArrayTypeMismatchException) {
                        ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_InvalidArrayType);
                    }
                }
            }

            public int Count {
                get { return dictionary.Count;}
            }

            bool ICollection<TValue>.IsReadOnly {
                get { return true;}
            }

            void ICollection<TValue>.Add(TValue item){
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            void ICollection<TValue>.Clear(){
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
            }

            bool ICollection<TValue>.Contains(TValue item){
                return dictionary.ContainsValue(item);
            }

            bool ICollection<TValue>.Remove(TValue item){
                ThrowHelper.ThrowNotSupportedException(ExceptionResource.NotSupported_ValueCollectionSet);
                return false;
            }

            bool ICollection.IsSynchronized {
                get { return false; }
            }

            Object ICollection.SyncRoot { 
                get { return ((ICollection)dictionary).SyncRoot; }
            }

            [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
            public struct Enumerator : IEnumerator<TValue>, IEnumerator {
                private SortedDictionary<TKey, TValue>.Enumerator dictEnum;

                internal Enumerator(SortedDictionary<TKey, TValue> dictionary) {
                    dictEnum = dictionary.GetEnumerator();
                }

                public void Dispose() {
                    dictEnum.Dispose();
                }

                public bool MoveNext() {
                    return dictEnum.MoveNext();
                }

                public TValue Current {
                    get {                        
                        return dictEnum.Current.Value;
                    }
                }

                object IEnumerator.Current {
                    get {
                        if( dictEnum.NotStartedOrEnded) {
                             ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);                        
                        }
                        
                        return Current;
                    }
                }

                void IEnumerator.Reset() {
                    dictEnum.Reset();
                }
            }                        
        }

#if !FEATURE_NETCORE
        [Serializable]
#endif
        internal class KeyValuePairComparer : Comparer<KeyValuePair<TKey, TValue>> {
            internal IComparer<TKey> keyComparer;

            public KeyValuePairComparer(IComparer<TKey> keyComparer) {
                if ( keyComparer == null) {
                    this.keyComparer = Comparer<TKey>.Default;                    
                } else {
                    this.keyComparer = keyComparer;
                }
            }

            public  override int Compare( KeyValuePair<TKey, TValue> x, KeyValuePair<TKey, TValue> y) {
                return keyComparer.Compare(x.Key, y.Key);
            }            
        }
    }



    /// <summary>
    /// This class is intended as a helper for backwards compatibility with existing SortedDictionaries.
    /// TreeSet has been converted into SortedSet<T>, which will be exposed publicly. SortedDictionaries
    /// have the problem where they have already been serialized to disk as having a backing class named
    /// TreeSet. To ensure that we can read back anything that has already been written to disk, we need to
    /// make sure that we have a class named TreeSet that does everything the way it used to.
    /// 
    /// The only thing that makes it different from SortedSet is that it throws on duplicates
    /// </summary>
    /// <typeparam name="T"></typeparam>
#if !FEATURE_NETCORE
    [Serializable]
#endif
    internal class TreeSet<T> : SortedSet<T> {

        public TreeSet()
            : base() { }

        public TreeSet(IComparer<T> comparer) : base(comparer) { }

        public TreeSet(ICollection<T> collection) : base(collection) { }

        public TreeSet(ICollection<T> collection, IComparer<T> comparer) : base(collection, comparer) { }

#if !FEATURE_NETCORE
        public TreeSet(SerializationInfo siInfo, StreamingContext context) : base(siInfo, context) { }
#endif

        internal override bool AddIfNotPresent(T item) {
            bool ret = base.AddIfNotPresent(item);
            if (!ret) {                
                ThrowHelper.ThrowArgumentException(ExceptionResource.Argument_AddingDuplicate);                
            }
            return ret;
        }

    }

}

// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*=============================================================================
**
**
**
** Purpose: DebugView class for generic collections
**
** Date: Mar 09, 2004
**
=============================================================================*/

namespace System.Collections.Generic {
    using System;
    using System.Security.Permissions;
    using System.Diagnostics;    

    internal sealed class System_CollectionDebugView<T> {
        private ICollection<T> collection; 
        
        public System_CollectionDebugView(ICollection<T> collection) {
            if (collection == null) {
                throw new ArgumentNullException("collection");
            }

            this.collection = collection;
        }
       
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items   { 
            get {
                T[] items = new T[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }            

    internal sealed class System_QueueDebugView<T> {
        private Queue<T> queue; 
        
        public System_QueueDebugView(Queue<T> queue) {
            if (queue == null) {
                throw new ArgumentNullException("queue");
            }

            this.queue = queue;
        }
       
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items   { 
            get {
                return queue.ToArray();
            }
        }
    }            

    internal sealed class System_StackDebugView<T> {
        private Stack<T> stack; 
        
        public System_StackDebugView(Stack<T> stack) {
            if (stack == null) {
                throw new ArgumentNullException("stack");
            }

            this.stack = stack;
        }
       
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items   { 
            get {
                return stack.ToArray();
            }
        }
    }            

#if !SILVERLIGHT || FEATURE_NETCORE
    internal sealed class System_DictionaryDebugView<K, V> {
        private IDictionary<K, V> dict; 
        
        public System_DictionaryDebugView(IDictionary<K, V> dictionary) {
            if (dictionary == null)
                throw new ArgumentNullException("dictionary");

                this.dict = dictionary;
        }
       
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public KeyValuePair<K, V>[] Items   { 
            get {
                KeyValuePair<K, V>[] items = new KeyValuePair<K, V>[dict.Count];
                dict.CopyTo(items, 0);
                return items;
            }
        }
    }   
    internal sealed class System_DictionaryKeyCollectionDebugView<TKey, TValue> {
        private ICollection<TKey> collection; 
        
        public System_DictionaryKeyCollectionDebugView(ICollection<TKey> collection) {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);

                this.collection = collection;
        }
       
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TKey[] Items   { 
            get {
                TKey[] items = new TKey[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }        

    internal sealed class System_DictionaryValueCollectionDebugView<TKey, TValue> {
        private ICollection<TValue> collection; 
        
        public System_DictionaryValueCollectionDebugView(ICollection<TValue> collection) {
            if (collection == null)
                ThrowHelper.ThrowArgumentNullException(ExceptionArgument.collection);

                this.collection = collection;
        }
       
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public TValue[] Items   { 
            get {
                TValue[] items = new TValue[collection.Count];
                collection.CopyTo(items, 0);
                return items;
            }
        }
    }    
#endif // !SILVERLIGHT || FEATURE_NETCORE
     
}

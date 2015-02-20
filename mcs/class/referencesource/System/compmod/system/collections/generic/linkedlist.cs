
namespace System.Collections.Generic {
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
#if !SILVERLIGHT
    using System.Runtime.Serialization;   
#endif
    using System.Security.Permissions; 

    [System.Runtime.InteropServices.ComVisible(false)]  
    [DebuggerTypeProxy(typeof(System_CollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]    
#if SILVERLIGHT
    public class LinkedList<T>: ICollection<T>, System.Collections.ICollection
#else
    [Serializable()]    
    public class LinkedList<T>: ICollection<T>, System.Collections.ICollection
           ,ISerializable, IDeserializationCallback 
#endif
    {
        // This LinkedList is a doubly-Linked circular list.
        internal LinkedListNode<T> head;
        internal int count;
        internal int version;
        private Object _syncRoot;
        
#if !SILVERLIGHT
        private SerializationInfo siInfo; //A temporary variable which we need during deserialization.        
#endif

        // names for serialization
        const String VersionName = "Version";
        const String CountName = "Count";  
        const String ValuesName = "Data";

        public LinkedList() {
        }

        public LinkedList(IEnumerable<T> collection) {
            if (collection==null) {
                throw new ArgumentNullException("collection");
            }

            foreach( T item in collection) {
                AddLast(item);
            }
        }
#if !SILVERLIGHT
        protected LinkedList(SerializationInfo info, StreamingContext context) {
            siInfo = info;
        }
#endif

        public int Count {
            get { return count;}
        }

        public LinkedListNode<T> First {
            get { return head;}
        }

        public LinkedListNode<T> Last {
            get { return head == null? null: head.prev;}
        }

        bool ICollection<T>.IsReadOnly {
            get { return false; }
        }

        void ICollection<T>.Add(T value) {
            AddLast(value);
        }

        public LinkedListNode<T> AddAfter(LinkedListNode<T> node, T value) {
            ValidateNode(node);
            LinkedListNode<T> result = new LinkedListNode<T>(node.list, value); 
            InternalInsertNodeBefore(node.next, result);
            return result;
        }

        public void AddAfter(LinkedListNode<T> node, LinkedListNode<T> newNode) {
            ValidateNode(node);
            ValidateNewNode(newNode);
            InternalInsertNodeBefore(node.next, newNode);
            newNode.list = this;
        }    

        public LinkedListNode<T> AddBefore(LinkedListNode<T> node, T value) {
            ValidateNode(node);    
            LinkedListNode<T> result = new LinkedListNode<T>(node.list, value);
            InternalInsertNodeBefore(node, result);
            if ( node == head) {
                head = result;
            }
            return result;
        }    

        public void AddBefore(LinkedListNode<T> node, LinkedListNode<T> newNode) {
            ValidateNode(node);    
            ValidateNewNode(newNode);                        
            InternalInsertNodeBefore(node, newNode);
            newNode.list = this;
            if ( node == head) {
                head = newNode;
            }
        }    
 
        public LinkedListNode<T> AddFirst(T value) {
            LinkedListNode<T> result = new LinkedListNode<T>(this, value);
            if (head == null) {
                InternalInsertNodeToEmptyList(result);
            } 
            else {
                InternalInsertNodeBefore( head, result);               
                head = result;                
            }
            return result;
        }

        public void AddFirst(LinkedListNode<T> node) {
            ValidateNewNode(node);

            if (head == null) {
                InternalInsertNodeToEmptyList(node);
            } 
            else {
                InternalInsertNodeBefore( head, node); 
                head = node;                
            }
            node.list = this;
        }

        public LinkedListNode<T> AddLast(T value) {
            LinkedListNode<T> result = new LinkedListNode<T>(this, value);        
            if (head== null) {
                InternalInsertNodeToEmptyList(result);
            } 
            else {
                InternalInsertNodeBefore( head, result);               
            }       
            return result;
        }

        public void AddLast(LinkedListNode<T> node) {
            ValidateNewNode(node);            

            if (head == null) {
                InternalInsertNodeToEmptyList(node);
            } 
            else {
                InternalInsertNodeBefore( head, node); 
            }
            node.list = this;
        }

        public void Clear() {
            LinkedListNode<T> current = head;             
            while (current != null ) {
                LinkedListNode<T> temp = current;
                current = current.Next;   // use Next the instead of "next", otherwise it will loop forever
                temp.Invalidate();                
            }

            head = null;
            count = 0;             
            version++;     
        }

        public bool Contains(T value) {
            return Find(value) != null;    
        }

        public void CopyTo( T[] array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            if(index < 0 || index > array.Length) {
                throw new ArgumentOutOfRangeException("index",SR.GetString(SR.IndexOutOfRange, index) );                    
            }

            if (array.Length - index < Count) {
                throw new ArgumentException(SR.GetString(SR.Arg_InsufficientSpace));
            }

            LinkedListNode<T> node = head;
            if (node != null) {
                do {
                    array[index++] = node.item;                             
                    node = node.next;
                } while (node != head);
            }
        }

        public LinkedListNode<T> Find(T value) {
            LinkedListNode<T> node = head;
            EqualityComparer<T> c = EqualityComparer<T>.Default;            
            if (node != null) {
                if (value != null) {
                    do {
                        if (c.Equals(node.item, value)) {
                            return node;   
                        }
                        node = node.next;
                    } while (node != head);
                } 
                else {
                    do {
                        if (node.item == null) {
                            return node;
                        }
                        node = node.next;
                    } while (node != head);
                }
            }
            return null;        
        }

        public LinkedListNode<T> FindLast(T value) {
            if ( head == null) return null;

            LinkedListNode<T> last = head.prev;    
            LinkedListNode<T> node = last;
            EqualityComparer<T> c = EqualityComparer<T>.Default;
            if (node != null) {
                if (value != null) {
                    do {
                        if (c.Equals(node.item, value)) {
                            return node;
                        }

                        node = node.prev;
                    } while (node != last);
                } 
                else {
                    do {
                        if (node.item == null) {
                            return node;
                        }
                        node = node.prev;
                    } while (node != last);
                }
            }
            return null;                
        }

        public Enumerator GetEnumerator() {
            return new Enumerator(this);
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() {
            return GetEnumerator();
        }

        public bool Remove(T value) {
            LinkedListNode<T> node = Find(value);
            if (node != null) {
                InternalRemoveNode(node); 
                return true;
            }
            return false;
        }

        public void Remove(LinkedListNode<T> node) {
            ValidateNode(node);          
            InternalRemoveNode(node); 
        }
                    
        public void RemoveFirst() {
            if ( head == null) { throw new InvalidOperationException(SR.GetString(SR.LinkedListEmpty)); }
            InternalRemoveNode(head);             
        }

        public void RemoveLast() {
            if ( head == null) { throw new InvalidOperationException(SR.GetString(SR.LinkedListEmpty)); }
            InternalRemoveNode(head.prev);     
        }

#if !SILVERLIGHT
        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "System.dll is still using pre-v4 security model and needs this demand")]
        [SecurityPermissionAttribute(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.SerializationFormatter)] 		
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
            // Customized serialization for LinkedList.
            // We need to do this because it will be too expensive to Serialize each node.
            // This will give us the flexiblility to change internal implementation freely in future.
            if (info==null) {
                throw new ArgumentNullException("info");
            }
            info.AddValue(VersionName, version);
            info.AddValue(CountName, count); //This is the length of the bucket array.
            if ( count != 0) {
                T[] array = new T[Count];
                CopyTo(array, 0);
                info.AddValue(ValuesName, array, typeof(T[]));
            }
        }

        public virtual void OnDeserialization(Object sender) {
            if (siInfo==null) {
                return; //Somebody had a dependency on this Dictionary and fixed us up before the ObjectManager got to it.
            }

            int realVersion = siInfo.GetInt32(VersionName);
            int count = siInfo.GetInt32(CountName);

            if ( count != 0) {
                T[] array = (T[]) siInfo.GetValue(ValuesName, typeof(T[]));

                if (array==null) {
                    throw new SerializationException(SR.GetString(SR.Serialization_MissingValues));
                }
                for ( int i = 0; i < array.Length; i++) {
                    AddLast(array[i]);
                }         
            } 
            else {
                head = null;
            }

            version = realVersion;
            siInfo=null;
        }
#endif

        private void InternalInsertNodeBefore(LinkedListNode<T> node, LinkedListNode<T> newNode) {
            newNode.next = node;
            newNode.prev = node.prev;
            node.prev.next = newNode;
            node.prev = newNode;            
            version++;
            count++;
        }

        private void InternalInsertNodeToEmptyList(LinkedListNode<T> newNode) {
            Debug.Assert( head == null && count == 0, "LinkedList must be empty when this method is called!");
            newNode.next = newNode;
            newNode.prev = newNode;
            head = newNode;
            version++;
            count++; 
        }

        internal void InternalRemoveNode(LinkedListNode<T> node) {
            Debug.Assert( node.list == this, "Deleting the node from another list!");   
            Debug.Assert( head != null, "This method shouldn't be called on empty list!");
            if ( node.next == node) {
                Debug.Assert(count == 1 && head == node, "this should only be true for a list with only one node");
                head  = null;
            } 
            else {
                node.next.prev = node.prev;
                node.prev.next = node.next;
                if ( head == node) {
                    head = node.next;
                }
            }
            node.Invalidate();  
            count--;
            version++;          
        }

        internal void ValidateNewNode(LinkedListNode<T> node) {
            if (node == null) {
                throw new ArgumentNullException("node");                
            }

            if ( node.list != null) {
                throw new InvalidOperationException(SR.GetString(SR.LinkedListNodeIsAttached));                
            }
        }


        internal void ValidateNode(LinkedListNode<T> node) {
            if (node == null) {
                throw new ArgumentNullException("node");                
            }

            if ( node.list != this) {
                throw new InvalidOperationException(SR.GetString(SR.ExternalLinkedListNode));
            }
        }

        bool System.Collections.ICollection.IsSynchronized { 
            get { return false;} 
        }

        object System.Collections.ICollection.SyncRoot  { 
            get { 
                if( _syncRoot == null) {
                    System.Threading.Interlocked.CompareExchange<Object>(ref _syncRoot, new Object(), null);    
                }
                return _syncRoot; 
            }
        }

        void System.Collections.ICollection.CopyTo(Array  array, int index) {
            if (array == null) {
                throw new ArgumentNullException("array");
            }

            if (array.Rank != 1) {
                throw new ArgumentException(SR.GetString(SR.Arg_MultiRank));
            }

            if( array.GetLowerBound(0) != 0 ) {
                throw new ArgumentException(SR.GetString(SR.Arg_NonZeroLowerBound));
            }

            if (index < 0) {
                throw new ArgumentOutOfRangeException("index",SR.GetString(SR.IndexOutOfRange, index) );
            }

            if (array.Length - index < Count) {
                throw new ArgumentException(SR.GetString(SR.Arg_InsufficientSpace));
            }

            T[] tArray = array as T[];
            if (tArray != null) {
                CopyTo(tArray, index);
            } 
            else {
                //
                // Catch the obvious case assignment will fail.
                // We can found all possible problems by doing the check though.
                // For example, if the element type of the Array is derived from T,
                // we can't figure out if we can successfully copy the element beforehand.
                //
                Type targetType = array.GetType().GetElementType(); 
                Type sourceType = typeof(T);
                if(!(targetType.IsAssignableFrom(sourceType) || sourceType.IsAssignableFrom(targetType))) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_Array_Type));                
                }

                object[] objects = array as object[];
                if (objects == null) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_Array_Type));
                }
                    LinkedListNode<T> node = head;
                try {
                    if (node != null) {
                        do {
                            objects[index++] = node.item;
                            node = node.next;
                        } while (node != head);
                    }
                } 
                catch(ArrayTypeMismatchException) {
                    throw new ArgumentException(SR.GetString(SR.Invalid_Array_Type));
                }
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() {
            return GetEnumerator();            
        }

#if !SILVERLIGHT
        [Serializable()]
#endif
        [SuppressMessage("Microsoft.Performance", "CA1815:OverrideEqualsAndOperatorEqualsOnValueTypes", Justification = "not an expected scenario")]
        public struct Enumerator : IEnumerator<T>, System.Collections.IEnumerator
#if !SILVERLIGHT
                                   , ISerializable, IDeserializationCallback 
#endif
        {
            private LinkedList<T> list;
            private LinkedListNode<T> node;
            private int version;
            private T current;
            private int index;
#if !SILVERLIGHT
            private SerializationInfo siInfo; //A temporary variable which we need during deserialization.
#endif

            const string LinkedListName = "LinkedList";
            const string CurrentValueName = "Current";
            const string VersionName = "Version";
            const string IndexName = "Index";

            internal Enumerator(LinkedList<T> list) {
                this.list = list; 
                version = list.version; 
                node = list.head; 
                current = default(T); 
                index = 0;
#if !SILVERLIGHT
                siInfo = null;
#endif
            }

#if !SILVERLIGHT
            internal Enumerator(SerializationInfo info, StreamingContext context) {
                siInfo = info;          
                list = null; 
                version = 0; 
                node = null; 
                current = default(T); 
                index = 0;
            }
#endif

            public T Current {
                get { return current;}
            }

            object System.Collections.IEnumerator.Current {
                get { 
                    if( index == 0 || (index == list.Count + 1)) {
                         ThrowHelper.ThrowInvalidOperationException(ExceptionResource.InvalidOperation_EnumOpCantHappen);                        
                    }
                    
                    return current;
                }
            }

            public bool MoveNext() {
                if (version != list.version) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumFailedVersion));
                }

                if (node == null) {
                    index = list.Count + 1;
                    return false;
                }

                ++index;
                current = node.item;   
                node = node.next;  
                if (node == list.head) {
                    node = null;
                }
                return true;
            }

            void System.Collections.IEnumerator.Reset() {
                if (version != list.version) {
                    throw new InvalidOperationException(SR.GetString(SR.InvalidOperation_EnumFailedVersion));
                }
                
                current = default(T);
                node = list.head;
                index = 0;
            }

            public void Dispose() {
            }

#if !SILVERLIGHT
            void ISerializable.GetObjectData(SerializationInfo info, StreamingContext context) {
                if (info==null) {
                    throw new ArgumentNullException("info");
                }

                info.AddValue(LinkedListName, list);
                info.AddValue(VersionName, version);
                info.AddValue(CurrentValueName, current); 
                info.AddValue(IndexName, index); 
            }

            void IDeserializationCallback.OnDeserialization(Object sender) {
                if (list != null) {
                    return; //Somebody had a dependency on this Dictionary and fixed us up before the ObjectManager got to it.
                }

                if (siInfo==null) {
                    throw new SerializationException(SR.GetString(SR.Serialization_InvalidOnDeser));
                }

                list = (LinkedList<T>)siInfo.GetValue(LinkedListName, typeof(LinkedList<T>));
                version = siInfo.GetInt32(VersionName);
                current = (T)siInfo.GetValue(CurrentValueName, typeof(T));
                index = siInfo.GetInt32(IndexName);

                if( list.siInfo != null) {
                    list.OnDeserialization(sender);
                }

                if( index == list.Count + 1) {  // end of enumeration
                    node = null;
                }
                else {    
                    node = list.First;
                    // We don't care if we can point to the correct node if the LinkedList was changed   
                    // MoveNext will throw upon next call and Current has the correct value. 
                    if( node != null && index != 0) {
                        for(int i =0; i< index; i++) {
                            node = node.next; 
                         }
                         if( node == list.First) {
                             node = null;       
                         }
                     }   
                }
                siInfo=null;
            }           
#endif
        }

    }

    // Note following class is not serializable since we customized the serialization of LinkedList. 
    [System.Runtime.InteropServices.ComVisible(false)] 
    public sealed class LinkedListNode<T> {
        internal LinkedList<T> list;
        internal LinkedListNode<T> next;
        internal LinkedListNode<T> prev;
        internal T item;
        
        public LinkedListNode( T value) {
            this.item = value;
        }

        internal LinkedListNode(LinkedList<T> list, T value) {
            this.list = list;
            this.item = value;
        }

        public LinkedList<T> List {
            get { return list;}
        }

        public LinkedListNode<T> Next {
            get { return next == null || next == list.head? null: next;}
        }

        public LinkedListNode<T> Previous {
            get { return prev == null || this == list.head? null: prev;}
        }

        public T Value {
            get { return item;}
            set { item = value;}
        }

        internal void Invalidate() {
            list = null;
            next = null;
            prev = null;
        }           
    }   
}


// ==++==
//
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// =+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+=+
//
// ConcurrentBag.cs
//
// <OWNER>[....]</OWNER>
//
// 
//An unordered collection that allows duplicates and that provides add and get operations.
// =-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-=-

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Runtime.Serialization;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Permissions;
using System.Threading;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace System.Collections.Concurrent
{
    /// <summary>
    /// Represents an thread-safe, unordered collection of objects. 
    /// </summary>
    /// <typeparam name="T">Specifies the type of elements in the bag.</typeparam>
    /// <remarks>
    /// <para>
    /// Bags are useful for storing objects when ordering doesn't matter, and unlike sets, bags support
    /// duplicates. <see cref="ConcurrentBag{T}"/> is a thread-safe bag implementation, optimized for
    /// scenarios where the same thread will be both producing and consuming data stored in the bag.
    /// </para>
    /// <para>
    /// <see cref="ConcurrentBag{T}"/> accepts null reference (Nothing in Visual Basic) as a valid 
    /// value for reference types.
    /// </para>
    /// <para>
    /// All public and protected members of <see cref="ConcurrentBag{T}"/> are thread-safe and may be used
    /// concurrently from multiple threads.
    /// </para>
    /// </remarks>
#if !SILVERLIGHT
    [Serializable]
#endif
    [ComVisible(false)]
    [DebuggerTypeProxy(typeof(SystemThreadingCollection_IProducerConsumerCollectionDebugView<>))]
    [DebuggerDisplay("Count = {Count}")]
#if !FEATURE_NETCORE
    [HostProtection(Synchronization = true, ExternalThreading = true)]
#endif
    public class ConcurrentBag<T> : IProducerConsumerCollection<T>
    {

        // ThreadLocalList object that contains the data per thread
#if !SILVERLIGHT
        [NonSerialized]
#endif
        ThreadLocal<ThreadLocalList> m_locals;

        // This head and tail pointers points to the first and last local lists, to allow enumeration on the thread locals objects
#if !SILVERLIGHT
        [NonSerialized]
#endif
        volatile ThreadLocalList m_headList, m_tailList;

        // A flag used to tell the operations thread that it must synchronize the operation, this flag is set/unset within
        // GlobalListsLock lock
#if !SILVERLIGHT
        [NonSerialized]
#endif
        bool m_needSync;

#if !SILVERLIGHT
        // Used for custom serialization.
        private T[] m_serializationArray;
#endif

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentBag{T}"/>
        /// class.
        /// </summary>
        public ConcurrentBag()
        {
            Initialize(null);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcurrentBag{T}"/>
        /// class that contains elements copied from the specified collection.
        /// </summary>
        /// <param name="collection">The collection whose elements are copied to the new <see
        /// cref="ConcurrentBag{T}"/>.</param>
        /// <exception cref="ArgumentNullException"><paramref name="collection"/> is a null reference
        /// (Nothing in Visual Basic).</exception>
        public ConcurrentBag(IEnumerable<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection", SR.GetString(SR.ConcurrentBag_Ctor_ArgumentNullException));
            }
            Initialize(collection);
        }


        /// <summary>
        /// Local helper function to initalize a new bag object
        /// </summary>
        /// <param name="collection">An enumeration containing items with which to initialize this bag.</param>
        private void Initialize(IEnumerable<T> collection)
        {
            m_locals = new ThreadLocal<ThreadLocalList>();

            // Copy the collection to the bag
            if (collection != null)
            {
                ThreadLocalList list = GetThreadList(true);
                foreach (T item in collection)
                {
                    list.Add(item, false);
                }
            }
        }

        /// <summary>
        /// Adds an object to the <see cref="ConcurrentBag{T}"/>.
        /// </summary>
        /// <param name="item">The object to be added to the
        /// <see cref="ConcurrentBag{T}"/>. The value can be a null reference
        /// (Nothing in Visual Basic) for reference types.</param>
        public void Add(T item)
        {
            // Get the local list for that thread, create a new list if this thread doesn't exist 
            //(first time to call add)
            ThreadLocalList list = GetThreadList(true);
            AddInternal(list, item);
        }

        /// <summary>
        /// </summary>
        /// <param name="list"></param>
        /// <param name="item"></param>
        private void AddInternal(ThreadLocalList list, T item)
        {
            bool lockTaken = false;
            try
            {
#pragma warning disable 0420
                Interlocked.Exchange(ref list.m_currentOp, (int)ListOperation.Add);
#pragma warning restore 0420
                //Synchronization cases:
                // if the list count is less than two to avoid conflict with any stealing thread
                // if m_needSync is set, this means there is a thread that needs to freeze the bag
                if (list.Count < 2 || m_needSync)
                {
                    // reset it back to zero to avoid deadlock with stealing thread
                    list.m_currentOp = (int)ListOperation.None;
                    Monitor.Enter(list, ref lockTaken);
                }
                list.Add(item, lockTaken);
            }
            finally
            {
                list.m_currentOp = (int)ListOperation.None;
                if (lockTaken)
                {
                    Monitor.Exit(list);
                }
            }
        }

        /// <summary>
        /// Attempts to add an object to the <see cref="ConcurrentBag{T}"/>.
        /// </summary>
        /// <param name="item">The object to be added to the 
        /// <see cref="ConcurrentBag{T}"/>. The value can be a null reference
        /// (Nothing in Visual Basic) for reference types.</param>
        /// <returns>Always returns true</returns>
        bool IProducerConsumerCollection<T>.TryAdd(T item)
        {
            Add(item);
            return true;
        }

        /// <summary>
        /// Attempts to remove and return an object from the <see
        /// cref="ConcurrentBag{T}"/>.
        /// </summary>
        /// <param name="result">When this method returns, <paramref name="result"/> contains the object
        /// removed from the <see cref="ConcurrentBag{T}"/> or the default value
        /// of <typeparamref name="T"/> if the operation failed.</param>
        /// <returns>true if an object was removed successfully; otherwise, false.</returns>
        public bool TryTake(out T result)
        {
            return TryTakeOrPeek(out result, true);
        }

        /// <summary>
        /// Attempts to return an object from the <see cref="ConcurrentBag{T}"/>
        /// without removing it.
        /// </summary>
        /// <param name="result">When this method returns, <paramref name="result"/> contains an object from
        /// the <see cref="ConcurrentBag{T}"/> or the default value of
        /// <typeparamref name="T"/> if the operation failed.</param>
        /// <returns>true if and object was returned successfully; otherwise, false.</returns>
        public bool TryPeek(out T result)
        {
            return TryTakeOrPeek(out result, false);
        }

        /// <summary>
        /// Local helper function to Take or Peek an item from the bag
        /// </summary>
        /// <param name="result">To receive the item retrieved from the bag</param>
        /// <param name="take">True means Take operation, false means Peek operation</param>
        /// <returns>True if succeeded, false otherwise</returns>
        private bool TryTakeOrPeek(out T result, bool take)
        {

            // Get the local list for that thread, return null if the thread doesn't exit 
            //(this thread never add before) 
            ThreadLocalList list = GetThreadList(false);
            if (list == null || list.Count == 0)
            {
                return Steal(out result, take);
            }

            bool lockTaken = false;
            try
            {
                if (take) // Take operation
                {
#pragma warning disable 0420
                    Interlocked.Exchange(ref list.m_currentOp, (int)ListOperation.Take);
#pragma warning restore 0420
                    //Synchronization cases:
                    // if the list count is less than or equal two to avoid conflict with any stealing thread
                    // if m_needSync is set, this means there is a thread that needs to freeze the bag
                    if (list.Count <= 2 || m_needSync)
                    {
                        // reset it back to zero to avoid deadlock with stealing thread
                        list.m_currentOp = (int)ListOperation.None;
                        Monitor.Enter(list, ref lockTaken);

                        // Double check the count and steal if it became empty
                        if (list.Count == 0)
                        {
                            // Release the lock before stealing
                            if (lockTaken)
                            {
                                try { }
                                finally
                                {
                                    lockTaken = false; // reset lockTaken to avoid calling Monitor.Exit again in the finally block
                                    Monitor.Exit(list);
                                }
                            }
                            return Steal(out result, true);
                        }
                    }
                    list.Remove(out result);
                }
                else
                {
                    if (!list.Peek(out result))
                    {
                        return Steal(out result, false);
                    }
                }
            }
            finally
            {
                list.m_currentOp = (int)ListOperation.None;
                if (lockTaken)
                {
                    Monitor.Exit(list);
                }
            }
            return true;
        }


        /// <summary>
        /// Local helper function to retrieve a thread local list by a thread object
        /// </summary>
        /// <param name="forceCreate">Create a new list if the thread does ot exist</param>
        /// <returns>The local list object</returns>
        private ThreadLocalList GetThreadList(bool forceCreate)
        {
            ThreadLocalList list = m_locals.Value;

            if (list != null)
            {
                return list;
            }
            else if (forceCreate)
            {
                // Acquire the lock to update the m_tailList pointer
                lock (GlobalListsLock)
                {
                    if (m_headList == null)
                    {
                        list = new ThreadLocalList(Thread.CurrentThread);
                        m_headList = list;
                        m_tailList = list;
                    }
                    else
                    {

                        list = GetUnownedList();
                        if (list == null)
                        {
                            list = new ThreadLocalList(Thread.CurrentThread);
                            m_tailList.m_nextList = list;
                            m_tailList = list;
                        }
                    }
                    m_locals.Value = list;
                }
            }
            else
            {
                return null;
            }
            Debug.Assert(list != null);
            return list;

        }

        /// <summary>
        /// Try to reuse an unowned list if exist
        /// unowned lists are the lists that their owner threads are aborted or terminated
        /// this is workaround to avoid memory leaks.
        /// </summary>
        /// <returns>The list object, null if all lists are owned</returns>
        private ThreadLocalList GetUnownedList()
        {
            //the global lock must be held at this point
            Contract.Assert(Monitor.IsEntered(GlobalListsLock));

            ThreadLocalList currentList = m_headList;
            while (currentList != null)
            {
                if (currentList.m_ownerThread.ThreadState == System.Threading.ThreadState.Stopped)
                {
                    currentList.m_ownerThread = Thread.CurrentThread; // the caller should acquire a lock to make this line thread safe
                    return currentList;
                }
                currentList = currentList.m_nextList;
            }
            return null;
        }


        /// <summary>
        /// Local helper method to steal an item from any other non empty thread
        /// It enumerate all other threads in two passes first pass acquire the lock with TryEnter if succeeded
        /// it steals the item, otherwise it enumerate them again in 2nd pass and acquire the lock using Enter
        /// </summary>
        /// <param name="result">To receive the item retrieved from the bag</param>
        /// <param name="take">Whether to remove or peek.</param>
        /// <returns>True if succeeded, false otherwise.</returns>
        private bool Steal(out T result, bool take)
        {
#if !FEATURE_PAL && !SILVERLIGHT    // PAL doesn't support  eventing
            if (take)
                CDSCollectionETWBCLProvider.Log.ConcurrentBag_TryTakeSteals();
            else
                CDSCollectionETWBCLProvider.Log.ConcurrentBag_TryPeekSteals();
#endif

            bool loop;
            List<int> versionsList = new List<int>(); // save the lists version
            do
            {
                versionsList.Clear(); //clear the list from the previous iteration
                loop = false;
              

                ThreadLocalList currentList = m_headList;
                while (currentList != null)
                {
                    versionsList.Add(currentList.m_version);
                    if (currentList.m_head != null && TrySteal(currentList, out result, take))
                    {
                        return true;
                    }
                    currentList = currentList.m_nextList;
                }

                // verify versioning, if other items are added to this list since we last visit it, we should retry
                currentList = m_headList;
                foreach (int version in versionsList)
                {
                    if (version != currentList.m_version) //oops state changed
                    {
                        loop = true;
                        if (currentList.m_head != null && TrySteal(currentList, out result, take))
                            return true;
                    }
                    currentList = currentList.m_nextList;
                }
            } while (loop);


            result = default(T);
            return false;
        }

        /// <summary>
        /// local helper function tries to steal an item from given local list
        /// </summary>
        private bool TrySteal(ThreadLocalList list, out T result, bool take)
        {
            lock (list)
            {
                if (CanSteal(list))
                {
                    list.Steal(out result, take);
                    return true;
                }
                result = default(T);
                return false;
            }

        }
        /// <summary>
        /// Local helper function to check the list if it became empty after acquiring the lock
        /// and wait if there is unsynchronized Add/Take operation in the list to be done
        /// </summary>
        /// <param name="list">The list to steal</param>
        /// <returns>True if can steal, false otherwise</returns>
        private bool CanSteal(ThreadLocalList list)
        {
            if (list.Count <= 2 && list.m_currentOp != (int)ListOperation.None)
            {
                SpinWait spinner = new SpinWait();
                while (list.m_currentOp != (int)ListOperation.None)
                {
                    spinner.SpinOnce();
                }
            }
            if (list.Count > 0)
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Copies the <see cref="ConcurrentBag{T}"/> elements to an existing
        /// one-dimensional <see cref="T:System.Array">Array</see>, starting at the specified array
        /// index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the
        /// destination of the elements copied from the
        /// <see cref="ConcurrentBag{T}"/>. The <see
        /// cref="T:System.Array">Array</see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying
        /// begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is a null reference (Nothing in
        /// Visual Basic).</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than
        /// zero.</exception>
        /// <exception cref="ArgumentException"><paramref name="index"/> is equal to or greater than the
        /// length of the <paramref name="array"/>
        /// -or- the number of elements in the source <see
        /// cref="ConcurrentBag{T}"/> is greater than the available space from
        /// <paramref name="index"/> to the end of the destination <paramref name="array"/>.</exception>
        public void CopyTo(T[] array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", SR.GetString(SR.ConcurrentBag_CopyTo_ArgumentNullException));
            }
            if (index < 0)
            {
                throw new ArgumentOutOfRangeException
                    ("index", SR.GetString(SR.ConcurrentBag_CopyTo_ArgumentOutOfRangeException));
            }

            // Short path if the bag is empty
            if (m_headList == null)
                return;

            bool lockTaken = false;
            try
            {
                FreezeBag(ref lockTaken);
                ToList().CopyTo(array, index);
            }
            finally
            {
                UnfreezeBag(lockTaken);
            }
        }

        /// <summary>
        /// Copies the elements of the <see cref="T:System.Collections.ICollection"/> to an <see
        /// cref="T:System.Array"/>, starting at a particular
        /// <see cref="T:System.Array"/> index.
        /// </summary>
        /// <param name="array">The one-dimensional <see cref="T:System.Array">Array</see> that is the
        /// destination of the elements copied from the
        /// <see cref="ConcurrentBag{T}"/>. The <see
        /// cref="T:System.Array">Array</see> must have zero-based indexing.</param>
        /// <param name="index">The zero-based index in <paramref name="array"/> at which copying
        /// begins.</param>
        /// <exception cref="ArgumentNullException"><paramref name="array"/> is a null reference (Nothing in
        /// Visual Basic).</exception>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is less than
        /// zero.</exception>
        /// <exception cref="ArgumentException">
        /// <paramref name="array"/> is multidimensional. -or-
        /// <paramref name="array"/> does not have zero-based indexing. -or-
        /// <paramref name="index"/> is equal to or greater than the length of the <paramref name="array"/>
        /// -or- The number of elements in the source <see cref="T:System.Collections.ICollection"/> is
        /// greater than the available space from <paramref name="index"/> to the end of the destination
        /// <paramref name="array"/>. -or- The type of the source <see
        /// cref="T:System.Collections.ICollection"/> cannot be cast automatically to the type of the
        /// destination <paramref name="array"/>.
        /// </exception>
        void ICollection.CopyTo(Array array, int index)
        {
            if (array == null)
            {
                throw new ArgumentNullException("array", SR.GetString(SR.ConcurrentBag_CopyTo_ArgumentNullException));
            }

            bool lockTaken = false;
            try
            {
                FreezeBag(ref lockTaken);
                ((ICollection)ToList()).CopyTo(array, index);
            }
            finally
            {
                UnfreezeBag(lockTaken);
            }

        }


        /// <summary>
        /// Copies the <see cref="ConcurrentBag{T}"/> elements to a new array.
        /// </summary>
        /// <returns>A new array containing a snapshot of elements copied from the <see
        /// cref="ConcurrentBag{T}"/>.</returns>
        public T[] ToArray()
        {
            // Short path if the bag is empty
            if (m_headList == null)
                return new T[0];

            bool lockTaken = false;
            try
            {
                FreezeBag(ref lockTaken);
                return ToList().ToArray();
            }
            finally
            {
                UnfreezeBag(lockTaken);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see
        /// cref="ConcurrentBag{T}"/>.
        /// </summary>
        /// <returns>An enumerator for the contents of the <see
        /// cref="ConcurrentBag{T}"/>.</returns>
        /// <remarks>
        /// The enumeration represents a moment-in-time snapshot of the contents
        /// of the bag.  It does not reflect any updates to the collection after 
        /// <see cref="GetEnumerator"/> was called.  The enumerator is safe to use
        /// concurrently with reads from and writes to the bag.
        /// </remarks>
        public IEnumerator<T> GetEnumerator()
        {
            // Short path if the bag is empty
            if (m_headList == null)
                return new List<T>().GetEnumerator(); // empty list

            bool lockTaken = false;
            try
            {
                FreezeBag(ref lockTaken);
                return ToList().GetEnumerator();
            }
            finally
            {
                UnfreezeBag(lockTaken);
            }
        }

        /// <summary>
        /// Returns an enumerator that iterates through the <see
        /// cref="ConcurrentBag{T}"/>.
        /// </summary>
        /// <returns>An enumerator for the contents of the <see
        /// cref="ConcurrentBag{T}"/>.</returns>
        /// <remarks>
        /// The items enumerated represent a moment-in-time snapshot of the contents
        /// of the bag.  It does not reflect any update to the collection after 
        /// <see cref="GetEnumerator"/> was called.
        /// </remarks>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ConcurrentBag<T>)this).GetEnumerator();
        }
#if !SILVERLIGHT
        /// <summary>
        /// Get the data array to be serialized
        /// </summary>
        [OnSerializing]
        private void OnSerializing(StreamingContext context)
        {
            // save the data into the serialization array to be saved
            m_serializationArray = ToArray();
        }

        /// <summary>
        /// Construct the stack from a previously seiralized one
        /// </summary>
        [OnDeserialized]
        private void OnDeserialized(StreamingContext context)
        {
            m_locals = new ThreadLocal<ThreadLocalList>();

            ThreadLocalList list = GetThreadList(true);
            foreach (T item in m_serializationArray)
            {
                list.Add(item, false);
            }
            m_headList = list;
            m_tailList = list;

            m_serializationArray = null;
        }
#endif
        /// <summary>
        /// Gets the number of elements contained in the <see cref="ConcurrentBag{T}"/>.
        /// </summary>
        /// <value>The number of elements contained in the <see cref="ConcurrentBag{T}"/>.</value>
        /// <remarks>
        /// The count returned represents a moment-in-time snapshot of the contents
        /// of the bag.  It does not reflect any updates to the collection after 
        /// <see cref="GetEnumerator"/> was called.
        /// </remarks>
        public int Count
        {
            get
            {
                // Short path if the bag is empty
                if (m_headList == null)
                    return 0;

                bool lockTaken = false;
                try
                {
                    FreezeBag(ref lockTaken);
                    return GetCountInternal();
                }
                finally
                {
                    UnfreezeBag(lockTaken);
                }
            }
        }

        /// <summary>
        /// Gets a value that indicates whether the <see cref="ConcurrentBag{T}"/> is empty.
        /// </summary>
        /// <value>true if the <see cref="ConcurrentBag{T}"/> is empty; otherwise, false.</value>
        public bool IsEmpty
        {
            get
            {
                if (m_headList == null)
                    return true;

                bool lockTaken = false;
                try
                {
                    FreezeBag(ref lockTaken);
                    ThreadLocalList currentList = m_headList;
                    while (currentList != null)
                    {
                        if (currentList.m_head != null)
                            //at least this list is not empty, we return false
                        {
                            return false;
                        }
                        currentList = currentList.m_nextList;
                    }
                    return true;
                }
                finally
                {
                    UnfreezeBag(lockTaken);
                }
            }
        }

        /// <summary>
        /// Gets a value indicating whether access to the <see cref="T:System.Collections.ICollection"/> is
        /// synchronized with the SyncRoot.
        /// </summary>
        /// <value>true if access to the <see cref="T:System.Collections.ICollection"/> is synchronized
        /// with the SyncRoot; otherwise, false. For <see cref="ConcurrentBag{T}"/>, this property always
        /// returns false.</value>
        bool ICollection.IsSynchronized
        {
            get { return false; }
        }

        /// <summary>
        /// Gets an object that can be used to synchronize access to the <see
        /// cref="T:System.Collections.ICollection"/>. This property is not supported.
        /// </summary>
        /// <exception cref="T:System.NotSupportedException">The SyncRoot property is not supported.</exception>
        object ICollection.SyncRoot
        {
            get
            {
                throw new NotSupportedException(SR.GetString(SR.ConcurrentCollection_SyncRoot_NotSupported));
            }
        }


        /// <summary>
        ///  A global lock object, used in two cases:
        ///  1- To  maintain the m_tailList pointer for each new list addition process ( first time a thread called Add )
        ///  2- To freeze the bag in GetEnumerator, CopyTo, ToArray and Count members
        /// </summary>
        private object GlobalListsLock
        {
            get 
            {
                Contract.Assert(m_locals != null);
                return m_locals; 
            }
        }


        #region Freeze bag helper methods
        /// <summary>
        /// Local helper method to freeze all bag operations, it
        /// 1- Acquire the global lock to prevent any other thread to freeze the bag, and also new new thread can be added
        /// to the dictionary
        /// 2- Then Acquire all local lists locks to prevent steal and synchronized operations
        /// 3- Wait for all un-synchronized operations to be done
        /// </summary>
        /// <param name="lockTaken">Retrieve the lock taken result for the global lock, to be passed to Unfreeze method</param>
        private void FreezeBag(ref bool lockTaken)
        {
            Contract.Assert(!Monitor.IsEntered(GlobalListsLock));

            // global lock to be safe against multi threads calls count and corrupt m_needSync
            Monitor.Enter(GlobalListsLock, ref lockTaken);

            // This will force any future add/take operation to be synchronized
            m_needSync = true;

            //Acquire all local lists locks
            AcquireAllLocks();

            // Wait for all un-synchronized operation to be done
            WaitAllOperations();
        }

        /// <summary>
        /// Local helper method to unfreeze the bag from a frozen state
        /// </summary>
        /// <param name="lockTaken">The lock taken result from the Freeze method</param>
        private void UnfreezeBag(bool lockTaken)
        {
            ReleaseAllLocks();
            m_needSync = false;
            if (lockTaken)
            {
                Monitor.Exit(GlobalListsLock);
            }
        }

        /// <summary>
        /// local helper method to acquire all local lists locks
        /// </summary>
        private void AcquireAllLocks()
        {
            Contract.Assert(Monitor.IsEntered(GlobalListsLock));

            bool lockTaken = false;
            ThreadLocalList currentList = m_headList;
            while (currentList != null)
            {
                // Try/Finally bllock to avoid thread aport between acquiring the lock and setting the taken flag
                try
                {
                    Monitor.Enter(currentList, ref lockTaken);
                }
                finally
                {
                    if (lockTaken)
                    {
                        currentList.m_lockTaken = true;
                        lockTaken = false;
                    }
                }
                currentList = currentList.m_nextList;
            }
        }

        /// <summary>
        /// Local helper method to release all local lists locks
        /// </summary>
        private void ReleaseAllLocks()
        {
            ThreadLocalList currentList = m_headList;
            while (currentList != null)
            {

                if (currentList.m_lockTaken)
                {
                    currentList.m_lockTaken = false;
                    Monitor.Exit(currentList);
                }
                currentList = currentList.m_nextList;
            }
        }

        /// <summary>
        /// Local helper function to wait all unsynchronized operations
        /// </summary>
        private void WaitAllOperations()
        {
            Contract.Assert(Monitor.IsEntered(GlobalListsLock));

            ThreadLocalList currentList = m_headList;
            while (currentList != null)
            {
                if (currentList.m_currentOp != (int)ListOperation.None)
                {
                    SpinWait spinner = new SpinWait();
                    while (currentList.m_currentOp != (int)ListOperation.None)
                    {
                        spinner.SpinOnce();
                    }
                }
                currentList = currentList.m_nextList;
            }
        }

        /// <summary>
        /// Local helper function to get the bag count, the caller should call it from Freeze/Unfreeze block
        /// </summary>
        /// <returns>The current bag count</returns>
        private int GetCountInternal()
        {
            Contract.Assert(Monitor.IsEntered(GlobalListsLock));

            int count = 0;
            ThreadLocalList currentList = m_headList;
            while (currentList != null)
            {
                checked
                {
                    count += currentList.Count;
                }
                currentList = currentList.m_nextList;
            }
            return count;
        }

        /// <summary>
        /// Local helper function to return the bag item in a list, this is mainly used by CopyTo and ToArray
        /// This is not thread safe, should be called in Freeze/UnFreeze bag block
        /// </summary>
        /// <returns>List the contains the bag items</returns>
        private List<T> ToList()
        {
            Contract.Assert(Monitor.IsEntered(GlobalListsLock));

            List<T> list = new List<T>();
            ThreadLocalList currentList = m_headList;
            while (currentList != null)
            {
                Node currentNode = currentList.m_head;
                while (currentNode != null)
                {
                    list.Add(currentNode.m_value);
                    currentNode = currentNode.m_next;
                }
                currentList = currentList.m_nextList;
            }

            return list;
        }

        #endregion


        #region Inner Classes

        /// <summary>
        /// A class that represents a node in the lock thread list
        /// </summary>
#if !SILVERLIGHT
        [Serializable]
#endif
        internal class Node
        {
            public Node(T value)
            {
                m_value = value;
            }
            public readonly T m_value;
            public Node m_next;
            public Node m_prev;
        }

        /// <summary>
        /// A class that represents the lock thread list
        /// </summary>
        internal class ThreadLocalList
        {
            // Tead node in the list, null means the list is empty
            internal volatile Node m_head;

            // Tail node for the list
            private volatile Node m_tail;

            // The current list operation
            internal volatile int m_currentOp;

            // The list count from the Add/Take prespective
            private int m_count;

            // The stealing count
            internal int m_stealCount;

            // Next list in the dictionary values
            internal volatile ThreadLocalList m_nextList;

            // Set if the locl lock is taken
            internal bool m_lockTaken;

            // The owner thread for this list
            internal Thread m_ownerThread;

            // the version of the list, incremented only when the list changed from empty to non empty state
            internal volatile int m_version;

            /// <summary>
            /// ThreadLocalList constructor
            /// </summary>
            /// <param name="ownerThread">The owner thread for this list</param>
            internal ThreadLocalList(Thread ownerThread)
            {
                m_ownerThread = ownerThread;
            }
            /// <summary>
            /// Add new item to head of the list
            /// </summary>
            /// <param name="item">The item to add.</param>
            /// <param name="updateCount">Whether to update the count.</param>
            internal void Add(T item, bool updateCount)
            {
                checked
                {
                    m_count++;
                }
                Node node = new Node(item);
                if (m_head == null)
                {
                    Debug.Assert(m_tail == null);
                    m_head = node;
                    m_tail = node;
                    m_version++; // changing from empty state to non empty state
                }
                else
                {
                    node.m_next = m_head;
                    m_head.m_prev = node;
                    m_head = node;
                }
                if (updateCount) // update the count to avoid overflow if this add is synchronized
                {
                    m_count = m_count - m_stealCount;
                    m_stealCount = 0;
                }
            }

            /// <summary>
            /// Remove an item from the head of the list
            /// </summary>
            /// <param name="result">The removed item</param>
            internal void Remove(out T result)
            {
                Debug.Assert(m_head != null);
                Node head = m_head;
                m_head = m_head.m_next;
                if (m_head != null)
                {
                    m_head.m_prev = null;
                }
                else
                {
                    m_tail = null;
                }
                m_count--;
                result = head.m_value;

            }

            /// <summary>
            /// Peek an item from the head of the list
            /// </summary>
            /// <param name="result">the peeked item</param>
            /// <returns>True if succeeded, false otherwise</returns>
            internal bool Peek(out T result)
            {
                Node head = m_head;
                if (head != null)
                {
                    result = head.m_value;
                    return true;
                }
                result = default(T);
                return false;
            }

            /// <summary>
            /// Steal an item from the tail of the list
            /// </summary>
            /// <param name="result">the removed item</param>
            /// <param name="remove">remove or peek flag</param>
            internal void Steal(out T result, bool remove)
            {
                Node tail = m_tail;
                Debug.Assert(tail != null);
                if (remove) // Take operation
                {
                    m_tail = m_tail.m_prev;
                    if (m_tail != null)
                    {
                        m_tail.m_next = null;
                    }
                    else
                    {
                        m_head = null;
                    }
                    // Increment the steal count
                    m_stealCount++;
                }
                result = tail.m_value;
            }


            /// <summary>
            /// Gets the total list count, it's not thread safe, may provide incorrect count if it is called concurrently
            /// </summary>
            internal int Count
            {
                get
                {
                    return m_count - m_stealCount;
                }
            }
        }

        /// <summary>
        /// List operations
        /// </summary>
        internal enum ListOperation
        {
            None,
            Add,
            Take
        };
        #endregion
    }


    #region Internal Types

    /// <summary>
    /// A simple class for the debugger view window
    /// </summary>
    internal sealed class SystemThreadingCollection_IProducerConsumerCollectionDebugView<T>
    {
        IProducerConsumerCollection<T> m_collection;
        public SystemThreadingCollection_IProducerConsumerCollectionDebugView(IProducerConsumerCollection<T> collection)
        {
            if (collection == null)
            {
                throw new ArgumentNullException("collection");
            }
            m_collection = collection;
        }

        /// <summary>
        /// Returns a snapshot of the underlying collection's elements.
        /// </summary>
        [DebuggerBrowsable(DebuggerBrowsableState.RootHidden)]
        public T[] Items
        {
            get { return m_collection.ToArray(); }
        }
    }

    #endregion

}

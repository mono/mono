//------------------------------------------------------------------------------
// <copyright file="httpapplicationstate.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>                                                                
//------------------------------------------------------------------------------

/*
 * Application State Dictionary class
 * 
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web {
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Security.Permissions;

    using System.Collections;
    using System.Collections.Specialized;
    using System.Web;
    using System.Web.Util;

    //
    //  Application state collection
    //
    

    /// <devdoc>
    ///    <para>
    ///       The HttpApplicationState class enables developers to
    ///       share global information across multiple requests, sessions, and pipelines
    ///       within an ASP.NET application. (An ASP.NET application is the sum of all files, pages,
    ///       handlers, modules, and code
    ///       within the scope of a virtual directory and its
    ///       subdirectories on a single web server).
    ///    </para>
    /// </devdoc>
    public sealed class HttpApplicationState : NameObjectCollectionBase {
        // app lock with auto-unlock feature
        private HttpApplicationStateLock _lock = new HttpApplicationStateLock();

        // static object collections
        private HttpStaticObjectsCollection _applicationStaticObjects;
        private HttpStaticObjectsCollection _sessionStaticObjects;

        internal HttpApplicationState() : this(null, null) {
        }

        internal HttpApplicationState(HttpStaticObjectsCollection applicationStaticObjects,
                                      HttpStaticObjectsCollection sessionStaticObjects) 
            : base(Misc.CaseInsensitiveInvariantKeyComparer) {
            _applicationStaticObjects = applicationStaticObjects;

            if (_applicationStaticObjects == null)
                _applicationStaticObjects = new HttpStaticObjectsCollection();

            _sessionStaticObjects = sessionStaticObjects;

            if (_sessionStaticObjects == null)
                _sessionStaticObjects = new HttpStaticObjectsCollection();
        }

        //
        // Internal accessor to session static objects collection
        //

        internal HttpStaticObjectsCollection SessionStaticObjects {
            get { return _sessionStaticObjects;}
        }

        //
        // Implementation of standard collection stuff
        //


        /// <devdoc>
        ///    <para>Gets
        ///       the number of item objects in the application state collection.</para>
        /// </devdoc>
        public override int Count {
            get {
                int c = 0;
                _lock.AcquireRead(); 
                try {
                    c = base.Count;
                }
                finally {
                    _lock.ReleaseRead();
                }

                return c;
            }
        }

        // modifying methods


        /// <devdoc>
        ///    <para>
        ///       Adds
        ///       a new state object to the application state collection.
        ///    </para>
        /// </devdoc>
        public void Add(String name, Object value) {
            _lock.AcquireWrite(); 
            try {
                BaseAdd(name, value);
            }
            finally {
                _lock.ReleaseWrite();
            }
        }


        /// <devdoc>
        ///    <para>Updates an HttpApplicationState value within the collection.</para>
        /// </devdoc>
        public void Set(String name, Object value) {
            _lock.AcquireWrite(); 
            try {
                BaseSet(name, value);
            }
            finally {
                _lock.ReleaseWrite();
            }
        }


        /// <devdoc>
        ///    <para>Removes
        ///       an
        ///       object from the application state collection by name.</para>
        /// </devdoc>
        public void Remove(String name) {
            _lock.AcquireWrite(); 
            try {
                BaseRemove(name);
            }
            finally {
                _lock.ReleaseWrite();
            }
        }


        /// <devdoc>
        ///    <para>Removes
        ///       an
        ///       object from the application state collection by name.</para>
        /// </devdoc>
        public void RemoveAt(int index) {
            _lock.AcquireWrite(); 
            try {
                BaseRemoveAt(index);
            }
            finally {
                _lock.ReleaseWrite();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Removes
        ///       all objects from the application state collection.
        ///    </para>
        /// </devdoc>
        public void Clear() {
            _lock.AcquireWrite(); 
            try {
                BaseClear();
            }
            finally {
                _lock.ReleaseWrite();
            }
        }


        /// <devdoc>
        ///    <para>
        ///       Removes
        ///       all objects from the application state collection.
        ///    </para>
        /// </devdoc>
        public void RemoveAll() {
            Clear();
        }

        // access by key


        /// <devdoc>
        ///    <para>
        ///       Enables user to retrieve application state object by name.
        ///    </para>
        /// </devdoc>
        public Object Get(String name) {
            Object obj = null;
            _lock.AcquireRead(); 
            try {
                obj = BaseGet(name);
            }
            finally {
                _lock.ReleaseRead();
            }

            return obj;
        }


        /// <devdoc>
        ///    <para>Enables
        ///       a user to add/remove/update a single application state object.</para>
        /// </devdoc>
        public Object this[String name]
        {
            get { return Get(name);}
            set { Set(name, value);}
        }

        // access by index


        /// <devdoc>
        ///    <para>
        ///       Enables user
        ///       to retrieve a single application state object by index.
        ///    </para>
        /// </devdoc>
        public Object Get(int index) {
            Object obj = null;

            _lock.AcquireRead(); 
            try {
                obj = BaseGet(index);
            }
            finally {
                _lock.ReleaseRead();
            }

            return obj;
        }


        /// <devdoc>
        ///    <para>
        ///       Enables user to retrieve an application state object name by index.
        ///    </para>
        /// </devdoc>
        public String GetKey(int index) {
            String s = null;
            _lock.AcquireRead(); 
            try {
                s = BaseGetKey(index);
            }
            finally {
                _lock.ReleaseRead();
            }

            return s;
        }


        /// <devdoc>
        ///    <para>
        ///       Enables
        ///       user to retrieve an application state object by index.
        ///    </para>
        /// </devdoc>
        public Object this[int index]
        {
            get { return Get(index);}
        }

        // access to keys and values as arrays
        

        /// <devdoc>
        ///    <para>
        ///       Enables user
        ///       to retrieve all application state object names in collection.
        ///    </para>
        /// </devdoc>
        public String[] AllKeys {
            get {
                String [] allKeys = null;

                _lock.AcquireRead(); 
                try {
                    allKeys = BaseGetAllKeys();
                }
                finally {
                    _lock.ReleaseRead();
                }

                return allKeys;
            }
        }

        //
        // Public properties
        //


        /// <devdoc>
        ///    <para>
        ///       Returns "this". Provided for legacy ASP compatibility.
        ///    </para>
        /// </devdoc>
        public HttpApplicationState Contents {
            get { return this;}
        }


        /// <devdoc>
        ///    <para>
        ///       Exposes all objects declared via an &lt;object
        ///       runat=server&gt;&lt;/object&gt; tag within the ASP.NET application file.
        ///    </para>
        /// </devdoc>
        public HttpStaticObjectsCollection StaticObjects {
            get { return _applicationStaticObjects;}
        }

        //
        //  Locking support
        //


        /// <devdoc>
        ///    <para>
        ///       Locks
        ///       access to all application state variables. Facilitates access
        ///       synchronization.
        ///    </para>
        /// </devdoc>
        public void Lock() {
            _lock.AcquireWrite();
        }


        /// <devdoc>
        ///    <para>
        ///       Unocks access to all application state variables. Facilitates access
        ///       synchronization.
        ///    </para>
        /// </devdoc>
        public void UnLock() {
            _lock.ReleaseWrite();
        }

        internal void EnsureUnLock() {
            _lock.EnsureReleaseWrite();
        }
    }


    //
    //  Recursive read-write lock that allows removing of all
    //  outstanding write locks from the current thread at once
    //
    internal class HttpApplicationStateLock : ReadWriteObjectLock {
        private int _recursionCount;
        private int _threadId;

        internal HttpApplicationStateLock() {
        }

        internal override void AcquireRead() {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();

            if (_threadId != currentThreadId)
                base.AcquireRead();  // only if no write lock
        }

        internal override void ReleaseRead() {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();

            if (_threadId != currentThreadId)
                base.ReleaseRead();  // only if no write lock
        }

        internal override void AcquireWrite() {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();

            if (_threadId == currentThreadId) {
                _recursionCount++;
            }
            else {
                base.AcquireWrite();
                _threadId = currentThreadId;
                _recursionCount = 1;
            }
        }

        internal override void ReleaseWrite() {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();

            if (_threadId == currentThreadId) {
                if (--_recursionCount == 0) {
                    _threadId = 0;
                    base.ReleaseWrite();
                }
            }
        }

        //
        // release all write locks held by the current thread
        //

        internal void EnsureReleaseWrite() {
            int currentThreadId = SafeNativeMethods.GetCurrentThreadId();

            if (_threadId == currentThreadId) {
                _threadId = 0;
                _recursionCount = 0;
                base.ReleaseWrite();
            }
        }
    }

}

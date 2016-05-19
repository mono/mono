//------------------------------------------------------------------------------
// <copyright file="ConnectionPool.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net {

    using System;
    using System.Net.Sockets;
    using System.Collections;
    using System.Diagnostics;
    using System.Globalization;
    using System.Runtime.InteropServices;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;

    internal delegate void GeneralAsyncDelegate(object request, object state);
    internal delegate PooledStream CreateConnectionDelegate(ConnectionPool pool);

    /// <devdoc>
    /// <para>
    ///     Impliments basic ConnectionPooling by pooling PooledStreams
    /// </para>
    /// </devdoc>
    internal class ConnectionPool {
        private enum State {
            Initializing,
            Running,
            ShuttingDown,
        }

        private static TimerThread.Callback s_CleanupCallback = new TimerThread.Callback(CleanupCallbackWrapper);
        private static TimerThread.Callback s_CancelErrorCallback = new TimerThread.Callback(CancelErrorCallbackWrapper);
        private static TimerThread.Queue s_CancelErrorQueue = TimerThread.GetOrCreateQueue(ErrorWait);

        private const int MaxQueueSize    = (int)0x00100000;

        // The order of these is important; we want the WaitAny call to be signaled
        // for a free object before a creation signal.  Only the index first signaled
        // object is returned from the WaitAny call.
        private const int SemaphoreHandleIndex = (int)0x0;
        private const int ErrorHandleIndex     = (int)0x1;
        private const int CreationHandleIndex  = (int)0x2;

        private const int WaitTimeout   = (int)0x102;
        private const int WaitAbandoned = (int)0x80;

        private const int ErrorWait     = 5 * 1000; // 5 seconds

        private readonly TimerThread.Queue m_CleanupQueue;

        private State                    m_State;
        private InterlockedStack         m_StackOld;
        private InterlockedStack         m_StackNew;

        private int                      m_WaitCount;
        private WaitHandle[]             m_WaitHandles;

        private Exception                m_ResError;
        private volatile bool            m_ErrorOccured;

        private TimerThread.Timer        m_ErrorTimer;

        private ArrayList                m_ObjectList;
        private int                      m_TotalObjects;

        private Queue                    m_QueuedRequests;
        private Thread                   m_AsyncThread;

        private int                      m_MaxPoolSize;
        private int                      m_MinPoolSize;
        private ServicePoint             m_ServicePoint;
        private CreateConnectionDelegate m_CreateConnectionCallback;

        private Mutex CreationMutex {
            get {
                return (Mutex) m_WaitHandles[CreationHandleIndex];
            }
        }

        private ManualResetEvent ErrorEvent {
            get {
                return (ManualResetEvent) m_WaitHandles[ErrorHandleIndex];
            }
        }

        private Semaphore Semaphore {
            get {
                return (Semaphore) m_WaitHandles[SemaphoreHandleIndex];
            }
        }

        /// <summary>
        ///    <para>Constructor - binds pool with a servicePoint and sets up a cleanup Timer to remove Idle Connections</para>
        /// </summary>
        internal ConnectionPool(ServicePoint servicePoint, int maxPoolSize, int minPoolSize, int idleTimeout, CreateConnectionDelegate createConnectionCallback) : base() {
            m_State                = State.Initializing;

            m_CreateConnectionCallback = createConnectionCallback;
            m_MaxPoolSize = maxPoolSize;
            m_MinPoolSize = minPoolSize;
            m_ServicePoint = servicePoint;

            Initialize();

            if (idleTimeout > 0) {
                // special case: if the timeout value is 1 then the timer thread should have a duration
                // of 1 to avoid having the timer callback run constantly
                m_CleanupQueue = TimerThread.GetOrCreateQueue(idleTimeout == 1 ? 1 : (idleTimeout / 2));
                m_CleanupQueue.CreateTimer(s_CleanupCallback, this);
            }
        }

        /// <summary>
        ///    <para>Internal init stuff, creates stacks, queue, wait handles etc</para>
        /// </summary>
        private void Initialize() {
            m_StackOld          = new InterlockedStack();
            m_StackNew          = new InterlockedStack();

            m_QueuedRequests = new Queue();

            m_WaitHandles     = new WaitHandle[3];
            m_WaitHandles[SemaphoreHandleIndex] = new Semaphore(0, MaxQueueSize);
            m_WaitHandles[ErrorHandleIndex]     = new ManualResetEvent(false);
            m_WaitHandles[CreationHandleIndex]  = new Mutex();

            m_ErrorTimer         = null;  // No error yet.

            m_ObjectList            = new ArrayList();
            m_State = State.Running;
        }


        /// <summary>
        ///    <para>Async state object, used for storing state on async calls</para>
        /// </summary>
        private class AsyncConnectionPoolRequest {
            public AsyncConnectionPoolRequest(ConnectionPool pool, object owningObject, GeneralAsyncDelegate asyncCallback, int creationTimeout) {
                Pool = pool;
                OwningObject = owningObject;
                AsyncCallback = asyncCallback;
                CreationTimeout = creationTimeout;
            }
            public object OwningObject;
            public GeneralAsyncDelegate AsyncCallback;
            public bool Completed;
            public ConnectionPool Pool;
            public int CreationTimeout;
        }

        /// <summary>
        ///    <para>Queues a AsyncConnectionPoolRequest to our queue of requests needing
        ///     a pooled stream. If an AsyncThread is not created, we create one,
        ///     and let it process the queued items</para>
        /// </summary>
        private void QueueRequest(AsyncConnectionPoolRequest asyncRequest) {
            lock(m_QueuedRequests) {
                m_QueuedRequests.Enqueue(asyncRequest);
                if (m_AsyncThread == null) {
                    m_AsyncThread = new Thread(new ThreadStart(AsyncThread));
                    m_AsyncThread.IsBackground = true;
                    m_AsyncThread.Start();
                }
            }
        }

        /// <summary>
        ///    <para>Processes async queued requests that are blocked on needing a free pooled stream
        ///         works as follows:
        ///         1. while there are blocked requests, take one out of the queue
        ///         2. Wait for a free connection, when one becomes avail, then notify the request that its there
        ///         3. repeat 1 until there are no more queued requests
        ///         4. if there are no more requests waiting to for a free stream, then close down this thread
        ///</para>
        /// </summary>
        private void AsyncThread() {
            do {
                while (m_QueuedRequests.Count > 0) {
                    bool continueLoop = true;
                    AsyncConnectionPoolRequest asyncState = null;
                    lock (m_QueuedRequests) {
                        asyncState = (AsyncConnectionPoolRequest) m_QueuedRequests.Dequeue();
                    }

                    WaitHandle [] localWaitHandles = m_WaitHandles;
                    PooledStream PooledStream = null;
                    try {
                        while ((PooledStream == null) && continueLoop) {
                            int result = WaitHandle.WaitAny(localWaitHandles, asyncState.CreationTimeout, false);
                            PooledStream =
                                Get(asyncState.OwningObject, result, ref continueLoop, ref localWaitHandles);
                        }

                        PooledStream.Activate(asyncState.OwningObject, asyncState.AsyncCallback);
                    } catch (Exception e) {
                        if(PooledStream != null){
                            PutConnection(PooledStream, asyncState.OwningObject, asyncState.CreationTimeout, false);
                        }
                        asyncState.AsyncCallback(asyncState.OwningObject, e);
                    }
                }
                Thread.Sleep(500);
                lock(m_QueuedRequests) {
                    if (m_QueuedRequests.Count == 0) {
                        m_AsyncThread = null;
                        break;
                    }
                }
            } while (true);
        }

        /// <summary>
        ///    <para>Count of total pooled streams associated with this pool, including streams that are being used</para>
        /// </summary>
        internal int Count {
            get { return(m_TotalObjects); }
        }

        /// <summary>
        ///    <para>Our ServicePoint, used for IP resolution</para>
        /// </summary>
        internal ServicePoint ServicePoint {
            get {
                return m_ServicePoint;
            }
        }

        /// <summary>
        ///    <para>Our Max Size of outstanding pooled streams</para>
        /// </summary>
        internal int MaxPoolSize {
            get {
                return m_MaxPoolSize;
            }
        }

        /// <summary>
        ///    <para>Our Min Size of the pool to remove idled items down to</para>
        /// </summary>
        internal int MinPoolSize {
            get {
                return m_MinPoolSize;
            }
        }

        /// <summary>
        ///    <para>An Error occurred usually due to an abort</para>
        /// </summary>
        private bool ErrorOccurred {
            get { return m_ErrorOccured; }
        }

        private static void CleanupCallbackWrapper(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ConnectionPool pThis = (ConnectionPool) context;

            try
            {
                pThis.CleanupCallback();
            }
            finally
            {
                pThis.m_CleanupQueue.CreateTimer(s_CleanupCallback, context);
            }
        }

        /// <summary>
        /// Cleans up everything in both the old and new stack.  If a connection is in use
        /// then it will be on neither stack and it is the responsibility of the object 
        /// using that connection to clean it up when it is finished using it.  This does 
        /// not clean up the ConnectionPool object and new connections can still be 
        /// created if needed in the future should this ConnectionPool object be reused
        /// 
        /// preconditions: none
        /// 
        /// postconditions: any connections not currently in use by an object will be 
        /// gracefully terminated and purged from this connection pool
        /// </summary>
        internal void ForceCleanup()
        {
            if (Logging.On) {
                Logging.Enter(Logging.Web, "ConnectionPool::ForceCleanup");
            }
                      
            // If WaitOne returns false, all connections in the pool are in use 
            // so no cleanup should be performed. The last object owning
            // a connection from the pool will perform final cleanup.
            while (Count > 0) {
                if (Semaphore.WaitOne(0, false)) {
                    // Try to clean up from new stack first, if there isn't anything on new
                    // then try old.  When we lock the Semaphore, it gives us a license to 
                    // remove only one connection from the pool but it can be from either
                    // stack since if the Semaphore is locked by another thread it means that
                    // there must have been more than one connection available in either stack
                    PooledStream pooledStream = (PooledStream)m_StackNew.Pop();

                    // no streams in stack new, there must therefore be one in stack old since we
                    // were able to acquire the semaphore
                    if(pooledStream == null) {
                        pooledStream = (PooledStream)m_StackOld.Pop();
                    }

                    Debug.Assert(pooledStream != null, "Acquired Semaphore with no connections in either stack");
                    Destroy(pooledStream);   
                }
                else {
                    // couldn't get semaphore, nothing to do here
                    break;
                }
            }    
                   
            if (Logging.On) {
                Logging.Exit(Logging.Web, "ConnectionPool::ForceCleanup");
            }
        }

        /// <summary>
        ///    <para>This is called by a timer, to check for needed cleanup of idle pooled streams</para>
        /// </summary>
        private void CleanupCallback()
        {
            // Called when the cleanup-timer ticks over.
            //
            // This is the automatic prunning method.  Every period, we will perform a two-step
            // process.  First, for the objects above MinPool, we will obtain the semaphore for
            // the object and then destroy it if it was on the old stack.  We will continue this
            // until we either reach MinPool size, or we are unable to obtain a free object, or
            // until we have exhausted all the objects on the old stack.  After that, push all
            // objects on the new stack to the old stack.  So, every period the objects on the
            // old stack are destroyed and the objects on the new stack are pushed to the old
            // stack.  All objects that are currently out and in use are not on either stack.
            // With this logic, a object is prunned if unused for at least one period but not
            // more than two periods.

            // Destroy free objects above MinPool size from old stack.
            while(Count > MinPoolSize) { // While above MinPoolSize...

                // acquiring the Semaphore gives us a license to remove one and only
                // one connection from the pool
                if (Semaphore.WaitOne(0, false) ) { // != WaitTimeout
                    // We obtained a objects from the semaphore.
                    PooledStream pooledStream = (PooledStream) m_StackOld.Pop();

                    if (null != pooledStream) {
                        // If we obtained one from the old stack, destroy it.
                        Destroy(pooledStream);
                    }
                    else {
                        // Else we exhausted the old stack, so break
                        // and release the Semaphore to indicate that 
                        // no connection was actually removed so whatever
                        // we had locked is still available.
                        Semaphore.ReleaseSemaphore();
                        break;
                    }
                }
                else break;
            }

            // Push to the old-stack.  For each free object, move object from new stack
            // to old stack.  The Semaphore guarantees that we are allowed to handle
            // one connection at a time so moving a connection between stacks is safe since
            // one connection is reserved for the duration of this loop and we only touch
            // one connection at a time on the new stack
            if(Semaphore.WaitOne(0, false)) { //  != WaitTimeout
                for(;;) {
                    PooledStream pooledStream = (PooledStream) m_StackNew.Pop();

                    if (null == pooledStream)
                        break;

                    GlobalLog.Assert(!pooledStream.IsEmancipated, "Pooled object not in pool.");
                    GlobalLog.Assert(pooledStream.CanBePooled, "Pooled object is not poolable.");

                    m_StackOld.Push(pooledStream);
                }
                // no connections were actually destroyed so signal that a connection is now
                // available since we are no longer reserving a connection by holding the 
                // Semaphore
                Semaphore.ReleaseSemaphore();
            }
        }

        /// <summary>
        ///    <para>Creates a new PooledStream, performs checks as well on the new stream</para>
        /// </summary>
        private PooledStream Create(CreateConnectionDelegate createConnectionCallback) {
            GlobalLog.Enter("ConnectionPool#" + ValidationHelper.HashString(this) + "::Create");
            PooledStream newObj = null;

            try {
                newObj = createConnectionCallback(this);

                if (null == newObj)
                    throw new InternalException();    // Create succeeded, but null object

                if (!newObj.CanBePooled)
                    throw new InternalException();    // Create succeeded, but non-poolable object

                newObj.PrePush(null);

                lock (m_ObjectList.SyncRoot) {
                    m_ObjectList.Add(newObj);
                    m_TotalObjects = m_ObjectList.Count;
                }

                GlobalLog.Print("Create pooledStream#"+ValidationHelper.HashString(newObj));
            }
            catch(Exception e)  {
                GlobalLog.Print("Pool Exception: " + e.Message);

                newObj = null; // set to null, so we do not return bad new object
                // Failed to create instance
                m_ResError = e;
                Abort();
            }            
            GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::Create",ValidationHelper.HashString(newObj));
            return newObj;
        }


        /// <summary>
        ///    <para>Destroys a pooled stream from the pool</para>
        /// </summary>
        private void Destroy(PooledStream pooledStream) {
            GlobalLog.Print("Destroy pooledStream#"+ValidationHelper.HashString(pooledStream));

            if (null != pooledStream) {
                try
                {
                    lock (m_ObjectList.SyncRoot) {
                        m_ObjectList.Remove(pooledStream);
                        m_TotalObjects = m_ObjectList.Count;
                    }
                }
                finally
                {
                    pooledStream.Dispose();
                }
            }
        }

        private static void CancelErrorCallbackWrapper(TimerThread.Timer timer, int timeNoticed, object context)
        {
            ((ConnectionPool) context).CancelErrorCallback();
        }

        /// <summary>
        ///    <para>Called on error, after we waited a set amount of time from aborting</para>
        /// </summary>
        private void CancelErrorCallback()
        {
            TimerThread.Timer timer = m_ErrorTimer;
            if (timer != null && timer.Cancel())
            {
                m_ErrorOccured = false;
                ErrorEvent.Reset();
                m_ErrorTimer = null;
                m_ResError = null;
            }
        }

        /// <summary>
        ///    <para>Retrieves a pooled stream from the pool proper
        ///     this work by first attemting to find something in the pool on the New stack
        ///     and then trying the Old stack if something is not there availble </para>
        /// </summary>
        private PooledStream GetFromPool(object owningObject) {
            PooledStream res = null;
            GlobalLog.Enter("ConnectionPool#" + ValidationHelper.HashString(this) + "::GetFromPool");
            res = (PooledStream) m_StackNew.Pop();
            if (null == res) {
                res = (PooledStream) m_StackOld.Pop();
            }

            // The semaphore guaranteed that a connection was available so if res is
            // null it means that this contract has been violated somewhere
            GlobalLog.Assert(res != null, "GetFromPool called with nothing in the pool!");

            if (null != res) {
                res.PostPop(owningObject);
                GlobalLog.Print("GetFromGeneralPool pooledStream#"+ValidationHelper.HashString(res));
            }

            GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::GetFromPool",ValidationHelper.HashString(res));
            return(res);
        }

        /// <summary>
        ///    <para>Retrieves the pooled stream out of the pool, does this by using the result
        ///    of a WaitAny as input, and then based on whether it has a mutex, event, semaphore,
        ///     or timeout decides what action to take</para>
        /// </summary>
        private PooledStream Get(object owningObject, int result, ref bool continueLoop, ref WaitHandle [] waitHandles) {
                PooledStream pooledStream = null;
                GlobalLog.Enter("ConnectionPool#" + ValidationHelper.HashString(this) + "::Get", result.ToString());


                // From the WaitAny docs: "If more than one object became signaled during
                // the call, this is the array index of the signaled object with the
                // smallest index value of all the signaled objects."  This is important
                // so that the free object signal will be returned before a creation
                // signal.

                switch (result) {
                case WaitTimeout:
                    Interlocked.Decrement(ref m_WaitCount);
                    continueLoop = false;
                    GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::Get","throw Timeout WebException");
                    throw new WebException(NetRes.GetWebStatusString("net_timeout", WebExceptionStatus.ConnectFailure), WebExceptionStatus.Timeout);


                case ErrorHandleIndex:
                    // Throw the error that PoolCreateRequest stashed.
                    int newWaitCount = Interlocked.Decrement(ref m_WaitCount);
                    continueLoop = false;
                    Exception exceptionToThrow = m_ResError;
                    if (newWaitCount == 0) {
                        CancelErrorCallback();
                    }
                    throw exceptionToThrow;

                // The creation mutex signaled, which means no connections are available in 
                // the connection pool.  This means you might be able to create a connection.
                case CreationHandleIndex:
                    try {
                        continueLoop = true;
                        // try creating a new connection
                        pooledStream = UserCreateRequest();

                        if (null != pooledStream) {
                            pooledStream.PostPop(owningObject);
                            Interlocked.Decrement(ref m_WaitCount);
                            continueLoop = false;

                        }
                        else {
                            // If we were not able to create an object, check to see if
                            // we reached MaxPoolSize.  If so, we will no longer wait on
                            // the CreationHandle, but instead wait for a free object or
                            // the timeout.

                            // Consider changing: if we receive the CreationHandle midway into the wait
                            // period and re-wait, we will be waiting on the full period
                            if (Count >= MaxPoolSize && 0 != MaxPoolSize) {
                                if (!ReclaimEmancipatedObjects()) {
                                    // modify handle array not to wait on creation mutex anymore
                                    waitHandles    = new WaitHandle[2];
                                    waitHandles[0] = m_WaitHandles[0];
                                    waitHandles[1] = m_WaitHandles[1];
                                }
                            }

                        }
                    }
                    finally {
                        CreationMutex.ReleaseMutex();
                    }
                    break;

                default:
                    // the semaphore was signaled which can only happen
                    // when a connection has been placed in the pool
                    // so there is guaranteed available inventory
                    Interlocked.Decrement(ref m_WaitCount);
                    pooledStream = GetFromPool(owningObject);
                    continueLoop = false;
                    break;
                }
                GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::Get",ValidationHelper.HashString(pooledStream));
                return pooledStream;
        }

        /// <devdoc>
        ///    <para>Aborts the queued requests to the pool</para>
        /// </devdoc>
        internal void Abort() {
            if (m_ResError == null) {
                m_ResError = new WebException(
                        NetRes.GetWebStatusString("net_requestaborted", WebExceptionStatus.RequestCanceled),
                        WebExceptionStatus.RequestCanceled);
            }
            ErrorEvent.Set();
            m_ErrorOccured = true;
            m_ErrorTimer = s_CancelErrorQueue.CreateTimer(s_CancelErrorCallback, this);
        }

        /// <devdoc>
        ///    <para>Attempts to create a PooledStream, by trying to get a pooled Connection,
        ///         or by creating its own new one</para>
        /// </devdoc>
        internal PooledStream GetConnection(object owningObject, 
                                            GeneralAsyncDelegate asyncCallback, 
                                            int creationTimeout) {
            int result;
            PooledStream stream = null;
            bool continueLoop = true;
            bool async = (asyncCallback != null) ? true : false;

            GlobalLog.Enter("ConnectionPool#" + ValidationHelper.HashString(this) + "::GetConnection");

            if(m_State != State.Running) {
                throw new InternalException();
            }

            Interlocked.Increment(ref m_WaitCount);
            WaitHandle[] localWaitHandles = m_WaitHandles;

            if (async) {
                result = WaitHandle.WaitAny(localWaitHandles, 0, false);
                if (result != WaitTimeout) {
                    stream = Get(owningObject, result, ref continueLoop, ref localWaitHandles);
                }
                if (stream == null) {
                    GlobalLog.Print("GetConnection:"+ValidationHelper.HashString(this)+" going async");
                    AsyncConnectionPoolRequest asyncState = new AsyncConnectionPoolRequest(this, owningObject, asyncCallback, creationTimeout);
                    QueueRequest(asyncState);
                }
            } else {
                // loop while we don't have an error/timeout and we haven't gotten a stream yet
                while ((stream == null) && continueLoop) {
                    result = WaitHandle.WaitAny(localWaitHandles, creationTimeout, false);
                    stream = Get(owningObject, result, ref continueLoop, ref localWaitHandles);
                }
            }

            if (null != stream) {
                // if there is already a stream, then we're not going async
                if (!stream.IsInitalizing) {
                    asyncCallback = null;
                }

                try{
                    // If activate returns false, it is going to finish asynchronously 
                    // and therefore the stream will be returned in a callback and
                    // we should not return it here (return null)
                    if (stream.Activate(owningObject, asyncCallback) == false)
                        stream = null;
                }
                catch{
                    PutConnection(stream,owningObject,creationTimeout, false);
                    throw;
                }
            } else if (!async) {
                throw new InternalException();
            }

            GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::GetConnection", ValidationHelper.HashString(stream));
            return(stream);
        }

        /// <devdoc>
        ///     <para>
        ///     Attempts to return a PooledStream to the pool.  Default is that it can be reused if it can 
        ///     also be pooled.
        ///     </para>
        /// </devdoc>
        internal void PutConnection(PooledStream pooledStream, object owningObject, int creationTimeout)
        {
            // ok to reuse
            PutConnection(pooledStream, owningObject, creationTimeout, true);
        }

        /// <devdoc>
        ///    <para>
        ///    Attempts to return a PooledStream to the pool.  If canReuse is false, then the 
        ///    connection will be destroyed even if it is marked as reusable and a new conneciton will
        ///    be created.  If it is true, then the connection will still be checked to ensure that
        ///    it can be pooled and will be cleaned up if it can not for another reason.
        ///    </para>
        /// </devdoc>
        internal void PutConnection(PooledStream pooledStream, object owningObject, int creationTimeout, bool canReuse) {
            GlobalLog.Print("ConnectionPool#" + ValidationHelper.HashString(this) + "::PutConnection");
            if (pooledStream == null) {
                throw new ArgumentNullException("pooledStream");
            }

            pooledStream.PrePush(owningObject);

            if (m_State != State.ShuttingDown) {
                pooledStream.Deactivate();

                // cancel our error status, if we have no new requests waiting anymore
                if (m_WaitCount == 0) {
                    CancelErrorCallback();
                }

                if (canReuse && pooledStream.CanBePooled) {
                    PutNew(pooledStream);
                }
                else {
                    try {
                        Destroy(pooledStream);
                    } finally { // Make sure to release the mutex even under error conditions.
                        // Make sure we recreate a new pooled stream, if there are requests for a stream
                        // at this point
                        if (m_WaitCount > 0) {
                            if (!CreationMutex.WaitOne(creationTimeout, false)) {
                                Abort();
                            } else {
                                try {
                                    pooledStream = UserCreateRequest();
                                    if (null != pooledStream) {
                                        PutNew(pooledStream);
                                    }
                                } finally {
                                    CreationMutex.ReleaseMutex();
                                }
                            }
                        }
                    }
                }
            }
            else {
                // If we're shutting down, we destroy the object.
                Destroy(pooledStream);
            }
        }


        /// <devdoc>
        ///    <para>Places a new/reusable stream in the new stack of the pool</para>
        /// </devdoc>
        private void PutNew(PooledStream pooledStream) {
            GlobalLog.Enter("ConnectionPool#" + ValidationHelper.HashString(this) + "::PutNew", "#"+ValidationHelper.HashString(pooledStream));

            GlobalLog.Assert(null != pooledStream, "Why are we adding a null object to the pool?");
            GlobalLog.Assert(pooledStream.CanBePooled, "Non-poolable object in pool.");

            m_StackNew.Push(pooledStream);
            // ensure that the semaphore's count is incremented to signal an available connection is in
            // the pool
            Semaphore.ReleaseSemaphore();
            GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::PutNew");
        }


        /// <devdoc>
        ///    <para>Reclaim any pooled Streams that have seen their users/WebRequests GCed away</para>
        /// </devdoc>
        private bool ReclaimEmancipatedObjects() {
            bool emancipatedObjectFound = false;
            GlobalLog.Enter("ConnectionPool#" + ValidationHelper.HashString(this) + "::ReclaimEmancipatedObjects");

            lock(m_ObjectList.SyncRoot) {

                object[] objectList = m_ObjectList.ToArray();
                if (null != objectList) {

                    for (int i = 0; i < objectList.Length; ++i) {
                        PooledStream pooledStream = (PooledStream) objectList[i];

                        if (null != pooledStream) {
                            bool locked = false;

                            try {
                                Monitor.TryEnter(pooledStream, ref locked);

                                if (locked) {
                                    if (pooledStream.IsEmancipated) {

                                        GlobalLog.Print("EmancipatedObject pooledStream#"+ValidationHelper.HashString(pooledStream));
                                        PutConnection(pooledStream, null, Timeout.Infinite);
                                        emancipatedObjectFound = true;
                                    }
                                }
                            }
                            finally {
                                if (locked)
                                    Monitor.Exit(pooledStream);
                            }
                        }
                    }
                }
            }
            GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::ReclaimEmancipatedObjects",emancipatedObjectFound.ToString());
            return emancipatedObjectFound;
        }

        /// <devdoc>
        ///    <para>Creates a new PooledStream is allowable</para>
        /// </devdoc>
        private PooledStream UserCreateRequest() {
            // called by user when they were not able to obtain a free object but
            // instead obtained creation mutex
            GlobalLog.Enter("ConnectionPool#" + ValidationHelper.HashString(this) + "::UserCreateRequest");

            PooledStream pooledStream = null;

            if (!ErrorOccurred) {
                 if (Count < MaxPoolSize || 0 == MaxPoolSize) {
                    // If we have an odd number of total objects, reclaim any dead objects.
                    // If we did not find any objects to reclaim, create a new one.

                    // 
                     if ((Count & 0x1) == 0x1 || !ReclaimEmancipatedObjects())
                        pooledStream = Create(m_CreateConnectionCallback);
                }
            }
            GlobalLog.Leave("ConnectionPool#" + ValidationHelper.HashString(this) + "::UserCreateRequest", ValidationHelper.HashString(pooledStream));
            return pooledStream;
        }
    }


    /// <devdoc>
    ///    <para>Used to Pool streams in a thread safe manner</para>
    /// </devdoc>
    sealed internal class InterlockedStack {
        private readonly Stack _stack = new Stack();
        private int _count;

#if DEBUG
        private readonly Hashtable doublepush = new Hashtable();
#endif

        internal InterlockedStack() {
        }

        internal void Push(Object pooledStream) {
            GlobalLog.Assert(null != pooledStream, "push null");
            if (null == pooledStream) { throw new ArgumentNullException("pooledStream"); }
            lock(_stack.SyncRoot) {
#if DEBUG
                GlobalLog.Assert(null == doublepush[pooledStream], "object already in stack");
                doublepush[pooledStream] = _stack.Count;
#endif
                _stack.Push(pooledStream);
#if DEBUG
                GlobalLog.Assert(_count+1 == _stack.Count, "push count mishandle");
#endif
                _count = _stack.Count;
            }
        }

        internal Object Pop() {
            lock(_stack.SyncRoot) {
                object pooledStream = null;
                if (0 <_stack.Count) {
                    pooledStream = _stack.Pop();
#if DEBUG
                    GlobalLog.Assert(_count-1 == _stack.Count, "pop count mishandle");
                    doublepush.Remove(pooledStream);
#endif
                    _count = _stack.Count;
                }
                return pooledStream;
            }
        }
    }

}


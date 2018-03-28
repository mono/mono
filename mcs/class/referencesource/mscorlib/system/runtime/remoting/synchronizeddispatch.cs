// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
//   Synchronization Property for URT Contexts. Uses the ThreadPool API.
//   An instance of this property in a context enforces a synchronization
//   domain for the context (and all contexts that share the same instance).
//   This means that at any instant, at most 1 thread could be executing
//   in all contexts that share an instance of this property.
//
//   This is done by contributing sinks that intercept and serialize in-coming
//   calls for the respective contexts.
//
//   If the property is marked for re-entrancy, then call-outs are 
//   intercepted too. The call-out interception allows other waiting threads
//   to enter the synchronization domain for maximal throughput.
//   
namespace System.Runtime.Remoting.Contexts {
    using System.Threading;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Activation;
    using System.Security.Permissions;
    using System;
    using System.Diagnostics.Contracts;
    using Queue = System.Collections.Queue;
    using ArrayList = System.Collections.ArrayList;
    [System.Security.SecurityCritical]  // auto-generated_required
    [Serializable]
    [AttributeUsage(AttributeTargets.Class)]
    [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]    
    [System.Runtime.InteropServices.ComVisible(true)]
    public class SynchronizationAttribute
        : ContextAttribute, IContributeServerContextSink, 
                    IContributeClientContextSink
    {
        // The class should not be instantiated in a context that has Synchronization
        public const int NOT_SUPPORTED  = 0x00000001;
        
        // The class does not care if the context has Synchronization or not
        public const int SUPPORTED      = 0x00000002;
    
        // The class should be instantiated in a context that has Synchronization
        public const int REQUIRED    = 0x00000004;
    
        // The class should be instantiated in a context with a new instance of 
        // Synchronization property each time
        public const int REQUIRES_NEW = 0x00000008;
    
        private const String PROPERTY_NAME = "Synchronization";
    
        private static readonly int _timeOut = -1;
        // event that releases a thread-pool worker thread
        [NonSerialized]
        internal AutoResetEvent _asyncWorkEvent;
        [NonSerialized]
        private RegisteredWaitHandle _waitHandle;

        // queue of work items.
        [NonSerialized]
        internal Queue _workItemQueue;
        // flag for the domain lock (access always synchronized on the _workItemQueue)
        [NonSerialized]
        internal bool _locked;
        // flag to indicate if the lock should be released during call-outs
        internal bool _bReEntrant;
        // flag for use as an attribute on types
        internal int _flavor;

        [NonSerialized]
        private SynchronizationAttribute _cliCtxAttr;
        // Logical call id (used only in non-reentrant case for deadlock avoidance)
        [NonSerialized]
        private String _syncLcid;
        [NonSerialized]
        private ArrayList _asyncLcidList;
        
    
        public virtual bool Locked {get { return _locked;} set { _locked=value; } } 
        public virtual bool IsReEntrant { get { return _bReEntrant;} }  

        internal String SyncCallOutLCID
        {
            get 
            { 
                Contract.Assert(
                    !_bReEntrant, 
                    "Should not use this for the reentrant case");
                    
                return _syncLcid;
            }

            set
            {
                Contract.Assert(
                    !_bReEntrant, 
                    "Should not use this for the reentrant case");

                Contract.Assert(
                    _syncLcid==null 
                        || (_syncLcid!=null && value==null) 
                        || _syncLcid.Equals(value), 
                    "context can be associated with one logical call at a time");
                
                _syncLcid = value;
            }
        }

        internal ArrayList AsyncCallOutLCIDList
        {
            get { return _asyncLcidList; }
        }

        internal bool IsKnownLCID(IMessage reqMsg)
        {
            String msgLCID = 
                ((LogicalCallContext)reqMsg.Properties[Message.CallContextKey])
                    .RemotingData.LogicalCallID;
            return ( msgLCID.Equals(_syncLcid)
                    || _asyncLcidList.Contains(msgLCID));
            
        }

    
        /*
        *   Constructor for the synchronized dispatch property
        */
        public SynchronizationAttribute()
        
            : this(REQUIRED, false) {
        }
    
        /*
        *   Constructor. 
        *   If reEntrant is true, we allow other calls to come in
        *   if the currently running call leaves the domain for a call-out.
        */
        public SynchronizationAttribute(bool reEntrant)
        
            : this(REQUIRED, reEntrant) {
        }
    
        public SynchronizationAttribute(int flag)
        
            : this(flag, false) {
        }
    
        public SynchronizationAttribute(int flag, bool reEntrant)
        
            // Invoke ContextProperty ctor!
            : base(PROPERTY_NAME) {
            
            _bReEntrant = reEntrant;
    
            switch (flag)
            {
            case NOT_SUPPORTED:
            case SUPPORTED:
            case REQUIRED:
            case REQUIRES_NEW:
                _flavor = flag;
                break;
            default:
                throw new ArgumentException(Environment.GetResourceString("Argument_InvalidFlag"), "flag");
            }
        }
    
        // Dispose off the WaitHandle registered in Initialization
        internal void Dispose()
        {
            //Unregister the RegisteredWaitHandle
            if (_waitHandle != null)
                _waitHandle.Unregister(null);
        }

        // Override ContextAttribute's implementation of IContextAttribute::IsContextOK
        [System.Security.SecurityCritical]
        [System.Runtime.InteropServices.ComVisible(true)]
        public override bool IsContextOK(Context ctx, IConstructionCallMessage msg)
        {
            if (ctx == null)
                throw new ArgumentNullException("ctx");
            if (msg == null)
                throw new ArgumentNullException("msg");
            Contract.EndContractBlock();

            // <

            bool isOK = true;
            if (_flavor == REQUIRES_NEW)
            {
                isOK = false;
                // Each activation request instantiates a new attribute class.
                // We are relying on that for the REQUIRES_NEW case!
                Contract.Assert(ctx.GetProperty(PROPERTY_NAME) != this,
                    "ctx.GetProperty(PROPERTY_NAME) != this");
            }
            else
            {
                SynchronizationAttribute syncProp = (SynchronizationAttribute) ctx.GetProperty(PROPERTY_NAME);
                if (   ( (_flavor == NOT_SUPPORTED)&&(syncProp != null) )
                    || ( (_flavor == REQUIRED)&&(syncProp == null) )
                    )
                {
                    isOK = false;
                }

                if (_flavor == REQUIRED)
                {
                    // pick up the property from the current context
                    _cliCtxAttr = syncProp;
                }
            }
            return isOK;
        }
    
        // Override ContextAttribute's impl. of IContextAttribute::GetPropForNewCtx
        [System.Security.SecurityCritical]
        [System.Runtime.InteropServices.ComVisible(true)]
        public override void GetPropertiesForNewContext(IConstructionCallMessage ctorMsg)
        {
            if ( (_flavor==NOT_SUPPORTED) || (_flavor==SUPPORTED) || (null == ctorMsg) )
            {
                return ;
            }

            if (_cliCtxAttr != null)
            {
                Contract.Assert(_flavor == REQUIRED,"Use cli-ctx property only for the REQUIRED flavor");
                ctorMsg.ContextProperties.Add((IContextProperty)_cliCtxAttr);
                _cliCtxAttr = null;
            }
            else
            {
                ctorMsg.ContextProperties.Add((IContextProperty)this);
            }
        }
    
        // We need this to make the use of the property as an attribute 
        // light-weight. This allows us to delay initialize everything we
        // need to fully function as a ContextProperty.
        internal virtual void InitIfNecessary()
        {
            lock(this) 
            {
                if (_asyncWorkEvent == null)
                {
                    // initialize thread pool event to non-signaled state.
                    _asyncWorkEvent = new AutoResetEvent(false);
        
                    _workItemQueue = new Queue();
                    _asyncLcidList = new ArrayList();
                    
                    WaitOrTimerCallback callBackDelegate = 
                        new WaitOrTimerCallback(this.DispatcherCallBack);
        
                    // Register a callback to be executed by the thread-pool
                    // each time the event is signaled.
                    _waitHandle = ThreadPool.RegisterWaitForSingleObject(
                                    _asyncWorkEvent, 
                                    callBackDelegate, 
                                    null, // state info
                                    _timeOut, 
                                    false); // bExecuteOnlyOnce
                }
            }
        }
    
        /* 
        * Call back function -- executed for each work item that 
        * was enqueued. This is invoked by a thread-pool thread for
        * async work items and the caller thread for sync items.
        */
        private void DispatcherCallBack(Object stateIgnored, bool ignored)
        {
            // This function should be called by only one thread at a time. We will 
            // ensure this by releasing exactly one waiting thread to go work on 
            // a WorkItem

            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] --- In DispatherCallBack ");

            Contract.Assert(_locked==true,"_locked==true");
            WorkItem work;     
            // get the work item out of the queue.
            lock (_workItemQueue)
            {
                work = (WorkItem) _workItemQueue.Dequeue();
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] --- Dequeued Work for: " + work._thread.GetHashCode());
            }
            Contract.Assert(work!=null,"work!=null");
            Contract.Assert(work.IsSignaled() && !(work.IsDummy()),"work.IsSignaled() && !(work.IsDummy())");
            // execute the work item (WorkItem.Execute will switch to the proper context)
            ExecuteWorkItem(work);
            HandleWorkCompletion();
            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] --- CallBack finished for: " + work._thread.GetHashCode());
        }
    
        /*
        *   This is used by the call-out (client context) sinks to notify 
        *   the domain manager that the thread is leaving
        */
        internal virtual void HandleThreadExit()
        {
            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~~ Thread EXIT ~~~~");
            // For now treat this as if the work was completed!
            Contract.Assert(_locked==true,"_locked==true");
            HandleWorkCompletion();    
        }
    
        /* 
        *   This is used by a returning call-out thread to request
        *   that it be queued for re-entry into the domain.
        */
        internal virtual void HandleThreadReEntry()
        {
            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~~ Thread REQUEST REENTRY ~~~~");
            // Treat this as if a new work item needs to be done
            // <

            WorkItem work = new WorkItem(null, null, null);
            work.SetDummy();
            HandleWorkRequest(work);
        }
    
        /*
        *   This gets called at the end of work.Execute and from 
        *   HandleThreadExit() in the re-entrant scenario.
        *   This is the point where we decide what to do next!
        */
        internal virtual void HandleWorkCompletion()
        {
            // We should still have the lock held for the workItem that just completed
            Contract.Assert(_locked==true,"_locked==true");
            // Now we check the queue to see if we need to release any one?
            WorkItem nextWork = null;
            bool bNotify = false;
            lock (_workItemQueue)
            {      
                if (_workItemQueue.Count >= 1)
                {
                    nextWork = (WorkItem) _workItemQueue.Peek();
                    bNotify = true;
                    nextWork.SetSignaled();
                }
                else
                {
                    // We set locked to false only in the case there is no
                    // next work to be done.
                    // NOTE: this is the only place _locked in ever set to false!
                    //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] Domain UNLOCKED!");
                    _locked = false;
                }
            }
            // See if we found a non-signaled work item at the head. 
            if (bNotify)
            {
                // In both sync and async cases we just hand off the _locked state to
                // the next thread which will execute.
                if (nextWork.IsAsync())
                {
                    // Async-WorkItem: signal ThreadPool event to release one thread
                    _asyncWorkEvent.Set();
                    //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ### Signal " + nextWork._thread.GetHashCode() + (nextWork.IsDummy()?" DUMMY ":" REAL "));
                }
                else
                {
                    // Sync-WorkItem: notify the waiting sync-thread.
                    lock(nextWork)
                    {
                        Monitor.Pulse(nextWork);
                        //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~ Notify " + nextWork._thread.GetHashCode() + (nextWork.IsDummy()?" DUMMY ":" REAL ") );
                    }
                }
            }
        }
    
        /*
        *   This is called by any new incoming thread or from
        *   HandleThreadReEntry() when a call-out thread wants to
        *   re-enter the domain. 
        *   In the latter case, the WorkItem is a dummy item, it
        *   just serves the purpose of something to block on till
        *   the thread is given a green signal to re-enter.
        */
        internal virtual void HandleWorkRequest(WorkItem work)
        {
            // <


            bool bQueued;

            // Check for nested call backs
            if (!IsNestedCall(work._reqMsg))
            {
                // See what type of work it is
                if (work.IsAsync()) 
                {
                    // Async work is always queued.
                    bQueued = true;
                    // Enqueue the workItem
                    lock (_workItemQueue)
                    {
                        //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ### Async Item EnQueue " + work._thread.GetHashCode());
                        work.SetWaiting();
                        _workItemQueue.Enqueue(work);
                        // If this is the only work item in the queue we will
                        // have to trigger the thread-pool event ourselves
                        if ( (!_locked) && (_workItemQueue.Count == 1) )
                        {
                            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ### Async Signal Self: " + work._thread.GetHashCode());
                            work.SetSignaled();
                            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ### Domain Locked!");
                            _locked = true;
                            _asyncWorkEvent.Set();                    
                        }
                    }
                }
                else
                {        
                    // Sync work is queued only if there are other items
                    // already in the queue.
                    lock(work)
                    {
                        // Enqueue if we need to
                        lock(_workItemQueue)
                        {
                            if ((!_locked) && (_workItemQueue.Count == 0))
                            {                    
                                _locked = true;
                                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ### Domain Locked!");
                                bQueued = false;
                            }
                            else
                            {
                                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~ ENQUEUE Sync!" + (work.IsDummy()?" DUMMY ":" REAL ") + work._thread);
                                bQueued = true;
                                work.SetWaiting();
                                _workItemQueue.Enqueue(work);
                            }
                        }
                        
                        if (bQueued == true)
                        {
                            // If we queued a work item we must wait for some
                            // other thread to peek at it and Notify us.
                            
                            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~ WORK::WAIT" + work._thread);
                            Monitor.Wait(work);
                            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~ FINISH Work::WAIT" + work._thread);
                            Contract.Assert(_locked==true,"_locked==true");
                            // Our turn to complete the work! 
                            // Execute the callBack only if this is real work
                            if (!work.IsDummy())
                            {
                                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~ Invoke DispatcherCallBack " + work._thread);
                                // We invoke the callback here that does exactly
                                // what we need to do ... dequeue work, execute, checkForMore
                                DispatcherCallBack(null, true);
                            }
                            else
                            {
                                // DummyWork is just use to block/unblock a returning call.
                                // Throw away our dummy WorkItem. 
                                lock(_workItemQueue)
                                {
                                    _workItemQueue.Dequeue();
                                }
                                // We don't check for more work here since we are already 
                                // in the midst of an executing WorkItem (at the end of which
                                // the check will be performed)
                            }
                        }
                        else
                        {
                            // We did not queue the work item.
                            if (!work.IsDummy())
                            {
                                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~ Execute direct" + work._thread);
                                // Execute the work.
                                Contract.Assert(_locked==true,"_locked==true");
                                work.SetSignaled();
                                ExecuteWorkItem(work);
                                // Check for more work
                                HandleWorkCompletion();
                            }
                        }
                    }
                }
            }    
            else
            {
                // We allow the nested calls to execute directly                
                
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] ~~~ Execute Nested Call direct" + work._thread);
                // Execute the work.
                Contract.Assert(_locked==true,"_locked==true");
                work.SetSignaled();
                work.Execute();
                // We are still inside the top level call ...
                // so after work.Execute finishes we don't check for more work
                // or unlock the domain as we do elsewhere.
            }            
        }

        internal void ExecuteWorkItem(WorkItem work)
        {
            work.Execute();
        }

        internal bool IsNestedCall(IMessage reqMsg)
        {
            // This returns TRUE only if it is a non-reEntrant context
            // AND 
            // (the LCID of the reqMsg matches that of 
            // the top level sync call lcid associated with the context.
            //  OR
            // it matches one of the async call out lcids)
            
            bool bNested = false;
            if (!IsReEntrant)
            {
                String lcid = SyncCallOutLCID;                
                if (lcid != null)
                {
                    // This means we are inside a top level call
                    LogicalCallContext callCtx = 
                        (LogicalCallContext)reqMsg.Properties[Message.CallContextKey];
                        
                    if ( callCtx!=null && 
                        lcid.Equals(callCtx.RemotingData.LogicalCallID))
                    {
                        // This is a nested call (we made a call out during
                        // the top level call and eventually that has resulted 
                        // in an incoming call with the same lcid)
                        bNested = true;
                    }                    
                }
                if (!bNested && AsyncCallOutLCIDList.Count>0)
                {
                    // This means we are inside a top level call
                    LogicalCallContext callCtx = 
                        (LogicalCallContext)reqMsg.Properties[Message.CallContextKey];
                    if (AsyncCallOutLCIDList.Contains(callCtx.RemotingData.LogicalCallID))
                    {
                        bNested = true;
                    }
                }
            }
            return bNested;
        }
        
        
        /*
        *   Implements IContributeServerContextSink::GetServerContextSink
        *   Create a SynchronizedDispatch sink and return it.
        */
        [System.Security.SecurityCritical]
        public virtual IMessageSink GetServerContextSink(IMessageSink nextSink)
        {
            InitIfNecessary();
            
            SynchronizedServerContextSink propertySink = 
                new SynchronizedServerContextSink(
                            this,   
                            nextSink);
                            
            return (IMessageSink) propertySink;
        }
    
        /*
        *   Implements IContributeClientContextSink::GetClientContextSink
        *   Create a CallOut sink and return it.
        */
        [System.Security.SecurityCritical]
        public virtual IMessageSink GetClientContextSink(IMessageSink nextSink)
        {
            InitIfNecessary();
            
            SynchronizedClientContextSink propertySink = 
                new SynchronizedClientContextSink(
                            this,
                            nextSink);
                                                                        
            return (IMessageSink) propertySink;
        }
        
    }
    
    /*************************************** SERVER SINK ********************************/
    /*
    *   Implements the sink contributed by the Synch-Dispatch
    *   Property. The sink holds a back pointer to the property.
    *   The sink intercepts incoming calls to objects resident in
    *   the Context and co-ordinates with the property to enforce
    *   the domain policy.
    */
    internal class SynchronizedServerContextSink
            : InternalSink, IMessageSink
    {
        internal IMessageSink   _nextSink;
        [System.Security.SecurityCritical] // auto-generated
        internal SynchronizationAttribute _property;
    
        [System.Security.SecurityCritical]  // auto-generated
        internal SynchronizedServerContextSink(SynchronizationAttribute prop, IMessageSink nextSink)
        {
            _property = prop;
            _nextSink = nextSink;
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        ~SynchronizedServerContextSink()
        {
            _property.Dispose();
        }
        
        /*
        * Implements IMessageSink::SyncProcessMessage
        */
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {
            // 1. Create a work item 
            WorkItem work = new WorkItem(reqMsg,
                                        _nextSink,
                                        null /* replySink */);
    
            // 2. Notify the property to handle the WorkItem
            // The work item may get put in a Queue or may execute directly
            // if the domain is free.
            _property.HandleWorkRequest(work);
    
            // 3. Pick up retMsg from the WorkItem and return
            return work.ReplyMessage;
        }
    
        /*
        *   Implements IMessageSink::AsyncProcessMessage
        */
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink) 
        {
            // 1. Create a work item 
            WorkItem work = new WorkItem(reqMsg,
                                        _nextSink,
                                        replySink);
            work.SetAsync();
            // 2. We always queue the work item in async case
            _property.HandleWorkRequest(work); 
            // 3. Return an IMsgCtrl
            return null;    
        }
    
        /*  
        * Implements IMessageSink::GetNextSink
        */
        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                return _nextSink;
            }
        }
    }
    
    //*************************************** WORK ITEM ********************************//
    /*
    *   A work item holds the info about a call to Sync or
    *   Async-ProcessMessage.
    */
    internal class WorkItem
    {
        private const int FLG_WAITING  = 0x0001;
        private const int FLG_SIGNALED = 0x0002;
        private const int FLG_ASYNC      = 0x0004;
        private const int FLG_DUMMY     = 0x0008;
    
        internal int _flags;
        internal IMessage _reqMsg;
        internal IMessageSink _nextSink;
        // ReplySink will be null for an sync work item.
        internal IMessageSink _replySink;
        // ReplyMsg is set once the sync call is completed
        internal IMessage _replyMsg;
    
        // Context in which the work should execute.
        internal Context _ctx;

        [System.Security.SecurityCritical] // auto-generated
        internal LogicalCallContext _callCtx;
        internal static InternalCrossContextDelegate _xctxDel = new InternalCrossContextDelegate(ExecuteCallback);
    
        //DBGDBG
        //internal int _thread;   
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        static WorkItem()
        {
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal WorkItem(IMessage reqMsg, IMessageSink nextSink, IMessageSink replySink)
        {
            _reqMsg = reqMsg;
            _replyMsg = null;
            _nextSink = nextSink;
            _replySink = replySink;
            _ctx = Thread.CurrentContext;
            _callCtx = Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext;
            //DBGDBG 
            //_thread = Thread.CurrentThread.GetHashCode();
        }
    
        // To mark a work item being enqueued
        internal virtual void SetWaiting()
        {
            Contract.Assert(!IsWaiting(),"!IsWaiting()");
            _flags |= FLG_WAITING;
        }
    
        internal virtual bool IsWaiting()
        {
            return (_flags&FLG_WAITING) == FLG_WAITING;
        }
    
        // To mark a work item that has been given the green light!
        internal virtual void SetSignaled()
        {
            Contract.Assert(!IsSignaled(),"!IsSignaled()");
            _flags |= FLG_SIGNALED;
        }
    
        internal virtual bool IsSignaled()
        {
            return (_flags & FLG_SIGNALED) == FLG_SIGNALED;
        }
    
        internal virtual void SetAsync()
        {
            _flags |= FLG_ASYNC;
        }
        
        internal virtual bool IsAsync()
        {
            return (_flags & FLG_ASYNC) == FLG_ASYNC;
        }
    
        internal virtual void SetDummy()
        {
            _flags |= FLG_DUMMY;
        }
        
        internal virtual bool IsDummy()
        {
            return (_flags & FLG_DUMMY) == FLG_DUMMY;
        }


        [System.Security.SecurityCritical]  // auto-generated
        internal static Object ExecuteCallback(Object[] args)
        {
            WorkItem This = (WorkItem) args[0];
            
            if (This.IsAsync())
            {
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] AsyncWork.Execute");
                This._nextSink.AsyncProcessMessage(This._reqMsg, This._replySink);            
            }
            else if (This._nextSink != null)
            {
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] SyncWork.Execute");               
                This._replyMsg = This._nextSink.SyncProcessMessage(This._reqMsg);
            }          
            return null;
        }
    
        /*
        *   Execute is called to complete a work item (sync or async).
        *   Execute assumes that the context is set correctly and the lock
        *   is taken (i.e. it makes no policy decisions)
        * 
        *   It is called from the following 3 points:
        *       1. thread pool thread executing the callback for an async item
        *       2. calling thread executing the callback for a queued sync item
        *       3. calling thread directly calling Execute for a non-queued sync item
        */
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual void Execute()
        {
            // Execute should be called with the domain policy enforced
            // i.e. a Synchronization domain should be locked etc ...
            Contract.Assert(IsSignaled(),"IsSignaled()");

            Thread.CurrentThread.InternalCrossContextCallback(_ctx, _xctxDel, new Object[] { this } );
        }
        internal virtual IMessage ReplyMessage { get {return _replyMsg;}}   
    }
    
    //*************************************** CLIENT SINK ********************************//
    
    /*
    *   Implements the client context sink contributed by the
    *   Property. The sink holds a back pointer to the property.
    *   The sink intercepts outgoing calls from objects the Context 
    *   and co-ordinates with the property to enforce the domain policy.
    */
    internal class SynchronizedClientContextSink
            : InternalSink, IMessageSink
    {
        internal IMessageSink   _nextSink;
        [System.Security.SecurityCritical] // auto-generated
        internal SynchronizationAttribute _property;
    
        [System.Security.SecurityCritical]  // auto-generated
        internal SynchronizedClientContextSink(SynchronizationAttribute prop, IMessageSink nextSink)
        {
            _property = prop;
            _nextSink = nextSink;
        }
        
        [System.Security.SecuritySafeCritical]  // auto-generated
        ~SynchronizedClientContextSink()
        {
            _property.Dispose();
        }
        
        /*
        *   Implements IMessageSink::SyncProcessMessage for the call-out sink
        */
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage SyncProcessMessage(IMessage reqMsg)
        {            
            Contract.Assert(_property.Locked == true,"_property.Locked == true");
            IMessage replyMsg;
            if (_property.IsReEntrant)
            {
                // In this case we are required to let anybody waiting for
                // the domain to enter and execute
                // Notify the property that we are leaving 
                _property.HandleThreadExit();

                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] R: Sync call-out");
                replyMsg = _nextSink.SyncProcessMessage(reqMsg);
    
                // We will just block till we are given permission to re-enter
                // Notify the property that we wish to re-enter the domain.
                // This will block the thread here if someone is in the domain.
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] R: Sync call-out returned, waiting for lock");                
                _property.HandleThreadReEntry(); 
                Contract.Assert(_property.Locked == true,"_property.Locked == true");
            }
            else
            {
                // In the non-reentrant case we are just a pass-through sink
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] NR: Sync call-out (pass through)");                
                // We should mark the domain with our LCID so that call-backs are allowed to enter..
                LogicalCallContext cctx = 
                    (LogicalCallContext) reqMsg.Properties[Message.CallContextKey];
                                                         
                String lcid = cctx.RemotingData.LogicalCallID;
                bool bClear = false;
                if (lcid == null)
                {
                    // We used to assign call-ids in RemotingProxy.cs at the
                    // start of each Invoke. As an optimization we now do it 
                    // here in a delayed fashion... since currently only 
                    // Synchronization needs it
                    // Note that for Sync-calls we would just inherit an LCID
                    // if the call has one, if not we create one. However for
                    // async calls we always generate a new LCID.
                    lcid = Identity.GetNewLogicalCallID();
                    cctx.RemotingData.LogicalCallID = lcid;
                    bClear = true;

                    Contract.Assert(
                        _property.SyncCallOutLCID == null,
                        "Synchronization domain is already in a callOut state");
                }

                bool bTopLevel=false;
                if (_property.SyncCallOutLCID==null)
                {
                    _property.SyncCallOutLCID = lcid;
                    bTopLevel = true;
                }
                    
                Contract.Assert(lcid.Equals(_property.SyncCallOutLCID), "Bad synchronization domain state!");                    
                
                replyMsg = _nextSink.SyncProcessMessage(reqMsg);

                // if a top level call out returned we clear the callId in the domain
                if (bTopLevel)
                {
                    _property.SyncCallOutLCID = null;

                    // The sync callOut is done, we do not need the lcid
                    // that was associated with the call any more.
                    // (clear it only if we added one to the reqMsg)
                    if (bClear)
                    {
                        // Note that we make changes to the callCtx in 
                        // the reply message ... since this is the one that
                        // will get installed back on the thread that called
                        // the proxy.
                        LogicalCallContext cctxRet = 
                            (LogicalCallContext) replyMsg.Properties[Message.CallContextKey];
                        Contract.Assert(    
                            cctxRet != null,
                            "CallContext should be non-null");
                        cctxRet.RemotingData.LogicalCallID = null;
                    }
                }
                
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] NR: Sync call-out returned");
            }
            return replyMsg;
        }
    
        /*
        *   Implements IMessageSink::AsyncProcessMessage for the call-out sink
        */
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
        {
            IMessageCtrl msgCtrl = null;
            
            Contract.Assert(_property.Locked == true,"_property.Locked == true");

            if (!_property.IsReEntrant)
            {
                // In this case new calls are not allowed to enter the domain
                // We need to track potentially more than one async-call-outs
                // and allow the completion notifications to come in for those

                LogicalCallContext cctx = 
                    (LogicalCallContext) reqMsg.Properties[Message.CallContextKey];
                // We used to generate a new lcid automatically in RemotingProxy
                // Invoke at the start of each Async call.
                // However now we do it here as an optimization (since only
                // Synchronization needs it)
                // RemotingProxy invoke code does Clone() the callContext at 
                // the start of each Async call so we don't have to worry 
                // about stomping someone else's lcid here.

                                                         
                String lcid =  Identity.GetNewLogicalCallID();
                cctx.RemotingData.LogicalCallID = lcid;
                    

                Contract.Assert(
                    _property.SyncCallOutLCID == null,
                    "Cannot handle async call outs when already in a top-level sync call out");
                //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] NR: Async CallOut: adding to lcidList: " + lcid);                                            
                _property.AsyncCallOutLCIDList.Add(lcid);
            }
            // We will call AsyncProcessMessage directly on this thread 
            // since the thread should not block much. However, we will
            // have to intercept the callback on the replySink chain for
            // which we wrap the caller provided replySink into our sink.
            AsyncReplySink mySink = new AsyncReplySink(replySink, _property);          
            
            // NOTE: we will need to yield the Synchronization Domain at
            // some time or another to get our own callBack to complete.

            // Note that for the Async call-outs we have to provide an interception 
            // sink whether we are re-entrant or not since we want 
            // the replySink.SyncProcessMessage call to be wait for the lock just like
            // any other call-in.
            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] Async call-out");
            
            msgCtrl = _nextSink.AsyncProcessMessage(reqMsg, (IMessageSink)mySink);
            //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] Async call-out AsyncPM returned, reply to come separately");

            return msgCtrl;
        }
    
        /*
        *   Implements IMessageSink::GetNextSink
        */
        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                return _nextSink;
            }

        }
    
        /*
        *   This class just implements the CallBack sink we provide to 
        *   intercept the callback of an Async out-call. The CallBack sink
        *   ensures that arbitrary threads do not enter our Synchronization
        *   Domain without asking us if it is Ok!
        */
        internal class AsyncReplySink : IMessageSink
        {
            internal IMessageSink _nextSink;
            [System.Security.SecurityCritical] // auto-generated
            internal SynchronizationAttribute _property;
            [System.Security.SecurityCritical]  // auto-generated
            internal AsyncReplySink(IMessageSink nextSink, SynchronizationAttribute prop)
            {
                _nextSink = nextSink;
                _property = prop;
            }
    
            [System.Security.SecurityCritical]  // auto-generated
            public virtual IMessage SyncProcessMessage(IMessage reqMsg)
            {
                
                // We handle this as a regular new Sync workItem
                // 1. Create a work item 
                WorkItem work = new WorkItem(reqMsg,
                                            _nextSink,
                                            null /* replySink */);
    
                // 2. Notify the property to handle the WorkItem
                // The work item may get put in a Queue or may execute right away.
                _property.HandleWorkRequest(work);

                if (!_property.IsReEntrant)
                {
                    // Remove the async lcid we had added to the call out list.
                    //DBGConsole.WriteLine(Thread.CurrentThread.GetHashCode()+"] NR: InterceptionSink::SyncPM Removing async call-out lcid: " + ((LogicalCallContext)reqMsg.Properties[Message.CallContextKey]).RemotingData.LogicalCallID);                   
                    _property.AsyncCallOutLCIDList.Remove(
                        ((LogicalCallContext)reqMsg.Properties[Message.CallContextKey]).RemotingData.LogicalCallID);
                }
    
                // 3. Pick up retMsg from the WorkItem and return
                return work.ReplyMessage;                    
            }
    
            [System.Security.SecurityCritical]  // auto-generated
            public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink)
            {
                throw new NotSupportedException();
            }
    
            /*
            * Implements IMessageSink::GetNextSink
            */
            public IMessageSink NextSink
            {
                [System.Security.SecurityCritical]  // auto-generated
                get
                {
                    return _nextSink;
                }
            }
        }   
    }

}

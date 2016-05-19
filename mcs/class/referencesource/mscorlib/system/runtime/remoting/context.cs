// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:     Context.cs
**
**
** Purpose: Remoting Context core implementation.
**
**
===========================================================*/
namespace System.Runtime.Remoting.Contexts {
    
    using System;
    using System.Security;
    using System.Security.Permissions;
    using System.Threading;
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Serialization;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Globalization;
    using System.Diagnostics.Contracts;
    //  CallBacks provide a facility to request execution of some code
    //  in another context.
    //  CrossContextDelegate type is defined for context call backs. 
    //  Each context has a CallBackObject which can be used to perform
    //  callbacks on the context. The delegate used to request a callback
    //  through the CallBackObject must be of CrossContextDelegate type.
    /// <internalonly/>
    [System.Runtime.InteropServices.ComVisible(true)]
    public delegate void CrossContextDelegate();
    
    /// <internalonly/>
    // deliberately not [serializable]
    [System.Runtime.InteropServices.ComVisible(true)]
    public class Context
    {
        // flags to mark the state of the context object
        // marks the context as the default context
        internal const int CTX_DEFAULT_CONTEXT   = 0x00000001;
        
        // marks the context as frozen
        internal const int CTX_FROZEN                = 0x00000002;
        
        // Tells the context channel that the context has properties 
        // that use the threadPool themselves.
        // In that case, the channel does not itself use threadPool.
        // This is OFF by default
        internal const int CTX_THREADPOOL_AWARE = 0x00000004;
    
        private const int GROW_BY                        = 0x8;
        private const int STATICS_BUCKET_SIZE           = 0x8;
        
        private IContextProperty[] _ctxProps;    // array of name-value pairs of properties        
        private DynamicPropertyHolder _dphCtx;  // Support for Dynamic Sinks
        private volatile LocalDataStoreHolder _localDataStore;
        private IMessageSink _serverContextChain;   
        private IMessageSink _clientContextChain;
        private AppDomain _appDomain;       // AppDomain property of the context
        private Object[] _ctxStatics;       // Holder for context statics

        //**********************
        // This needs to be the first NON-OBJECT field!
        private IntPtr _internalContext; // address of the VM context object!
        //**********************

        // at this point we just differentiate contexts based on context ID
        private int _ctxID;
        private int _ctxFlags;
        private int _numCtxProps;   // current count of properties

        // Context statics stuff
        private int _ctxStaticsCurrentBucket;
        private int _ctxStaticsFreeIndex;

        // Support for dynamic properties.
        private static DynamicPropertyHolder _dphGlobal = new DynamicPropertyHolder();
    
        // Support for Context Local Storage
        private static LocalDataStoreMgr _localDataStoreMgr = new LocalDataStoreMgr();

        // ContextID counter (for public context id)        
        private static int _ctxIDCounter = 0; 
        // <


        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public Context()
            : this(0)
        {
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        private Context(int flags) {
                                            
            _ctxFlags = flags;
            if ((_ctxFlags & CTX_DEFAULT_CONTEXT) != 0)
            {
                _ctxID = 0;     // ID 0 == default context
            }
            else
            {
                _ctxID = Interlocked.Increment(ref _ctxIDCounter);
            }
            
            // Every context inherits the appdomain level properties
            // Get the properties for the appdomain and add it to
            // this context.
            DomainSpecificRemotingData data = Thread.GetDomain().RemotingData;
            if(null != data)
            {
                IContextProperty[] ctxProps = data.AppDomainContextProperties;
                if(null != ctxProps)
                {
                    for(int i = 0; i < ctxProps.Length; i++)
                    {
                        SetProperty(ctxProps[i]);
                    }
                }                                    
            }
            
            // Freeze the default context now
            if ((_ctxFlags & CTX_DEFAULT_CONTEXT) != 0)
            {
                this.Freeze();
            }
            // This call will set up the cycles between the VM & managed context
            // It will also set the managed context's AppDomain property to 
            // the current AppDomain.Will also publish ifthe default ctx, so should be 
            // in the very end
            SetupInternalContext((_ctxFlags&CTX_DEFAULT_CONTEXT) == CTX_DEFAULT_CONTEXT);
                        
            Message.DebugOut("Creating Context with ID " + _ctxID + " and flags " + flags + " " + Environment.NewLine);
        }

        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void SetupInternalContext(bool bDefault);

    /// <internalonly/>
        [System.Security.SecuritySafeCritical]  // auto-generated
        ~Context()
        {
            // We clean up the backing objects only for the non-default
            // contexts. For default contexts these are cleaned up during
            // AppDomain shutdown.
            if (_internalContext != IntPtr.Zero && (_ctxFlags & CTX_DEFAULT_CONTEXT) == 0)
            {
                CleanupInternalContext();
            }
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern void CleanupInternalContext();
            
        /// <internalonly/>
        public virtual Int32 ContextID 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get 
            {
                return _ctxID;
            }
        }

        internal virtual IntPtr InternalContextID
        {
            get 
            {
                return _internalContext;
            }
        }
    
        internal virtual AppDomain AppDomain
        {
            get {return _appDomain;}
        }

        internal bool IsDefaultContext
        {
            get { return _ctxID == 0; }
        }

        /// <internalonly/>
        public static Context DefaultContext 
        {
            [System.Security.SecurityCritical]  // auto-generated_required
            get 
            {
                return Thread.GetDomain().GetDefaultContext();
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static Context CreateDefaultContext()
        {
            return new Context(CTX_DEFAULT_CONTEXT);
        }
        
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual IContextProperty GetProperty(String name)
        {
            if (_ctxProps == null || name == null)
            {
                return null;
            }
            IContextProperty prop = null;
            for (int i=0; i<_numCtxProps; i++)
            {
                if (_ctxProps[i].Name.Equals(name))
                {
                    prop = _ctxProps[i];
                    break;
                }
            }
            return prop;
        }
    
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void SetProperty(IContextProperty prop)
        {
            // We do not let people add properties to the default context.
            /* We allow appdomain level properties to be added to the default context
            if ((_ctxFlags & CTX_DEFAULT_CONTEXT) != 0)
            {
                throw new InvalidOperationException(Environment.GetResourceString("InvalidOperation_AddContextFrozen"));
            }
            */
            
            if (prop == null || prop.Name == null)
            {
                throw new ArgumentNullException((prop==null) ? "prop" : "property name");
            }
            Contract.EndContractBlock();
    
            if ((_ctxFlags & CTX_FROZEN) != 0)
            {
                throw new InvalidOperationException(
                    Environment.GetResourceString("InvalidOperation_AddContextFrozen"));
            }
    
            lock (this)
            {
                // Check if we have a property by this name
                CheckPropertyNameClash(prop.Name, _ctxProps, _numCtxProps);
                
                // check if we need to grow the array.
                if (_ctxProps == null || _numCtxProps == _ctxProps.Length)    
                {
                    _ctxProps = GrowPropertiesArray(_ctxProps);
                }
                // now add the property
                _ctxProps[_numCtxProps++] = prop;
            }
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual void  InternalFreeze()
        {
            _ctxFlags |= CTX_FROZEN;
            // From this point on attempts to add properties will throw
            // So we don't need to take a lock.
            for (int i=0; i<_numCtxProps; i++)
            {
                _ctxProps[i].Freeze(this);
            }
            
        }
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public virtual void Freeze()
        {
            lock(this) {
                if ((_ctxFlags & CTX_FROZEN) != 0)
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_ContextAlreadyFrozen"));
                }
                InternalFreeze();
            }
        }
    
        internal virtual void SetThreadPoolAware()
        {
            // Cannot turn off ThreadPool support for the default context
            Contract.Assert(
                (_ctxFlags & CTX_DEFAULT_CONTEXT) == 0,
                "This operation is not allowed on the default context!");
            _ctxFlags |= CTX_THREADPOOL_AWARE;
        }
    
        internal virtual bool IsThreadPoolAware 
        {
            get { return (_ctxFlags & CTX_THREADPOOL_AWARE) == CTX_THREADPOOL_AWARE;}
        }
        
        /// <internalonly/>
        public virtual IContextProperty[] ContextProperties
        {
            // we return a copy of the current set of properties
            // the user may iterate from 0 to array.length on it.
            [System.Security.SecurityCritical]  // auto-generated_required
            get 
            {
                if (_ctxProps == null)
                {
                    return null;
                }   
                lock (this)
                {         
                    IContextProperty[] retProps = new IContextProperty[_numCtxProps];
                    Array.Copy(_ctxProps, retProps, _numCtxProps);
                    return retProps;
                }
            }
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal static void CheckPropertyNameClash(String name, IContextProperty[] props, int count)
        {
            for (int i=0; i<count; i++)
            {
                if (props[i].Name.Equals(name))
                {
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_DuplicatePropertyName"));
                }
            }        
        }
    
        internal static IContextProperty[] GrowPropertiesArray(IContextProperty[] props)
        {
            // grow the array of IContextProperty objects
            int newSize = (props != null ? props.Length : 0)  + GROW_BY;
            IContextProperty[] newProps = new IContextProperty[newSize];
            if (props != null)
            {
                // Copy existing properties over.
                Array.Copy(props, newProps, props.Length);
            }
            return newProps;
        }
            
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual IMessageSink GetServerContextChain()
        {
            if (_serverContextChain == null) 
            {
                // a bare chain would have just this one sink.
                IMessageSink newServerContextChain = ServerContextTerminatorSink.MessageSink;
    
                // now loop over properties to add some real sinks.
                Object prop = null;
                int iSink = _numCtxProps;
                while (iSink-- > 0) 
                {
                    // see if this property wants to contribute a ServerContextSink
                    // we have to start chaining in the reverse order
                    prop = _ctxProps[iSink];
                    IContributeServerContextSink sink = prop as IContributeServerContextSink;
                    if (null != sink )
                    {
                        // yes, chain the sink ahead of the chain of sinks constructed so far.
                        newServerContextChain = sink.GetServerContextSink( newServerContextChain);
                        if (newServerContextChain == null)
                        {
                            throw new RemotingException( 
                                Environment.GetResourceString(
                                    "Remoting_Contexts_BadProperty"));
                        }
                    }
                }
                lock (this)
                {  
                    if (_serverContextChain == null)
                    {
                        _serverContextChain = newServerContextChain;
                    }
                }
            }
            return _serverContextChain;
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual IMessageSink GetClientContextChain()
        {
            Message.DebugOut("Context::GetClientContextChain: IN _ctxID =" + _ctxID + Environment.NewLine);
            if (_clientContextChain == null)
            {
                Message.DebugOut("Context::GetClientContextChain: _clientContextChain == null, creating chain" + Environment.NewLine);
                // a bare chain would have just this one sink.
                IMessageSink newClientContextChain = ClientContextTerminatorSink.MessageSink;
    
                // now loop over properties to add some real sinks.
                // Note that for the client chain we go through the properties 
                // in the reverse order as compared to the server chain. 
                // Thus if a lock was taken as the last action of an incoming
                // call, it is released as the first action of an outgoing call.                
                Object prop = null;
                int iSink = 0;
                while (iSink < _numCtxProps) 
                {
                    Message.DebugOut("Context::GetClientContextChain: checking property " + 
                                     _ctxProps[iSink].Name + Environment.NewLine);
                    // see if this property wants to contribute a ClientContextSink
                    // we have to start chaining in the reverse order
                    prop = _ctxProps[iSink];
                    IContributeClientContextSink sink = prop as IContributeClientContextSink;
                    if (null != sink)
                    {
                        Message.DebugOut("Context::GetClientContextChain: calling GetClientContextSink on " + 
                                         _ctxProps[iSink].Name + Environment.NewLine);
                        // yes, chain the sink ahead of the chain of sinks constructed so far.
                        newClientContextChain = sink.GetClientContextSink(newClientContextChain);
                        if (newClientContextChain == null)
                        {
                            throw new RemotingException( 
                                Environment.GetResourceString(
                                    "Remoting_Contexts_BadProperty"));
                        }
                    }
                    iSink++;
                }
                // now check if we ----d and set appropriately
                lock (this)
                {
                    if (_clientContextChain==null)
                    {
                        _clientContextChain = newClientContextChain;
                    }
                    // else the chain we created should get GC-ed.
                }            
            }
            return _clientContextChain;        
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual IMessageSink CreateServerObjectChain(MarshalByRefObject serverObj)
        {
            // a bare chain would just be the dispatcher sink       
            IMessageSink serverObjectChain = new ServerObjectTerminatorSink(serverObj);
            
            // now loop over properties to add some real sinks.
            Object prop = null;
            int iSink = _numCtxProps;
            while (iSink-- > 0) 
            {
                // see if this property wants to contribute a ServerObjectSink
                // we have to start chaining in the reverse order
                prop = _ctxProps[iSink];
                IContributeObjectSink sink = prop as IContributeObjectSink;
                if (null != sink)
                {
                    // yes, chain the sink ahead of the chain of sinks constructed so far.
                    serverObjectChain =  sink.GetObjectSink( serverObj, serverObjectChain);
                    if (serverObjectChain == null)
                    {
                            throw new RemotingException( 
                                Environment.GetResourceString(
                                    "Remoting_Contexts_BadProperty"));
                    }
                }
            }
            return serverObjectChain;
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual IMessageSink CreateEnvoyChain(MarshalByRefObject objectOrProxy)
        {
            // a bare chain would just be the dispatcher sink
            IMessageSink envoyChain = EnvoyTerminatorSink.MessageSink;
            
            // now loop over properties to add some real sinks.
            // Note: the sinks in the envoy chain should be in mirror image
            // order relative to sinks on the server side
            Object prop = null;
            int iSink = 0;
            
            MarshalByRefObject exposedObj = objectOrProxy;
        
            while (iSink < _numCtxProps) 
            {
                // see if this property wants to contribute a ClientContextSink
                // we have to start chaining in the reverse order
                prop = _ctxProps[iSink];
                IContributeEnvoySink sink = prop as IContributeEnvoySink; 
                if (null != sink)
                {
                    // yes, chain the sink ahead of the chain of sinks constructed so far.
                    envoyChain = sink.GetEnvoySink(exposedObj, envoyChain);
                    if (envoyChain == null)
                    {
                        throw new RemotingException( 
                            Environment.GetResourceString(
                                "Remoting_Contexts_BadProperty"));
                    }
                }
                iSink++;
            }
            return envoyChain;
        }
    

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~ Activation Support ~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        [System.Security.SecurityCritical]  // auto-generated
        internal IMessage NotifyActivatorProperties(
            IMessage msg, bool bServerSide)
        {
            Contract.Assert( (msg is IConstructionCallMessage) 
                                || (msg is IConstructionReturnMessage),
                            "Bad activation msg type");
            Contract.Assert( !((msg is IConstructionCallMessage) && 
                                (msg is IConstructionReturnMessage)),
                            "Activation message cannot be both call & return type");
                                                        
            Contract.Assert((_ctxFlags & CTX_FROZEN) == CTX_FROZEN,
                            "ServerContext not frozen during activation!");


            // Any exception thrown by the notifications is caught and 
            // folded into a return message to return to the caller.
            IMessage errMsg = null;

            try
            {
                // Since the context is frozen the properties array is in 
                // effect read-only! 
                int iProp = _numCtxProps;            
                Object prop = null;
                
                while (iProp-- != 0) 
                {
                    // see if this property is interested in Activation
                    prop = _ctxProps[iProp];
                    IContextPropertyActivator activator = prop as IContextPropertyActivator; 
                    if (null != activator)
                    {
                        // yes, notify as appropriate
                        IConstructionCallMessage ccm = msg as IConstructionCallMessage;
                        if (null != ccm)
                        {
                            // IConsructionCallMessage on its way forward
                            if (!bServerSide)
                            {
                                // activation starting at client side
                                activator.CollectFromClientContext(ccm);                       
                            }
                            else
                            {
                                // activation starting at server side
                                // 
                                activator.DeliverClientContextToServerContext(ccm);
                            }
                        }
                        else
                        {
                            // IConstructionReturnMessage on its way back
                            if (bServerSide)
                            {
                                // activation returning from server side
                                activator.CollectFromServerContext((IConstructionReturnMessage)msg);
                            }
                            else
                            {
                                // activation back at client side
                                // <
                                activator.DeliverServerContextToClientContext((IConstructionReturnMessage)msg);
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                IMethodCallMessage mcm = null;
                if (msg is IConstructionCallMessage)
                {
                    mcm = (IMethodCallMessage) msg;
                }
                else
                {
                    mcm = new ErrorMessage();
                }
                errMsg = new ReturnMessage(e, mcm);
                if (msg != null)
                {
                    ((ReturnMessage)errMsg).SetLogicalCallContext(
                            (LogicalCallContext)
                            msg.Properties[Message.CallContextKey]);
                }
            }
            return errMsg;
        }
    
    /// <internalonly/>
        public override String ToString()
        {
            return    "ContextID: " + _ctxID;
        }

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~ Transition Support ~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        // This is the simple cross-context call back function invoke on
        // the context to do a call-back in.
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public void DoCallBack(CrossContextDelegate deleg)
        {
            /*DBG Console.WriteLine("public DoCallBack: targetCtx: " 
            + Int32.Format(this.InternalContextID,"x")); DBG*/
                
            if (deleg == null)
            {
                throw new ArgumentNullException("deleg");
            }
            Contract.EndContractBlock();

            if ((_ctxFlags & CTX_FROZEN) == 0)
            {
                throw new RemotingException( 
                    Environment.GetResourceString(
                        "Remoting_Contexts_ContextNotFrozenForCallBack"));
            }

            Context currCtx = Thread.CurrentContext;
            if (currCtx == this)
            {
                // We are already in the target context, just execute deleg
                // NOTE: If in the future we decide to leave the context
                // and reenter it for this case we will need to change 
                // Context::RequestCallBack method in VM also!
                deleg();
            }
            else
            {                               
                // We pass 0 for target domain ID for x-context case.
                currCtx.DoCallBackGeneric(this.InternalContextID, deleg);
                GC.KeepAlive(this);
            }
        }

        // This is called when EE needs to do a transition and execute some
        // code. Before calling, EE determines if this is a x-domain case.              
        // targetDomainID will be 0 if it is a simple x-context call.
        //<
        [System.Security.SecurityCritical]  // auto-generated
        internal static void DoCallBackFromEE(
            IntPtr targetCtxID, IntPtr privateData, int targetDomainID)
        {
            Contract.Assert(targetCtxID != IntPtr.Zero, "Bad transition context");
                
            /*DBG Console.WriteLine("private DoCallBackFromEE: targetCtx: " 
            + Int32.Format(targetCtxID,"x") 
            + " PvtData: " + Int32.Format(privateData,"x"));DBG*/

            if (targetDomainID == 0)
            {
                CallBackHelper cb = new CallBackHelper(
                                            privateData,
                                            true /*fromEE*/,
                                            targetDomainID); 
                CrossContextDelegate ctxDel = new CrossContextDelegate(cb.Func);
                Thread.CurrentContext.DoCallBackGeneric(targetCtxID, ctxDel);                                                                     
            }
            else
            {
                // for x-appdomain calls, we can't pass a delegate since that 
                //   would require us to deserialize it on the other side which
                //   is not allowed for non-public methods.
                TransitionCall msgCall = new TransitionCall(targetCtxID, privateData, targetDomainID);           
            
                Message.PropagateCallContextFromThreadToMessage(msgCall);
                //DBG Console.WriteLine("CallBackGeneric starting!");
                IMessage retMsg = Thread.CurrentContext.GetClientContextChain().SyncProcessMessage(msgCall); 
                Message.PropagateCallContextFromMessageToThread(retMsg);
            
                IMethodReturnMessage msg = retMsg as IMethodReturnMessage;
                if (null != msg)
                {
                    if (msg.Exception != null)
                        throw msg.Exception;
                }
            }
        } // DoCallBackFromEE

                // This is called by both the call back functions above.
        [System.Security.SecurityCritical]  // auto-generated
        internal void DoCallBackGeneric(
            IntPtr targetCtxID, CrossContextDelegate deleg)
        {               
            TransitionCall msgCall = new TransitionCall(targetCtxID, deleg);           
            
            Message.PropagateCallContextFromThreadToMessage(msgCall);
            //DBG Console.WriteLine("CallBackGeneric starting!");
            IMessage retMsg = this.GetClientContextChain().SyncProcessMessage(msgCall); 
            if (null != retMsg)
            {
                Message.PropagateCallContextFromMessageToThread(retMsg);
            }
            
            IMethodReturnMessage msg = retMsg as IMethodReturnMessage;
            if (null != msg)
            {
                if (msg.Exception != null)
                    throw msg.Exception;
            }
            //DBG Console.WriteLine("CallBackGeneric finished!");
        }
        
        // This is the E-Call that we route the EE-CallBack request to
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        internal static extern void ExecuteCallBackInEE(IntPtr privateData);        

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~ Context Local Store  ~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        private LocalDataStore MyLocalStore 
        {
            get 
            { 
                if (_localDataStore == null)
                {
                    // It's OK to lock the manager here because it is going to lock
                    // itself anyway.
                    lock (_localDataStoreMgr)
                    {
                        if (_localDataStore == null)
                        {
                            // The local store has not yet been created for this thread.
                            _localDataStore = _localDataStoreMgr.CreateLocalDataStore();
                        }
                    }
                }
                return _localDataStore.Store;
            }
        }
        
        //  All of these are exact shadows of corresponding ThreadLocalStore
        //  APIs.
        /// <internalonly/>
       [System.Security.SecurityCritical]  // auto-generated_required
        public static LocalDataStoreSlot AllocateDataSlot()
        {
            return _localDataStoreMgr.AllocateDataSlot();            
        }
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public static LocalDataStoreSlot AllocateNamedDataSlot(String name)
        {
            return _localDataStoreMgr.AllocateNamedDataSlot(name);            
        }
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public static LocalDataStoreSlot GetNamedDataSlot(String name)
        {
            return _localDataStoreMgr.GetNamedDataSlot(name);            
        }
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public static void FreeNamedDataSlot(String name)
        {
            _localDataStoreMgr.FreeNamedDataSlot(name);            
        }
        
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public static void SetData(LocalDataStoreSlot slot, Object data)
        {
                        Thread.CurrentContext.MyLocalStore.SetData(slot, data);                    
        }
        
        /// <internalonly/>
        [System.Security.SecurityCritical]  // auto-generated_required
        public static Object GetData(LocalDataStoreSlot slot)
        {
            return Thread.CurrentContext.MyLocalStore.GetData(slot);              
        }
        
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~ Context Statics ~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        private int ReserveSlot()
        {
            // This will be called with the context crst held so we 
            // don't take a lock here)
            if (_ctxStatics == null)
            {
                // allocate the first bucket
                _ctxStatics = new Object[STATICS_BUCKET_SIZE];
                // set next bucket field to null
                _ctxStatics[0] = null;
                _ctxStaticsFreeIndex = 1;
                _ctxStaticsCurrentBucket = 0;
            }

            // See if we have to allocate a new bucket
            if (_ctxStaticsFreeIndex == STATICS_BUCKET_SIZE)
            {
                Object[] newBucket = new Object[STATICS_BUCKET_SIZE];

                // walk the chain to locate the last bucket
                Object[] bucket = _ctxStatics;
                while (bucket[0] != null)
                {
                    bucket = (Object[]) bucket[0];
                }
                // chain in the new bucket
                bucket[0] = newBucket;
                _ctxStaticsFreeIndex = 1;
                _ctxStaticsCurrentBucket++;
            }

            // bucket# in highWord, index in lowWord
            return _ctxStaticsFreeIndex++|_ctxStaticsCurrentBucket<<16;
        }
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~ End Context Statics ~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~

        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~ Dynamic Sink Support  ~~~~~~~~~~~~~~~~~~~~
        //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
        //   This allows people to register a property implementing IContributeDynamicSink
        //   with the remoting service. Based on the obj and ctx parameters, the property
        //   is asked to contribute a sink that is placed at some location in the path of 
        //   remoting calls.
        //   The property may be unregistered, which implies that the sink will be dropped
        //   for subsequent remoting calls.
        //   If multiple properties are registered, their sinks may be called in an
        //   arbitrary order which may change between calls.
        //   
        //   If obj is non null then:
        //       - if it is a proxy, all calls made on the proxy are intercepted
    //       - if it is a real object, all calls on the object are intercepted
    //       - the ctx argument must be null
    //   If ctx is non-null then:
    //       - the obj argument must be null
    //       - all calls entering and leaving the context are intercepted
    //   If both ctx and obj are null then:
    //       - all calls entering and leaving all contexts are intercepted
    //
    /// <internalonly/>
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.Infrastructure)]
        public static bool RegisterDynamicProperty(IDynamicProperty prop, ContextBoundObject obj, Context ctx)
        {
            bool fRegistered = false;

            if (prop == null || prop.Name == null || !(prop is IContributeDynamicSink))
            {
                throw new ArgumentNullException("prop");
            }
            if (obj != null && ctx != null)
            {
                // Exactly one of these is allowed to be non-null.
                throw new ArgumentException(Environment.GetResourceString("Argument_NonNullObjAndCtx"));
            }
            if (obj != null)
            {
                // ctx is ignored and must be null.
                fRegistered = IdentityHolder.AddDynamicProperty(obj, prop);                
            }
            else
            {
                // ctx may or may not be null
                fRegistered = Context.AddDynamicProperty(ctx, prop);
            }

            return fRegistered;
        }

    /// <internalonly/>
        [System.Security.SecuritySafeCritical]  // auto-generated
        [SecurityPermissionAttribute(SecurityAction.Demand, Flags=SecurityPermissionFlag.Infrastructure)]
        public static bool UnregisterDynamicProperty(String name, ContextBoundObject obj, Context ctx)
        {
            // name, obj, ctx arguments should be exactly the same as a previous
            // RegisterDynamicProperty call
            if (name == null)
            {
                throw new ArgumentNullException("name");
            }
            if (obj != null && ctx != null)
            {
                throw new ArgumentException(Environment.GetResourceString("Argument_NonNullObjAndCtx"));
            }
            Contract.EndContractBlock();

            bool fUnregister = false;

            if (obj != null)
            {
                // ctx is ignored and must be null.
                fUnregister = IdentityHolder.RemoveDynamicProperty(obj, name);
            }
            else
            {
                // ctx may or may not be null
                fUnregister = Context.RemoveDynamicProperty(ctx, name);
            }

            return fUnregister;
        }

        /*
         *  Support for dynamic sinks at context level
         *
         */        
        [System.Security.SecurityCritical]  // auto-generated
        internal static bool AddDynamicProperty(Context ctx, IDynamicProperty prop)
        {
            // Check if we have a property by this name
            if (ctx != null)
            {
                return ctx.AddPerContextDynamicProperty(prop);
            }
            else
            {
                // We have to add a sink that should fire for all contexts
                return AddGlobalDynamicProperty(prop);
            }
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        private bool AddPerContextDynamicProperty(IDynamicProperty prop)
        {
            if (_dphCtx == null)
            {
                DynamicPropertyHolder dph = new DynamicPropertyHolder();
                lock (this)
                {
                    if (_dphCtx == null)
                    {
                        _dphCtx = dph;
                    }
                }
            }
            return _dphCtx.AddDynamicProperty(prop);
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        private static bool AddGlobalDynamicProperty(IDynamicProperty prop)
        {
            return _dphGlobal.AddDynamicProperty(prop);
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        internal static bool RemoveDynamicProperty(Context ctx, String name)
        {
            if (ctx != null)
            {
                return ctx.RemovePerContextDynamicProperty(name);
            }
            else
            {
                // We have to remove a global property
                return RemoveGlobalDynamicProperty(name);                        
            }
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        private bool RemovePerContextDynamicProperty(String name)
        {
            // We have to remove a property for this context
            if (_dphCtx == null)
            {
                throw new RemotingException(
                    String.Format(
                        CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Contexts_NoProperty"),
                        name
                        ));
            }
            return _dphCtx.RemoveDynamicProperty(name);
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        private static bool RemoveGlobalDynamicProperty(String name)
        {
            return _dphGlobal.RemoveDynamicProperty(name);
        }
    
        /*
         *  Returns an array of context specific dynamic properties
         *  registered for this context. The number of such properties
         *  is designated by length of the returned array.
         */
        internal virtual IDynamicProperty[] PerContextDynamicProperties
        {
            get 
            {
                if (_dphCtx == null)
                {
                    return null;
                }   
                else
                {
                    return _dphCtx.DynamicProperties;
                }
            }
        }
    
        /*
         *  Returns an array of global dynamic properties
         *  registered (for all contexts). The number of such properties
         *  is designated by length of the returned array.
         */    
        internal static ArrayWithSize GlobalDynamicSinks
        {
            [System.Security.SecurityCritical]  // auto-generated
            get 
            {
                return _dphGlobal.DynamicSinks;
            }
        }
    
        internal virtual ArrayWithSize DynamicSinks
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                if (_dphCtx == null)
                {
                    return null;
                }
                else
                {
                    return _dphCtx.DynamicSinks;
                }
            }
        }    

        [System.Security.SecurityCritical]  // auto-generated
        internal virtual bool NotifyDynamicSinks(
            IMessage msg,
            bool bCliSide, 
            bool bStart, 
            bool bAsync,
            bool bNotifyGlobals)
        {
            bool bHasDynamicSinks = false;
        
            if (bNotifyGlobals && (_dphGlobal.DynamicProperties != null))
            {
                ArrayWithSize globalSinks = GlobalDynamicSinks;
                if (globalSinks != null)
                {
                    DynamicPropertyHolder.NotifyDynamicSinks(
                                    msg, 
                                    globalSinks, 
                                    bCliSide,
                                    bStart,
                                    bAsync);
                    bHasDynamicSinks = true;
                }
            }
            
            ArrayWithSize perCtxSinks = DynamicSinks;
            if (perCtxSinks != null)
            {
                DynamicPropertyHolder.NotifyDynamicSinks(
                                    msg, 
                                    perCtxSinks, 
                                    bCliSide,
                                    bStart,
                                    bAsync);
                bHasDynamicSinks = true;
            }

            return bHasDynamicSinks;
        } // NotifyDynamicSinks
        
        //******************** END: Dynamic Sink Support ********************        
    }

    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    //~~~~~~~~~~~~~~~~~~~ More Transition Support ~~~~~~~~~~~~~~~~~~~
    //~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~~
    // This class is used as the backing object that implements a delegate
    // function to be used for internal call-backs. The delegate type must
    // be CrossContextDelegate (void Func(void) below) since we are using 
    // the DoCallBackGeneric also as the underlying mechanism for the 
    // exposed cross-context callbacks.
    [Serializable]
    internal class CallBackHelper
    {
        // Some flag definitions
        internal const int RequestedFromEE = 0x00000001;   // callBack from EE
        internal const int XDomainTransition= 0x00000100;  // going x-domain

        internal bool IsEERequested 
        { 
            get {return (_flags&RequestedFromEE)==RequestedFromEE;} 
            set {if (value) {_flags |= RequestedFromEE;}}                   
        }
            
        internal bool IsCrossDomain
        {
            set {if (value) {_flags |= XDomainTransition;}}                 
        }
        
        int _flags;     
        IntPtr _privateData;

        internal CallBackHelper(IntPtr privateData, bool bFromEE, int targetDomainID)
        {
            this.IsEERequested = bFromEE;
            this.IsCrossDomain = (targetDomainID!=0);
            _privateData = privateData;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal void Func()
        {
            /*DBG Console.WriteLine("DelegHelper::Func CTX:" 
            + Int32.Format(Thread.CurrentContext.InternalContextID,"x")
            +Environment.NewLine + "DMN: " + Int32.Format(Thread.GetDomainID(),"x")); DBG*/
            if (IsEERequested)
            {
                //DBG Console.WriteLine("Executing EE callback ");
                
                // EE requested this call back, call EE with its private data
                Context.ExecuteCallBackInEE(_privateData);
                
                //DBG Console.WriteLine("Execute CallBackInEE returned: " + Int32.Format(_retVal,"x"));
            }                       
            else
            {                       
                //DBG Console.WriteLine("Executing non-EE internal callback");
            }                       
        }
    }       // class CallBackHelper
}       //nameSpace Remoting

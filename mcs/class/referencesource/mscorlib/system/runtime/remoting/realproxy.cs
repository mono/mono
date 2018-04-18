// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    RealProxy.cs
**
**
** Purpose: Defines the base class from which proxy should
**          derive
**
**
===========================================================*/
namespace System.Runtime.Remoting.Proxies {
    using System;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.InteropServices;   
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Messaging;
    using System.Runtime.Remoting.Metadata;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Services;
    using System.Runtime.Serialization;
    using System.Runtime.Versioning;
    using System.Security;
    using System.Security.Permissions;
    using System.Security.Principal;
    using System.Threading;
    using System.Runtime.ConstrainedExecution;
    using System.Globalization;
    using System.Diagnostics.Contracts;
    

    // NOTE: Keep this in sync with unmanaged enum definition in Remoting.h
    [Serializable]
    internal enum CallType
    {
        InvalidCall     = 0x0,
        MethodCall      = 0x1,
        ConstructorCall = 0x2
    };

    [Flags]
    internal enum RealProxyFlags
    {
        None                = 0x0,
        RemotingProxy       = 0x1,
        Initialized         = 0x2
    };

    // NOTE: Keep this in sync with unmanaged struct "messageData" in Remoting.h
    [System.Runtime.InteropServices.StructLayout(LayoutKind.Sequential)]
    internal struct MessageData
    {
        internal IntPtr      pFrame;
        internal IntPtr      pMethodDesc;
        internal IntPtr      pDelegateMD;
        internal IntPtr      pSig;
        internal IntPtr      thGoverningType;
        internal int         iFlags;
    };

        

    [System.Security.SecurityCritical]  // auto-generated_required
    [SecurityPermissionAttribute(SecurityAction.InheritanceDemand, Flags=SecurityPermissionFlag.Infrastructure)]
    [System.Runtime.InteropServices.ComVisible(true)]
    abstract public class RealProxy 
    {
        // ************* NOTE ******
        // Object.h has unmanaged structure which maps this layout
        // if you add/remove/change fields make sure to update the structure
        // in object.h also
        
        // Private members
        private Object _tp;

        private Object _identity;

        private MarshalByRefObject _serverObject;

        private RealProxyFlags _flags;

        internal GCHandle   _srvIdentity;

        internal int        _optFlags;

        internal int        _domainID;
        
        // Static members
        private static IntPtr _defaultStub      = GetDefaultStub();

        private static IntPtr _defaultStubValue    = new IntPtr(-1);

        private static Object _defaultStubData  = _defaultStubValue;       
                
        [System.Security.SecuritySafeCritical] // static constructors should be safe to call
        static RealProxy()
        { 
        }

        // Constructor
        [System.Security.SecurityCritical]  // auto-generated
        protected RealProxy(Type classToProxy) : this(classToProxy, (IntPtr)0, null)
        {
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        protected RealProxy(Type classToProxy, IntPtr stub, Object stubData)
        {
            if(!classToProxy.IsMarshalByRef && !classToProxy.IsInterface)
            {
                throw new ArgumentException(
                    Environment.GetResourceString("Remoting_Proxy_ProxyTypeIsNotMBR"));
            }
            Contract.EndContractBlock();

            if((IntPtr)0 == stub)
            {
                Contract.Assert((IntPtr)0 != _defaultStub, "Default stub not set up");

                // The default stub checks for match of contexts defined by us
                stub = _defaultStub;
                // Start with a value of -1 because 0 is reserved for the default context
                stubData = _defaultStubData;   
            }

            _tp = null;
            if (stubData == null)
            {
                throw new ArgumentNullException("stubdata");
            }
            _tp = RemotingServices.CreateTransparentProxy(this, classToProxy, stub, stubData);
            RemotingProxy rp = this as RemotingProxy;
            if (rp != null)
            {
                _flags |= RealProxyFlags.RemotingProxy;
            }
        }

        // This is used (along the frequent path) of Invoke to avoid
        // casting to RemotingProxy
        internal bool IsRemotingProxy()
        {
            return (_flags & RealProxyFlags.RemotingProxy) == RealProxyFlags.RemotingProxy;
        }

        // This is mainly used for RemotingProxy case. It may be worthwhile
        // to make this virtual so extensible proxies can make use of this 
        // (and other flags) as well.
        internal bool Initialized
        {

            get { return (_flags & RealProxyFlags.Initialized) == RealProxyFlags.Initialized; }
            set 
            { 
                if (value)
                {
                    _flags |= RealProxyFlags.Initialized;
                }
                else
                {
                    _flags &= ~RealProxyFlags.Initialized;
                }
            }
        }
        
        // Method to initialize the server object for x-context scenarios
        // in an extensible way
        [System.Security.SecurityCritical]  // auto-generated_required
        [System.Runtime.InteropServices.ComVisible(true)]
        public IConstructionReturnMessage InitializeServerObject(IConstructionCallMessage ctorMsg)
        {
            IConstructionReturnMessage retMsg = null;

            if (_serverObject == null)
            {
                Type svrType = GetProxiedType();
                if((ctorMsg != null) && (ctorMsg.ActivationType != svrType))
                {
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_Proxy_BadTypeForActivation"), 
                                                          svrType.FullName,
                                                          ctorMsg.ActivationType));
                }

                // Create a blank object
                _serverObject = RemotingServices.AllocateUninitializedObject(svrType);

                // If the stub is the default stub, then set the server context 
                // to be the current context.
                SetContextForDefaultStub();

                // OK... we are all set to run the constructor call on the uninitialized object
                MarshalByRefObject proxy = (MarshalByRefObject)GetTransparentProxy();
                IMethodReturnMessage  msg = null;
                Exception e = null;
                if(null != ctorMsg)
                {
                    msg = RemotingServices.ExecuteMessage(proxy, ctorMsg);
                    e = msg.Exception;
                }
                else
                {
                    try
                    {
                        RemotingServices.CallDefaultCtor(proxy);
                    }
                    catch(Exception excep)
                    {
                        e = excep;
                    }
                }
                                 
                // Construct a return message
                if(null == e)
                {
                    Object[] outArgs = (msg == null ? null : msg.OutArgs);
                    int outLength = (null == outArgs ? 0 : outArgs.Length);
                    LogicalCallContext callCtx = (msg == null ? null : msg.LogicalCallContext);
                    retMsg = new ConstructorReturnMessage(proxy, 
                                                          outArgs, outLength,
                                                          callCtx, ctorMsg);                    

                    // setup identity
                    SetupIdentity();
                    if (IsRemotingProxy())
                    {
                        ((RemotingProxy) this).Initialized = true;
                    }
                }
                else
                {
                    // Exception occurred
                    retMsg = new ConstructorReturnMessage(e, ctorMsg);
                }
            }          
    
            
            return retMsg;
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        protected MarshalByRefObject GetUnwrappedServer()
        {
            return UnwrappedServerObject;
        }

        [System.Security.SecurityCritical]  // auto-generated_required
        protected MarshalByRefObject DetachServer()
        {
            Object tp = GetTransparentProxy();
            if (tp != null)
                RemotingServices.ResetInterfaceCache(tp);
            MarshalByRefObject server = _serverObject;
            _serverObject = null;
            server.__ResetServerIdentity();
            return server;
        }
        
        [System.Security.SecurityCritical]  // auto-generated_required
        protected void AttachServer(MarshalByRefObject s)
        {
            Object tp = GetTransparentProxy();
            if (tp != null)
                RemotingServices.ResetInterfaceCache(tp);
            AttachServerHelper(s);
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void SetupIdentity()
        {
            if (_identity == null)
            {
                _identity = IdentityHolder.FindOrCreateServerIdentity(
                                (MarshalByRefObject)_serverObject,
                                null,
                                IdOps.None);
                // Set the reference to the proxy in the identity object
                  ((Identity)_identity).RaceSetTransparentProxy(GetTransparentProxy());
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        private void SetContextForDefaultStub()
        {
            // Check whether the stub is ours or not...
            if(GetStub() == _defaultStub)
            {
                // Yes.. setup the context in the TP so that 
                // contexts can be matched correctly...
                Object oVal = GetStubData(this);
                if(oVal is IntPtr)
                {
                    IntPtr iVal = (IntPtr)oVal;

                    // Set the stub data only if it has been set to our default value,
                    // otherwise, the user has already indicated a preference for the
                    // stub data.
                    if(iVal.Equals(_defaultStubValue))
                    {
                        SetStubData(this, Thread.CurrentContext.InternalContextID);
                    }                
                }
            }
        }
        
        
        // Check whether the current context is the same as the
        // server context
        [System.Security.SecurityCritical]  // auto-generated
        internal bool DoContextsMatch()
        {
            bool fMatch = false;

            // Check whether the stub is ours or not...
            if(GetStub() == _defaultStub)
            {
                // Yes.. setup the context in the TP so that 
                // contexts can be matched correctly...
                Object oVal = GetStubData(this);
                if(oVal is IntPtr)
                {
                    IntPtr iVal = (IntPtr)oVal;
                    // Match the internal context ids...                     
                    if(iVal.Equals(Thread.CurrentContext.InternalContextID))
                    {
                        fMatch = true;
                    }
                }
            }

            return fMatch;
        }

        // This is directly called by RemotingServices::Wrap() when it needs
        // to bind a proxy with an uninitialized contextBound server object
        [System.Security.SecurityCritical]  // auto-generated
        internal void AttachServerHelper(MarshalByRefObject s)
        {
            if (s == null || _serverObject != null)
            {
                throw new ArgumentException(Environment.GetResourceString("ArgumentNull_Generic"), "s");
            }
            _serverObject = s;   
            // setup identity
            SetupIdentity();            
        }

        // Gets the stub pointer stashed away in the transparent proxy.
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern IntPtr GetStub();

        // Sets the stub data
        [System.Security.SecurityCritical]  // auto-generated_required
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern void SetStubData(RealProxy rp, Object stubData);

        internal void SetSrvInfo(GCHandle srvIdentity, int domainID)
        {
            _srvIdentity = srvIdentity;
            _domainID = domainID;
        }
        
        // Gets the stub data
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public static extern Object GetStubData(RealProxy rp);

        // Gets the default stub implemented by us
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private static extern IntPtr GetDefaultStub();
        
        
        // Accessor to obtain the type being proxied
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        public extern Type GetProxiedType();

        // Method to which transparent proxy delegates when
        // it gets called
        public abstract IMessage Invoke(IMessage msg);

        [System.Security.SecurityCritical]  // auto-generated
        public virtual ObjRef CreateObjRef(Type requestedType)
        {
            if(_identity == null)
            {
                throw new RemotingException(Environment.GetResourceString(
                    "Remoting_NoIdentityEntry"));
            }        
            
            return new ObjRef((MarshalByRefObject)GetTransparentProxy(), requestedType);    
        }
            
        [System.Security.SecurityCritical]  // auto-generated
        public virtual void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            Object obj = GetTransparentProxy();
            RemotingServices.GetObjectData(obj, info, context);            
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static void HandleReturnMessage(IMessage reqMsg, IMessage retMsg)
        {
            IMethodReturnMessage mrm = retMsg as IMethodReturnMessage;
            if (retMsg==null || (mrm == null))
            {
                throw new RemotingException(Environment.GetResourceString(
                        "Remoting_Message_BadType"));                    
            }

            Exception e = mrm.Exception;

            if (e != null)
            {
                throw e.PrepForRemoting();
            }
            else
            {
                if (!(retMsg is StackBasedReturnMessage))
                {
                    if (reqMsg is Message)
                    {
                        PropagateOutParameters(reqMsg, mrm.Args, mrm.ReturnValue);
                    }
                    else if (reqMsg is ConstructorCallMessage)
                    {
                        // NOTE: We do not extract the return value as 
                        // the process of returning a value from a ConstructorCallMessage
                        // results in marshaling.
                        PropagateOutParameters(reqMsg, mrm.Args, null);
                    }
                }
            }  
        }
        
        // Propagate the out parameters to the stack. This should be called once
        // the call has finished. The input message parameter should be the same
        // as the one which was passed to the first sink to start the call.
        [System.Security.SecurityCritical]  // auto-generated
        internal static void PropagateOutParameters(IMessage msg, 
                                                    Object[] outArgs, 
                                                    Object returnValue)
        {        
            // Check for method call
            Message m = msg as Message;

            // Check for constructor call
            if(null == m)
            {
                ConstructorCallMessage ccm = msg as ConstructorCallMessage;
                if(null != ccm)
                {
                    m = ccm.GetMessage();
                }
            }

            if(null == m)
            {
                throw new ArgumentException(
                    Environment.GetResourceString("Remoting_Proxy_ExpectedOriginalMessage"));
            }

            MethodBase mb = m.GetMethodBase();
            RemotingMethodCachedData cache = 
                    InternalRemotingServices.GetReflectionCachedData(mb);
            if (outArgs != null && outArgs.Length > 0)
            {
                Object[] args = m.Args; // original arguments           

                // If a byref parameter is marked only with [In], we need to copy the 
                //   original value from the request message into outargs, so that the
                //   value won't be bashed by CMessage::PropagateOutParameters below.
                ParameterInfo[] parameters = cache.Parameters;
                foreach (int index in cache.MarshalRequestArgMap)
                {
                    ParameterInfo param = parameters[index];
                    if (param.IsIn && param.ParameterType.IsByRef)
                    {
                        if (!param.IsOut)
                            outArgs[index] = args[index];
                    }
                }

                // copy non-byref arrays back into the same instance
                if (cache.NonRefOutArgMap.Length > 0)
                {
                    foreach (int index in cache.NonRefOutArgMap)
                    {
                        Array arg = args[index] as Array;
                        if (arg != null)
                        {
                            Array.Copy((Array)outArgs[index], arg, arg.Length);
                        }
                    }                    
                }

                // validate by-ref args (This must be done last)
                int[] outRefArgMap = cache.OutRefArgMap;
                if (outRefArgMap.Length > 0)
                {
                    foreach (int index in outRefArgMap)
                    {
                        ValidateReturnArg(outArgs[index], parameters[index].ParameterType);
                    }                    
                }                                
            } 

            // validate return value
            //   (We don't validate Message.BeginAsync because the return value
            //    is always an IAsyncResult and the method base is the one that
            //    represents the underlying synchronous method).
            int callType = m.GetCallType();
            if ((callType & Message.CallMask ) != Message.BeginAsync)
            {
                Type returnType = cache.ReturnType;
                if (returnType != null)
                {
                    ValidateReturnArg(returnValue, returnType);
                }
            }

            m.PropagateOutParameters(outArgs, returnValue);
        } // PropagateOutParameters

        private static void ValidateReturnArg(Object arg, Type paramType)
        {                       
            if (paramType.IsByRef)
                paramType = paramType.GetElementType();

            if (paramType.IsValueType)
            {
                if (arg == null)
                {
                    if (!(paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Nullable<>))) 
                        throw new RemotingException(
                            Environment.GetResourceString("Remoting_Proxy_ReturnValueTypeCannotBeNull"));
                }
                else if (!paramType.IsInstanceOfType(arg))
                {
                    throw new InvalidCastException(
                        Environment.GetResourceString("Remoting_Proxy_BadReturnType"));
                }
            }
            else
            {
                if (arg != null)
                {
                    if (!paramType.IsInstanceOfType(arg))
                    {
                        throw new InvalidCastException(
                            Environment.GetResourceString("Remoting_Proxy_BadReturnType"));
                    }
                }
            }
        } // ValidateReturnArg

        // This is shared code path that executes when an EndInvoke is called 
        // either on a delegate on a proxy 
        // OR a regular delegate (called asynchronously).
        [System.Security.SecurityCritical]  // auto-generated
        internal static IMessage EndInvokeHelper(Message reqMsg, bool bProxyCase)
        {
            AsyncResult ar = reqMsg.GetAsyncResult() as AsyncResult;
            IMessage retMsg = null; // used for proxy case only!
            if (ar == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Remoting_Message_BadAsyncResult"));                    
            }
            if (ar.AsyncDelegate != reqMsg.GetThisPtr())
            {
                throw new InvalidOperationException(Environment.GetResourceString(
                    "InvalidOperation_MismatchedAsyncResult"));
            }
            if (!ar.IsCompleted)
            {
                // Note: using ThreadPoolAware to detect if this is a 
                // ThreadAffinity or Synchronization context.
                ar.AsyncWaitHandle.WaitOne(
                        Timeout.Infinite,
                        Thread.CurrentContext.IsThreadPoolAware);
            }

            lock (ar)
            {
                if (ar.EndInvokeCalled)
                    throw new InvalidOperationException(
                        Environment.GetResourceString(
                            "InvalidOperation_EndInvokeCalledMultiple"));

                ar.EndInvokeCalled = true;
        

                IMethodReturnMessage mrm =  
                    (IMethodReturnMessage) ar.GetReplyMessage();

                Contract.Assert(
                    mrm != null, 
                    "Reply sink should ensure we have a reply message before signalling");

                // For the proxy case this is handled by RealProxy
                if (!bProxyCase)
                {
                    Exception e = mrm.Exception;

                    if (e != null)
                    {
                        // throw e;
                        throw e.PrepForRemoting();
                    }
                    else
                    {
                        reqMsg.PropagateOutParameters(
                            mrm.Args, 
                            mrm.ReturnValue);
                    }
                }
                else
                {
                    retMsg = mrm;
                }
                // Merge the call context back into the thread that 
                // called EndInvoke
                Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.Merge(
                    mrm.LogicalCallContext);
            }
            // Will be non-null only for proxy case!
            return retMsg;            
        } // EndInvokeHelper

        // itnerop methods
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IntPtr GetCOMIUnknown(bool fIsMarshalled)
        {
            // sub -class should override
            return MarshalByRefObject.GetComIUnknown((MarshalByRefObject)GetTransparentProxy());
        }
        
        public virtual void SetCOMIUnknown(IntPtr i)
        {
            // don't care
        }

        public virtual IntPtr SupportsInterface(ref Guid iid)
        {
            return IntPtr.Zero;
        }

        // Method used for traversing back to the TP
        public virtual Object GetTransparentProxy()
        {
            return _tp;
        }

        internal MarshalByRefObject UnwrappedServerObject
        {
            get { return (MarshalByRefObject) _serverObject; }
        }

        internal virtual Identity IdentityObject
        {
            [ReliabilityContract(Consistency.WillNotCorruptState, Cer.Success)]  
            get
            {
                return (Identity) _identity;
            }

            set
            {
                _identity = value;
            }
        }

        // Private method invoked by the transparent proxy
        [System.Security.SecurityCritical]  // auto-generated
        private void PrivateInvoke(ref MessageData msgData, int type)
        {
            IMessage reqMsg = null;
            CallType callType = (CallType)type;
            IMessage retMsg = null;
            int msgFlags = -1;

            // Used only for Construction case
            RemotingProxy rp = null;
            
            // Create a message object based on the type of call
            if(CallType.MethodCall == callType)
            {
                Message msg = new Message();
                msg.InitFields(msgData);
                reqMsg = msg;
                msgFlags = msg.GetCallType();
            }
            else if (CallType.ConstructorCall == (CallType)callType)
            {
                // We use msgFlags to handle CallContext around 
                // the virtual call to Invoke()
                msgFlags = Message.Sync;
                
                rp = this as RemotingProxy;
                ConstructorCallMessage ctorMsg = null;
                bool bIsWellKnown = false;
                if(!IsRemotingProxy())
                {
                    // Create a new constructor call message
                    // <

                    ctorMsg = new ConstructorCallMessage(null, null, null, (RuntimeType)GetProxiedType());
                }                                
                else
                {
                    // Extract the constructor message set in the first step of activation.
                    ctorMsg = rp.ConstructorMessage;                                         
                    // If the proxy is a wellknown client proxy, we don't 
                    // need to run the c'tor.
                    Identity id = rp.IdentityObject;
                    if (id != null)
                        bIsWellKnown = id.IsWellKnown();
                }
                
                if ((null == ctorMsg) || bIsWellKnown)
                {
                    // This is also used to short-circuit the activation path
                    // when we have a well known proxy that has already been
                    // initialized (there's a race condition if we don't do this).
                    //      
                
                    // This is a special case, where we have a remoting proxy
                    // but the constructormessage hasn't been setup.
                    // so let us just bail out.. 
                    // this is currently used by ServicedComponent's for cross appdomain
                    // pooling: <EMAIL>Microsoft</EMAIL>
                    //                    
                    ctorMsg = new ConstructorCallMessage(null, null, null, (RuntimeType)GetProxiedType());                    
                    // Set the constructor frame info in the CCM
                    ctorMsg.SetFrame(msgData); 
                    reqMsg = ctorMsg;

                    // If this was the default ctor, check that default .ctor was called.
                    if (bIsWellKnown)
                    {
                        Contract.Assert(rp!=null, "RemotingProxy expected here!");
                        // Clear any cached ctorMsg on the RemotingProxy
                        rp.ConstructorMessage = null;               

                        // We did execute a Connect. Throw if the client
                        // code is also trying to use a non-default constructor at
                        // the same time.
                        if (ctorMsg.ArgCount != 0)
                        {
                            throw new RemotingException(
                                Environment.GetResourceString(
                                    "Remoting_Activation_WellKnownCTOR"));
                        }
                    }
                    
                    // Create a constructor return message
                    retMsg = 
                        new ConstructorReturnMessage((MarshalByRefObject)GetTransparentProxy(), 
                            null, 
                            0, 
                            null, 
                            ctorMsg);
                }
                else
                {                
                    // Set the constructor frame info in the CCM
                    ctorMsg.SetFrame(msgData);
                    reqMsg = ctorMsg;
                }
            }
            else
            {
                Contract.Assert(false, "Unknown call type");
            }

            // Make sure that outgoing remote calls are counted.
            ChannelServices.IncrementRemoteCalls();

            // For non-remoting proxies, EndAsync should not call Invoke()
            // because the proxy cannot support Async and the call has already
            // finished executing in BeginAsync
            if (!IsRemotingProxy() 
                && ((msgFlags&Message.EndAsync)==Message.EndAsync))
            {

                Message msg = reqMsg as Message;
                retMsg = EndInvokeHelper(msg, true);
                Contract.Assert(null != retMsg, "null != retMsg");
            }

            // Invoke
            Contract.Assert(null != reqMsg, "null != reqMsg");
            if (null == retMsg)
            {
                // NOTE: there are cases where we setup a return message 
                // and we don't want the activation call to go through
                // refer to the note above for ServicedComponents and Cross Appdomain
                // pooling

                LogicalCallContext cctx = null;
                Thread currentThread = Thread.CurrentThread;
                // Pick up or clone the call context from the thread 
                // and install it in the reqMsg as appropriate
                cctx = currentThread.GetMutableExecutionContext().LogicalCallContext;
                SetCallContextInMessage(reqMsg, msgFlags, cctx);
                
                // Add the outgoing "Header"'s to the message.
                cctx.PropagateOutgoingHeadersToMessage(reqMsg);
                retMsg = Invoke(reqMsg);                

                // Get the call context returned and set it on the thread
                ReturnCallContextToThread(currentThread, retMsg, msgFlags, cctx);

                // Pull response "Header"'s out of the message
                Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.PropagateIncomingHeadersToCallContext(retMsg);
            }

            if (!IsRemotingProxy()
                && ((msgFlags&Message.BeginAsync) == Message.BeginAsync))
            {

                // This was a begin-async on a non-Remoting Proxy. For V-1 they 
                // cannot support Async and end up doing a Sync call. We need 
                // to fill up here to make the call look like async to 
                // the caller. 
                // Create the async result to return
                Message msg = reqMsg as Message;
                AsyncResult ar = new AsyncResult(msg);
                // Tell the async result that the call has actually completed 
                // so it can hold on to the return message.
                ar.SyncProcessMessage(retMsg);       
                // create a returnMessage to propagate just the asyncResult back
                // to the caller's stack.
                retMsg = new ReturnMessage(ar, null, 0, null/*cctx*/, msg);
            }
            
            // Propagate out parameters
            HandleReturnMessage(reqMsg, retMsg);

            // For constructor calls do some extra bookkeeping
            if(CallType.ConstructorCall == callType)
            {
                // NOTE: It is the responsiblity of the callee to propagate
                // the out parameters

                // Everything went well, we are ready to return
                // a proxy to the caller
                // Extract the return value
                MarshalByRefObject retObj = null;
                IConstructionReturnMessage ctorRetMsg = retMsg as IConstructionReturnMessage;
                if(null == ctorRetMsg)
                {
                    throw new RemotingException(
                        Environment.GetResourceString("Remoting_Proxy_BadReturnTypeForActivation"));
                }

                ConstructorReturnMessage crm = ctorRetMsg as ConstructorReturnMessage;
                if (null != crm)
                {
                    // If return message is of type ConstructorReturnMessage 
                    // this is an in-appDomain activation. So no unmarshaling
                    // needed.

                    retObj = (MarshalByRefObject)crm.GetObject();
                    if (retObj == null)
                    {
                        throw new RemotingException(
                            Environment.GetResourceString("Remoting_Activation_NullReturnValue"));
                    }
                }
                else
                {
                    // Fetch the objRef out of the returned message and unmarshal it
                    retObj = (MarshalByRefObject)RemotingServices.InternalUnmarshal(
                                (ObjRef)ctorRetMsg.ReturnValue,
                                GetTransparentProxy(),
                                true /*fRefine*/);

                    if (retObj == null)
                    {
                        throw new RemotingException(
                            Environment.GetResourceString("Remoting_Activation_NullFromInternalUnmarshal"));
                    }
                }

                if (retObj != (MarshalByRefObject)GetTransparentProxy())
                {
                    throw new RemotingException(
                        Environment.GetResourceString(
                            "Remoting_Activation_InconsistentState"));
                }
                
                if (IsRemotingProxy())
                {
                    // Clear any cached ctorMsg on the RemotingProxy
                    rp.ConstructorMessage = null;
                }
            }
        }

        void SetCallContextInMessage(
            IMessage reqMsg, int msgFlags, LogicalCallContext cctx)
        {
            Contract.Assert(msgFlags != -1, "Unexpected msgFlags?");
            Message msg = reqMsg as Message;
            
            switch (msgFlags)
            {
            case Message.Sync:
                if (msg != null)
                {
                    msg.SetLogicalCallContext(cctx);
                }
                else
                {
                    ((ConstructorCallMessage)reqMsg).SetLogicalCallContext(cctx);
                }
                break;
            }
        }            

        [System.Security.SecurityCritical]  // auto-generated
        void ReturnCallContextToThread(Thread currentThread, IMessage retMsg, int msgFlags, LogicalCallContext currCtx)
        {
            if (msgFlags == Message.Sync)
            {
                if (retMsg == null)
                    return;

                IMethodReturnMessage mrm = retMsg as IMethodReturnMessage;
                if (mrm == null)
                    return;                            
                            
                LogicalCallContext retCtx = mrm.LogicalCallContext;
                if (retCtx == null) {
                    currentThread.GetMutableExecutionContext().LogicalCallContext = currCtx;
                    return;
                }
                
                if (!(mrm is StackBasedReturnMessage))
                {
                    ExecutionContext ec = currentThread.GetMutableExecutionContext();
                    LogicalCallContext oldCtx = ec.LogicalCallContext;
                    ec.LogicalCallContext = retCtx;
                    if ((Object)oldCtx != (Object)retCtx)
                    {
                        // If the new call context does not match the old call context,
                        //   we must have gone remote. We need to keep the preserve
                        //   the principal from the original call context.
                        IPrincipal principal = oldCtx.Principal;
                        if (principal != null)
                            retCtx.Principal = principal;
                    }                    
                }
                //for other types (async/one-way etc) there is nothing to be 
                //done as we have just finished processing BeginInvoke or EndInvoke
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal virtual void Wrap()
        {
            // <



            ServerIdentity serverID = _identity as ServerIdentity;
            if((null != serverID) && (this is RemotingProxy))
            {
                Contract.Assert(null != serverID.ServerContext, "null != serverID.ServerContext");
                SetStubData(this, serverID.ServerContext.InternalContextID);
            }
        }

        protected RealProxy()
        {
        }
    }
}

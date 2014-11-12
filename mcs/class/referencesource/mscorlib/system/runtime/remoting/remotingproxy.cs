// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    RemotingProxy.cs
**
**
** Purpose: Defines the general purpose remoting proxy
**
**
===========================================================*/
namespace System.Runtime.Remoting.Proxies {
    using System.Threading;
    using System.Runtime.Remoting.Activation;
    using System.Runtime.Remoting.Messaging;    
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Channels;
    using System;
    using MethodInfo = System.Reflection.MethodInfo;
    using MethodBase = System.Reflection.MethodBase;
    using System.Globalization;
    // Remoting proxy
    [System.Security.SecurityCritical]  // auto-generated
    internal class RemotingProxy : RealProxy, IRemotingTypeInfo 
    {
        // Static Fields
        private static MethodInfo _getTypeMethod = typeof(System.Object).GetMethod("GetType");
        private static MethodInfo _getHashCodeMethod = typeof(System.Object).GetMethod("GetHashCode");

        private static RuntimeType s_typeofObject = (RuntimeType)typeof(System.Object);
        private static RuntimeType s_typeofMarshalByRefObject = (RuntimeType)typeof(System.MarshalByRefObject);

        //*******************WARNING******************************************
        // If you change the names of these fields then change the corresponding
        // names in remoting.cpp 
        //********************************************************************        
        private ConstructorCallMessage _ccm;
        private int _ctorThread;
            

        // Constructor
        public RemotingProxy(Type serverType)        
        : base(serverType) 
        {            
        }

        private RemotingProxy()
        {
            // Prevent anyone from creating a blank instance of a proxy
            // without the underlying server type
        }

        internal int CtorThread
        {
            get
            {
                return _ctorThread;
            }
            set
            {
                //NOTE : the assert below is correct for activated objects. 
                //But for a connected object (where new XXX() does a Connect()
                //the InternalActivate codepath may execute twice .. since
                //we would be returning the same proxy for multiple calls to
                //new XXX() & JIT would try to execute the default .ctor on
                //the returned proxy. 
                
                //BCLDebug.Assert(_ctorThread == 0, "ctorThread already set??");
                _ctorThread = value;
            }
        }

        // This is used when a TP is called with SyncProcessMessage
        internal static IMessage CallProcessMessage(IMessageSink ms, 
                                                    IMessage reqMsg, 
                                                    ArrayWithSize proxySinks,
                                                    Thread currentThread,
                                                    Context currentContext,
                                                    bool bSkippingContextChain)
        {                   
            // Notify Dynamic Sinks: CALL starting          
            if (proxySinks != null)
            {
                DynamicPropertyHolder.NotifyDynamicSinks(
                                            reqMsg, 
                                            proxySinks, 
                                            true,   // bCliSide
                                            true,   // bStart
                                            false); // bAsync
            }

            bool bHasDynamicSinks = false;
            if (bSkippingContextChain)
            {
                // this would have been done in the client context terminator sink
                bHasDynamicSinks = 
                    currentContext.NotifyDynamicSinks(reqMsg, 
                        true,   // bCliSide
                        true,   // bStart
                        false,  // bAsync
                        true);  // bNotifyGlobals 
   
                ChannelServices.NotifyProfiler(reqMsg, RemotingProfilerEvent.ClientSend);
            }
            
            if (ms == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Remoting_Proxy_NoChannelSink"));                    
            }

            IMessage retMsg = ms.SyncProcessMessage(reqMsg);

            if (bSkippingContextChain)
            {
                // this would have been done in the client context terminator sink
                ChannelServices.NotifyProfiler(retMsg, RemotingProfilerEvent.ClientReceive);

                if (bHasDynamicSinks)
                {
                    currentContext.NotifyDynamicSinks(
                        retMsg, 
                        true,   // bCliSide
                        false,   // bStart
                        false,  // bAsync
                        true);  // bNotifyGlobals  
                }
            }            

            IMethodReturnMessage mrm = retMsg as IMethodReturnMessage;
            if (retMsg == null || mrm == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Message_BadType"));                    
            }

            // notify DynamicSinks: CALL returned
            if (proxySinks != null)
            {
                DynamicPropertyHolder.NotifyDynamicSinks(
                                            retMsg, 
                                            proxySinks, 
                                            true,   // bCliSide
                                            false,  // bStart
                                            false); // bAsync
            }

           
            return retMsg;
        }

        // Implement Invoke
        [System.Security.SecurityCritical]
        public override IMessage Invoke(IMessage reqMsg) 
        {
            // Dispatch based on whether its a constructor call
            // or a method call

            IConstructionCallMessage ccm = reqMsg as IConstructionCallMessage;        
            
            if(ccm != null)
            {
                // Activate
                return InternalActivate(ccm);
            }
            else
            {
                // Handle regular method calls

                // Check that the initialization has completed
                if(!Initialized)
                {
                    // This covers the case where an object may call out
                    // on another object passing its "this" pointer during its
                    // .ctor. 
                    // The other object attempting to call on the this pointer
                    // (in x-context case) would be calling on a proxy not
                    // marked fully initialized. 
                    // <



                    // Let the original constructor thread go through but 
                    // throw for other threads. 
                    if (CtorThread == Thread.CurrentThread.GetHashCode())
                    {
                        ServerIdentity srvId = IdentityObject as ServerIdentity;
                        BCLDebug.Assert(
                            srvId != null
                            && 
                            ((ServerIdentity)IdentityObject).ServerContext != null,
                            "Wrap may associate with wrong context!");

                        // If we are here, the server object passed itself 
                        // out to another x-context object during the .ctor
                        // That guy is calling us back. Let us call Wrap() 
                        // earlier than usual so that envoy & channel sinks
                        // get set up!
                        // <



                        RemotingServices.Wrap( 
                            (ContextBoundObject) this.UnwrappedServerObject);

                    }
                    else
                    {
                        // Throw an exception to indicate that we are 
                        // calling on a proxy while the constructor call 
                        // is still running.
                        throw new RemotingException(Environment.GetResourceString("Remoting_Proxy_InvalidCall"));
                    }
                    
                }
                
                // Dispatch
                int callType = Message.Sync;
                Message msg = reqMsg as Message;
                if (msg != null)
                {
                    callType = msg.GetCallType(); 
                }                
                
                return InternalInvoke((IMethodCallMessage)reqMsg, false, callType);
            }
            
        } // Invoke
        

        // This is called for all remoted calls on a TP except Ctors
        // The method called may be [....], Async or OneWay(special case of Async)
        // In the Async case we come here for both BeginInvoke & EndInvoke
        internal virtual IMessage InternalInvoke(
            IMethodCallMessage reqMcmMsg, bool useDispatchMessage, int callType)
        {
            Message reqMsg = reqMcmMsg as Message;            
            if ((reqMsg == null) && (callType != Message.Sync))
            {
                // Only the synchronous call type is supported for messages that
                //   aren't of type Message.               
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Proxy_InvalidCallType"));
            }
        
            IMessage retMsg = null;
            Thread currentThread = Thread.CurrentThread;

            // pick up call context from the thread
            LogicalCallContext cctx = currentThread.GetMutableExecutionContext().LogicalCallContext;

            Identity idObj = IdentityObject;
            ServerIdentity serverID = idObj as ServerIdentity;
            if ((null != serverID) && idObj.IsFullyDisconnected())
            {
                throw new ArgumentException(
                   Environment.GetResourceString("Remoting_ServerObjectNotFound", reqMcmMsg.Uri));
            }

            // Short-circuit calls to Object::GetType and Object::GetHashCode
            MethodBase mb = reqMcmMsg.MethodBase;
            if(_getTypeMethod == mb)
            {
                // Time to load the true type of the remote object....
                Type t = GetProxiedType();
                return new ReturnMessage(t, null, 0, cctx, reqMcmMsg);
            }

            if (_getHashCodeMethod == mb)
            {
                int hashCode = idObj.GetHashCode();
                return new ReturnMessage(hashCode, null, 0, cctx, reqMcmMsg);
            }

            // check for channel sink
            if (idObj.ChannelSink == null)
            {
                IMessageSink chnlSink = null;
                IMessageSink envoySink = null;
                // If channelSink is null try to Create them again
                // the objref should be correctly fixed up at this point
                if(!idObj.ObjectRef.IsObjRefLite())
                {
                    RemotingServices.CreateEnvoyAndChannelSinks(null, idObj.ObjectRef, out chnlSink, out envoySink);
                }
                else
                {
                    RemotingServices.CreateEnvoyAndChannelSinks(idObj.ObjURI, null, out chnlSink, out envoySink);
                }
                // Set the envoy and channel sinks in a thread safe manner
                RemotingServices.SetEnvoyAndChannelSinks(idObj, chnlSink, envoySink);

                // If the channel sink is still null then throw
                if(idObj.ChannelSink == null)
                {
                    throw new RemotingException(
                        Environment.GetResourceString("Remoting_Proxy_NoChannelSink"));
                }
            }

            // Set the identity in the message object
            IInternalMessage iim = (IInternalMessage)reqMcmMsg;
            iim.IdentityObject = idObj;

            if (null != serverID)
            {
                Message.DebugOut("Setting serveridentity on message \n");
                iim.ServerIdentityObject = serverID;
                    
            }
            else
            {
                // We need to set the URI only for identities that 
                // are not the server identities. The uri is used to
                // dispatch methods for objects outside the appdomain.
                // Inside the appdomain (xcontext case) we dispatch 
                // by getting the server object from the server identity.
               iim.SetURI(idObj.URI);
            }       

            Message.DebugOut("InternalInvoke. Dispatching based on class type\n");
            AsyncResult ar = null;
            switch (callType)
            {
            case Message.Sync:
                Message.DebugOut("RemotingProxy.Invoke Call: SyncProcessMsg\n");
                BCLDebug.Assert(!useDispatchMessage,"!useDispatchMessage");                
                bool bSkipContextChain = false;
                Context currentContext = currentThread.GetCurrentContextInternal();
                IMessageSink nextSink = idObj.EnvoyChain;

                // if we are in the default context, there can be no 
                // client context chain, so we can skip the intermediate 
                // calls if there are no envoy sinks

                if (currentContext.IsDefaultContext)
                {
                    if (nextSink is EnvoyTerminatorSink)
                    {
                        bSkipContextChain = true;

                        // jump directly to the channel sink
                        nextSink = idObj.ChannelSink;
                    }
                }

                retMsg = CallProcessMessage(nextSink,
                                            reqMcmMsg, 
                                            idObj.ProxySideDynamicSinks,
                                            currentThread,
                                            currentContext,
                                            bSkipContextChain);

                break;

            case Message.BeginAsync:
            case Message.BeginAsync | Message.OneWay:                                        
                // For async calls we clone the call context from the thread
                // This is a limited clone (we dont deep copy the user data)
                cctx = (LogicalCallContext) cctx.Clone(); 
                iim.SetCallContext(cctx);  
                
                ar = new AsyncResult(reqMsg);
              
                InternalInvokeAsync(ar, reqMsg, useDispatchMessage, callType);

                Message.DebugOut("Propagate out params for BeginAsync\n");
                retMsg = new ReturnMessage(ar, null, 0, null/*cctx*/, reqMsg);
                break;

            case Message.OneWay:
                // For async calls we clone the call context from the thread
                // This is a limited clone (we dont deep copy the user data)
                cctx = (LogicalCallContext) cctx.Clone();
                iim.SetCallContext(cctx);
                InternalInvokeAsync(null, reqMsg, useDispatchMessage, callType);
                retMsg = new ReturnMessage(null, null, 0, null/*cctx*/, reqMcmMsg);
                break;

            case (Message.EndAsync | Message.OneWay):
                retMsg = new ReturnMessage(null, null, 0, null/*cctx*/, reqMcmMsg);
                break;

            case Message.EndAsync:
                // For endAsync, we merge back the returned callContext
                // into the thread's callContext
                retMsg = RealProxy.EndInvokeHelper(reqMsg, true);
                break;
            }
            
            return retMsg;
        }


        // This is called from InternalInvoke above when someone makes an
        // Async (or a one way) call on a TP
        internal void InternalInvokeAsync(IMessageSink ar,  Message reqMsg, 
            bool useDispatchMessage, int callType)
        {
            IMessageCtrl cc = null;
            Identity idObj = IdentityObject;
            ServerIdentity serverID = idObj as ServerIdentity;
            MethodCall cpyMsg= new MethodCall(reqMsg);
            IInternalMessage iim = ((IInternalMessage)cpyMsg);

            // Set the identity in the message object
            iim.IdentityObject = idObj;
            if (null != serverID)
            {
                Message.DebugOut("Setting SrvID on deser msg\n");
                iim.ServerIdentityObject = serverID;                    
            }

            if (useDispatchMessage)
            {
                Message.DebugOut(
                    "RemotingProxy.Invoke: Calling AsyncDispatchMessage\n");

                BCLDebug.Assert(ar != null,"ar != null");
                BCLDebug.Assert( (callType & Message.BeginAsync) != 0,
                                "BeginAsync flag not set!");

                Message.DebugOut("Calling AsynDispatchMessage \n");
                cc = ChannelServices.AsyncDispatchMessage(
                                        cpyMsg, 
                                        ((callType & Message.OneWay) != 0) 
                                        ? null : ar);
            }
            else if (null != idObj.EnvoyChain)
            {
                Message.DebugOut("RemotingProxy.Invoke: Calling AsyncProcessMsg on the envoy chain\n");

                cc = idObj.EnvoyChain.AsyncProcessMessage(
                                        cpyMsg, 
                                        ((callType & Message.OneWay) != 0) 
                                        ? null : ar);
            }
            else
            {
                // Channel sink cannot be null since it is the last sink in
                // the client context
                // Assert if Invoke is called without a channel sink
                BCLDebug.Assert(false, "How did we get here?");
                
                throw new InvalidOperationException(
                    Environment.GetResourceString("Remoting_Proxy_InvalidState"));
            }

            if ((callType & Message.BeginAsync) != 0)
            {

                if ((callType & Message.OneWay) != 0)
                {
                    ar.SyncProcessMessage(null);
                }
            }
        }

        // New method for activators.
        
        // This gets called during remoting intercepted activation when 
        // JIT tries to run a constructor on a TP (which remoting gave it
        // in place of an actual uninitialized instance of the expected type)
        private IConstructionReturnMessage InternalActivate(IConstructionCallMessage ctorMsg)
        {
            // Remember the hashcode of the constructor thread.
            CtorThread = Thread.CurrentThread.GetHashCode();

            IConstructionReturnMessage ctorRetMsg = ActivationServices.Activate(this, ctorMsg);

            // Set the flag to indicate that the object is initialized
            // Note: this assert is not valid for WKOs
            //BCLDebug.Assert(!Initialized, "Proxy marked as initialized before activation call completed");
            Initialized = true;

            return ctorRetMsg;
        }

        // Invoke for case where call is in the same context as the server object
        // (This special static method is used for AsyncDelegate-s ... this is called
        // directly from the EE)
        private static void Invoke(Object NotUsed, ref MessageData msgData)
        {
            Message m = new Message();
            m.InitFields(msgData);

            Object thisPtr = m.GetThisPtr();
            Delegate d;
            if ((d = thisPtr as Delegate) != null)
            {
                // <

                RemotingProxy rp = (RemotingProxy)
                    RemotingServices.GetRealProxy(d.Target);

                if (rp != null)
                {
                    rp.InternalInvoke(m, true, m.GetCallType());
                }
                else
                {
                    int callType = m.GetCallType();       
                    AsyncResult ar;
                    switch (callType)
                    {
                    case Message.BeginAsync:
                    case Message.BeginAsync | Message.OneWay:
                        // pick up call context from the thread
                        m.Properties[Message.CallContextKey] =
                            Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext.Clone();
                        ar = new AsyncResult(m);
                        AgileAsyncWorkerItem  workItem = 
                            new AgileAsyncWorkerItem(
                                    m, 
                                    ((callType & Message.OneWay) != 0) ? 
                                        null : ar, d.Target);

                        ThreadPool.QueueUserWorkItem(
                            new WaitCallback(
                                    AgileAsyncWorkerItem.ThreadPoolCallBack),
                            workItem);

                        if ((callType & Message.OneWay) != 0)
                        {
                            ar.SyncProcessMessage(null);
                        }
                        m.PropagateOutParameters(null, ar);
                        break;
                    case (Message.EndAsync | Message.OneWay):
                        return;

                    case Message.EndAsync:
                        // This will also merge back the call context
                        // onto the thread that called EndAsync
                        RealProxy.EndInvokeHelper(m, false);
                        break;
                    default:
                        BCLDebug.Assert(
                            false, 
                            "Should never be here. Sync delegate code for agile object ended up in remoting");
                        break;
                    }
                }
            }
            else
            {
                // Static invoke called with incorrect this pointer ...
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Remoting_Default"));                    
            }
        }

        internal ConstructorCallMessage ConstructorMessage
        {
            get
            {
                return _ccm;
            }

            set
            {
                _ccm = value;
            }
        }

        //
        // IRemotingTypeInfo interface
        //

        // Obtain the fully qualified name of the type that the proxy represents
        public String TypeName 
        {
            [System.Security.SecurityCritical]
            get
            {
                return GetProxiedType().FullName;
            }

            [System.Security.SecurityCritical]
            set
            {
                throw new NotSupportedException();
            }
        }

#if FEATURE_COMINTEROP
        // interop methods
        [System.Security.SecurityCritical]
        public override IntPtr GetCOMIUnknown(bool fIsBeingMarshalled)
        {
            IntPtr pUnk = IntPtr.Zero;
            Object otp = GetTransparentProxy();            
            bool fIsXProcess = RemotingServices.IsObjectOutOfProcess(otp);
            if (fIsXProcess)
            {
                // we are in a different process
                if (fIsBeingMarshalled)
                {
                    // we need to go to the server to get the real IUnknown
                    pUnk =  MarshalByRefObject.GetComIUnknown((MarshalByRefObject)otp);
                }
                else
                {
                    // create an IUnknown here
                    pUnk =  MarshalByRefObject.GetComIUnknown((MarshalByRefObject)otp);    
                }
            }
            else
            {
                bool fIsXAppDomain = RemotingServices.IsObjectOutOfAppDomain(otp);
                // we are in the same proces, ask the object for its IUnknown
                if (fIsXAppDomain)
                {
                    // do an appdomain switch
                    pUnk = ((MarshalByRefObject)otp).GetComIUnknown(fIsBeingMarshalled);
                }
                else
                {    
                    // otherwise go ahead and create a CCW here
                    pUnk = MarshalByRefObject.GetComIUnknown((MarshalByRefObject)otp);
                }
            }

            return pUnk;
        }

        [System.Security.SecurityCritical]
        public override void SetCOMIUnknown(IntPtr i)
        {
            // for now ignore this
        }
#endif // FEATURE_COMINTEROP

        // Check whether we can cast the transparent proxy to the given type
        [System.Security.SecurityCritical]
        public bool CanCastTo(Type castType, Object o)
        {
            if (castType == null)
                throw new ArgumentNullException("castType");

            RuntimeType rtType = castType as RuntimeType;
            if (rtType == null)
                throw new ArgumentException(Environment.GetResourceString("Argument_MustBeRuntimeType"));

            bool fCastOK = false;

            // The identity should be non-null
            BCLDebug.Assert(null != IdentityObject,"null != IdentityObject");

            Message.DebugOut("CheckCast for identity " + IdentityObject.GetType());

            if ((rtType == s_typeofObject) ||
                (rtType == s_typeofMarshalByRefObject))
            {
                return true;
            }

            // Get the objref of the proxy
            ObjRef oRef = IdentityObject.ObjectRef;

            // If the object ref is non-null then check against the type info
            // stored in the it
            if (null != oRef)
            {
                Object oTP = GetTransparentProxy();

                // Check that there is a matching type in the server object 
                // hierarchy represented in the objref                                      
                Message.DebugOut("Calling CanCastTo for type " + rtType);
                IRemotingTypeInfo typeInfo = oRef.TypeInfo;
                if(null != typeInfo)
                {
                    fCastOK = typeInfo.CanCastTo(rtType, oTP);
                    if (!fCastOK && typeInfo.GetType()==typeof(TypeInfo) && oRef.IsWellKnown() )
                    {
                        fCastOK = CanCastToWK(rtType);
                    }
                }                                
                else
                {
                    if (oRef.IsObjRefLite())
                    {
                        // we should do a dynamic cast across the network
                        fCastOK = MarshalByRefObject.CanCastToXmlTypeHelper(rtType, (MarshalByRefObject)o);  
                    }
                }
            }
            // This is a well known object which does not have a backing ObjRef
            else
            {
                fCastOK = CanCastToWK(rtType);
            }
            return fCastOK;
        }

        // WellKnown proxies we always allow casts to interfaces, and allow 
        // casting down a single branch in the type hierarchy (both are on good
        // faith. The calls are failed on server side if a bogus cast is done)
        bool CanCastToWK(Type castType)
        {
            Message.DebugOut( "CheckCast for well known objects and type " + castType);
            bool fCastOK = false;
            // Check whether the type to which we want to cast is
            // compatible with the current type
            if(castType.IsClass)
            {
                fCastOK = GetProxiedType().IsAssignableFrom(castType);
            }
            else
            {
                // NOTE: we are coming here also for x-context proxies
                // when unmanaged code cannot determine if the cast is not
                // okay <
                if (!(IdentityObject is ServerIdentity))
                {
                    BCLDebug.Assert(
                        IdentityObject.URI != null,
                        "Bad WellKnown ID");
                    // Always allow interface casts to succeed. If the 
                    // interface is not supported by the well known object
                    // then we will throw an exception when the interface
                    // is invoked.
                    fCastOK = true;
                }
            }
            
            return fCastOK;
        }        
    }


    internal class AgileAsyncWorkerItem
    {
        private IMethodCallMessage _message;
        private AsyncResult        _ar;
        private Object             _target;

        [System.Security.SecurityCritical]  // auto-generated
        public AgileAsyncWorkerItem(IMethodCallMessage message, AsyncResult ar, Object target)
        {
            _message = new MethodCall(message);
            _ar = ar;
            _target = target;
        }

        [System.Security.SecurityCritical]  // auto-generated
        public static void ThreadPoolCallBack(Object o)
        {
            ((AgileAsyncWorkerItem) o).DoAsyncCall();
        }


        [System.Security.SecurityCritical]  // auto-generated
        public void DoAsyncCall()
        {
            (new StackBuilderSink(_target)).AsyncProcessMessage(_message, _ar);
        }
    }

}

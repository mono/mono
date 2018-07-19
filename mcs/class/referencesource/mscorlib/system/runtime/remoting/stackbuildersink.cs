// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    StackBuilderSink.cs
**
**
** Purpose: Terminating sink which will build a stack and 
**          make a method call on an object
**
**
===========================================================*/
namespace System.Runtime.Remoting.Messaging {
    using System.Runtime.InteropServices;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Channels;
    using System.Runtime.Remoting.Metadata;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using System.Runtime.Versioning;
    using System.Security.Principal;
    /* Assembly Access */
    using System;
    using System.Globalization;
    using System.Threading;

    [Serializable]
    internal class StackBuilderSink : IMessageSink
    {

        //+============================================================
        //
        // Method:     Constructor, public
        //
        // Synopsis:   Store server object
        //
        // History:    06-May-1999  <EMAIL>MattSmit</EMAIL>   Created
        //-============================================================
        public StackBuilderSink(MarshalByRefObject server)
        {
            _server = server;
        }
        public StackBuilderSink(Object server)
        {
            _server = server;
            if (_server==null)
            {
                _bStatic = true;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage SyncProcessMessage(IMessage msg)
        {
            // Validate message here 
            IMessage errMsg = InternalSink.ValidateMessage(msg);        
            if (errMsg != null)
            {
                return errMsg;
            }

            IMethodCallMessage mcMsg = msg as IMethodCallMessage;            

            IMessage retMessage;
            LogicalCallContext oldCallCtx = null;

            LogicalCallContext lcc = Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext;
            object xADCall = lcc.GetData(CrossAppDomainSink.LCC_DATA_KEY);
            
            bool isCallContextSet = false;
            try
            {
                Object server = _server;

                BCLDebug.Assert((server!=null) == (!_bStatic),
                                "Invalid state in stackbuilder sink?");

                // validate the method base if necessary
                VerifyIsOkToCallMethod(server, mcMsg);
                
                // install call context onto the thread, holding onto
                // the one that is currently on the thread
   
                LogicalCallContext messageCallContext = null;
                if (mcMsg != null)
                {
                    messageCallContext = mcMsg.LogicalCallContext;                    
                }
                else
                {
                    messageCallContext = (LogicalCallContext)msg.Properties["__CallContext"];
                }
                                
                oldCallCtx = CallContext.SetLogicalCallContext(messageCallContext);
                isCallContextSet = true;

                messageCallContext.PropagateIncomingHeadersToCallContext(msg);

                PreserveThreadPrincipalIfNecessary(messageCallContext, oldCallCtx);
                

                // NOTE: target for dispatch will be NULL when the StackBuilderSink
                // is used for async delegates on static methods.

                // *** NOTE ***
                // Although we always pass _server to these calls in the EE,
                // when we execute using Message::Dispatch we are using TP as 
                // the this-ptr ... (what the call site thinks is the this-ptr)
                // when we execute using StackBuilderSink::PrivatePM we use
                // _server as the this-ptr (which could be different if there
                // is interception for strictly MBR types in the same AD).
                // ************
                if (IsOKToStackBlt(mcMsg, server) 
                    && ((Message)mcMsg).Dispatch(server))
                {
                    //retMessage = StackBasedReturnMessage.GetObjectFromPool((Message)mcMsg);
                    retMessage = new StackBasedReturnMessage();
                    ((StackBasedReturnMessage)retMessage).InitFields((Message)mcMsg);

                    // call context could be different then the one from before the call.
                    LogicalCallContext latestCallContext = Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext;
                    // retrieve outgoing response headers
                    latestCallContext.PropagateOutgoingHeadersToMessage(retMessage);
                    
                    // Install call context back into Message (from the thread)
                    ((StackBasedReturnMessage)retMessage).SetLogicalCallContext(latestCallContext);
                } 
                else
                {
                    MethodBase mb = GetMethodBase(mcMsg); 
                    Object[] outArgs = null;
                    Object ret = null;                      
                    
                    RemotingMethodCachedData methodCache = 
                        InternalRemotingServices.GetReflectionCachedData(mb);

                    Message.DebugOut("StackBuilderSink::Calling PrivateProcessMessage\n");

                    Object[] args = Message.CoerceArgs(mcMsg, methodCache.Parameters);
                
                    ret = PrivateProcessMessage(
                                        mb.MethodHandle,
                                        args,
                                        server,
                                        out outArgs); 
                    CopyNonByrefOutArgsFromOriginalArgs(methodCache, args, ref outArgs);


                    // call context could be different then the one from before the call.
                    LogicalCallContext latestCallContext = Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext;

                    if (xADCall != null && ((bool)xADCall) == true && latestCallContext != null)
                    {
                        // Special case Principal since if might not be serializable before returning
                        // ReturnMessage
                        latestCallContext.RemovePrincipalIfNotSerializable();
                    }

                    retMessage = new ReturnMessage( 
                                        ret, 
                                        outArgs, 
                                        (outArgs == null ? 0 : outArgs.Length),
                                        latestCallContext,
                                        mcMsg);

                    // retrieve outgoing response headers
                    latestCallContext.PropagateOutgoingHeadersToMessage(retMessage);
                    // restore the call context on the thread
                    CallContext.SetLogicalCallContext(oldCallCtx);
                }


            } catch (Exception e)
            {
                Message.DebugOut(
                "StackBuilderSink::The server object probably threw an exception " +
                                 e.Message + e.StackTrace + "\n" );
                retMessage = new ReturnMessage(e, mcMsg);
                ((ReturnMessage)retMessage).SetLogicalCallContext(mcMsg.LogicalCallContext);
                        
                if (isCallContextSet)
                    CallContext.SetLogicalCallContext(oldCallCtx);
            }


            return retMessage;
        }


        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(
            IMessage msg, IMessageSink replySink) 
        { 
            IMethodCallMessage mcMsg = (IMethodCallMessage)msg;

            IMessageCtrl retCtrl = null;
            IMessage retMessage = null;
            LogicalCallContext oldCallCtx = null;
            bool isCallContextSet = false;
            try{
                try
                {
                    LogicalCallContext callCtx =  (LogicalCallContext)
                        mcMsg.Properties[Message.CallContextKey];
                        
                    Object server = _server;

                    // validate the method base if necessary
                    VerifyIsOkToCallMethod(server, mcMsg);
                    
                    // install call context onto the thread, holding onto
                    // the one that is currently on the thread

                    oldCallCtx = CallContext.SetLogicalCallContext(callCtx);
                    isCallContextSet = true;
                    // retrieve incoming headers
                    callCtx.PropagateIncomingHeadersToCallContext(msg);

                    PreserveThreadPrincipalIfNecessary(callCtx, oldCallCtx);

                    // see if this is a server message that was dispatched asynchronously
                    ServerChannelSinkStack sinkStack = 
                        msg.Properties["__SinkStack"] as ServerChannelSinkStack;
                    if (sinkStack != null)
                        sinkStack.ServerObject = server;
                    
                    BCLDebug.Assert((server!=null)==(!_bStatic),
                                    "Invalid state in stackbuilder sink?");   
                                                
                    MethodBase mb = GetMethodBase(mcMsg);
                    Object[] outArgs = null;
                    Object ret = null;                
                    RemotingMethodCachedData methodCache = 
                        InternalRemotingServices.GetReflectionCachedData(mb);                                
                    Object[] args = Message.CoerceArgs(mcMsg, methodCache.Parameters);

                    ret = PrivateProcessMessage(mb.MethodHandle,
                                                args,
                                                server,
                                                out outArgs);
                    CopyNonByrefOutArgsFromOriginalArgs(methodCache, args, ref outArgs);
                                                           
                    if(replySink != null)
                    {     
                        // call context could be different then the one from before the call.
                        LogicalCallContext latestCallContext = Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext;
                        
                        if (latestCallContext != null)
                        {
                            // Special case Principal since if might not be serializable before returning
                            // ReturnMessage
                            latestCallContext.RemovePrincipalIfNotSerializable();
                        }
                        
                        retMessage = new ReturnMessage(
                                            ret, 
                                            outArgs, 
                                            (outArgs == null ? 0 : outArgs.Length), 
                                            latestCallContext, 
                                            mcMsg);

                        // retrieve outgoing response headers
                        latestCallContext.PropagateOutgoingHeadersToMessage(retMessage);
        
                    }
                } 
                catch (Exception e)
                {
                    if(replySink != null)
                    {
                        retMessage = new ReturnMessage(e, mcMsg);
                        ((ReturnMessage)retMessage).SetLogicalCallContext(
                                (LogicalCallContext)
                                    mcMsg.Properties[Message.CallContextKey]);

                    }
                }
                finally
                {
                    if (replySink != null)
                    {
                        // Call the reply sink without catching the exceptions
                        // in it. In v2.0 any exceptions in the callback for example
                        // would probably bring down the process
                        replySink.SyncProcessMessage(retMessage);
                    }
                }
            }
            finally {
                // restore the call context on the thread
                if (isCallContextSet)
                    CallContext.SetLogicalCallContext(oldCallCtx);
            }

            return retCtrl; 
        } // AsyncProcessMessage

        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                // there is no nextSink for the StackBuilderSink
                return null;
            }
        }

        // This check if the call-site on the TP is in our own AD
        // It also handles the special case where the TP is on 
        // a well-known object and we cannot do stack-blting
        [System.Security.SecurityCritical]  // auto-generated
        internal bool IsOKToStackBlt(IMethodMessage mcMsg, Object server)
        {
            bool bOK = false;
            Message msg = mcMsg as Message;
            if(null != msg)
            {
                IInternalMessage iiMsg = (IInternalMessage) msg;
 
                // If there is a frame in the message we can always
                // Blt it (provided it is not a proxy to a well-known
                // object in our own appDomain)!
                // The GetThisPtr == server test allows for people to wrap
                // our proxy with their own interception .. in that case
                // we should not blt the stack.
                if (msg.GetFramePtr() != IntPtr.Zero
                    && msg.GetThisPtr() == server
                    && 
                    (   iiMsg.IdentityObject == null ||
                        (  iiMsg.IdentityObject != null 
                            && iiMsg.IdentityObject == iiMsg.ServerIdentityObject
                        )
                    )
                )
                {
                    bOK = true;
                }
            }

            return bOK;
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        private static MethodBase GetMethodBase(IMethodMessage msg)
        {
            MethodBase mb = msg.MethodBase;
            if(null == mb)      
            {
                BCLDebug.Trace("REMOTE", "Method missing w/name ", msg.MethodName);
                    throw new RemotingException(
                        String.Format(
                            CultureInfo.CurrentCulture, Environment.GetResourceString(
                                "Remoting_Message_MethodMissing"),
                            msg.MethodName,
                            msg.TypeName));
            }
                    
            return mb;
        }

        [System.Security.SecurityCritical]  // auto-generated
        private static void VerifyIsOkToCallMethod(Object server, IMethodMessage msg)
        {
            bool bTypeChecked = false;
            MarshalByRefObject mbr = server as MarshalByRefObject;
            if (mbr != null)
            {
                bool fServer;
                Identity id = MarshalByRefObject.GetIdentity(mbr, out fServer);
                if (id != null)
                {
                    ServerIdentity srvId = id as ServerIdentity;
                    if ((srvId != null) && srvId.MarshaledAsSpecificType)
                    {
                        Type srvType = srvId.ServerType;
                        if (srvType != null)
                        {
                            MethodBase mb = GetMethodBase(msg);
                            RuntimeType declaringType = (RuntimeType)mb.DeclaringType;
                        
                            // make sure that srvType is not more restrictive than method base
                            // (i.e. someone marshaled with a specific type or interface exposed)
                            if ((declaringType != srvType) &&
                                !declaringType.IsAssignableFrom(srvType))
                            {
                                throw new RemotingException(
                                    String.Format(
                                        CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_InvalidCallingType"),
                                        mb.DeclaringType.FullName, srvType.FullName));
                                    
                            }
                            // Set flag so we don't repeat this work below.
                            if (declaringType.IsInterface)
                            {
                                VerifyNotIRemoteDispatch(declaringType);
                            }  
                            bTypeChecked = true;
                        }
                    }
                }

                // We must always verify that the type corresponding to 
                // the method being invoked is compatible with the real server 
                // type.
                if (!bTypeChecked)
                {
                    MethodBase mb = GetMethodBase(msg); 
                    RuntimeType reflectedType = (RuntimeType)mb.ReflectedType;
                    if (!reflectedType.IsInterface)
                    {
                        if (!reflectedType.IsInstanceOfType(mbr))
                        {
                            throw new RemotingException(
                                String.Format(
                                    CultureInfo.CurrentCulture, Environment.GetResourceString("Remoting_InvalidCallingType"),
                                    reflectedType.FullName, 
                                    mbr.GetType().FullName));
                        }
                    }
                    // This code prohibits calls made to System.EnterpriseServices.IRemoteDispatch
                    // so that remote call cannot bypass lowFilterLevel logic in the serializers.
                    // This special casing should be removed in the future
                    else
                    {
                        VerifyNotIRemoteDispatch(reflectedType);
                    }
                }
            }            

        } // VerifyIsOkToCallMethod

        // This code prohibits calls made to System.EnterpriseServices.IRemoteDispatch
        // so that remote call cannot bypass lowFilterLevel logic in the serializers.
        // This special casing should be removed in the future
        // Check whether we are calling IRemoteDispatch
        [System.Security.SecurityCritical]  // auto-generated
        private static void VerifyNotIRemoteDispatch(RuntimeType reflectedType)
        {
            if(reflectedType.FullName.Equals(sIRemoteDispatch) && 
               reflectedType.GetRuntimeAssembly().GetSimpleName().Equals(sIRemoteDispatchAssembly))
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_CantInvokeIRemoteDispatch"));
            }
        }

        // copies references of non-byref [In, Out] args from the input args to
        //   the output args array.
        internal void CopyNonByrefOutArgsFromOriginalArgs(RemotingMethodCachedData methodCache,
                                                         Object[] args,
                                                         ref Object[] marshalResponseArgs)
        {            
            int[] map = methodCache.NonRefOutArgMap;
            if (map.Length > 0)
            {
                if (marshalResponseArgs == null)
                    marshalResponseArgs = new Object[methodCache.Parameters.Length];
                
                foreach (int pos in map)
                {
                    marshalResponseArgs[pos] = args[pos];
                }
            }
        }


        // For the incoming call, we sometimes need to preserve the thread principal from
        //   the executing thread, instead of blindly bashing it with the one from the message.
        //   For instance, in cross process calls, the principal will always be null
        //   in the incoming message. However, when we are hosted in ASP.Net, ASP.Net will handle
        //   authentication and set up the thread principal. We should dispatch the call
        //   using the identity that it set up.
        [System.Security.SecurityCritical]  // auto-generated
        internal static void PreserveThreadPrincipalIfNecessary(
            LogicalCallContext messageCallContext, 
            LogicalCallContext threadCallContext)
        {
            BCLDebug.Assert(messageCallContext != null, "message should always have a call context");

            if (threadCallContext != null)
            {
                if (messageCallContext.Principal == null)
                {                
                    IPrincipal currentPrincipal = threadCallContext.Principal;
                    if (currentPrincipal != null)
                    {
                        messageCallContext.Principal = currentPrincipal;
                    }
                }
            }
        } // PreserveThreadPrincipalIfNecessary


        internal Object ServerObject
        {
            get { return _server; }
        }
        
        
        //+============================================================
        //
        // Method:     PrivateProcessMessage, public
        //
        // Synopsis:   does the actual work of building the stack, 
        //             finding the correct code address and calling it.
        //
        // History:    06-May-1999  <EMAIL>MattSmit</EMAIL>   Created
        //-============================================================
        [System.Security.SecurityCritical]  // auto-generated
        [ResourceExposure(ResourceScope.None)]
        [MethodImplAttribute(MethodImplOptions.InternalCall)]
        private extern Object _PrivateProcessMessage(
            IntPtr md, Object[] args, Object server,
            out Object[] outArgs);
        [System.Security.SecurityCritical]  // auto-generated
        public Object  PrivateProcessMessage(
            RuntimeMethodHandle md, Object[] args, Object server,
            out Object[] outArgs)
        {
            return _PrivateProcessMessage(md.Value, args, server, out outArgs);
        }

        private Object _server;              // target object
        private static string sIRemoteDispatch = "System.EnterpriseServices.IRemoteDispatch";
        private static string sIRemoteDispatchAssembly = "System.EnterpriseServices";

        bool _bStatic;
     }
}


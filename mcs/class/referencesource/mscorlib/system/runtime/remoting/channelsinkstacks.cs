// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
/*============================================================
**
** File:    ChannelSinkStacks.cs
**
** Purpose: Defines the stack interfaces.
**
**
===========================================================*/

using System;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Metadata;
using System.Security.Permissions;
using System.Threading;


namespace System.Runtime.Remoting.Channels {
        

     // interface for maintaining the sink stack
    //   The formatter sink MUST provide this object.
    //   No other sinks should have to check to see if this is null.
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IClientChannelSinkStack : IClientResponseChannelSinkStack
    {
        // Push a sink to the stack (it will be called on the way back to get
        //   the response stream).
        [System.Security.SecurityCritical]  // auto-generated_required
        void Push(IClientChannelSink sink, Object state);

        // Retrieve state previously pushed by sink.
        [System.Security.SecurityCritical]  // auto-generated_required
        Object Pop(IClientChannelSink sink);
        
    } // IChannelSinkStack

[System.Runtime.InteropServices.ComVisible(true)]
    public interface IClientResponseChannelSinkStack
    {
        // Call AsyncProcessResponse (on previous channel sink)
        [System.Security.SecurityCritical]  // auto-generated_required
        void AsyncProcessResponse(ITransportHeaders headers, Stream stream);

        // Called by client formatter sink in AsyncProcessResponse once it has
        //   deserialized the response message.
        [System.Security.SecurityCritical]  // auto-generated_required
        void DispatchReplyMessage(IMessage msg);

        // If an exception happens on the async channel sink path, the
        //   sink should call this method with the exception.
         [System.Security.SecurityCritical]  // auto-generated_required
       void DispatchException(Exception e);
     
    } // interface IClientResponseChannelSinkStack


    [System.Security.SecurityCritical]  // auto-generated_required
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ClientChannelSinkStack : IClientChannelSinkStack
    {
        private class SinkStack
        {
            public SinkStack PrevStack;
            
            public IClientChannelSink Sink;
            public Object State;
        }

        private SinkStack _stack = null;
        
        private IMessageSink _replySink = null;


        public ClientChannelSinkStack()
        {
        }

        // use this constructor when initiating an async call
        public ClientChannelSinkStack(IMessageSink replySink)
        {
            _replySink = replySink;
        }



        [System.Security.SecurityCritical]
        public void Push(IClientChannelSink sink, Object state)
        {
            SinkStack newStack = new SinkStack();
            newStack.PrevStack = _stack;
            newStack.Sink = sink;
            newStack.State = state;
            _stack = newStack;
        } // Push


        // retrieve state previously pushed by sink
        [System.Security.SecurityCritical]
        public Object Pop(IClientChannelSink sink)
        {
            if (_stack == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Channel_PopOnEmptySinkStack"));
            }

            // find this sink on the stack
            do 
            {
                if (_stack.Sink == sink)
                    break;

                _stack = _stack.PrevStack;
            } while (_stack != null);

            if (_stack.Sink == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Channel_PopFromSinkStackWithoutPush"));
            }

            Object state = _stack.State;
            _stack = _stack.PrevStack;

            return state;
        } // Pop


        [System.Security.SecurityCritical]  // auto-generated
        public void AsyncProcessResponse(ITransportHeaders headers, Stream stream)
        {
            // If the reply sink is null, this is a one way message, so we're not
            //   going to process the reply path.
            if (_replySink != null)
            {
                if (_stack == null)
                {
                    throw new RemotingException(
                        Environment.GetResourceString(
                            "Remoting_Channel_CantCallAPRWhenStackEmpty"));
                }

                IClientChannelSink sink = _stack.Sink;
                Object state = _stack.State;
                _stack = _stack.PrevStack;
    
                sink.AsyncProcessResponse(this, state, headers, stream);
            }
        } // AsyncProcessResponse


        // Client formatter sink should call this in AysncProcessResponse once
        //   it has deserialized a message.
        [System.Security.SecurityCritical]  // auto-generated
        public void DispatchReplyMessage(IMessage msg)
        {
            if (_replySink != null)
                _replySink.SyncProcessMessage(msg);
        } // DispatchReplyMessage


        [System.Security.SecurityCritical]  // auto-generated
        public void DispatchException(Exception e)
        {
            DispatchReplyMessage(new ReturnMessage(e, null));
        } // DispatchException
        
    } // ClientChannelSinkStack





    // interface for maintaining the sink stack
    //   The transport sink MUST provide this object.
    //   No other sinks should have to check to see if this is null.
[System.Runtime.InteropServices.ComVisible(true)]
    public interface IServerChannelSinkStack : IServerResponseChannelSinkStack
    {
        // Push a sink to the stack (it will be called on the way back to get
        //   the response stream).
        [System.Security.SecurityCritical]  // auto-generated_required
        void Push(IServerChannelSink sink, Object state);

        // Retrieve state previously pushed by sink.
        [System.Security.SecurityCritical]  // auto-generated_required
        Object Pop(IServerChannelSink sink);
        
        /// <internalonly/>

        // IMPORTANT: If a sink did a Push(), it must do a Pop()
        //   before calling GetResponseStream inside of ProcessMessage.
    
        // On the way back, if it is determined that a asynchronous processing is 
        //   needed, a sink should call Store() instead of Pop()
        [System.Security.SecurityCritical]  // auto-generated_required
        void Store(IServerChannelSink sink, Object state);
        
        /// <internalonly/>
        
        // Called by the server transport sink to complete the dispatch, if async
        //   processing is being used.        
        [System.Security.SecurityCritical]  // auto-generated_required
        void StoreAndDispatch(IServerChannelSink sink, Object state);
        
        /// <internalonly/>
        
        // handles callback after message has been dispatched asynchronously
        [System.Security.SecurityCritical]  // auto-generated_required
        void ServerCallback(IAsyncResult ar);
        
    } // IServerChannelSinkStack

[System.Runtime.InteropServices.ComVisible(true)]
    public interface IServerResponseChannelSinkStack
    {
        /// <internalonly/> 
        // Call AsyncProcessResponse (on previous channel sink)
        [System.Security.SecurityCritical]  // auto-generated_required
        void AsyncProcessResponse(IMessage msg, ITransportHeaders headers, Stream stream);

        // Call GetResponseStream (on previous channel sink)
        [System.Security.SecurityCritical]  // auto-generated_required
        Stream GetResponseStream(IMessage msg, ITransportHeaders headers);
    } // interface IServerResponseChannelSinkStack


    [System.Security.SecurityCritical]  // auto-generated_required
    [System.Runtime.InteropServices.ComVisible(true)]
    public class ServerChannelSinkStack : IServerChannelSinkStack
    {
        private class SinkStack
        {
            public SinkStack PrevStack;
            
            public IServerChannelSink Sink;
            public Object State;
        }

        private SinkStack _stack = null;
        private SinkStack _rememberedStack = null;

        // async callback support
        private IMessage   _asyncMsg = null;
        private MethodInfo _asyncEnd = null;
        private Object     _serverObject = null;
        private IMethodCallMessage   _msg = null;


        [System.Security.SecurityCritical]
        public void Push(IServerChannelSink sink, Object state)
        {
            SinkStack newStack = new SinkStack();
            newStack.PrevStack = _stack;
            newStack.Sink = sink;
            newStack.State = state;
            _stack = newStack;
        } // Push


        [System.Security.SecurityCritical]
        public Object Pop(IServerChannelSink sink)
        {
            if (_stack == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Channel_PopOnEmptySinkStack"));
            }
            
            // find this sink on the stack
            do 
            {
                if (_stack.Sink == sink)
                    break;

                _stack = _stack.PrevStack;
            } while (_stack != null);

            if (_stack.Sink == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Channel_PopFromSinkStackWithoutPush"));
            }
            
            Object state = _stack.State;
            _stack = _stack.PrevStack;

            return state;
        } // Pop

        
        [System.Security.SecurityCritical]  // auto-generated
        public void Store(IServerChannelSink sink, Object state)
        {
            if (_stack == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Remoting_Channel_StoreOnEmptySinkStack"));
            }

            // find this sink on the stack
            do 
            {
                if (_stack.Sink == sink)
                    break;

                _stack = _stack.PrevStack;
            } while (_stack != null);

            if (_stack.Sink == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Channel_StoreOnSinkStackWithoutPush"));
            }                            

            SinkStack remStack = new SinkStack();
            remStack.PrevStack = _rememberedStack;
            remStack.Sink = sink;
            remStack.State = state;
            _rememberedStack = remStack;

            Pop(sink);
        } // Store

        [System.Security.SecurityCritical]  // auto-generated
        public void StoreAndDispatch(IServerChannelSink sink, Object state)
        {
            Store(sink, state);
            FlipRememberedStack();
            
            CrossContextChannel.DoAsyncDispatch(_asyncMsg, null);
        } // Store

        // Reverses remebered stack so that return message may be dispatched.
        private void FlipRememberedStack()
        { 
            if (_stack != null)
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Remoting_Channel_CantCallFRSWhenStackEmtpy"));
        
            while (_rememberedStack != null)
            {
                SinkStack newStack = new SinkStack();
                newStack.PrevStack = _stack;
                newStack.Sink = _rememberedStack.Sink;
                newStack.State = _rememberedStack.State;
                _stack = newStack;
                _rememberedStack = _rememberedStack.PrevStack;
            }
        } // FlipRememberedStack


        [System.Security.SecurityCritical]  // auto-generated
        public void AsyncProcessResponse(IMessage msg, ITransportHeaders headers, Stream stream)
        {
            if (_stack == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Remoting_Channel_CantCallAPRWhenStackEmpty"));
            }
               
            IServerChannelSink sink = _stack.Sink;
            Object state = _stack.State;
            _stack = _stack.PrevStack;
    
            sink.AsyncProcessResponse(this, state, msg, headers, stream);
        } // AsyncProcessResponse


        [System.Security.SecurityCritical]  // auto-generated
        public Stream GetResponseStream(IMessage msg, ITransportHeaders headers)
        {
            if (_stack == null)
            {
                throw new RemotingException(
                    Environment.GetResourceString(
                        "Remoting_Channel_CantCallGetResponseStreamWhenStackEmpty"));
            }
        
            // save state
            IServerChannelSink savedSink = _stack.Sink;
            Object savedState = _stack.State;
            
            _stack = _stack.PrevStack;
            Stream stream = savedSink.GetResponseStream(this, savedState, msg, headers);

            // restore state
            Push(savedSink, savedState);

            return stream;
        } // GetResponseStream

        // Store server that is going to be called back
        internal Object ServerObject { set { _serverObject = value; } }
        
        [System.Security.SecurityCritical]  // auto-generated
        public void ServerCallback(IAsyncResult ar)
        {
            if (_asyncEnd != null)
            {
                RemotingMethodCachedData asyncEndCache = (RemotingMethodCachedData)
                    InternalRemotingServices.GetReflectionCachedData(_asyncEnd);

                MethodInfo syncMI = (MethodInfo)_msg.MethodBase;
                RemotingMethodCachedData syncCache = (RemotingMethodCachedData)
                    InternalRemotingServices.GetReflectionCachedData(syncMI);
            
                ParameterInfo[] paramList = asyncEndCache.Parameters;

                // construct list to pass into End
                Object[] parameters = new Object[paramList.Length];
                parameters[paramList.Length - 1] = ar; // last parameter is the async result

                Object[] syncMsgArgs = _msg.Args;

                // copy out and ref parameters to the parameters list
                AsyncMessageHelper.GetOutArgs(syncCache.Parameters, syncMsgArgs, parameters);

                Object[] outArgs;
                
                StackBuilderSink s = new StackBuilderSink(_serverObject);
                Object returnValue =
                    s.PrivateProcessMessage(_asyncEnd.MethodHandle,
                        System.Runtime.Remoting.Messaging.Message.CoerceArgs(_asyncEnd, parameters, paramList),
                        _serverObject,
                        out outArgs);   

                // The outArgs list is associated with the EndXXX method. We need to make sure
                //   it is sized properly for the out args of the XXX method.
                if (outArgs != null)                        
                    outArgs = ArgMapper.ExpandAsyncEndArgsToSyncArgs(syncCache, outArgs);
                    
                s.CopyNonByrefOutArgsFromOriginalArgs(syncCache, syncMsgArgs, ref outArgs);                

                IMessage retMessage = new ReturnMessage(
                    returnValue, outArgs, _msg.ArgCount, Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext, _msg);

                AsyncProcessResponse(retMessage, null, null);
            }
        } // ServerCallback     
        
    } // ServerChannelSinkStack    

    // helper class for transforming [....] message parameter lists into its
    //   async counterparts
    internal static class AsyncMessageHelper
    {
        internal static void GetOutArgs(ParameterInfo[] syncParams, Object[] syncArgs, 
                                        Object[] endArgs)
        {
            int outCount = 0;

            for (int co = 0; co < syncParams.Length; co++)
            {
                if (syncParams[co].IsOut || syncParams[co].ParameterType.IsByRef)
                {
                    endArgs[outCount++] = syncArgs[co];
                }
            }
            
        } // GetOutArgs
    } // AsyncMessageHelper

} // namespace System.Runtime.Remoting.Channels

// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
//
// Remoting Infrastructure Sink for making calls across context
// boundaries. 
//
namespace System.Runtime.Remoting.Channels {

    using System;
    using System.Collections;
    using System.Threading;
    using System.Runtime.Remoting;
    using System.Runtime.Remoting.Contexts;
    using System.Runtime.Remoting.Messaging;    
    using System.Runtime.Serialization;
    
    /* package scope */
    // deliberately not [serializable]
    internal class CrossContextChannel : InternalSink, IMessageSink
    {
        private const String _channelName = "XCTX";
        private const int _channelCapability = 0; 
        private const String _channelURI = "XCTX_URI";
    
        [System.Security.SecuritySafeCritical]  // auto-generated
        static CrossContextChannel()
        {
        }

        private static CrossContextChannel messageSink
        { 
            get { return Thread.GetDomain().RemotingData.ChannelServicesData.xctxmessageSink; }
            set { Thread.GetDomain().RemotingData.ChannelServicesData.xctxmessageSink = value; }
        }

        private static Object staticSyncObject = new Object();
        private static InternalCrossContextDelegate s_xctxDel = new InternalCrossContextDelegate(SyncProcessMessageCallback);

        internal static IMessageSink MessageSink 
        {
            get 
            {   
                if (messageSink == null) 
                {                
                    CrossContextChannel tmpSink = new CrossContextChannel();
                    
                    lock (staticSyncObject)
                    {
                        if (messageSink == null)
                        {
                            messageSink = tmpSink;
                        }
                    }
                    //Interlocked.CompareExchange(out messageSink, tmpSink, null);
                }
                return messageSink;
            }
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static Object SyncProcessMessageCallback(Object[] args)
        {
            IMessage reqMsg = args[0] as IMessage;
            Context  srvCtx = args[1] as Context;
            IMessage replyMsg = null;
            
            // If profiling of remoting is active, must tell the profiler that we have received
            // a message.
            if (RemotingServices.CORProfilerTrackRemoting())
            {
                Guid g = Guid.Empty;

                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    Object obj = reqMsg.Properties["CORProfilerCookie"];

                    if (obj != null)
                    {
                        g = (Guid) obj;
                    }
                }

                RemotingServices.CORProfilerRemotingServerReceivingMessage(g, false);
            }

            Message.DebugOut("::::::::::::::::::::::::: CrossContext Channel: passing to ServerContextChain");
            
            // Server side notifications for dynamic sinks are done
            // in the x-context channel ... this is to maintain 
            // symmetry of the point of notification between 
            // the client and server context
            srvCtx.NotifyDynamicSinks(
                        reqMsg,
                        false,  // bCliSide
                        true,   // bStart
                        false,  // bAsync
                        true);  // bNotifyGlobals

            replyMsg = srvCtx.GetServerContextChain().SyncProcessMessage(reqMsg);
            srvCtx.NotifyDynamicSinks(
                        replyMsg,
                        false,  // bCliSide
                        false,  // bStart
                        false,  // bAsync
                        true);  // bNotifyGlobals

            Message.DebugOut("::::::::::::::::::::::::: CrossContext Channel: back from ServerContextChain");

            // If profiling of remoting is active, must tell the profiler that we are sending a
            // reply message.
            if (RemotingServices.CORProfilerTrackRemoting())
            {
                Guid g;

                RemotingServices.CORProfilerRemotingServerSendingReply(out g, false);

                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    replyMsg.Properties["CORProfilerCookie"] = g;
                }
            }       
            return replyMsg;
        }
            
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage     SyncProcessMessage(IMessage reqMsg) 
        {
            Object[] args = new Object[] { null, null };
            IMessage replyMsg = null;
            
            try
            {
                Message.DebugOut("\n::::::::::::::::::::::::: CrossContext Channel: [....] call starting");
                IMessage errMsg = ValidateMessage(reqMsg);
                if (errMsg != null)
                {
                    return errMsg;
                }

                ServerIdentity srvID = GetServerIdentity(reqMsg);
                Message.DebugOut("Got Server identity \n");
                BCLDebug.Assert(null != srvID,"null != srvID");
                
                
                BCLDebug.Assert(null != srvID.ServerContext, "null != srvID.ServerContext");

                args[0] = reqMsg;
                args[1] = srvID.ServerContext;
                replyMsg = (IMessage) Thread.CurrentThread.InternalCrossContextCallback(srvID.ServerContext, s_xctxDel, args);
            }
            catch(Exception e)
            {
                Message.DebugOut("Arrgh.. XCTXSink::throwing exception " + e + "\n");
                replyMsg = new ReturnMessage(e, (IMethodCallMessage)reqMsg);
                if (reqMsg!=null)
                {
                    ((ReturnMessage)replyMsg).SetLogicalCallContext(
                            (LogicalCallContext)
                            reqMsg.Properties[Message.CallContextKey]);
                }
            }
                
            Message.DebugOut("::::::::::::::::::::::::::: CrossContext Channel: [....] call returning!!\n");                         
            return replyMsg;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static Object AsyncProcessMessageCallback(Object[] args)
        {
            AsyncWorkItem   workItem = null;

            IMessage        reqMsg      = (IMessage) args[0];
            IMessageSink    replySink   = (IMessageSink) args[1];
            Context         oldCtx      = (Context) args[2];
            Context         srvCtx      = (Context) args[3];
            IMessageCtrl    msgCtrl     = null;
            
            // we use the work item just as our replySink in this case
            if (replySink != null)
            {
                workItem = new AsyncWorkItem(replySink, oldCtx); 
            }
            
            Message.DebugOut("::::::::::::::::::::::::: CrossContext Channel: passing to ServerContextChain");

            srvCtx.NotifyDynamicSinks(
                     reqMsg,
                     false,  // bCliSide
                     true,   // bStart
                     true,   // bAsync
                     true);  // bNotifyGlobals

            // call the server context chain
            msgCtrl = 
                srvCtx.GetServerContextChain().AsyncProcessMessage(
                    reqMsg, 
                    (IMessageSink)workItem);

            // Note: for async calls, we will do the return notification
            // for dynamic properties only when the async call 
            // completes (i.e. when the replySink gets called) 

            Message.DebugOut("::::::::::::::::::::::::: CrossContext Channel: back from ServerContextChain");

            return msgCtrl;
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(IMessage reqMsg, IMessageSink replySink) 
        {
            Message.DebugOut("::::::::::::::::::::::::::: CrossContext Channel: Async call starting!!\n");
            // One way Async notifications may potentially pass a null reply sink.
            IMessage errMsg = ValidateMessage(reqMsg);

            Object[] args = new Object[] { null, null, null, null };
            
            IMessageCtrl    msgCtrl  = null;
            if (errMsg != null)
            {
                if (replySink!=null)
                {
                    replySink.SyncProcessMessage(errMsg);
                }
            }
            else
            {
                ServerIdentity srvID = GetServerIdentity(reqMsg);
                
                // If active, notify the profiler that an asynchronous remoting message was received.
                if (RemotingServices.CORProfilerTrackRemotingAsync())
                {
                    Guid g = Guid.Empty;

                    if (RemotingServices.CORProfilerTrackRemotingCookie())
                    {
                        Object obj = reqMsg.Properties["CORProfilerCookie"];

                        if (obj != null)
                        {
                            g = (Guid) obj;
                        }
                    }

                    RemotingServices.CORProfilerRemotingServerReceivingMessage(g, true);

                    // Only wrap the replySink if the call wants a reply
                    if (replySink != null)
                    {
                        // Now wrap the reply sink in our own so that we can notify the profiler of
                        // when the reply is sent.  Upon invocation, it will notify the profiler
                        // then pass control on to the replySink passed in above.
                        IMessageSink profSink = new ServerAsyncReplyTerminatorSink(replySink);

                        // Replace the reply sink with our own
                        replySink = profSink;
                    }
                }

                Context         srvCtx   = srvID.ServerContext;
                if (srvCtx.IsThreadPoolAware)
                {
                    // this is the case when we do not queue the work item since the 
                    // server context claims to be doing its own threading.

                    args[0] = reqMsg;
                    args[1] = replySink;
                    args[2] = Thread.CurrentContext;
                    args[3] = srvCtx;

                    InternalCrossContextDelegate xctxDel = new InternalCrossContextDelegate(AsyncProcessMessageCallback);

                    msgCtrl = (IMessageCtrl) Thread.CurrentThread.InternalCrossContextCallback(srvCtx, xctxDel, args);
                }
                else
                {
                    AsyncWorkItem   workItem = null;
                    
                    // This is the case where we take care of returning the calling
                    // thread asap by using the ThreadPool for completing the call.
                    
                    // we use a more elaborate WorkItem and delegate the work to the thread pool
                    workItem = new AsyncWorkItem(reqMsg, 
                                                 replySink, 
                                                 Thread.CurrentContext, 
                                                 srvID);
    
                    WaitCallback threadFunc = new WaitCallback(workItem.FinishAsyncWork);
                    // Note: Dynamic sinks are notified in the threadFunc
                    ThreadPool.QueueUserWorkItem(threadFunc);
                }
            }
    
            Message.DebugOut("::::::::::::::::::::::::::: CrossContext Channel: Async call returning!!\n");
            return msgCtrl;
        } // AsyncProcessMessage

        [System.Security.SecurityCritical]  // auto-generated
        internal static Object DoAsyncDispatchCallback(Object[] args)
        {
            AsyncWorkItem   workItem = null;
            
            IMessage        reqMsg      = (IMessage) args[0];
            IMessageSink    replySink   = (IMessageSink) args[1];
            Context         oldCtx      = (Context) args[2];
            Context         srvCtx      = (Context) args[3];
            IMessageCtrl    msgCtrl     = null;

            
            // we use the work item just as our replySink in this case
            if (replySink != null)
            {
                 workItem = new AsyncWorkItem(replySink, oldCtx); 
            }
            Message.DebugOut("::::::::::::::::::::::::: CrossContext Channel: passing to ServerContextChain");
            // call the server context chain
            msgCtrl = 
                srvCtx.GetServerContextChain().AsyncProcessMessage(reqMsg, (IMessageSink)workItem);
            Message.DebugOut("::::::::::::::::::::::::: CrossContext Channel: back from ServerContextChain");

            return msgCtrl;
        }


        [System.Security.SecurityCritical]  // auto-generated
        internal static IMessageCtrl DoAsyncDispatch(IMessage reqMsg, IMessageSink replySink)
        {
            Object[] args = new Object[] { null, null, null, null };
                
            ServerIdentity srvID = GetServerIdentity(reqMsg);
            
            // If active, notify the profiler that an asynchronous remoting message was received.
            if (RemotingServices.CORProfilerTrackRemotingAsync())
            {
                Guid g = Guid.Empty;

                if (RemotingServices.CORProfilerTrackRemotingCookie())
                {
                    Object obj = reqMsg.Properties["CORProfilerCookie"];
                    if (obj != null)
                        g = (Guid) obj;
                }

                RemotingServices.CORProfilerRemotingServerReceivingMessage(g, true);

                // Only wrap the replySink if the call wants a reply
                if (replySink != null)
                {
                    // Now wrap the reply sink in our own so that we can notify the profiler of
                    // when the reply is sent.  Upon invocation, it will notify the profiler
                    // then pass control on to the replySink passed in above.
                    IMessageSink profSink = 
                        new ServerAsyncReplyTerminatorSink(replySink);

                    // Replace the reply sink with our own
                    replySink = profSink;
                }
            }

            IMessageCtrl msgCtrl = null;
            Context srvCtx = srvID.ServerContext;
            
            //if (srvCtx.IsThreadPoolAware)
            //{
                // this is the case when we do not queue the work item since the 
                // server context claims to be doing its own threading.

                args[0] = reqMsg;
                args[1] = replySink;
                args[2] = Thread.CurrentContext;
                args[3] = srvCtx;

                InternalCrossContextDelegate xctxDel = new InternalCrossContextDelegate(DoAsyncDispatchCallback);
                
                msgCtrl = (IMessageCtrl) Thread.CurrentThread.InternalCrossContextCallback(srvCtx, xctxDel, args);

            //}

            return msgCtrl;
        } // DoDispatch
    
        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                // We are a terminating sink for this chain.
                return null;
            }
        }
  }
    
    /* package */
    internal class AsyncWorkItem : IMessageSink
    {
        // the replySink passed in to us in AsyncProcessMsg
        private IMessageSink _replySink;
        
        // the server identity we are calling
        private ServerIdentity _srvID;
        
        // the original context of the thread calling AsyncProcessMsg
        private Context _oldCtx;

        [System.Security.SecurityCritical] // auto-generated
        private LogicalCallContext _callCtx;
        
        // the request msg passed in
        private IMessage _reqMsg;    
        
        [System.Security.SecurityCritical]  // auto-generated
        internal AsyncWorkItem(IMessageSink replySink, Context oldCtx)
           
            : this(null, replySink, oldCtx, null) {
        }
    
        [System.Security.SecurityCritical]  // auto-generated
        internal AsyncWorkItem(IMessage reqMsg, IMessageSink replySink, Context oldCtx, ServerIdentity srvID)
        {
            _reqMsg = reqMsg;
            _replySink = replySink;
            _oldCtx = oldCtx;
            _callCtx = Thread.CurrentThread.GetMutableExecutionContext().LogicalCallContext;
            _srvID = srvID;
        }

        [System.Security.SecurityCritical]  // auto-generated
        internal static Object SyncProcessMessageCallback(Object[] args)
        {
            IMessageSink    replySink   = (IMessageSink) args[0];
            IMessage        msg         = (IMessage) args[1];

            return replySink.SyncProcessMessage(msg);
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage     SyncProcessMessage(IMessage msg)
        {
            // This gets called when the called object finishes the AsyncWork...

            // This is called irrespective of whether we delegated the initial
            // work to a thread pool thread or not. Quite likely it will be 
            // called on a user thread (i.e. a thread different from the 
            // forward call thread)
        
            // we just switch back to the old context before calling 
            // the next replySink

            IMessage retMsg = null;
            
            if (_replySink != null)
            {
                // This assert covers the common case (ThreadPool)
                // and checks that the reply thread for the async call 
                // indeed emerges from the server context.
                BCLDebug.Assert(
                    (_srvID == null)
                    || (_srvID.ServerContext == Thread.CurrentContext),
                    "Thread expected to be in the server context!");

                // Call the dynamic sinks to notify that the async call
                // has completed
                Thread.CurrentContext.NotifyDynamicSinks(
                    msg,    // this is the async reply
                    false,  // bCliSide
                    false,  // bStart
                    true,   // bAsync
                    true);  // bNotifyGlobals

                Object[] args = new Object[] { _replySink, msg };

                InternalCrossContextDelegate xctxDel = new InternalCrossContextDelegate(SyncProcessMessageCallback);

                retMsg = (IMessage) Thread.CurrentThread.InternalCrossContextCallback(_oldCtx, xctxDel, args);
            }
            return retMsg;
        }
        
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {
            // Can't call the reply sink asynchronously!
            throw new NotSupportedException(
                Environment.GetResourceString("NotSupported_Method"));
        }
    
        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get
            {
                return _replySink;
            }
        }    

        [System.Security.SecurityCritical]  // auto-generated
        internal static Object FinishAsyncWorkCallback(Object[] args)
        {
            AsyncWorkItem This = (AsyncWorkItem) args[0];
            Context srvCtx = This._srvID.ServerContext;
            
            LogicalCallContext threadPoolCallCtx = 
                CallContext.SetLogicalCallContext(This._callCtx);

            // Call the server context chain Async. We provide workItem as our
            // replySink ... this will cause the replySink.ProcessMessage 
            // to switch back to the context of the original caller thread.
    
            // Call the dynamic sinks to notify that the async call
            // is starting
            srvCtx.NotifyDynamicSinks(
                This._reqMsg,
                false,  // bCliSide
                true,   // bStart
                true,   // bAsync
                true);  // bNotifyGlobals

            // <

            IMessageCtrl ctrl = 
               srvCtx.GetServerContextChain().AsyncProcessMessage(
                    This._reqMsg, 
                    (IMessageSink)This);

            // change back to the old context        
            CallContext.SetLogicalCallContext(threadPoolCallCtx);
            
            return null;
        }
    
        /* package */
        [System.Security.SecurityCritical]  // auto-generated
        internal virtual void FinishAsyncWork(Object stateIgnored)
        {
            InternalCrossContextDelegate xctxDel = new InternalCrossContextDelegate(FinishAsyncWorkCallback);

            Object[] args = new Object[] { this };
            
            Thread.CurrentThread.InternalCrossContextCallback(_srvID.ServerContext, xctxDel, args);
        }    
    }
}

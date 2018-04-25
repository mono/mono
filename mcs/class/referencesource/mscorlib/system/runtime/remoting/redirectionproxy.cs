// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// File: RedirectionProxy.cs

using System;
using System.Runtime.InteropServices;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;


namespace System.Runtime.Remoting
{

    internal class RedirectionProxy : MarshalByRefObject, IMessageSink
    {
        private MarshalByRefObject _proxy;
        [System.Security.SecurityCritical] // auto-generated
        private RealProxy _realProxy;
        private Type _serverType;
        private WellKnownObjectMode _objectMode;

        [System.Security.SecurityCritical]  // auto-generated
        internal RedirectionProxy(MarshalByRefObject proxy, Type serverType)
        {
            _proxy = proxy;
            _realProxy = RemotingServices.GetRealProxy(_proxy);
            _serverType = serverType;
            _objectMode = WellKnownObjectMode.Singleton;
        } // RedirectionProxy


        public WellKnownObjectMode ObjectMode 
        {
            set { _objectMode = value; }
        } // ObjectMode        
    
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage SyncProcessMessage(IMessage msg)
        {        
            IMessage replyMsg = null;
            
            try
            {
                msg.Properties["__Uri"] = _realProxy.IdentityObject.URI;     

                if (_objectMode == WellKnownObjectMode.Singleton)
                {
                    replyMsg = _realProxy.Invoke(msg);
                }
                else
                {
                    // This is a single call object, so we need to create
                    // a new instance.
                    MarshalByRefObject obj = (MarshalByRefObject)Activator.CreateInstance(_serverType, true);
                    BCLDebug.Assert(RemotingServices.IsTransparentProxy(obj), "expecting a proxy");
                  
                    RealProxy rp = RemotingServices.GetRealProxy(obj);
                    replyMsg = rp.Invoke(msg);
                }                
            }
            catch (Exception e)
            {
                replyMsg = new ReturnMessage(e, msg as IMethodCallMessage);
            }

            return replyMsg;
        } // SyncProcessMessage
        
        
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {        
            // <



            IMessage replyMsg = null;

            replyMsg = SyncProcessMessage(msg);

            if (replySink != null)
                replySink.SyncProcessMessage(replyMsg);
            
            return null;
        } // AsyncProcessMessage
        
        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return null; }
        }
    
    } // class RedirectionProxy



    // This is only to be used for wellknown Singleton COM objects.
    internal class ComRedirectionProxy : MarshalByRefObject, IMessageSink
    {
        private MarshalByRefObject _comObject;
        private Type _serverType;

        internal ComRedirectionProxy(MarshalByRefObject comObject, Type serverType)
        {
            BCLDebug.Assert(serverType.IsCOMObject, "This must be a COM object type.");

            _comObject = comObject;
            _serverType = serverType;
        } // ComRedirectionProxy

    
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessage SyncProcessMessage(IMessage msg)
        {    
            IMethodCallMessage mcmReqMsg = (IMethodCallMessage)msg;       
            IMethodReturnMessage replyMsg = null;

            replyMsg = RemotingServices.ExecuteMessage(_comObject, mcmReqMsg);
            
            if (replyMsg != null)
            {
                // If an "RPC server not available" (HRESULT=0x800706BA) COM
                //   exception is thrown, we will try to recreate the object once.
                const int RPC_S_SERVER_UNAVAILABLE = unchecked((int)0x800706BA);
                const int RPC_S_CALL_FAILED_DNE    = unchecked((int)0x800706BF);
                COMException comException = replyMsg.Exception as COMException;
                if ((comException != null) && 
                    ((comException._HResult == RPC_S_SERVER_UNAVAILABLE) || 
                     (comException._HResult == RPC_S_CALL_FAILED_DNE)))
                {                
                    _comObject = (MarshalByRefObject)Activator.CreateInstance(_serverType, true);

                    replyMsg = RemotingServices.ExecuteMessage(_comObject, mcmReqMsg);
                }                    
            }
            
            return replyMsg;
        } // SyncProcessMessage
        
        
        [System.Security.SecurityCritical]  // auto-generated
        public virtual IMessageCtrl AsyncProcessMessage(IMessage msg, IMessageSink replySink)
        {        
            // <



            IMessage replyMsg = null;

            replyMsg = SyncProcessMessage(msg);

            if (replySink != null)
                replySink.SyncProcessMessage(replyMsg);
            
            return null;
        } // AsyncProcessMessage
        
        public IMessageSink NextSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return null; }
        }
    
    } // class ComRedirectionProxy
    

} // namespace System.Runtime.Remoting


// ==++==
// 
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// 
// ==--==
// File: DispatchChannelSink.cs

using System;
using System.Collections;
using System.IO;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Diagnostics.Contracts;


namespace System.Runtime.Remoting.Channels
{
    internal class DispatchChannelSinkProvider : IServerChannelSinkProvider
    {    
        internal DispatchChannelSinkProvider()
        {
        } // DispatchChannelSinkProvider

        [System.Security.SecurityCritical]  // auto-generated
        public void GetChannelData(IChannelDataStore channelData)
        {
        }

        [System.Security.SecurityCritical]  // auto-generated
        public IServerChannelSink CreateSink(IChannelReceiver channel)
        {
            return new DispatchChannelSink();
        }

        public IServerChannelSinkProvider Next
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return null; }
            [System.Security.SecurityCritical]  // auto-generated
            set { throw new NotSupportedException(); }
        }
    } // class DispatchChannelSinkProvider


    internal class DispatchChannelSink : IServerChannelSink
    {
       
        internal DispatchChannelSink()
        {
        } // DispatchChannelSink
        
   
        [System.Security.SecurityCritical]  // auto-generated
        public ServerProcessing ProcessMessage(IServerChannelSinkStack sinkStack,
            IMessage requestMsg,
            ITransportHeaders requestHeaders, Stream requestStream,
            out IMessage responseMsg, out ITransportHeaders responseHeaders,
            out Stream responseStream)
        {
            if (requestMsg == null)
            {
                throw new ArgumentNullException(
                    "requestMsg", 
                    Environment.GetResourceString("Remoting_Channel_DispatchSinkMessageMissing"));
            }
            Contract.EndContractBlock();

            // check arguments
            if (requestStream != null)
            {
                throw new RemotingException(
                    Environment.GetResourceString("Remoting_Channel_DispatchSinkWantsNullRequestStream"));
            }

            responseHeaders = null;
            responseStream = null;
            return ChannelServices.DispatchMessage(sinkStack, requestMsg, out responseMsg);                
        } // ProcessMessage
           

        [System.Security.SecurityCritical]  // auto-generated
        public void AsyncProcessResponse(IServerResponseChannelSinkStack sinkStack, Object state,
                                         IMessage msg, ITransportHeaders headers, Stream stream)                 
        {
            // We never push ourselves to the sink stack, so this won't be called.
            throw new NotSupportedException();            
        } // AsyncProcessResponse


        [System.Security.SecurityCritical]  // auto-generated
        public Stream GetResponseStream(IServerResponseChannelSinkStack sinkStack, Object state,
                                        IMessage msg, ITransportHeaders headers)
        {
            // We never push ourselves to the sink stack, so this won't be called.
            throw new NotSupportedException(); 
        } // GetResponseStream


        public IServerChannelSink NextChannelSink
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return null; }
        }


        public IDictionary Properties
        {
            [System.Security.SecurityCritical]  // auto-generated
            get { return null; }
        } 
         
        
    } // class DispatchChannelSink


} // namespace System.Runtime.Remoting.Channels

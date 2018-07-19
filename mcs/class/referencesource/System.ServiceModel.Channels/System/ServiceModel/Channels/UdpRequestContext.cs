// <copyright>
// Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace System.ServiceModel.Channels
{
    using System;
    using System.Diagnostics;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    internal class UdpRequestContext : RequestContextBase
    {
        private NetworkInterfaceMessageProperty networkInterfaceMessageProperty;
        private UdpOutputChannel outputChannel;
        private Uri via;
        
        public UdpRequestContext(UdpOutputChannel outputChannel, Message requestMessage)
            : base(requestMessage, outputChannel.InternalCloseTimeout, outputChannel.InternalSendTimeout)
        {
            Fx.Assert(outputChannel != null, "replyChannel can't be null");
            this.outputChannel = outputChannel;
            
            if (!NetworkInterfaceMessageProperty.TryGet(requestMessage, out this.networkInterfaceMessageProperty))
            {
                Fx.Assert("requestMessage must always contain NetworkInterfaceMessageProperty");
            }

            RemoteEndpointMessageProperty remoteEndpointMessageProperty;
            if (!requestMessage.Properties.TryGetValue(RemoteEndpointMessageProperty.Name, out remoteEndpointMessageProperty))
            {
                Fx.Assert("requestMessage must always contain RemoteEndpointMessageProperty");
            }

            UriBuilder uriBuilder = new UriBuilder(UdpConstants.Scheme, remoteEndpointMessageProperty.Address, remoteEndpointMessageProperty.Port);
            this.via = uriBuilder.Uri;
        }
        
        protected override void OnAbort()
        {
        }

        protected override IAsyncResult OnBeginReply(Message message, TimeSpan timeout, AsyncCallback callback, object state)
        {
            if (message != null)
            {
                this.SetAddressingInformation(message);
                return this.outputChannel.BeginSend(message, timeout, callback, state);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }

        protected override void OnEndReply(IAsyncResult result)
        {
            this.outputChannel.EndSend(result);
        }

        protected override void OnReply(Message message, TimeSpan timeout)
        {
            if (message != null)
            {
                this.SetAddressingInformation(message);
                this.outputChannel.Send(message, timeout);                
            }
        }

        private void SetAddressingInformation(Message message)
        {
            this.networkInterfaceMessageProperty.AddTo(message);
            message.Properties.Via = this.via;
        }
    }
}

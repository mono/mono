//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery.Version11
{
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.Threading;

    class AnnouncementInnerClient11 : ClientBase<IAnnouncementContract11>, IAnnouncementInnerClient
    {
        DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator;

        BeginOperationDelegate onBeginHelloOperationDelegate;
        EndOperationDelegate onEndHelloOperationDelegate;
        SendOrPostCallback onHelloOperationCompletedDelegate;

        BeginOperationDelegate onBeginByeOperationDelegate;
        EndOperationDelegate onEndByeOperationDelegate;
        SendOrPostCallback onByeOperationCompletedDelegate;

        public AnnouncementInnerClient11(AnnouncementEndpoint announcementEndpoint)
            : base(announcementEndpoint)
        {
            this.discoveryMessageSequenceGenerator = new DiscoveryMessageSequenceGenerator();
        }

        event EventHandler<AsyncCompletedEventArgs> HelloOperationCompletedEventHandler;
        event EventHandler<AsyncCompletedEventArgs> ByeOperationCompletedEventHandler;

        event EventHandler<AsyncCompletedEventArgs> IAnnouncementInnerClient.HelloOperationCompleted
        {
            add
            {
                this.HelloOperationCompletedEventHandler += value;
            }
            remove
            {
                this.HelloOperationCompletedEventHandler -= value;
            }
        }

        event EventHandler<AsyncCompletedEventArgs> IAnnouncementInnerClient.ByeOperationCompleted
        {
            add
            {
                this.ByeOperationCompletedEventHandler += value;
            }
            remove
            {
                this.ByeOperationCompletedEventHandler -= value;
            }
        }

        public DiscoveryMessageSequenceGenerator DiscoveryMessageSequenceGenerator
        {
            get
            {
                return this.discoveryMessageSequenceGenerator;
            }
            set
            {
                this.discoveryMessageSequenceGenerator = value;
            }
        }

        public new ChannelFactory ChannelFactory
        {
            get
            {
                return base.ChannelFactory;
            }
        }

        public new IClientChannel InnerChannel
        {
            get
            {
                return base.InnerChannel;
            }
        }

        public new ServiceEndpoint Endpoint
        {
            get
            {
                return base.Endpoint;
            }
        }

        public ICommunicationObject InnerCommunicationObject
        {
            get
            {
                return this as ICommunicationObject;
            }

        }

        public void HelloOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            HelloMessage11 message = HelloMessage11.Create(DiscoveryMessageSequenceGenerator.Next(), endpointDiscoveryMetadata);
            base.Channel.HelloOperation(message);
        }

        public void ByeOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata)
        {
            ByeMessage11 message = ByeMessage11.Create(DiscoveryMessageSequenceGenerator.Next(), endpointDiscoveryMetadata);
            base.Channel.ByeOperation(message);
        }

        public IAsyncResult BeginHelloOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state)
        {
            HelloMessage11 message = HelloMessage11.Create(DiscoveryMessageSequenceGenerator.Next(), endpointDiscoveryMetadata);
            return base.Channel.BeginHelloOperation(message, callback, state);
        }

        public void EndHelloOperation(IAsyncResult result)
        {
            base.Channel.EndHelloOperation(result);
        }

        public IAsyncResult BeginByeOperation(EndpointDiscoveryMetadata endpointDiscoveryMetadata, AsyncCallback callback, object state)
        {
            ByeMessage11 message = ByeMessage11.Create(DiscoveryMessageSequenceGenerator.Next(), endpointDiscoveryMetadata);
            return base.Channel.BeginByeOperation(message, callback, state);
        }

        public void EndByeOperation(IAsyncResult result)
        {
            base.Channel.EndByeOperation(result);
        }

        public void HelloOperationAsync(EndpointDiscoveryMetadata endpointDiscoveryMetadata, object userState)
        {
            HelloMessage11 message = HelloMessage11.Create(DiscoveryMessageSequenceGenerator.Next(), endpointDiscoveryMetadata);

            if ((this.onBeginHelloOperationDelegate == null))
            {
                this.onBeginHelloOperationDelegate = new BeginOperationDelegate(this.OnBeginHelloOperation);
            }
            if ((this.onEndHelloOperationDelegate == null))
            {
                this.onEndHelloOperationDelegate = new EndOperationDelegate(this.OnEndHelloOperation);
            }
            if ((this.onHelloOperationCompletedDelegate == null))
            {
                this.onHelloOperationCompletedDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnHelloOperationCompleted));
            }
            base.InvokeAsync(
                this.onBeginHelloOperationDelegate,
                new object[] { message },
                this.onEndHelloOperationDelegate,
                this.onHelloOperationCompletedDelegate,
                userState);
        }

        public void ByeOperationAsync(EndpointDiscoveryMetadata endpointDiscoveryMetadata, object userState)
        {
            ByeMessage11 message = ByeMessage11.Create(DiscoveryMessageSequenceGenerator.Next(), endpointDiscoveryMetadata);

            if (this.onBeginByeOperationDelegate == null)
            {
                this.onBeginByeOperationDelegate = new BeginOperationDelegate(this.OnBeginByeOperation);
            }
            if ((this.onEndByeOperationDelegate == null))
            {
                this.onEndByeOperationDelegate = new EndOperationDelegate(this.OnEndByeOperation);
            }
            if ((this.onByeOperationCompletedDelegate == null))
            {
                this.onByeOperationCompletedDelegate = Fx.ThunkCallback(new SendOrPostCallback(this.OnByeOperationCompleted));
            }
            base.InvokeAsync(
                this.onBeginByeOperationDelegate,
                new object[] { message },
                this.onEndByeOperationDelegate,
                this.onByeOperationCompletedDelegate,
                userState);
        }

        IAsyncResult BeginHelloOperation(HelloMessage11 message, AsyncCallback callback, object state)
        {
            return base.Channel.BeginHelloOperation(message, callback, state);
        }

        IAsyncResult BeginByeOperation(ByeMessage11 message, AsyncCallback callback, object state)
        {
            return base.Channel.BeginByeOperation(message, callback, state);
        }


        IAsyncResult OnBeginHelloOperation(object[] inValues, System.AsyncCallback callback, object asyncState)
        {
            HelloMessage11 message = ((HelloMessage11)(inValues[0]));
            return this.BeginHelloOperation(message, callback, asyncState);
        }

        object[] OnEndHelloOperation(System.IAsyncResult result)
        {
            this.EndHelloOperation(result);
            return null;
        }

        void OnHelloOperationCompleted(object state)
        {
            if ((this.HelloOperationCompletedEventHandler != null))
            {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.HelloOperationCompletedEventHandler(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }

        IAsyncResult OnBeginByeOperation(object[] inValues, System.AsyncCallback callback, object asyncState)
        {
            ByeMessage11 message = ((ByeMessage11)(inValues[0]));
            return this.BeginByeOperation(message, callback, asyncState);
        }

        object[] OnEndByeOperation(System.IAsyncResult result)
        {
            this.EndByeOperation(result);
            return null;
        }

        void OnByeOperationCompleted(object state)
        {
            if (this.ByeOperationCompletedEventHandler != null)
            {
                InvokeAsyncCompletedEventArgs e = ((InvokeAsyncCompletedEventArgs)(state));
                this.ByeOperationCompletedEventHandler(this, new System.ComponentModel.AsyncCompletedEventArgs(e.Error, e.Cancelled, e.UserState));
            }
        }
    }
}

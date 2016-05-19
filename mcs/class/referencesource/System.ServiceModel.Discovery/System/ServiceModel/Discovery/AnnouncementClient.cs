//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Discovery
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Description;
    using System.ServiceModel.Discovery.Configuration;
    using System.Threading.Tasks;
    using System.Xml;
    using SR2 = System.ServiceModel.Discovery.SR;

    [Fx.Tag.XamlVisible(false)]
    public sealed class AnnouncementClient : ICommunicationObject, IDisposable
    {
        IAnnouncementInnerClient innerClient;

        public AnnouncementClient()
            : this("*")
        {
        }

        public AnnouncementClient(string endpointConfigurationName)
        {
            if (endpointConfigurationName == null)
            {
                throw FxTrace.Exception.ArgumentNull("endpointConfigurationName");
            }

            AnnouncementEndpoint announcementEndpoint =
                ConfigurationUtility.LookupEndpointFromClientSection<AnnouncementEndpoint>(
                endpointConfigurationName);

            Initialize(announcementEndpoint);
        }

        public AnnouncementClient(AnnouncementEndpoint announcementEndpoint)
        {
            if (announcementEndpoint == null)
            {
                throw FxTrace.Exception.ArgumentNull("announcementEndpoint");
            }

            Initialize(announcementEndpoint);
        }

        public event EventHandler<AsyncCompletedEventArgs> AnnounceOnlineCompleted
        {
            add
            {
                if (this.InternalAnnounceOnlineCompleted == null)
                {
                    this.innerClient.HelloOperationCompleted += OnInnerClientHelloCompleted;
                }
                this.InternalAnnounceOnlineCompleted += value;
            }

            remove
            {
                this.InternalAnnounceOnlineCompleted -= value;
                if (this.InternalAnnounceOnlineCompleted == null)
                {
                    this.innerClient.HelloOperationCompleted -= OnInnerClientHelloCompleted;
                }
            }
        }

        public event EventHandler<AsyncCompletedEventArgs> AnnounceOfflineCompleted
        {
            add
            {
                if (this.InternalAnnounceOfflineCompleted == null)
                {
                    this.innerClient.ByeOperationCompleted += OnInnerClientByeCompleted;
                }
                this.InternalAnnounceOfflineCompleted += value;
            }

            remove
            {
                this.InternalAnnounceOfflineCompleted -= value;
                if (this.InternalAnnounceOfflineCompleted == null)
                {
                    this.innerClient.ByeOperationCompleted -= OnInnerClientByeCompleted;
                }
            }
        }

        event EventHandler<AsyncCompletedEventArgs> InternalAnnounceOnlineCompleted;
        event EventHandler<AsyncCompletedEventArgs> InternalAnnounceOfflineCompleted;

        event EventHandler ICommunicationObject.Closed
        {
            add
            {
                if (this.InternalClosed == null)
                {
                    this.InnerCommunicationObject.Closed += OnInnerCommunicationObjectClosed;
                }
                this.InternalClosed += value;
            }

            remove
            {
                this.InternalClosed -= value;
                if (this.InternalClosed == null)
                {
                    this.InnerCommunicationObject.Closed -= OnInnerCommunicationObjectClosed;
                }
            }
        }

        event EventHandler ICommunicationObject.Closing
        {
            add
            {
                if (this.InternalClosing == null)
                {
                    this.InnerCommunicationObject.Closing += OnInnerCommunicationObjectClosing;
                }
                this.InternalClosing += value;
            }

            remove
            {
                this.InternalClosing -= value;
                if (this.InternalClosing == null)
                {
                    this.InnerCommunicationObject.Closing -= OnInnerCommunicationObjectClosing;
                }
            }
        }

        event EventHandler ICommunicationObject.Faulted
        {
            add
            {
                if (this.InternalFaulted == null)
                {
                    this.InnerCommunicationObject.Faulted += OnInnerCommunicationObjectFaulted;
                }
                this.InternalFaulted += value;
            }

            remove
            {
                this.InternalFaulted -= value;
                if (this.InternalFaulted == null)
                {
                    this.InnerCommunicationObject.Faulted -= OnInnerCommunicationObjectFaulted;
                }
            }
        }

        event EventHandler ICommunicationObject.Opened
        {
            add
            {
                if (this.InternalOpened == null)
                {
                    this.InnerCommunicationObject.Opened += OnInnerCommunicationObjectOpened;
                }
                this.InternalOpened += value;
            }

            remove
            {
                this.InternalOpened -= value;
                if (this.InternalOpened == null)
                {
                    this.InnerCommunicationObject.Opened -= OnInnerCommunicationObjectOpened;
                }
            }
        }

        event EventHandler ICommunicationObject.Opening
        {
            add
            {
                if (this.InternalOpening == null)
                {
                    this.InnerCommunicationObject.Opening += OnInnerCommunicationObjectOpening;
                }
                this.InternalOpening += value;
            }

            remove
            {
                this.InternalOpening -= value;
                if (this.InternalOpening == null)
                {
                    this.InnerCommunicationObject.Opening -= OnInnerCommunicationObjectOpening;
                }
            }
        }

        event EventHandler InternalClosed;
        event EventHandler InternalClosing;
        event EventHandler InternalFaulted;
        event EventHandler InternalOpened;
        event EventHandler InternalOpening;

        public DiscoveryMessageSequenceGenerator MessageSequenceGenerator
        {
            get
            {
                return this.innerClient.DiscoveryMessageSequenceGenerator;
            }

            set
            {
                if (value == null)
                {
                    throw FxTrace.Exception.ArgumentNull("value");
                }
                if (((ICommunicationObject)this).State != CommunicationState.Created)
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR2.DiscoverySetMessageSequenceInvalidState));
                }
                this.innerClient.DiscoveryMessageSequenceGenerator = value;
            }
        }

        public ChannelFactory ChannelFactory
        {
            get
            {
                return InnerClient.ChannelFactory;
            }
        }

        public ClientCredentials ClientCredentials
        {
            get
            {
                return InnerClient.ClientCredentials;
            }
        }

        public ServiceEndpoint Endpoint
        {
            get
            {
                return InnerClient.Endpoint;
            }
        }

        public IClientChannel InnerChannel
        {
            get
            {
                return InnerClient.InnerChannel;
            }
        }

        CommunicationState ICommunicationObject.State
        {
            get
            {
                return InnerCommunicationObject.State;
            }
        }

        IAnnouncementInnerClient InnerClient
        {
            get
            {
                return this.innerClient;
            }
        }

        ICommunicationObject InnerCommunicationObject
        {
            get
            {
                return InnerClient.InnerCommunicationObject;
            }
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.InheritThrows(From = "Open", FromDeclaringType = typeof(ICommunicationObject))]
        public void Open()
        {
            ((ICommunicationObject)this).Open();
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public void AnnounceOnlineAsync(EndpointDiscoveryMetadata discoveryMetadata)
        {
            AnnounceOnlineAsync(discoveryMetadata, null);
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public void AnnounceOnlineAsync(EndpointDiscoveryMetadata discoveryMetadata, object userState)
        {
            if (discoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMetadata");
            }

            using (new AnnouncementOperationContextScope(InnerChannel))
            {
                InnerClient.HelloOperationAsync(discoveryMetadata, userState);
            }
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public void AnnounceOfflineAsync(EndpointDiscoveryMetadata discoveryMetadata)
        {
            AnnounceOfflineAsync(discoveryMetadata, null);
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public void AnnounceOfflineAsync(EndpointDiscoveryMetadata discoveryMetadata, object userState)
        {
            if (discoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMetadata");
            }

            using (new AnnouncementOperationContextScope(InnerChannel))
            {
                InnerClient.ByeOperationAsync(discoveryMetadata, userState);
            }
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public void AnnounceOnline(EndpointDiscoveryMetadata discoveryMetadata)
        {
            if (discoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMetadata");
            }

            using (new AnnouncementOperationContextScope(InnerChannel))
            {
                this.InnerClient.HelloOperation(discoveryMetadata);
            }
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        [Fx.Tag.Blocking(CancelMethod = "Abort")]
        public void AnnounceOffline(EndpointDiscoveryMetadata discoveryMetadata)
        {
            if (discoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMetadata");
            }

            using (new AnnouncementOperationContextScope(InnerChannel))
            {
                this.InnerClient.ByeOperation(discoveryMetadata);
            }
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public IAsyncResult BeginAnnounceOnline(EndpointDiscoveryMetadata discoveryMetadata, AsyncCallback callback, object state)
        {
            if (discoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMetadata");
            }

            using (new AnnouncementOperationContextScope(InnerChannel))
            {
                return InnerClient.BeginHelloOperation(discoveryMetadata, callback, state);
            }
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public void EndAnnounceOnline(IAsyncResult result)
        {
            InnerClient.EndHelloOperation(result);
        }

        [Fx.Tag.Throws(typeof(AggregateException), "Inherits from Task exception contract")]
        public Task AnnounceOnlineTaskAsync(EndpointDiscoveryMetadata discoveryMetadata)
        {
            return Task.Factory.FromAsync<EndpointDiscoveryMetadata>(this.BeginAnnounceOnline, this.EndAnnounceOnline, discoveryMetadata, /* state */ null);
        }

        [Fx.Tag.Throws(typeof(AggregateException), "Inherits from Task exception contract")]
        public Task AnnounceOfflineTaskAsync(EndpointDiscoveryMetadata discoveryMetadata)
        {
            return Task.Factory.FromAsync<EndpointDiscoveryMetadata>(this.BeginAnnounceOffline, this.EndAnnounceOffline, discoveryMetadata, /* state */ null);
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public IAsyncResult BeginAnnounceOffline(EndpointDiscoveryMetadata discoveryMetadata, AsyncCallback callback, object state)
        {
            if (discoveryMetadata == null)
            {
                throw FxTrace.Exception.ArgumentNull("discoveryMetadata");
            }

            using (new AnnouncementOperationContextScope(InnerChannel))
            {
                return InnerClient.BeginByeOperation(discoveryMetadata, callback, state);
            }
        }

        [Fx.Tag.Throws(typeof(CommunicationException), "Inherits from Channel exception contract")]
        [Fx.Tag.Throws(typeof(TimeoutException), "Inherits from Channel exception contract")]
        public void EndAnnounceOffline(IAsyncResult result)
        {
            InnerClient.EndByeOperation(result);
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.InheritThrows(From = "Close", FromDeclaringType = typeof(ICommunicationObject))]
        public void Close()
        {
            ((ICommunicationObject)this).Close();
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.InheritThrows(From = "Open", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Open()
        {
            InnerCommunicationObject.Open();
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.InheritThrows(From = "Open", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Open(TimeSpan timeout)
        {
            InnerCommunicationObject.Open(timeout);
        }

        [Fx.Tag.InheritThrows(From = "BeginOpen", FromDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginOpen(AsyncCallback callback, object state)
        {
            return InnerCommunicationObject.BeginOpen(callback, state);
        }

        [Fx.Tag.InheritThrows(From = "BeginOpen", FromDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerCommunicationObject.BeginOpen(timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "EndOpen", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.EndOpen(IAsyncResult result)
        {
            InnerCommunicationObject.EndOpen(result);
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.InheritThrows(From = "Close", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Close()
        {
            InnerCommunicationObject.Close();
        }

        [Fx.Tag.Blocking(CancelMethod = "Abort", CancelDeclaringType = typeof(ICommunicationObject))]
        [Fx.Tag.InheritThrows(From = "Close", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Close(TimeSpan timeout)
        {
            InnerCommunicationObject.Close(timeout);
        }

        [Fx.Tag.InheritThrows(From = "BeginClose", FromDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginClose(AsyncCallback callback, object state)
        {
            return InnerCommunicationObject.BeginClose(callback, state);
        }

        [Fx.Tag.InheritThrows(From = "BeginClose", FromDeclaringType = typeof(ICommunicationObject))]
        IAsyncResult ICommunicationObject.BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return InnerCommunicationObject.BeginClose(timeout, callback, state);
        }

        [Fx.Tag.InheritThrows(From = "EndClose", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.EndClose(IAsyncResult result)
        {
            InnerCommunicationObject.EndClose(result);
        }

        [Fx.Tag.InheritThrows(From = "Abort", FromDeclaringType = typeof(ICommunicationObject))]
        void ICommunicationObject.Abort()
        {
            InnerCommunicationObject.Abort();
        }

        void IDisposable.Dispose()
        {
            this.Close();
        }

        void Initialize(AnnouncementEndpoint announcementEndpoint)
        {
            if (announcementEndpoint.Binding != null && announcementEndpoint.Binding.MessageVersion.Addressing == AddressingVersion.None)
            {
                throw FxTrace.Exception.Argument(
                    "announcementEndpoint",
                    SR.EndpointWithInvalidMessageVersion(
                        announcementEndpoint.GetType().Name,
                        AddressingVersion.None,
                        this.GetType().Name,
                        AddressingVersion.WSAddressing10,
                        AddressingVersion.WSAddressingAugust2004));
            }

            this.innerClient = announcementEndpoint.DiscoveryVersion.Implementation.CreateAnnouncementInnerClient(announcementEndpoint);            
        }

        void RaiseEvent(EventHandler handler, EventArgs e)
        {
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void OnInnerCommunicationObjectClosed(object sender, EventArgs e)
        {
            RaiseEvent(this.InternalClosed, e);
        }

        void OnInnerCommunicationObjectClosing(object sender, EventArgs e)
        {
            RaiseEvent(this.InternalClosing, e);
        }

        void OnInnerCommunicationObjectFaulted(object sender, EventArgs e)
        {
            RaiseEvent(this.InternalFaulted, e);
        }

        void OnInnerCommunicationObjectOpened(object sender, EventArgs e)
        {
            RaiseEvent(this.InternalOpened, e);
        }

        void OnInnerCommunicationObjectOpening(object sender, EventArgs e)
        {
            RaiseEvent(this.InternalOpening, e);
        }

        void OnInnerClientHelloCompleted(object sender, AsyncCompletedEventArgs e)
        {
            EventHandler<AsyncCompletedEventArgs> handler = this.InternalAnnounceOnlineCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        void OnInnerClientByeCompleted(object sender, AsyncCompletedEventArgs e)
        {
            EventHandler<AsyncCompletedEventArgs> handler = this.InternalAnnounceOfflineCompleted;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        sealed class AnnouncementOperationContextScope : IDisposable
        {
            OperationContextScope operationContextScope;
            UniqueId originalMessageId = null;

            public AnnouncementOperationContextScope(IClientChannel clientChannel)
            {
                if (DiscoveryUtility.IsCompatible(OperationContext.Current, clientChannel))
                {
                    // reuse the same context
                    this.originalMessageId = OperationContext.Current.OutgoingMessageHeaders.MessageId;
                }
                else
                {
                    // create new context
                    this.operationContextScope = new OperationContextScope(clientChannel);
                }

                if (this.originalMessageId == null)
                {
                    // this is either a new context or an existing one with no message id.
                    OperationContext.Current.OutgoingMessageHeaders.MessageId = new UniqueId();
                }
            }

            public void Dispose()
            {
                if (this.operationContextScope != null)
                {
                    this.operationContextScope.Dispose();
                }
                else
                {
                    OperationContext.Current.OutgoingMessageHeaders.MessageId = this.originalMessageId;
                }
            }
        }
    }
}

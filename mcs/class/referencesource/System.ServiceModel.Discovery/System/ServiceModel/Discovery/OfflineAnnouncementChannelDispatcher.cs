//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Discovery
{
    using System;
    using System.Collections.ObjectModel;
    using System.ServiceModel.Channels;
    using System.Runtime;
    using System.ServiceModel.Dispatcher;
    using System.Globalization;

    sealed class OfflineAnnouncementChannelDispatcher : ChannelDispatcherBase
    {
        ServiceHostBase serviceHostBase;
        IChannelListener closeListener;

        internal OfflineAnnouncementChannelDispatcher(ServiceHostBase serviceHostBase, Collection<AnnouncementEndpoint> announcementEndpoints, Collection<EndpointDiscoveryMetadata> publishedEndpoints, DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator)
        {
            Fx.Assert(serviceHostBase != null, "The serviceHostBase must be non null.");

            this.serviceHostBase = serviceHostBase;
            this.closeListener = new CloseListener(announcementEndpoints, publishedEndpoints, discoveryMessageSequenceGenerator);
        }

        public override ServiceHostBase Host
        {
            get
            {
                return this.serviceHostBase;
            }
        }

        public override IChannelListener Listener
        {
            get
            {
                return this.closeListener;
            }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get
            {
                return TimeSpan.FromMinutes(1);
            }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get
            {
                return TimeSpan.FromMinutes(1);
            }
        }

        protected override void OnAbort()
        {
            this.closeListener.Abort();
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.closeListener.BeginClose(timeout, callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            this.closeListener.EndClose(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
            this.closeListener.Close(timeout);
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return this.closeListener.BeginOpen(timeout, callback, state);
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            this.closeListener.EndOpen(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.closeListener.Open(timeout);
        }

        class CloseListener : CommunicationObject, IChannelListener
        {
            Collection<AnnouncementEndpoint> announcementEndpoints;
            Collection<EndpointDiscoveryMetadata> publishedEndpoints;
            AnnouncementDispatcherAsyncResult announceOfflineAsyncResult;
            DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator;
            bool abortAnnouncement;

            public CloseListener(Collection<AnnouncementEndpoint> announcementEndpoints, Collection<EndpointDiscoveryMetadata> publishedEndpoints, DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator)
            {
                Fx.Assert(announcementEndpoints != null && announcementEndpoints.Count > 0, "The Announcement Endpoints collection must be non null and not empty.");
                Fx.Assert(publishedEndpoints != null, "The Published Endpoints collection must be non null.");
                Fx.Assert(discoveryMessageSequenceGenerator != null, "The discoveryMessageSequenceGenerator must be non null.");

                this.announcementEndpoints = announcementEndpoints;
                this.publishedEndpoints = publishedEndpoints;
                this.discoveryMessageSequenceGenerator = discoveryMessageSequenceGenerator;
                this.abortAnnouncement = false;
            }

            public Uri Uri
            {
                get
                {
                    return new Uri(ProtocolStrings.VersionInternal.AdhocAddress);                    
                }
            }

            protected override TimeSpan DefaultCloseTimeout
            {
                get
                {
                    return TimeSpan.FromMinutes(1);
                }
            }

            protected override TimeSpan DefaultOpenTimeout
            {
                get
                {
                    return TimeSpan.FromMinutes(1);
                }
            }

            protected override void OnAbort()
            {
                this.abortAnnouncement = true;
                if (this.announceOfflineAsyncResult != null)
                {
                    this.announceOfflineAsyncResult.Cancel();
                }
            }

            protected override void OnClose(TimeSpan timeout)
            {
                OnEndClose(OnBeginClose(timeout, null, null));
            }

            protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
            {
                this.announceOfflineAsyncResult = new AnnouncementDispatcherAsyncResult(this.announcementEndpoints, this.publishedEndpoints, this.discoveryMessageSequenceGenerator, false, callback, state);
                if (this.abortAnnouncement)
                {
                    // Fixes the ---- when OnAbort is called after OnBeginClose but before this.announceOnlineAsyncResult is created
                    this.announceOfflineAsyncResult.Cancel();
                }
                else
                {
                    this.announceOfflineAsyncResult.Start(timeout, true);
                }
                return this.announceOfflineAsyncResult;
            }

            protected override void OnEndClose(IAsyncResult result)
            {
                AnnouncementDispatcherAsyncResult.End(result);
            }

            protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult(callback, state);
            }

            protected override void OnEndOpen(IAsyncResult result)
            {
                CompletedAsyncResult.End(result);
            }

            protected override void OnOpen(TimeSpan timeout)
            {
            }

            public IAsyncResult BeginWaitForChannel(TimeSpan timeout, AsyncCallback callback, object state)
            {
                return new CompletedAsyncResult<bool>(true, callback, state);
            }

            public bool EndWaitForChannel(IAsyncResult result)
            {
                return CompletedAsyncResult<bool>.End(result);
            }

            public virtual T GetProperty<T>()
                where T : class
            {
                if (typeof(T) == typeof(IChannelListener))
                {
                    return (T)(object)this;
                }

                return default(T);
            }

            public bool WaitForChannel(TimeSpan timeout)
            {
                return true;
            }
        }
    }
}

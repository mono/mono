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

    class OnlineAnnouncementChannelDispatcher : ChannelDispatcherBase
    {
        [Fx.Tag.SynchronizationObject()]
        object thisLock;

        Collection<AnnouncementEndpoint> announcementEndpoints;
        Collection<EndpointDiscoveryMetadata> publishedEndpoints;
        int dispatchersToWait;
        AnnouncementDispatcherAsyncResult announceOnlineAsyncResult;
        DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator;
        ServiceHostBase serviceHostBase;
        TimeoutHelper asyncOpenTimeoutHelper;


        internal OnlineAnnouncementChannelDispatcher(ServiceHostBase serviceHostBase, Collection<AnnouncementEndpoint> announcementEndpoints, Collection<EndpointDiscoveryMetadata> publishedEndpoints, DiscoveryMessageSequenceGenerator discoveryMessageSequenceGenerator)
        {
            Fx.Assert(serviceHostBase != null, "The serviceHostBase must be non null.");
            Fx.Assert(announcementEndpoints != null && announcementEndpoints.Count > 0, "The Announcement Endpoints collection must be non null and not empty.");
            Fx.Assert(publishedEndpoints != null, "The Published Endpoints collection must be non null.");
            Fx.Assert(discoveryMessageSequenceGenerator != null, "The discoveryMessageSequenceGenerator must be non null.");

            this.serviceHostBase = serviceHostBase;
            this.announcementEndpoints = announcementEndpoints;
            this.publishedEndpoints = publishedEndpoints;
            this.discoveryMessageSequenceGenerator = discoveryMessageSequenceGenerator;
            this.thisLock = new object();
            InitChannelDispatchers(serviceHostBase);
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
                return null;
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

        void OnChannelDispatcherOpened(object sender, EventArgs e)
        {
            bool startAnnouncements = false;
            lock (this.thisLock)
            {
                if ((--this.dispatchersToWait) == 0)
                {
                    if (this.announceOnlineAsyncResult != null)
                    {
                        startAnnouncements = true;
                        this.dispatchersToWait--;
                    }
                }
            }
            if (startAnnouncements)
            {
                this.announceOnlineAsyncResult.Start(this.asyncOpenTimeoutHelper.RemainingTime(), false);
            }
        }

        void InitChannelDispatchers(ServiceHostBase serviceHostBase)
        {
            this.dispatchersToWait = serviceHostBase.ChannelDispatchers.Count;
            EventHandler handler = new EventHandler(OnChannelDispatcherOpened);
            foreach (ChannelDispatcherBase channelDispatcher in serviceHostBase.ChannelDispatchers)
            {
                channelDispatcher.Opened += handler;
            }
        }

        protected override void OnAbort()
        {
            if (this.announceOnlineAsyncResult != null)
            {
                this.announceOnlineAsyncResult.Cancel();
            }
        }

        protected override IAsyncResult OnBeginOpen(TimeSpan timeout, AsyncCallback callback, object state)
        {
            bool startAnnouncements = false;
            this.asyncOpenTimeoutHelper = new TimeoutHelper(timeout);
            this.asyncOpenTimeoutHelper.RemainingTime();
            this.announceOnlineAsyncResult = new AnnouncementDispatcherAsyncResult(this.announcementEndpoints, this.publishedEndpoints, this.discoveryMessageSequenceGenerator, true, callback, state);
            lock (this.thisLock)
            {
                if (this.dispatchersToWait == 0)
                {
                    startAnnouncements = true;
                    this.dispatchersToWait--;
                }
            }
            if (this.State != CommunicationState.Opening)
            {
                // Fixes the ---- when OnAbort is called after OnBeginOpen but before this.announceOnlineAsyncResult is created
                this.announceOnlineAsyncResult.Cancel();
            }
            else if (startAnnouncements)
            {
                this.announceOnlineAsyncResult.Start(this.asyncOpenTimeoutHelper.RemainingTime(), true);
            }
            return this.announceOnlineAsyncResult;
        }

        protected override void OnEndOpen(IAsyncResult result)
        {
            AnnouncementDispatcherAsyncResult.End(result);
        }

        protected override void OnOpen(TimeSpan timeout)
        {
            this.announceOnlineAsyncResult = new AnnouncementDispatcherAsyncResult(this.announcementEndpoints, this.publishedEndpoints, this.discoveryMessageSequenceGenerator, true, null, null);
            if (this.State != CommunicationState.Opening)
            {
                // Fixes the ---- when OnAbort is called after OnOpen but before this.announceOnlineAsyncResult is created
                this.announceOnlineAsyncResult.Cancel();
            }
            else
            {
                this.announceOnlineAsyncResult.Start(timeout, true);
            }
            AnnouncementDispatcherAsyncResult.End(this.announceOnlineAsyncResult);
        }

        protected override IAsyncResult OnBeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            return new CompletedAsyncResult(callback, state);
        }

        protected override void OnEndClose(IAsyncResult result)
        {
            CompletedAsyncResult.End(result);
        }

        protected override void OnClose(TimeSpan timeout)
        {
        }
    }
}

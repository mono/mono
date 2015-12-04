//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.ServiceModel.Activities.Dispatcher
{
    using System.Activities.Hosting;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Runtime;
    using System.Runtime.DurableInstancing;
    using System.ServiceModel.Channels;
    using System.Threading;

    sealed class BufferedReceiveManager : IExtension<ServiceHostBase>
    {
        static AsyncCallback onEndAbandon;
        Dictionary<InstanceKey, List<BufferedReceiveMessageProperty>> bufferedProperties;
        PendingMessageThrottle throttle;
        WorkflowServiceHost host;

        int initialized;

        [Fx.Tag.SynchronizationObject(Blocking = false)]
        object thisLock;

        public BufferedReceiveManager(int maxPendingMessagesPerChannel)
        {
            this.throttle = new PendingMessageThrottle(maxPendingMessagesPerChannel);
            this.thisLock = new object();
        }

        public bool BufferReceive(OperationContext operationContext, ReceiveContext receiveContext, string bookmarkName, BufferedReceiveState state, bool retry)
        {
            Fx.Assert(receiveContext != null, "ReceiveContext must be present in order to perform buffering");

            bool success = false;

            BufferedReceiveMessageProperty property = null;
            if (BufferedReceiveMessageProperty.TryGet(operationContext.IncomingMessageProperties, out property))
            {
                CorrelationMessageProperty correlation = null;
                if (CorrelationMessageProperty.TryGet(operationContext.IncomingMessageProperties, out correlation))
                {
                    InstanceKey instanceKey = correlation.CorrelationKey;
                    int channelKey = operationContext.Channel.GetHashCode();
                    if (this.throttle.Acquire(channelKey))
                    {
                        try
                        {
                            // Tag the property with identifying data to be used during later processing
                            if (UpdateProperty(property, receiveContext, channelKey, bookmarkName, state))
                            {
                                // Cleanup if we are notified the ReceiveContext faulted underneath us
                                receiveContext.Faulted += delegate(object sender, EventArgs e)
                                {
                                    lock (this.thisLock)
                                    {
                                        if (this.bufferedProperties.ContainsKey(instanceKey))
                                        {
                                            if (this.bufferedProperties[instanceKey].Remove(property))
                                            {
                                                try
                                                {
                                                    property.RequestContext.DelayClose(false);
                                                    property.RequestContext.Abort();
                                                }
                                                catch (Exception exception)
                                                {
                                                    if (Fx.IsFatal(exception))
                                                    {
                                                        throw;
                                                    }

                                                    // ---- these exceptions as we are already on the error path
                                                }

                                                this.throttle.Release(channelKey);
                                            }
                                        }
                                    }
                                };

                                // Actual Buffering
                                lock (this.thisLock)
                                {
                                    // Optimistic state check in case we just ----d with the receiveContext
                                    // faulting. If the receiveContext still faults after the state check, the above
                                    // cleanup routine will handle things correctly. In both cases, a double-release
                                    // of the throttle is protected.
                                    if (receiveContext.State == ReceiveContextState.Received)
                                    {
                                        bool found = false;
                                        // if the exception indicates retry-able (such as RetryException),
                                        // we will simply retry.  This happens when racing with abort and 
                                        // WF informing the client to retry (BufferedReceiveManager is a
                                        // client in this case).
                                        if (retry)
                                        {
                                            property.RequestContext.DelayClose(true);
                                            property.RegisterForReplay(operationContext);
                                            property.ReplayRequest();
                                            property.Notification.NotifyInvokeReceived(property.RequestContext.InnerRequestContext);
                                            found = true;
                                        }
                                        else
                                        {
                                            ReadOnlyCollection<BookmarkInfo> bookmarks = this.host.DurableInstanceManager.PersistenceProviderDirectory.GetBookmarksForInstance(instanceKey);
                                            // Retry in case match the existing bookmark
                                            if (bookmarks != null)
                                            {
                                                for (int i = 0; i < bookmarks.Count; ++i)
                                                {
                                                    BookmarkInfo bookmark = bookmarks[i];
                                                    if (bookmark.BookmarkName == bookmarkName)
                                                    {
                                                        // Found it so retry...
                                                        property.RequestContext.DelayClose(true);
                                                        property.RegisterForReplay(operationContext);
                                                        property.ReplayRequest();
                                                        property.Notification.NotifyInvokeReceived(property.RequestContext.InnerRequestContext);
                                                        found = true;
                                                        break;
                                                    }
                                                }
                                            }
                                        }

                                        if (!found)
                                        {
                                            List<BufferedReceiveMessageProperty> properties;
                                            if (!this.bufferedProperties.TryGetValue(instanceKey, out properties))
                                            {
                                                properties = new List<BufferedReceiveMessageProperty>();
                                                this.bufferedProperties.Add(instanceKey, properties);
                                            }
                                            property.RequestContext.DelayClose(true);
                                            property.RegisterForReplay(operationContext);
                                            properties.Add(property);
                                        }
                                        else
                                        {
                                            this.throttle.Release(channelKey);
                                        }
                                        success = true;
                                    }
                                }
                            }
                        }
                        finally
                        {
                            if (!success)
                            {
                                this.throttle.Release(channelKey);
                            }
                        }
                    }
                }
            }

            return success;
        }

        public void Retry(HashSet<InstanceKey> associatedInstances, ReadOnlyCollection<BookmarkInfo> availableBookmarks)
        {
            List<BookmarkInfo> bookmarks = new List<BookmarkInfo>(availableBookmarks);
            foreach (InstanceKey instanceKey in associatedInstances)
            {
                lock (this.thisLock)
                {
                    if (this.bufferedProperties.ContainsKey(instanceKey))
                    {
                        List<BufferedReceiveMessageProperty> properties = this.bufferedProperties[instanceKey];
                        int index = 0;

                        while (index < properties.Count && bookmarks.Count > 0)
                        {
                            BufferedReceiveMessageProperty property = properties[index];

                            // Determine if this property is now ready to be processed
                            int channelKey = 0;
                            bool found = false;
                            for (int i = 0; i < bookmarks.Count; ++i)
                            {
                                BookmarkInfo bookmark = (BookmarkInfo)bookmarks[i];
                                PropertyData data = (PropertyData)property.UserState;
                                if (bookmark.BookmarkName == data.BookmarkName)
                                {
                                    // Found it so retry...
                                    bookmarks.RemoveAt(i);
                                    channelKey = data.ChannelKey;
                                    property.ReplayRequest();
                                    property.Notification.NotifyInvokeReceived(property.RequestContext.InnerRequestContext);
                                    found = true;
                                    break;
                                }
                            }

                            if (!found)
                            {
                                index++;
                            }
                            else
                            {
                                properties.RemoveAt(index);
                                this.throttle.Release(channelKey);
                            }
                        }
                    }
                }

                if (bookmarks.Count == 0)
                {
                    break;
                }
            }
        }

        public void AbandonBufferedReceives(HashSet<InstanceKey> associatedInstances)
        {
            foreach (InstanceKey instanceKey in associatedInstances)
            {
                lock (this.thisLock)
                {
                    if (this.bufferedProperties.ContainsKey(instanceKey))
                    {
                        foreach (BufferedReceiveMessageProperty property in this.bufferedProperties[instanceKey])
                        {
                            PropertyData data = (PropertyData)property.UserState;
                            AbandonReceiveContext(data.ReceiveContext);
                            this.throttle.Release(data.ChannelKey);
                        }

                        this.bufferedProperties.Remove(instanceKey);
                    }
                }
            }
        }

        // clean up any remaining buffered receives as part of ServiceHost close.
        internal void AbandonBufferedReceives()
        {
            lock (this.thisLock)
            {
                foreach (List<BufferedReceiveMessageProperty> value in this.bufferedProperties.Values)
                {
                    foreach (BufferedReceiveMessageProperty property in value)
                    {
                        PropertyData data = (PropertyData)property.UserState;
                        AbandonReceiveContext(data.ReceiveContext);
                        this.throttle.Release(data.ChannelKey);
                    }
                }
                this.bufferedProperties.Clear();
            }
        }

        // Best-effort to abandon the receiveContext
        internal static void AbandonReceiveContext(ReceiveContext receiveContext)
        {
            if (receiveContext != null)
            {
                if (onEndAbandon == null)
                {
                    onEndAbandon = Fx.ThunkCallback(new AsyncCallback(OnEndAbandon));
                }

                try
                {
                    IAsyncResult result = receiveContext.BeginAbandon(
                        TimeSpan.MaxValue, onEndAbandon, receiveContext);
                    if (result.CompletedSynchronously)
                    {
                        HandleEndAbandon(result);
                    }
                }
                catch (Exception exception)
                {
                    if (Fx.IsFatal(exception))
                    {
                        throw;
                    }

                    // We ---- any Abandon exception - best effort.
                    FxTrace.Exception.AsWarning(exception);
                }
            }
        }

        static bool HandleEndAbandon(IAsyncResult result)
        {
            ReceiveContext receiveContext = (ReceiveContext)result.AsyncState;
            receiveContext.EndAbandon(result);
            return true;
        }

        static void OnEndAbandon(IAsyncResult result)
        {
            if (result.CompletedSynchronously)
            {
                return;
            }

            try
            {
                HandleEndAbandon(result);
            }
            catch (Exception exception)
            {
                if (Fx.IsFatal(exception))
                {
                    throw;
                }

                // We ---- any Abandon exception - best effort.
                FxTrace.Exception.AsWarning(exception);
            }
        }

        void IExtension<ServiceHostBase>.Attach(ServiceHostBase owner)
        {
            if (owner == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("owner"));
            }

            if (Interlocked.CompareExchange(ref this.initialized, 1, 0) != 0)
            {
                throw FxTrace.Exception.AsError(
                    new InvalidOperationException(SR.BufferedReceiveBehaviorMultipleUse));
            }

            owner.ThrowIfClosedOrOpened();

            Fx.Assert(owner is WorkflowServiceHost, "owner must be of WorkflowServiceHost type!");
            this.host = (WorkflowServiceHost)owner;
            Initialize();
        }

        void IExtension<ServiceHostBase>.Detach(ServiceHostBase owner)
        {
        }

        bool UpdateProperty(BufferedReceiveMessageProperty property, ReceiveContext receiveContext, int channelKey, string bookmarkName, BufferedReceiveState state)
        {
            // If there's data already there make sure the state is allowed
            if (property.UserState == null)
            {
                property.UserState = new PropertyData()
                {
                    ReceiveContext = receiveContext,
                    ChannelKey = channelKey,
                    BookmarkName = bookmarkName,
                    State = state
                };
            }
            else
            {
                PropertyData data = (PropertyData)property.UserState;

                // We should not buffer twice at the same state
                if (data.State == state)
                {
                    return false;
                }

                data.State = state;
            }

            return true;
        }

        void Initialize()
        {
            this.bufferedProperties = new Dictionary<InstanceKey, List<BufferedReceiveMessageProperty>>();
        }

        class PendingMessageThrottle
        {
            [Fx.Tag.SynchronizationObject(Blocking = false)]
            Dictionary<int, ThrottleEntry> pendingMessages;

            int maxPendingMessagesPerChannel;
            int warningRestoreLimit;

            public PendingMessageThrottle(int maxPendingMessagesPerChannel)
            {
                this.maxPendingMessagesPerChannel = maxPendingMessagesPerChannel;
                this.warningRestoreLimit = (int)Math.Floor(0.7 * (double)maxPendingMessagesPerChannel);
                this.pendingMessages = new Dictionary<int, ThrottleEntry>();
            }

            public bool Acquire(int channelKey)
            {
                lock (this.pendingMessages)
                {
                    if (!this.pendingMessages.ContainsKey(channelKey))
                    {
                        this.pendingMessages.Add(channelKey, new ThrottleEntry());
                    }

                    ThrottleEntry entry = this.pendingMessages[channelKey];
                    if (entry.Count < this.maxPendingMessagesPerChannel)
                    {
                        entry.Count++;
                        if (TD.PendingMessagesPerChannelRatioIsEnabled())
                        {
                            TD.PendingMessagesPerChannelRatio(entry.Count, this.maxPendingMessagesPerChannel);
                        }
                        return true;
                    }
                    else
                    {
                        if (TD.MaxPendingMessagesPerChannelExceededIsEnabled())
                        {
                            if (!entry.WarningIssued)
                            {
                                TD.MaxPendingMessagesPerChannelExceeded(this.maxPendingMessagesPerChannel);
                                entry.WarningIssued = true;
                            }
                        }

                        return false;
                    }
                }
            }

            public void Release(int channelKey)
            {
                lock (this.pendingMessages)
                {
                    ThrottleEntry entry = this.pendingMessages[channelKey];
                    Fx.Assert(entry.Count > 0, "The pending message throttle was released too many times");

                    entry.Count--;
                    if (TD.PendingMessagesPerChannelRatioIsEnabled())
                    {
                        TD.PendingMessagesPerChannelRatio(entry.Count, this.maxPendingMessagesPerChannel);
                    }
                    if (entry.Count == 0)
                    {
                        this.pendingMessages.Remove(channelKey);
                    }
                    else if (entry.Count < this.warningRestoreLimit)
                    {
                        entry.WarningIssued = false;
                    }
                }
            }

            class ThrottleEntry
            {
                public ThrottleEntry()
                {
                }

                public bool WarningIssued
                {
                    get;
                    set;
                }

                public int Count
                {
                    get;
                    set;
                }
            }
        }

        class PropertyData
        {
            public PropertyData()
            {
            }

            public ReceiveContext ReceiveContext
            {
                get;
                set;
            }

            public int ChannelKey
            {
                get;
                set;
            }

            public string BookmarkName
            {
                get;
                set;
            }

            public BufferedReceiveState State
            {
                get;
                set;
            }
        }
    }
}

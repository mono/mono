//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.ServiceModel.Routing
{
    using System;
    using System.Diagnostics.CodeAnalysis;
    using System.Collections.Generic;
    using System.Runtime;
    using System.ServiceModel.Description;
    using System.ServiceModel.Channels;
    using System.ComponentModel;
    using System.Threading;

    class SessionChannels
    {
        Guid activityID;
        [Fx.Tag.SynchronizationObject]
        Dictionary<RoutingEndpointTrait, IRoutingClient> sessions;
        List<IRoutingClient> sessionList;

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "gets called in RoutingService.ctor()")]
        public SessionChannels(Guid activityID)
        {
            this.activityID = activityID;
            this.sessions = new Dictionary<RoutingEndpointTrait, IRoutingClient>();
            this.sessionList = new List<IRoutingClient>();
        }

        void ChannelFaulted(object sender, EventArgs args)
        {
            FxTrace.Trace.SetAndTraceTransfer(this.activityID, true);
            IRoutingClient client = (IRoutingClient)sender;
            if (TD.RoutingServiceChannelFaultedIsEnabled())
            {
                TD.RoutingServiceChannelFaulted(client.Key.ToString());
            }
            this.AbortChannel(client.Key);
        }

        [SuppressMessage(FxCop.Category.Performance, FxCop.Rule.AvoidUncalledPrivateCode, Justification = "BeginClose is called by RoutingChannelExtension.AttachService")]
        public IAsyncResult BeginClose(TimeSpan timeout, AsyncCallback callback, object state)
        {
            List<ICommunicationObject> localClients = null;

            lock (this.sessions)
            {
                if (this.sessions.Count > 0)
                {
                    localClients = this.sessionList.ConvertAll<ICommunicationObject>((client) => (ICommunicationObject)client);
                    this.sessionList.Clear();
                    this.sessions.Clear();
                }
            }

            if (localClients != null && localClients.Count > 0)
            {
                localClients.ForEach((client) =>
                {
                    if (TD.RoutingServiceClosingClientIsEnabled())
                    {
                        TD.RoutingServiceClosingClient(((IRoutingClient)client).Key.ToString());
                    }
                });
                return new CloseCollectionAsyncResult(timeout, callback, state, localClients);
            }
            else
            {
                return new CompletedAsyncResult(callback, state);
            }
        }

        public void EndClose(IAsyncResult asyncResult)
        {
            if (asyncResult is CloseCollectionAsyncResult)
            {
                CloseCollectionAsyncResult.End(asyncResult);
            }
            else
            {
                CompletedAsyncResult.End(asyncResult);
            }
        }

        internal IRoutingClient GetOrCreateClient<TContract>(RoutingEndpointTrait key, RoutingService service, bool impersonating)
        {
            IRoutingClient value;
            lock (this.sessions)
            {
                if (!this.sessions.TryGetValue(key, out value))
                {
                    //Create the client here
                    value = ClientFactory.Create(key, service, impersonating);
                    value.Faulted += ChannelFaulted;
                    this.sessions[key] = value;
                    sessionList.Add(value);
                }
            }
            return value;
        }

        public void AbortAll()
        {
            List<IRoutingClient> clients = new List<IRoutingClient>();

            lock (this.sessions)
            {
                foreach (IRoutingClient client in this.sessions.Values)
                {
                    clients.Add(client);
                }
                this.sessions.Clear();
                this.sessionList.Clear();
            }

            foreach (IRoutingClient client in clients)
            {
                RoutingUtilities.Abort((ICommunicationObject)client, client.Key);
            }
        }

        public void AbortChannel(RoutingEndpointTrait key)
        {
            IRoutingClient client;
            lock (this.sessions)
            {
                if (this.sessions.TryGetValue(key, out client))
                {
                    this.sessions.Remove(key);
                    this.sessionList.Remove(client);
                }
            }

            if (client != null)
            {
                RoutingUtilities.Abort((ICommunicationObject)client, client.Key);
            }
        }

        public IRoutingClient ReleaseChannel()
        {
            IRoutingClient client = null;
            lock (this.sessions)
            {
                int count = this.sessionList.Count;
                if (count > 0)
                {
                    client = this.sessionList[count - 1];
                    this.sessionList.RemoveAt(count - 1);
                    this.sessions.Remove(client.Key);
                }
            }
            return client;
        }
    }
}

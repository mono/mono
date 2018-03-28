//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel
{
    using System;
    using System.Runtime;
    using System.ServiceModel.Channels;
    using System.ServiceModel.Dispatcher;
    using System.Threading;

    public sealed class PeerNode : IOnlineStatus
    {
        PeerNodeImplementation innerNode = null;
        SynchronizationContext synchronizationContext = null;
        MessageEncodingBindingElement encoderElement;

        internal PeerNode(PeerNodeImplementation peerNode)
        {
            this.innerNode = peerNode;
        }

        public event EventHandler Offline;
        public event EventHandler Online;

        internal void FireOffline(object source, EventArgs args)
        {
            FireEvent(Offline, source, args);
        }

        internal void FireOnline(object source, EventArgs args)
        {
            FireEvent(Online, source, args);
        }

        void FireEvent(EventHandler handler, object source, EventArgs args)
        {
            if (handler != null)
            {
                try
                {
                    SynchronizationContext context = synchronizationContext;
                    if (context != null)
                    {
                        context.Send(delegate(object state) { handler(source, args); }, null);
                    }
                    else
                    {
                        handler(source, args);
                    }
                }
                catch (Exception e)
                {
                    if (Fx.IsFatal(e)) throw;
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperCallback(SR.GetString(SR.NotificationException), e);
                }
            }
        }

        public bool IsOnline { get { return InnerNode.IsOnline; } }

        internal bool IsOpen { get { return InnerNode.IsOpen; } }

        public int Port { get { return InnerNode.ListenerPort; } }

        public PeerMessagePropagationFilter MessagePropagationFilter
        {
            get { return InnerNode.MessagePropagationFilter; }
            set { InnerNode.MessagePropagationFilter = value; }
        }

        internal void OnOpen()
        {
            synchronizationContext = ThreadBehavior.GetCurrentSynchronizationContext();
            this.innerNode.Offline += FireOffline;
            this.innerNode.Online += FireOnline;
            this.innerNode.EncodingElement = this.encoderElement;
        }

        internal void OnClose()
        {
            this.innerNode.Offline -= FireOffline;
            this.innerNode.Online -= FireOnline;
            synchronizationContext = null;
        }

        internal PeerNodeImplementation InnerNode
        {
            get { return innerNode; }
        }

        public void RefreshConnection()
        {
            PeerNodeImplementation node = InnerNode;
            if (node != null)
            {
                node.RefreshConnection();
            }
        }

        public override string ToString()
        {
            if (this.IsOpen)
            {
                return SR.GetString(SR.PeerNodeToStringFormat, this.InnerNode.MeshId, this.InnerNode.NodeId, this.IsOnline, this.IsOpen, this.Port);
            }
            else
            {
                return SR.GetString(SR.PeerNodeToStringFormat, "", -1, this.IsOnline, this.IsOpen, -1);
            }
        }

        private MessageEncodingBindingElement EncodingElement
        {
            get { return this.encoderElement; }
            set { this.encoderElement = value; }

        }
    }
}

//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.Channels
{
    using System.Diagnostics;
    using System.ServiceModel;
    using System.ServiceModel.Diagnostics;

    public abstract class ChannelBase : CommunicationObject, IChannel, IDefaultCommunicationTimeouts
    {
        ChannelManagerBase channelManager;

        protected ChannelBase(ChannelManagerBase channelManager)
        {
            if (channelManager == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("channelManager");
            }

            this.channelManager = channelManager;

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.ChannelCreated,
                    SR.GetString(SR.TraceCodeChannelCreated, TraceUtility.CreateSourceString(this)), this);
            }
        }

        TimeSpan IDefaultCommunicationTimeouts.CloseTimeout
        {
            get { return this.DefaultCloseTimeout; }
        }

        TimeSpan IDefaultCommunicationTimeouts.OpenTimeout
        {
            get { return this.DefaultOpenTimeout; }
        }

        TimeSpan IDefaultCommunicationTimeouts.ReceiveTimeout
        {
            get { return this.DefaultReceiveTimeout; }
        }

        TimeSpan IDefaultCommunicationTimeouts.SendTimeout
        {
            get { return this.DefaultSendTimeout; }
        }

        protected override TimeSpan DefaultCloseTimeout
        {
            get { return ((IDefaultCommunicationTimeouts)this.channelManager).CloseTimeout; }
        }

        protected override TimeSpan DefaultOpenTimeout
        {
            get { return ((IDefaultCommunicationTimeouts)this.channelManager).OpenTimeout; }
        }

        protected TimeSpan DefaultReceiveTimeout
        {
            get { return ((IDefaultCommunicationTimeouts)this.channelManager).ReceiveTimeout; }
        }

        protected TimeSpan DefaultSendTimeout
        {
            get { return ((IDefaultCommunicationTimeouts)this.channelManager).SendTimeout; }
        }

        protected ChannelManagerBase Manager
        {
            get
            {
                return channelManager;
            }
        }

        public virtual T GetProperty<T>() where T : class
        {
            IChannelFactory factory = this.channelManager as IChannelFactory;
            if (factory != null)
            {
                return factory.GetProperty<T>();
            }

            IChannelListener listener = this.channelManager as IChannelListener;
            if (listener != null)
            {
                return listener.GetProperty<T>();
            }

            return null;
        }

        protected override void OnClosed()
        {
            base.OnClosed();

            if (DiagnosticUtility.ShouldTraceVerbose)
            {
                TraceUtility.TraceEvent(TraceEventType.Verbose, TraceCode.ChannelDisposed,
                    SR.GetString(SR.TraceCodeChannelDisposed, TraceUtility.CreateSourceString(this)), this);
            }
        }
    }
}

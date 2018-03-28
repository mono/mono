//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.ServiceModel.Activation;
    using System.Collections.Generic;
    using System.Net;
    using System.Net.Security;
    using System.Net.Sockets;
    using System.Runtime.Serialization;
    using System.Security.Authentication.ExtendedProtection;
    using System.Xml;
    using System.ComponentModel;

    public class TcpTransportBindingElement : ConnectionOrientedTransportBindingElement
    {
        int listenBacklog;
        bool portSharingEnabled;
        bool teredoEnabled;
        TcpConnectionPoolSettings connectionPoolSettings;
        ExtendedProtectionPolicy extendedProtectionPolicy;
        bool isListenBacklogSet;

        public TcpTransportBindingElement()
            : base()
        {
            this.listenBacklog = TcpTransportDefaults.GetListenBacklog();
            this.portSharingEnabled = TcpTransportDefaults.PortSharingEnabled;
            this.teredoEnabled = TcpTransportDefaults.TeredoEnabled;
            this.connectionPoolSettings = new TcpConnectionPoolSettings();
            this.extendedProtectionPolicy = ChannelBindingUtility.DefaultPolicy;
        }

        protected TcpTransportBindingElement(TcpTransportBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.listenBacklog = elementToBeCloned.listenBacklog;
            this.isListenBacklogSet = elementToBeCloned.isListenBacklogSet;
            this.portSharingEnabled = elementToBeCloned.portSharingEnabled;
            this.teredoEnabled = elementToBeCloned.teredoEnabled;
            this.connectionPoolSettings = elementToBeCloned.connectionPoolSettings.Clone();
            this.extendedProtectionPolicy = elementToBeCloned.ExtendedProtectionPolicy;
        }

        public TcpConnectionPoolSettings ConnectionPoolSettings
        {
            get { return this.connectionPoolSettings; }
        }

        public int ListenBacklog
        {
            get
            {
                return this.listenBacklog;
            }

            set
            {
                if (value <= 0)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value",
                        SR.GetString(SR.ValueMustBePositive)));
                }

                this.listenBacklog = value;
                this.isListenBacklogSet = true;
            }
        }

        internal bool IsListenBacklogSet
        {
            get { return this.isListenBacklogSet; }
        }

        // server
        [DefaultValue(TcpTransportDefaults.PortSharingEnabled)]
        public bool PortSharingEnabled
        {
            get
            {
                return this.portSharingEnabled;
            }
            set
            {
                this.portSharingEnabled = value;
            }
        }

        public override string Scheme
        {
            get { return "net.tcp"; }
        }

        // server
        [DefaultValue(TcpTransportDefaults.TeredoEnabled)]
        public bool TeredoEnabled
        {
            get
            {
                return this.teredoEnabled;
            }

            set
            {
                this.teredoEnabled = value;
            }
        }

        public ExtendedProtectionPolicy ExtendedProtectionPolicy
        {
            get
            {
                return this.extendedProtectionPolicy;
            }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value.PolicyEnforcement == PolicyEnforcement.Always &&
                    !System.Security.Authentication.ExtendedProtection.ExtendedProtectionPolicy.OSSupportsExtendedProtection)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                        new PlatformNotSupportedException(SR.GetString(SR.ExtendedProtectionNotSupported)));
                }

                this.extendedProtectionPolicy = value;
            }
        }

        internal override string WsdlTransportUri
        {
            get
            {
                return TransportPolicyConstants.TcpTransportUri;
            }
        }

        public override BindingElement Clone()
        {
            return new TcpTransportBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            return (IChannelFactory<TChannel>)(object)new TcpChannelFactory<TChannel>(this, context);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            TcpChannelListener listener;
            if (typeof(TChannel) == typeof(IReplyChannel))
            {
                listener = new TcpReplyChannelListener(this, context);
            }
            else if (typeof(TChannel) == typeof(IDuplexSessionChannel))
            {
                listener = new TcpDuplexChannelListener(this, context);
            }
            else
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel", SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            AspNetEnvironment.Current.ApplyHostedContext(listener, context);
            return (IChannelListener<TChannel>)(object)listener;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(IBindingDeliveryCapabilities))
            {
                return (T)(object)new BindingDeliveryCapabilitiesHelper();
            }
            else if (typeof(T) == typeof(ExtendedProtectionPolicy))
            {
                return (T)(object)this.ExtendedProtectionPolicy;
            }
            else if (typeof(T) == typeof(ITransportCompressionSupport))
            {
                return (T)(object)new TransportCompressionSupportHelper();
            }
            else
            {
                return base.GetProperty<T>(context);
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (!base.IsMatch(b))
            {
                return false;
            }

            TcpTransportBindingElement tcp = b as TcpTransportBindingElement;
            if (tcp == null)
            {
                return false;
            }

            if (this.listenBacklog != tcp.listenBacklog)
            {
                return false;
            }
            if (this.portSharingEnabled != tcp.portSharingEnabled)
            {
                return false;
            }
            if (this.teredoEnabled != tcp.teredoEnabled)
            {
                return false;
            }
            if (!this.connectionPoolSettings.IsMatch(tcp.connectionPoolSettings))
            {
                return false;
            }

            if (!ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, tcp.ExtendedProtectionPolicy))
            {
                return false;
            }

            return true;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeExtendedProtectionPolicy()
        {
            return !ChannelBindingUtility.AreEqual(this.ExtendedProtectionPolicy, ChannelBindingUtility.DefaultPolicy);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeListenBacklog()
        {
            return this.isListenBacklogSet;
        }

        class BindingDeliveryCapabilitiesHelper : IBindingDeliveryCapabilities
        {
            internal BindingDeliveryCapabilitiesHelper()
            {
            }
            bool IBindingDeliveryCapabilities.AssuresOrderedDelivery
            {
                get { return true; }
            }

            bool IBindingDeliveryCapabilities.QueuedDelivery
            {
                get { return false; }
            }
        }

        class TransportCompressionSupportHelper : ITransportCompressionSupport
        {
            public bool IsCompressionFormatSupported(CompressionFormat compressionFormat)
            {
                return true;
            }
        }
    }
}

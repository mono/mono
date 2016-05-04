//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Runtime;
    using System.ServiceModel;
    using System.ServiceModel.Description;

    public abstract class Binding : IDefaultCommunicationTimeouts
    {
        TimeSpan closeTimeout = ServiceDefaults.CloseTimeout;
        string name;
        string namespaceIdentifier;
        TimeSpan openTimeout = ServiceDefaults.OpenTimeout;
        TimeSpan receiveTimeout = ServiceDefaults.ReceiveTimeout;
        TimeSpan sendTimeout = ServiceDefaults.SendTimeout;
        internal const string DefaultNamespace = NamingHelper.DefaultNamespace;

        protected Binding()
        {
            this.name = null;
            this.namespaceIdentifier = DefaultNamespace;
        }

        protected Binding(string name, string ns)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("name", SR.GetString(SR.SFXBindingNameCannotBeNullOrEmpty));
            }
            if (ns == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("ns");
            }

            if (ns.Length > 0)
            {
                NamingHelper.CheckUriParameter(ns, "ns");
            }

            this.name = name;
            this.namespaceIdentifier = ns;
        }


        [DefaultValue(typeof(TimeSpan), ServiceDefaults.CloseTimeoutString)]
        public TimeSpan CloseTimeout
        {
            get { return this.closeTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.closeTimeout = value;
            }
        }

        public string Name
        {
            get
            {
                if (this.name == null)
                    this.name = this.GetType().Name;

                return this.name;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("value", SR.GetString(SR.SFXBindingNameCannotBeNullOrEmpty));

                this.name = value;
            }
        }

        public string Namespace
        {
            get { return this.namespaceIdentifier; }
            set
            {
                if (value == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("value");
                }

                if (value.Length > 0)
                {
                    NamingHelper.CheckUriProperty(value, "Namespace");
                }
                this.namespaceIdentifier = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), ServiceDefaults.OpenTimeoutString)]
        public TimeSpan OpenTimeout
        {
            get { return this.openTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.openTimeout = value;
            }
        }

        [DefaultValue(typeof(TimeSpan), ServiceDefaults.ReceiveTimeoutString)]
        public TimeSpan ReceiveTimeout
        {
            get { return this.receiveTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.receiveTimeout = value;
            }
        }

        public abstract string Scheme { get; }

        public MessageVersion MessageVersion
        {
            get
            {
                return this.GetProperty<MessageVersion>(new BindingParameterCollection());
            }
        }

        [DefaultValue(typeof(TimeSpan), ServiceDefaults.SendTimeoutString)]
        public TimeSpan SendTimeout
        {
            get { return this.sendTimeout; }
            set
            {
                if (value < TimeSpan.Zero)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRange0)));
                }
                if (TimeoutHelper.IsTooLarge(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value", value, SR.GetString(SR.SFxTimeoutOutOfRangeTooBig)));
                }

                this.sendTimeout = value;
            }
        }

        public IChannelFactory<TChannel> BuildChannelFactory<TChannel>(params object[] parameters)
        {
            return this.BuildChannelFactory<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            EnsureInvariants();
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            IChannelFactory<TChannel> channelFactory = context.BuildInnerChannelFactory<TChannel>();
            context.ValidateBindingElementsConsumed();
            this.ValidateSecurityCapabilities(channelFactory.GetProperty<ISecurityCapabilities>(), parameters);

            return channelFactory;
        }

        void ValidateSecurityCapabilities(ISecurityCapabilities runtimeSecurityCapabilities, BindingParameterCollection parameters)
        {
            ISecurityCapabilities bindingSecurityCapabilities = this.GetProperty<ISecurityCapabilities>(parameters);

            if (!SecurityCapabilities.IsEqual(bindingSecurityCapabilities, runtimeSecurityCapabilities))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.SecurityCapabilitiesMismatched, this)));
            }
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(params object[] parameters)
            where TChannel : class, IChannel
        {
            return this.BuildChannelListener<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, params object[] parameters)
            where TChannel : class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, params object[] parameters)
            where TChannel : class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, listenUriRelativeAddress, new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, params object[] parameters)
            where TChannel : class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, listenUriRelativeAddress, listenUriMode, new BindingParameterCollection(parameters));
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingParameterCollection parameters)
            where TChannel : class, IChannel
        {
            UriBuilder listenUriBuilder = new UriBuilder(this.Scheme, DnsCache.MachineName);
            return this.BuildChannelListener<TChannel>(listenUriBuilder.Uri, String.Empty, ListenUriMode.Unique, parameters);
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, BindingParameterCollection parameters)
            where TChannel : class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, String.Empty, ListenUriMode.Explicit, parameters);
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, BindingParameterCollection parameters)
            where TChannel : class, IChannel
        {
            return this.BuildChannelListener<TChannel>(listenUriBaseAddress, listenUriRelativeAddress, ListenUriMode.Explicit, parameters);
        }

        public virtual IChannelListener<TChannel> BuildChannelListener<TChannel>(Uri listenUriBaseAddress, string listenUriRelativeAddress, ListenUriMode listenUriMode, BindingParameterCollection parameters)
            where TChannel : class, IChannel
        {
            EnsureInvariants();
            BindingContext context = new BindingContext(new CustomBinding(this), parameters, listenUriBaseAddress, listenUriRelativeAddress, listenUriMode);
            IChannelListener<TChannel> channelListener = context.BuildInnerChannelListener<TChannel>();
            context.ValidateBindingElementsConsumed();
            this.ValidateSecurityCapabilities(channelListener.GetProperty<ISecurityCapabilities>(), parameters);

            return channelListener;
        }

        public bool CanBuildChannelFactory<TChannel>(params object[] parameters)
        {
            return this.CanBuildChannelFactory<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual bool CanBuildChannelFactory<TChannel>(BindingParameterCollection parameters)
        {
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public bool CanBuildChannelListener<TChannel>(params object[] parameters) where TChannel : class, IChannel
        {
            return this.CanBuildChannelListener<TChannel>(new BindingParameterCollection(parameters));
        }

        public virtual bool CanBuildChannelListener<TChannel>(BindingParameterCollection parameters) where TChannel : class, IChannel
        {
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        // the elements should NOT reference internal elements used by the Binding
        public abstract BindingElementCollection CreateBindingElements();

        public T GetProperty<T>(BindingParameterCollection parameters)
            where T : class
        {
            BindingContext context = new BindingContext(new CustomBinding(this), parameters);
            return context.GetInnerProperty<T>();
        }

        void EnsureInvariants()
        {
            EnsureInvariants(null);
        }

        internal void EnsureInvariants(string contractName)
        {
            BindingElementCollection elements = this.CreateBindingElements();
            TransportBindingElement transport = null;
            int index;
            for (index = 0; index < elements.Count; index++)
            {
                transport = elements[index] as TransportBindingElement;
                if (transport != null)
                    break;
            }

            if (transport == null)
            {
                if (contractName == null)
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.CustomBindingRequiresTransport, this.Name)));
                }
                else
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                        SR.GetString(SR.SFxCustomBindingNeedsTransport1, contractName)));
                }
            }
            if (index != elements.Count - 1)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.TransportBindingElementMustBeLast, this.Name, transport.GetType().Name)));
            }
            if (string.IsNullOrEmpty(transport.Scheme))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.InvalidBindingScheme, transport.GetType().Name)));
            }

            if (this.MessageVersion == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidOperationException(
                    SR.GetString(SR.MessageVersionMissingFromBinding, this.Name)));
            }
        }

        internal void CopyTimeouts(IDefaultCommunicationTimeouts source)
        {
            this.CloseTimeout = source.CloseTimeout;
            this.OpenTimeout = source.OpenTimeout;
            this.ReceiveTimeout = source.ReceiveTimeout;
            this.SendTimeout = source.SendTimeout;
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeName()
        {
            return (this.Name != this.GetType().Name);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeNamespace()
        {
            return (this.Namespace != DefaultNamespace);
        }
    }
}


//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System;
    using System.ComponentModel;
    using System.Net.Security;
    using System.Runtime.CompilerServices;
    using System.ServiceModel.Administration;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    [TypeForwardedFrom("System.WorkflowServices, Version=3.5.0.0, Culture=neutral, PublicKeyToken=31bf3856ad364e35")]
    public class ContextBindingElement : BindingElement, IPolicyExportExtension, IContextSessionProvider, IWmiInstanceProvider, IContextBindingElement
    {
        internal const ContextExchangeMechanism DefaultContextExchangeMechanism = ContextExchangeMechanism.ContextSoapHeader;
        internal const bool DefaultContextManagementEnabled = true;
        internal const ProtectionLevel DefaultProtectionLevel = ProtectionLevel.Sign;
        ContextExchangeMechanism contextExchangeMechanism;
        ICorrelationDataSource instanceCorrelationData;
        bool contextManagementEnabled;
        ProtectionLevel protectionLevel;

        public ContextBindingElement()
            : this(DefaultProtectionLevel, DefaultContextExchangeMechanism, null, DefaultContextManagementEnabled)
        {
            // empty
        }

        public ContextBindingElement(ProtectionLevel protectionLevel)
            : this(protectionLevel, DefaultContextExchangeMechanism, null, DefaultContextManagementEnabled)
        {
            // empty
        }

        public ContextBindingElement(ProtectionLevel protectionLevel, ContextExchangeMechanism contextExchangeMechanism)
            : this(protectionLevel, contextExchangeMechanism, null, DefaultContextManagementEnabled)
        {
            // empty
        }


        public ContextBindingElement(ProtectionLevel protectionLevel, ContextExchangeMechanism contextExchangeMechanism, Uri clientCallbackAddress)
            : this(protectionLevel, contextExchangeMechanism, clientCallbackAddress, DefaultContextManagementEnabled)
        {
            // empty
        }

        public ContextBindingElement(ProtectionLevel protectionLevel, ContextExchangeMechanism contextExchangeMechanism, Uri clientCallbackAddress, bool contextManagementEnabled)
        {
            this.ProtectionLevel = protectionLevel;
            this.ContextExchangeMechanism = contextExchangeMechanism;
            this.ClientCallbackAddress = clientCallbackAddress;
            this.ContextManagementEnabled = contextManagementEnabled;
        }

        ContextBindingElement(ContextBindingElement other)
            : base(other)
        {
            this.ProtectionLevel = other.ProtectionLevel;
            this.ContextExchangeMechanism = other.ContextExchangeMechanism;
            this.ClientCallbackAddress = other.ClientCallbackAddress;
            this.ContextManagementEnabled = other.ContextManagementEnabled;
        }

        [DefaultValue(null)]
        public Uri ClientCallbackAddress
        {
            get;
            set;
        }

        [DefaultValue(DefaultContextExchangeMechanism)]
        public ContextExchangeMechanism ContextExchangeMechanism
        {
            get
            {
                return this.contextExchangeMechanism;
            }
            set
            {
                if (!ContextExchangeMechanismHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.contextExchangeMechanism = value;
            }
        }

        [DefaultValue(DefaultContextManagementEnabled)]
        public bool ContextManagementEnabled
        {
            get
            {
                return this.contextManagementEnabled;
            }
            set
            {
                this.contextManagementEnabled = value;
            }
        }

        [DefaultValue(DefaultProtectionLevel)]
        public ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                if (!ProtectionLevelHelper.IsDefined(value))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ArgumentOutOfRangeException("value"));
                }
                this.protectionLevel = value;
            }
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelFactory<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ContextBindingElementCannotProvideChannelFactory, typeof(TChannel).ToString())));
            }

            this.EnsureContextExchangeMechanismCompatibleWithScheme(context);
            this.EnsureContextExchangeMechanismCompatibleWithTransportCookieSetting(context);

            return new ContextChannelFactory<TChannel>(context, this.ContextExchangeMechanism, this.ClientCallbackAddress, this.ContextManagementEnabled);
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (!this.CanBuildChannelListener<TChannel>(context))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(SR.GetString(SR.ContextBindingElementCannotProvideChannelListener, typeof(TChannel).ToString())));
            }

            this.EnsureContextExchangeMechanismCompatibleWithScheme(context);

            return new ContextChannelListener<TChannel>(context, this.ContextExchangeMechanism);
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            return (typeof(TChannel) == typeof(IOutputChannel)
                || typeof(TChannel) == typeof(IOutputSessionChannel)
                || typeof(TChannel) == typeof(IRequestChannel)
                || typeof(TChannel) == typeof(IRequestSessionChannel)
                || (typeof(TChannel) == typeof(IDuplexSessionChannel) && this.ContextExchangeMechanism != ContextExchangeMechanism.HttpCookie))
                && context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            return ((typeof(TChannel) == typeof(IInputChannel)
                || typeof(TChannel) == typeof(IInputSessionChannel)
                || typeof(TChannel) == typeof(IReplyChannel)
                || typeof(TChannel) == typeof(IReplySessionChannel)
                || (typeof(TChannel) == typeof(IDuplexSessionChannel) && this.ContextExchangeMechanism != ContextExchangeMechanism.HttpCookie))
                && context.CanBuildInnerChannelListener<TChannel>());
        }

        public override BindingElement Clone()
        {
            return new ContextBindingElement(this);
        }

        public virtual void ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            ContextBindingElementPolicy.ExportRequireContextAssertion(this, context.GetBindingAssertions());
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(T) == typeof(ChannelProtectionRequirements) && this.ProtectionLevel != ProtectionLevel.None)
            {
                ChannelProtectionRequirements innerRequirements = context.GetInnerProperty<ChannelProtectionRequirements>();
                if (innerRequirements == null)
                {
                    return (T)(object)ContextMessageHeader.GetChannelProtectionRequirements(this.ProtectionLevel);
                }
                else
                {
                    ChannelProtectionRequirements requirements = new ChannelProtectionRequirements(innerRequirements);
                    requirements.Add(ContextMessageHeader.GetChannelProtectionRequirements(this.ProtectionLevel));
                    return (T)(object)requirements;
                }
            }
            else if (typeof(T) == typeof(IContextSessionProvider))
            {
                return (T)(object)this;
            }
            else if (typeof(T) == typeof(IContextBindingElement))
            {
                return (T)(object)this;
            }
            else if (typeof(T) == typeof(ICorrelationDataSource))
            {
                ICorrelationDataSource correlationData = instanceCorrelationData;

                if (correlationData == null)
                {
                    ICorrelationDataSource innerCorrelationData = context.GetInnerProperty<ICorrelationDataSource>();
                    correlationData = CorrelationDataSourceHelper.Combine(innerCorrelationData, ContextExchangeCorrelationDataDescription.DataSource);
                    instanceCorrelationData = correlationData;
                }

                return (T)(object)correlationData;
            }

            return context.GetInnerProperty<T>();
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }

            ContextBindingElement other = b as ContextBindingElement;
            if (other == null)
            {
                return false;
            }

            if (this.ClientCallbackAddress != other.ClientCallbackAddress)
            {
                return false;
            }

            if (this.ContextExchangeMechanism != other.ContextExchangeMechanism)
            {
                return false;
            }

            if (this.ContextManagementEnabled != other.ContextManagementEnabled)
            {
                return false;
            }

            if (this.ProtectionLevel != other.protectionLevel)
            {
                return false;
            }

            return true;
        }

        void IWmiInstanceProvider.FillInstance(IWmiInstance wmiInstance)
        {
            wmiInstance.SetProperty("ProtectionLevel", this.protectionLevel.ToString());
            wmiInstance.SetProperty("ContextExchangeMechanism", this.contextExchangeMechanism.ToString());
            wmiInstance.SetProperty("ContextManagementEnabled", this.contextManagementEnabled);
        }

        string IWmiInstanceProvider.GetInstanceType()
        {
            return "ContextBindingElement";
        }

        internal static void ValidateContextBindingElementOnAllEndpointsWithSessionfulContract(ServiceDescription description, IServiceBehavior callingBehavior)
        {
            if (description == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("description");
            }
            if (callingBehavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("callingBehavior");
            }

            BindingParameterCollection bpc = new BindingParameterCollection();
            foreach (ServiceEndpoint endpoint in description.Endpoints)
            {
                if (endpoint.Binding != null
                    && endpoint.Contract != null
                    && !endpoint.InternalIsSystemEndpoint(description)
                    && endpoint.Contract.SessionMode != SessionMode.NotAllowed)
                {
                    if (endpoint.Binding.GetProperty<IContextBindingElement>(bpc) == null)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(SR.GetString(
                            SR.BehaviorRequiresContextProtocolSupportInBinding,
                            callingBehavior.GetType().Name, endpoint.Name, endpoint.ListenUri.ToString())));
                    }
                }
            }
        }

        void EnsureContextExchangeMechanismCompatibleWithScheme(BindingContext context)
        {
            if (context.Binding != null
                && this.contextExchangeMechanism == ContextExchangeMechanism.HttpCookie
                && !"http".Equals(context.Binding.Scheme, StringComparison.OrdinalIgnoreCase)
                && !"https".Equals(context.Binding.Scheme, StringComparison.OrdinalIgnoreCase))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                    new InvalidOperationException(
                    SR.GetString(
                    SR.HttpCookieContextExchangeMechanismNotCompatibleWithTransportType,
                    context.Binding.Scheme, context.Binding.Namespace, context.Binding.Name)));
            }
        }

        void EnsureContextExchangeMechanismCompatibleWithTransportCookieSetting(BindingContext context)
        {
            if (context.Binding != null && this.contextExchangeMechanism == ContextExchangeMechanism.HttpCookie)
            {
                foreach (BindingElement bindingElement in context.Binding.Elements)
                {
                    HttpTransportBindingElement http = bindingElement as HttpTransportBindingElement;
                    if (http != null && http.AllowCookies)
                    {
                        throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(
                            new InvalidOperationException(
                            SR.GetString(
                            SR.HttpCookieContextExchangeMechanismNotCompatibleWithTransportCookieSetting,
                            context.Binding.Namespace, context.Binding.Name)));
                    }
                }
            }
        }

        class ContextExchangeCorrelationDataDescription : CorrelationDataDescription
        {
            static CorrelationDataSourceHelper cachedCorrelationDataSource;

            ContextExchangeCorrelationDataDescription()
            {
            }

            public static ICorrelationDataSource DataSource
            {
                get
                {
                    if (cachedCorrelationDataSource == null)
                    {
                        cachedCorrelationDataSource = new CorrelationDataSourceHelper(
                            new CorrelationDataDescription[] { new ContextExchangeCorrelationDataDescription() });
                    }

                    return cachedCorrelationDataSource;
                }
            }

            public override bool IsOptional
            {
                get { return true; }
            }

            public override bool IsDefault
            {
                get { return true; }
            }

            public override bool KnownBeforeSend
            {
                get { return true; }
            }

            public override string Name
            {
                get { return ContextExchangeCorrelationHelper.CorrelationName; }
            }

            public override bool ReceiveValue
            {
                get { return true; }
            }

            public override bool SendValue
            {
                get { return true; }
            }
        }
    }
}

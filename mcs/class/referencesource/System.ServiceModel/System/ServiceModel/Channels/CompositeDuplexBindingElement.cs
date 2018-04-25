//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime.Serialization;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Xml;


    public sealed class CompositeDuplexBindingElement : BindingElement, IPolicyExportExtension
    {
        Uri clientBaseAddress;

        public CompositeDuplexBindingElement()
        {
        }

        CompositeDuplexBindingElement(CompositeDuplexBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.clientBaseAddress = elementToBeCloned.ClientBaseAddress;
        }

        [DefaultValue(null)]
        public Uri ClientBaseAddress
        {
            get
            {
                return this.clientBaseAddress;
            }

            set
            {
                this.clientBaseAddress = value;
            }
        }

        public override BindingElement Clone()
        {
            return new CompositeDuplexBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IOutputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel",
                    SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(TChannel) != typeof(IInputChannel))
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgument("TChannel",
                    SR.GetString(SR.ChannelTypeNotSupported, typeof(TChannel)));
            }

            if (context.ListenUriBaseAddress == null)
            {
                if (this.clientBaseAddress != null)
                {
                    context.ListenUriBaseAddress = this.clientBaseAddress;
                    context.ListenUriRelativeAddress = Guid.NewGuid().ToString();
                    context.ListenUriMode = ListenUriMode.Explicit;
                }
                else
                {
                    // 
#pragma warning suppress 56506 // Microsoft, context.Binding will never be null.
                    context.ListenUriRelativeAddress = String.Empty;
                    context.ListenUriMode = ListenUriMode.Unique;
                }
            }

            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            return (typeof(TChannel) == typeof(IOutputChannel))
                && context.CanBuildInnerChannelFactory<IOutputChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            return (typeof(TChannel) == typeof(IInputChannel))
                && context.CanBuildInnerChannelListener<IInputChannel>();
        }

        ChannelProtectionRequirements GetProtectionRequirements()
        {
            ChannelProtectionRequirements result = new ChannelProtectionRequirements();
            XmlQualifiedName refPropHeaderName = new XmlQualifiedName(XD.UtilityDictionary.UniqueEndpointHeaderName.Value,
                    XD.UtilityDictionary.UniqueEndpointHeaderNamespace.Value);
            MessagePartSpecification headerParts = new MessagePartSpecification(refPropHeaderName);
            headerParts.MakeReadOnly();
            result.IncomingSignatureParts.AddParts(headerParts);
            result.OutgoingSignatureParts.AddParts(headerParts);
            return result;
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }
            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                ISecurityCapabilities lowerCapabilities = context.GetInnerProperty<ISecurityCapabilities>();
                if (lowerCapabilities != null)
                {
                    // composite duplex cannot ensure that messages it receives are from the part it sends
                    // messages to. So it cannot offer server auth
                    return (T)(object)(new SecurityCapabilities(lowerCapabilities.SupportsClientAuthentication,
                        false, lowerCapabilities.SupportsClientWindowsIdentity, lowerCapabilities.SupportedRequestProtectionLevel,
                        System.Net.Security.ProtectionLevel.None));
                }
                else
                {
                    return null;
                }
            }
            else if (typeof(T) == typeof(ChannelProtectionRequirements))
            {
                ChannelProtectionRequirements myRequirements = this.GetProtectionRequirements();
                myRequirements.Add(context.GetInnerProperty<ChannelProtectionRequirements>() ?? new ChannelProtectionRequirements());
                return (T)(object)myRequirements;
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }

            CompositeDuplexBindingElement duplex = b as CompositeDuplexBindingElement;
            if (duplex == null)
            {
                return false;
            }

            return (this.clientBaseAddress == duplex.clientBaseAddress);
        }

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (context == null)
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");

            exporter.State[typeof(SupportedAddressingMode).Name] = SupportedAddressingMode.NonAnonymous;
            context.GetBindingAssertions().Add(CreateCompositeDuplexAssertion());
        }

        static XmlElement CreateCompositeDuplexAssertion()
        {
            XmlDocument doc = new XmlDocument();
            return doc.CreateElement(TransportPolicyConstants.CompositeDuplexPrefix, TransportPolicyConstants.CompositeDuplex, TransportPolicyConstants.CompositeDuplexNamespace);
        }
    }
}

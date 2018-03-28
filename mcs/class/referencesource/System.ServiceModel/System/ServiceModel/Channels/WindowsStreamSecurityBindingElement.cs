//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.ServiceModel.Channels
{
    using System.Collections.Generic;
    using System.ServiceModel.Description;
    using System.Net.Security;
    using System.ServiceModel;
    using System.ServiceModel.Security;
    using System.ComponentModel;

    using System.Xml;

    public class WindowsStreamSecurityBindingElement : StreamUpgradeBindingElement,
        ITransportTokenAssertionProvider,
        IPolicyExportExtension
    {
        ProtectionLevel protectionLevel;

        public WindowsStreamSecurityBindingElement()
            : base()
        {
            this.protectionLevel = ConnectionOrientedTransportDefaults.ProtectionLevel;
        }

        protected WindowsStreamSecurityBindingElement(WindowsStreamSecurityBindingElement elementToBeCloned)
            : base(elementToBeCloned)
        {
            this.protectionLevel = elementToBeCloned.protectionLevel;
        }

        [DefaultValue(ConnectionOrientedTransportDefaults.ProtectionLevel)]
        public ProtectionLevel ProtectionLevel
        {
            get
            {
                return this.protectionLevel;
            }
            set
            {
                ProtectionLevelHelper.Validate(value);
                this.protectionLevel = value;
            }
        }

        public override BindingElement Clone()
        {
            return new WindowsStreamSecurityBindingElement(this);
        }

        public override IChannelFactory<TChannel> BuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelFactory<TChannel>();
        }

        public override bool CanBuildChannelFactory<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelFactory<TChannel>();
        }

        public override IChannelListener<TChannel> BuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.BuildInnerChannelListener<TChannel>();
        }

        public override bool CanBuildChannelListener<TChannel>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

#pragma warning suppress 56506 // Microsoft, BindingContext.BindingParameters cannot be null
            context.BindingParameters.Add(this);
            return context.CanBuildInnerChannelListener<TChannel>();
        }

        public override StreamUpgradeProvider BuildClientStreamUpgradeProvider(BindingContext context)
        {
            return new WindowsStreamSecurityUpgradeProvider(this, context, true);
        }

        public override StreamUpgradeProvider BuildServerStreamUpgradeProvider(BindingContext context)
        {
            return new WindowsStreamSecurityUpgradeProvider(this, context, false);
        }

        public override T GetProperty<T>(BindingContext context)
        {
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            if (typeof(T) == typeof(ISecurityCapabilities))
            {
                return (T)(object)new SecurityCapabilities(true, true, true, protectionLevel, protectionLevel);
            }
            else if (typeof(T) == typeof(IdentityVerifier))
            {
                return (T)(object)IdentityVerifier.CreateDefault();
            }
            else
            {
                return context.GetInnerProperty<T>();
            }
        }

        internal static void ImportPolicy(MetadataImporter importer, PolicyConversionContext policyContext)
        {
            XmlElement assertion = PolicyConversionContext.FindAssertion(policyContext.GetBindingAssertions(),
                TransportPolicyConstants.WindowsTransportSecurityName, TransportPolicyConstants.DotNetFramingNamespace, true);

            if (assertion != null)
            {
                WindowsStreamSecurityBindingElement windowsBindingElement
                    = new WindowsStreamSecurityBindingElement();

                XmlReader reader = new XmlNodeReader(assertion);
                reader.ReadStartElement();
                string protectionLevelString = null;
                if (reader.IsStartElement(
                    TransportPolicyConstants.ProtectionLevelName,
                    TransportPolicyConstants.DotNetFramingNamespace) && !reader.IsEmptyElement)
                {
                    protectionLevelString = reader.ReadElementContentAsString();
                }
                if (string.IsNullOrEmpty(protectionLevelString))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new XmlException(
                        SR.GetString(SR.ExpectedElementMissing, TransportPolicyConstants.ProtectionLevelName, TransportPolicyConstants.DotNetFramingNamespace)));
                }
                windowsBindingElement.ProtectionLevel = (ProtectionLevel)Enum.Parse(typeof(ProtectionLevel), protectionLevelString);
                policyContext.BindingElements.Add(windowsBindingElement);
            }
        }

        #region ITransportTokenAssertionProvider Members

        public XmlElement GetTransportTokenAssertion()
        {
            XmlDocument document = new XmlDocument();
            XmlElement assertion =
                document.CreateElement(TransportPolicyConstants.DotNetFramingPrefix,
                TransportPolicyConstants.WindowsTransportSecurityName,
                TransportPolicyConstants.DotNetFramingNamespace);
            XmlElement protectionLevelElement = document.CreateElement(TransportPolicyConstants.DotNetFramingPrefix,
                TransportPolicyConstants.ProtectionLevelName, TransportPolicyConstants.DotNetFramingNamespace);
            protectionLevelElement.AppendChild(document.CreateTextNode(this.ProtectionLevel.ToString()));
            assertion.AppendChild(protectionLevelElement);
            return assertion;
        }

        #endregion

        void IPolicyExportExtension.ExportPolicy(MetadataExporter exporter, PolicyConversionContext context)
        {
            if (exporter == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("exporter");
            }
            if (context == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("context");
            }

            SecurityBindingElement.ExportPolicyForTransportTokenAssertionProviders(exporter, context);
        }

        internal override bool IsMatch(BindingElement b)
        {
            if (b == null)
            {
                return false;
            }
            WindowsStreamSecurityBindingElement security = b as WindowsStreamSecurityBindingElement;
            if (security == null)
            {
                return false;
            }
            if (this.protectionLevel != security.protectionLevel)
            {
                return false;
            }

            return true;
        }
    }
}

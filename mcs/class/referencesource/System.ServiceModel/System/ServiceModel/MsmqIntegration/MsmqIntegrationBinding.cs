//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------
namespace System.ServiceModel.MsmqIntegration
{
    using System;
    using System.ComponentModel;
    using System.ServiceModel;
    using System.Text;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Configuration;
    using System.Globalization;
    using System.Net;
    using System.Net.Security;
    using System.Runtime.Serialization;
    using System.Security.Principal;
    using System.ServiceModel.Channels;
    using Config = System.ServiceModel.Configuration;
    using System.ServiceModel.Security;

    using System.Xml;

    public class MsmqIntegrationBinding : MsmqBindingBase
    {
        // private BindingElements
        MsmqIntegrationSecurity security = new MsmqIntegrationSecurity();

        public MsmqIntegrationBinding()
        {
            Initialize();
        }

        public MsmqIntegrationBinding(string configurationName)
        {
            Initialize();
            ApplyConfiguration(configurationName);
        }

        public MsmqIntegrationBinding(MsmqIntegrationSecurityMode securityMode)
        {
            if (!MsmqIntegrationSecurityModeHelper.IsDefined(securityMode))
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new InvalidEnumArgumentException("securityMode", (int)securityMode, typeof(MsmqIntegrationSecurityMode)));
            Initialize();
            this.security.Mode = securityMode;
        }

        public MsmqIntegrationSecurity Security
        {
            get { return this.security; }
            set { this.security = value; }
        }

        internal Type[] TargetSerializationTypes
        {
            get
            {
                return (transport as MsmqIntegrationBindingElement).TargetSerializationTypes;
            }
            set
            {
                (transport as MsmqIntegrationBindingElement).TargetSerializationTypes = value;
            }
        }

        [DefaultValue(MsmqIntegrationDefaults.SerializationFormat)]
        public MsmqMessageSerializationFormat SerializationFormat
        {
            get { return (transport as MsmqIntegrationBindingElement).SerializationFormat; }
            set { (transport as MsmqIntegrationBindingElement).SerializationFormat = value; }
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public bool ShouldSerializeSecurity()
        {
            if (this.security.Mode != MsmqIntegrationSecurityMode.Transport)
            {
                return true;
            }

            if (this.security.Transport.MsmqAuthenticationMode != MsmqDefaults.MsmqAuthenticationMode ||
               this.security.Transport.MsmqEncryptionAlgorithm != MsmqDefaults.MsmqEncryptionAlgorithm ||
               this.security.Transport.MsmqSecureHashAlgorithm != MsmqDefaults.MsmqSecureHashAlgorithm ||
               this.security.Transport.MsmqProtectionLevel != MsmqDefaults.MsmqProtectionLevel)
            {
                return true;
            }
            return false;
        }

        void Initialize()
        {
            transport = new MsmqIntegrationBindingElement();
        }

        void ApplyConfiguration(string configurationName)
        {
            Config.MsmqIntegrationBindingCollectionElement section = Config.MsmqIntegrationBindingCollectionElement.GetBindingCollectionElement();
            Config.MsmqIntegrationBindingElement element = section.Bindings[configurationName];
            if (element == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                    SR.GetString(SR.ConfigInvalidBindingConfigurationName,
                                 configurationName,
                                 Config.ConfigurationStrings.MsmqIntegrationBindingCollectionElementName)));
            }
            else
            {
                element.ApplyConfiguration(this);
            }
        }

        public override BindingElementCollection CreateBindingElements()
        {   // return collection of BindingElements
            BindingElementCollection bindingElements = new BindingElementCollection();
            // order of BindingElements is important
            // add transport
            this.security.ConfigureTransportSecurity(transport);
            bindingElements.Add(transport);

            return bindingElements.Clone();
        }
    }
}

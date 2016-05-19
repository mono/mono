//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.ServiceModel;
    using System.Globalization;
    using System.Net.Security;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.Reflection;
    using System.Runtime.InteropServices;

    public partial class ClientCredentialsElement : BehaviorExtensionElement
    {
        public ClientCredentialsElement()
        {
        }

        [ConfigurationProperty(ConfigurationStrings.Type, DefaultValue = "")]
        [StringValidator(MinLength = 0)]
        public string Type
        {
            get { return (string)base[ConfigurationStrings.Type]; }
            set
            {
                if (String.IsNullOrEmpty(value))
                {
                    value = String.Empty;
                }
                base[ConfigurationStrings.Type] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.UseIdentityConfiguration, DefaultValue = false)]
        public bool UseIdentityConfiguration
        {
            get
            {
                return (bool)base[ConfigurationStrings.UseIdentityConfiguration];
            }
            set
            {
                base[ConfigurationStrings.UseIdentityConfiguration] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.ClientCertificate)]
        public X509InitiatorCertificateClientElement ClientCertificate
        {
            get { return (X509InitiatorCertificateClientElement)base[ConfigurationStrings.ClientCertificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceCertificate)]
        public X509RecipientCertificateClientElement ServiceCertificate
        {
            get { return (X509RecipientCertificateClientElement)base[ConfigurationStrings.ServiceCertificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Windows)]
        public WindowsClientElement Windows
        {
            get { return (WindowsClientElement)base[ConfigurationStrings.Windows]; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuedToken)]
        public IssuedTokenClientElement IssuedToken
        {
            get { return (IssuedTokenClientElement)base[ConfigurationStrings.IssuedToken]; }
        }

        [ConfigurationProperty(ConfigurationStrings.HttpDigest)]
        public HttpDigestClientElement HttpDigest
        {
            get { return (HttpDigestClientElement)base[ConfigurationStrings.HttpDigest]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Peer)]
        public PeerCredentialElement Peer
        {
            get { return (PeerCredentialElement)base[ConfigurationStrings.Peer]; }
        }

        [ConfigurationProperty(ConfigurationStrings.SupportInteractive, DefaultValue = ClientCredentials.SupportInteractiveDefault)]
        public bool SupportInteractive
        {
            get { return (bool)base[ConfigurationStrings.SupportInteractive]; }
            set { base[ConfigurationStrings.SupportInteractive] = value; }
        }

        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ClientCredentialsElement source = (ClientCredentialsElement)from;
#pragma warning suppress 56506 //[....]; base.CopyFrom() checks for 'from' being null
            this.ClientCertificate.Copy(source.ClientCertificate);
            this.ServiceCertificate.Copy(source.ServiceCertificate);
            this.Windows.Copy(source.Windows);
            this.IssuedToken.Copy(source.IssuedToken);
            this.HttpDigest.Copy(source.HttpDigest);
            this.Peer.Copy(source.Peer);
            this.SupportInteractive = source.SupportInteractive;
            this.Type = source.Type;
            this.UseIdentityConfiguration = source.UseIdentityConfiguration;
        }

        protected internal override object CreateBehavior()
        {
            ClientCredentials behavior;
            if (string.IsNullOrEmpty(this.Type))
            {
                behavior = new ClientCredentials();
            }
            else
            {
                Type credentialsType = System.Type.GetType(this.Type, true);
                if (!typeof(ClientCredentials).IsAssignableFrom(credentialsType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigInvalidClientCredentialsType, this.Type, credentialsType.AssemblyQualifiedName)));
                }
                behavior = (ClientCredentials) Activator.CreateInstance(credentialsType);
            }
            ApplyConfiguration(behavior);

            return behavior;
        }

        protected internal void ApplyConfiguration( ClientCredentials behavior )
        {
            if (null == behavior)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("behavior");
            }

            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.Windows].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Windows.ApplyConfiguration(behavior.Windows);
            }
            if (propertyInfo[ConfigurationStrings.ClientCertificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ClientCertificate.ApplyConfiguration(behavior.ClientCertificate);
            }
            if (propertyInfo[ConfigurationStrings.ServiceCertificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ServiceCertificate.ApplyConfiguration(behavior.ServiceCertificate);
            }
            if (propertyInfo[ConfigurationStrings.IssuedToken].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.IssuedToken.ApplyConfiguration(behavior.IssuedToken);
            }
            if (propertyInfo[ConfigurationStrings.HttpDigest].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.HttpDigest.ApplyConfiguration(behavior.HttpDigest);
            }
            if (propertyInfo[ConfigurationStrings.Peer].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Peer.ApplyConfiguration(behavior.Peer);
            }
            if ( propertyInfo[ConfigurationStrings.UseIdentityConfiguration].ValueOrigin != PropertyValueOrigin.Default )
            {
                behavior.UseIdentityConfiguration = this.UseIdentityConfiguration;
            }

            behavior.SupportInteractive = this.SupportInteractive;
        }

        public override Type BehaviorType
        {
            get { return typeof(ClientCredentials); }
        }
    }
}




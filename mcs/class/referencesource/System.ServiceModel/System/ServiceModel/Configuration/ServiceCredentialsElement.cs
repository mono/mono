//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.ServiceModel.Configuration
{
    using System.Configuration;
    using System.Globalization;
    using System.IdentityModel.Configuration;
    using System.Net.Security;
    using System.Security.Cryptography.X509Certificates;
    using System.ServiceModel;
    using System.ServiceModel.Description;
    using System.ServiceModel.Security;

    public partial class ServiceCredentialsElement : BehaviorExtensionElement
    {
        public ServiceCredentialsElement()
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

        [ConfigurationProperty(ConfigurationStrings.ClientCertificate)]
        public X509InitiatorCertificateServiceElement ClientCertificate
        {
            get { return (X509InitiatorCertificateServiceElement)base[ConfigurationStrings.ClientCertificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.ServiceCertificate)]
        public X509RecipientCertificateServiceElement ServiceCertificate
        {
            get { return (X509RecipientCertificateServiceElement)base[ConfigurationStrings.ServiceCertificate]; }
        }

        [ConfigurationProperty(ConfigurationStrings.UserNameAuthentication)]
        public UserNameServiceElement UserNameAuthentication
        {
            get { return (UserNameServiceElement)base[ConfigurationStrings.UserNameAuthentication]; }
        }

        [ConfigurationProperty( ConfigurationStrings.UseIdentityConfiguration, DefaultValue = false, IsRequired = false )]
        public bool UseIdentityConfiguration
        {
            get { return (bool)base[ConfigurationStrings.UseIdentityConfiguration]; }
            set { base[ConfigurationStrings.UseIdentityConfiguration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.IdentityConfiguration, IsRequired = false, DefaultValue = System.IdentityModel.Configuration.ConfigurationStrings.DefaultServiceName)]
        [StringValidator(MinLength = 0)]
        public string IdentityConfiguration
        {
            get { return (string)base[ConfigurationStrings.IdentityConfiguration]; }
            set { base[ConfigurationStrings.IdentityConfiguration] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.WindowsAuthentication)]
        public WindowsServiceElement WindowsAuthentication
        {
            get { return (WindowsServiceElement)base[ConfigurationStrings.WindowsAuthentication]; }
        }

        [ConfigurationProperty(ConfigurationStrings.Peer)]
        public PeerCredentialElement Peer
        {
            get { return (PeerCredentialElement)base[ConfigurationStrings.Peer]; }
        }

        [ConfigurationProperty(ConfigurationStrings.IssuedTokenAuthentication)]
        public IssuedTokenServiceElement IssuedTokenAuthentication
        {
            get { return (IssuedTokenServiceElement)base[ConfigurationStrings.IssuedTokenAuthentication]; }
        }

        [ConfigurationProperty(ConfigurationStrings.SecureConversationAuthentication)]
        public SecureConversationServiceElement SecureConversationAuthentication
        {
            get { return (SecureConversationServiceElement)base[ConfigurationStrings.SecureConversationAuthentication]; }
        }



        public override void CopyFrom(ServiceModelExtensionElement from)
        {
            base.CopyFrom(from);

            ServiceCredentialsElement source = (ServiceCredentialsElement)from;
#pragma warning suppress 56506 //Microsoft; base.CopyFrom() checks for 'from' being null
            this.ClientCertificate.Copy(source.ClientCertificate);
            this.ServiceCertificate.Copy(source.ServiceCertificate);
            this.UserNameAuthentication.Copy(source.UserNameAuthentication);
            this.WindowsAuthentication.Copy(source.WindowsAuthentication);
            this.Peer.Copy(source.Peer);
            this.IssuedTokenAuthentication.Copy(source.IssuedTokenAuthentication);
            this.SecureConversationAuthentication.Copy(source.SecureConversationAuthentication);
            this.Type = source.Type;
            this.UseIdentityConfiguration = source.UseIdentityConfiguration;
            this.IdentityConfiguration = source.IdentityConfiguration;
        }

        protected internal override object CreateBehavior()
        {
            ServiceCredentials behavior;
            if (string.IsNullOrEmpty(this.Type))
            {
                behavior = new ServiceCredentials();
            }
            else
            {
                Type credentialsType = System.Type.GetType(this.Type, true);
                if (!typeof(ServiceCredentials).IsAssignableFrom(credentialsType))
                {
                    throw DiagnosticUtility.ExceptionUtility.ThrowHelperError(new ConfigurationErrorsException(
                        SR.GetString(SR.ConfigInvalidServiceCredentialsType, this.Type, credentialsType.AssemblyQualifiedName)));
                }
                behavior = (ServiceCredentials)Activator.CreateInstance(credentialsType);
            }
            ApplyConfiguration(behavior);

            return behavior;
        }

        protected internal void ApplyConfiguration(ServiceCredentials behavior)
        {
            if (behavior == null)
            {
                throw DiagnosticUtility.ExceptionUtility.ThrowHelperArgumentNull("behavior");
            }

            PropertyInformationCollection propertyInfo = this.ElementInformation.Properties;
            if (propertyInfo[ConfigurationStrings.UserNameAuthentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.UserNameAuthentication.ApplyConfiguration(behavior.UserNameAuthentication);
            }
            if (propertyInfo[ConfigurationStrings.WindowsAuthentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.WindowsAuthentication.ApplyConfiguration(behavior.WindowsAuthentication);
            }
            if (propertyInfo[ConfigurationStrings.ClientCertificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ClientCertificate.ApplyConfiguration(behavior.ClientCertificate);
            }
            if (propertyInfo[ConfigurationStrings.ServiceCertificate].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.ServiceCertificate.ApplyConfiguration(behavior.ServiceCertificate);
            }
            if (propertyInfo[ConfigurationStrings.Peer].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.Peer.ApplyConfiguration(behavior.Peer);
            }
            if (propertyInfo[ConfigurationStrings.IssuedTokenAuthentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.IssuedTokenAuthentication.ApplyConfiguration(behavior.IssuedTokenAuthentication);
            }
            if (propertyInfo[ConfigurationStrings.SecureConversationAuthentication].ValueOrigin != PropertyValueOrigin.Default)
            {
                this.SecureConversationAuthentication.ApplyConfiguration(behavior.SecureConversationAuthentication);
            }
            if (propertyInfo[ConfigurationStrings.UseIdentityConfiguration].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.UseIdentityConfiguration = this.UseIdentityConfiguration;
            }
            if (propertyInfo[ConfigurationStrings.IdentityConfiguration].ValueOrigin != PropertyValueOrigin.Default)
            {
                behavior.IdentityConfiguration = new IdentityConfiguration( IdentityConfiguration );
            }            
        }

        public override Type BehaviorType
        {
            get { return typeof(ServiceCredentials); }
        }
    }
}




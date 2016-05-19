//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System.Configuration;
    using System.IdentityModel.Tokens;
    
#pragma warning disable 1591
    [ConfigurationCollection( typeof( CustomTypeElement ), CollectionType = ConfigurationElementCollectionType.BasicMap )]
    public sealed partial class SecurityTokenHandlerElementCollection : ConfigurationElementCollection
    {
        public SecurityTokenHandlerElementCollection()
        {
        }

        protected override ConfigurationElement CreateNewElement()
        {
            return new CustomTypeElement();
        }

        protected override object GetElementKey( ConfigurationElement element )
        {
            return ( (CustomTypeElement)element ).Type;
        }

        protected override void Init()
        {
            //
            // Keep this list synchronized with SecurityTokenHandlerCollection.CreateDefaultSecurityTokenHandlerCollection
            //
            BaseAdd( new CustomTypeElement( typeof( SamlSecurityTokenHandler ) ) );
            BaseAdd( new CustomTypeElement( typeof( Saml2SecurityTokenHandler ) ) );
            BaseAdd( new CustomTypeElement( typeof( WindowsUserNameSecurityTokenHandler ) ) );
            BaseAdd( new CustomTypeElement( typeof( X509SecurityTokenHandler ) ) );
            BaseAdd( new CustomTypeElement( typeof( KerberosSecurityTokenHandler ) ) );
            BaseAdd( new CustomTypeElement( typeof( RsaSecurityTokenHandler ) ) );
            BaseAdd( new CustomTypeElement( typeof( SessionSecurityTokenHandler ) ) );
            BaseAdd( new CustomTypeElement( typeof( EncryptedSecurityTokenHandler ) ) );
            
        }

        [ConfigurationProperty( ConfigurationStrings.Name, Options = ConfigurationPropertyOptions.IsKey )]
        [StringValidator(MinLength = 0)]
        public string Name
        {
            get { return (string)this[ConfigurationStrings.Name]; }
            set { this[ConfigurationStrings.Name] = value; }
        }

        [ConfigurationProperty( ConfigurationStrings.SecurityTokenHandlerConfiguration, IsRequired = false )]
        public SecurityTokenHandlerConfigurationElement SecurityTokenHandlerConfiguration
        {
            get { return (SecurityTokenHandlerConfigurationElement)this[ConfigurationStrings.SecurityTokenHandlerConfiguration]; }
            set { this[ConfigurationStrings.SecurityTokenHandlerConfiguration] = value; }
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return ( ( ElementInformation.Properties[ConfigurationStrings.Name].ValueOrigin != PropertyValueOrigin.Default ) ||
                         SecurityTokenHandlerConfiguration.IsConfigured ||
                         Count > 0 );
            }
        }
    }

#pragma warning restore 1591
}

//------------------------------------------------------------------------------
// <copyright file="ProtectedConfigurationSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System.Collections;
    using System.Collections.Specialized;
    using System.Xml;
    using System.Globalization;
    using System.Security.Permissions;
    using System.Diagnostics.CodeAnalysis;

    public sealed class ProtectedConfigurationSection : ConfigurationSection
    {
        internal ProtectedConfigurationProvider GetProviderFromName(string providerName)
        {
            ProviderSettings ps = Providers[providerName];

            if (ps == null) {
                throw new Exception(SR.GetString(SR.ProtectedConfigurationProvider_not_found, providerName));
            }

            return InstantiateProvider(ps);
        }

        internal ProtectedConfigurationProviderCollection GetAllProviders()
        {
            ProtectedConfigurationProviderCollection coll = new ProtectedConfigurationProviderCollection();
            foreach(ProviderSettings ps in Providers)
            {
                coll.Add(InstantiateProvider(ps));
            }
            return coll;
        }

        [PermissionSet(SecurityAction.Assert, Unrestricted=true)]
        [SuppressMessage("Microsoft.Security", "CA2106:SecureAsserts", Justification = "This assert is potentially dangerous and shouldn't be present but is necessary for back-compat.")]
        private ProtectedConfigurationProvider CreateAndInitializeProviderWithAssert(Type t, ProviderSettings pn) {
            ProtectedConfigurationProvider provider = (ProtectedConfigurationProvider)TypeUtil.CreateInstanceWithReflectionPermission(t);
            NameValueCollection pars = pn.Parameters;
            NameValueCollection cloneParams = new NameValueCollection(pars.Count);

            foreach (string key in pars) {
                cloneParams[key] = pars[key];
            }

            provider.Initialize(pn.Name, cloneParams);
            return provider;
        }

        private ProtectedConfigurationProvider InstantiateProvider(ProviderSettings pn)
        {
            Type t = TypeUtil.GetTypeWithReflectionPermission(pn.Type, true);
            if (!typeof(ProtectedConfigurationProvider).IsAssignableFrom(t)) {
                throw new Exception(SR.GetString(SR.WrongType_of_Protected_provider));
            }

            // Needs to check APTCA bit.  See VSWhidbey 429996.
            if (!TypeUtil.IsTypeAllowedInConfig(t)) {
                throw new Exception(SR.GetString(SR.Type_from_untrusted_assembly, t.FullName));
            }

            // Needs to check Assert Fulltrust in order for runtime to work.  See VSWhidbey 429996.
            return CreateAndInitializeProviderWithAssert(t, pn);
        }

        internal static string DecryptSection(string encryptedXml, ProtectedConfigurationProvider provider) {
            XmlDocument doc = new XmlDocument();
            doc.LoadXml(encryptedXml);
            XmlNode resultNode = provider.Decrypt(doc.DocumentElement);
            return resultNode.OuterXml;
        }

        private const string    EncryptedSectionTemplate = "<{0} {1}=\"{2}\"> {3} </{0}>";

        internal static string FormatEncryptedSection(string encryptedXml, string sectionName, string providerName) {
            return String.Format(CultureInfo.InvariantCulture, EncryptedSectionTemplate,
                        sectionName,    // The section to encrypt
                        BaseConfigurationRecord.KEYWORD_PROTECTION_PROVIDER, // protectionProvider keyword
                        providerName,  // The provider name
                        encryptedXml   // the encrypted xml
                        );
        }
        
        internal static string EncryptSection(string clearXml, ProtectedConfigurationProvider provider) {
            XmlDocument xmlDocument = new XmlDocument();
            xmlDocument.PreserveWhitespace = true;
            xmlDocument.LoadXml(clearXml);
            string sectionName = xmlDocument.DocumentElement.Name;
            XmlNode encNode = provider.Encrypt(xmlDocument.DocumentElement);
            return encNode.OuterXml;
        }


        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////
        //////////////////////////////////////////////////////////////////////

        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propProviders =
            new ConfigurationProperty("providers", 
                                        typeof(ProtectedProviderSettings), 
                                        new ProtectedProviderSettings(), 
                                        ConfigurationPropertyOptions.None);
        
        private static readonly ConfigurationProperty _propDefaultProvider =
            new ConfigurationProperty("defaultProvider", 
                                        typeof(string), 
                                        "RsaProtectedConfigurationProvider", 
                                        null,
                                        ConfigurationProperty.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.None);

        static ProtectedConfigurationSection()
        {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propProviders);
            _properties.Add(_propDefaultProvider);
        }

        public ProtectedConfigurationSection()
        {
        }

        protected internal override ConfigurationPropertyCollection Properties
        {
            get
            {
                return _properties;
            }
        }

        private ProtectedProviderSettings _Providers
        {
            get
            {
                return (ProtectedProviderSettings)base[_propProviders];
            }
        }

        [ConfigurationProperty("providers")]
        public ProviderSettingsCollection Providers
        {
            get
            {
                return _Providers.Providers;
            }
        }

        [ConfigurationProperty("defaultProvider", DefaultValue = "RsaProtectedConfigurationProvider")]
        public string DefaultProvider
        {
            get
            {
                return (string)base[_propDefaultProvider];
            }
            set
            {
                base[_propDefaultProvider] = value;
            }
        }

    }
}

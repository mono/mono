//------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------

using System.Configuration;
using System.IdentityModel.Tokens;


namespace System.IdentityModel.Configuration
{
    /// <summary>
    /// Class the represents system.identityModel section in the configuration file
    /// </summary>
    public sealed partial class SystemIdentityModelSection : ConfigurationSection
    {
        /// <summary>
        /// The name of the configuration section defined by Windows Identity Foundation.
        /// </summary>
        public const string SectionName = ConfigurationStrings.SystemIdentityModel;

        /// <summary>
        /// Returns the current <see cref="SystemIdentityModelSection"/> reference
        /// </summary>
        public static SystemIdentityModelSection Current
        {
            get
            {
                return ConfigurationManager.GetSection( SectionName ) as SystemIdentityModelSection;
            }
        }

        /// <summary>
        /// Returns the unnamed <see cref="IdentityConfigurationElement"/> reference from the configuration file
        /// </summary>
        public static IdentityConfigurationElement DefaultIdentityConfigurationElement
        {
            get
            {
                SystemIdentityModelSection section = SystemIdentityModelSection.Current;

                return ( section != null ) ? section.IdentityConfigurationElements.GetElement( ConfigurationStrings.DefaultServiceName ) : null;
            }
        }

        /// <summary>
        /// Returns <see cref="IdentityConfigurationElementCollection"/> collection 
        /// </summary>
        [ConfigurationProperty( ConfigurationStrings.DefaultCollectionName, Options = ConfigurationPropertyOptions.IsDefaultCollection )]
        public IdentityConfigurationElementCollection IdentityConfigurationElements
        {
            get { return (IdentityConfigurationElementCollection)this[ConfigurationStrings.DefaultCollectionName]; }
        }

        /// <summary>
        /// Returns a value indicating whether this element has been configured with non-default values.
        /// </summary>
        internal bool IsConfigured
        {
            get
            {
                return IdentityConfigurationElements.IsConfigured;
            }
        }
    }
}

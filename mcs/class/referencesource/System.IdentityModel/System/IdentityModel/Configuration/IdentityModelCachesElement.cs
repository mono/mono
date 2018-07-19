//-----------------------------------------------------------------------
// <copyright file="IdentityModelCaches.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//-----------------------------------------------------------------------

namespace System.IdentityModel.Configuration
{
    using System.ComponentModel;
    using System.Configuration;

    /// <summary>
    /// The <c>IdentityModelCachesElement</c> class provides XML configuration for WIF caching services.
    /// </summary>
    public sealed partial class IdentityModelCachesElement : ConfigurationElement
    {
        /// <summary>
        /// Gets or sets the <see cref="TokenReplayCache"/> type. This element is optional and the specified custom replay cache
        /// must derive from <see cref="System.IdentityModel.Tokens.TokenReplayCache"/>.
        /// To enable replay detection, configure the <see cref="System.IdentityModel.Configuration.TokenReplayDetectionElement"/>.
        /// </summary>
        [ConfigurationProperty(ConfigurationStrings.TokenReplayCache, IsRequired = false)]
        public CustomTypeElement TokenReplayCache
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.TokenReplayCache]; }
            set { this[ConfigurationStrings.TokenReplayCache] = value; }
        }

        /// <summary>
        /// Gets or sets the <see cref="SessionSecurityTokenCache"/> type. This element is optional and the specified custom cache
        /// must derive from <see cref="System.IdentityModel.Tokens.SessionSecurityTokenCache"/>.
        /// This cache is used for caching <see cref="System.IdentityModel.Tokens.SessionSecurityToken" />.
        /// </summary>
        [ConfigurationProperty(ConfigurationStrings.SessionSecurityTokenCache, IsRequired = false)]
        public CustomTypeElement SessionSecurityTokenCache
        {
            get { return (CustomTypeElement)this[ConfigurationStrings.SessionSecurityTokenCache]; }
            set { this[ConfigurationStrings.SessionSecurityTokenCache] = value; }
        }
            
        /// <summary>
        /// Gets a value indicating whether this element has been configured.
        /// </summary>
        public bool IsConfigured
        {
            get
            {
                return this.TokenReplayCache != null || this.SessionSecurityTokenCache != null;
            }
        }        
    }
}

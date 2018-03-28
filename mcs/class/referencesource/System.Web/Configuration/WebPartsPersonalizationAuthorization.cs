//------------------------------------------------------------------------------
// <copyright file="WebPartsPersonalizationAuthorization.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {

    using System;
    using System.Configuration;
    using System.Collections;
    using System.Collections.Specialized;
    using System.Security.Principal;
    using System.Web;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Web.UI;
    using System.Web.UI.WebControls;
    using System.Web.UI.WebControls.WebParts;
    using System.Web.Util;
    using System.Xml;
    using System.Security.Permissions;

    public sealed class WebPartsPersonalizationAuthorization : ConfigurationElement {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propRules =
            new ConfigurationProperty(null, 
                                        typeof(AuthorizationRuleCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.IsDefaultCollection);

        static WebPartsPersonalizationAuthorization() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propRules);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public AuthorizationRuleCollection Rules {
            get {
                return (AuthorizationRuleCollection)base[_propRules];
            }
        }

        internal bool IsUserAllowed(IPrincipal user, String verb) {
            return Rules.IsUserAllowed(user, verb);
        }
    }
}

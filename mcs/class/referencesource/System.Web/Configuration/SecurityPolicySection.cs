//------------------------------------------------------------------------------
// <copyright file="SecurityPolicySection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Security.Permissions;

    /* This data is in a location in Machine.Config... How do I deal with that?  For now I will leave the
       section in machine.config, The initialization of the trust levels are overwritable in this collection.
        <securityPolicy>
                <trustLevel name="Full" policyFile="internal" />
                <trustLevel name="High" policyFile="web_hightrust.config" />
                <trustLevel name="Medium" policyFile="web_mediumtrust.config" />
                <trustLevel name="Low"  policyFile="web_lowtrust.config" />
                <trustLevel name="Minimal" policyFile="web_minimaltrust.config" />
            </securityPolicy>
    */
    public sealed class SecurityPolicySection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propTrustLevels =
            new ConfigurationProperty(null, 
                                        typeof(TrustLevelCollection), 
                                        null, 
                                        ConfigurationPropertyOptions.IsDefaultCollection);

        static SecurityPolicySection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propTrustLevels);
        }

        public SecurityPolicySection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection = true)]
        public TrustLevelCollection TrustLevels {
            get {
                return (TrustLevelCollection)base[_propTrustLevels];
            }
        }
    }
}

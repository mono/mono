//------------------------------------------------------------------------------
// <copyright file="DeploymentSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration
{
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.IO;
    using System.Text;
    using System.Security.Permissions;

    public sealed class DeploymentSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;
        private static readonly ConfigurationProperty _propRetail = 
            new ConfigurationProperty("retail", typeof(bool),false,ConfigurationPropertyOptions.None);

        /*
        <!--
        deployment Attributes:
          retail="[true|false]" - turns on retail deployment mode
        -->
        <deployment
            retail="false"
        />
        */
        static DeploymentSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propRetail);
        }

        public DeploymentSection() {
        }


        protected override ConfigurationPropertyCollection Properties  {
            get {
                return _properties;
            }
        }
         
        [ConfigurationProperty("retail", DefaultValue = false)]
        public bool Retail {
            get {
                return (bool)base[_propRetail];
            }
            set {
                base[_propRetail] = value;
            }
        }

        private static bool s_hasCachedData;
        private static bool s_retail;

        internal static bool RetailInternal {
            get {
                if (!s_hasCachedData) {
                    s_retail = RuntimeConfig.GetAppConfig().Deployment.Retail;
                    s_hasCachedData = true;
                }

                return s_retail;
            }
        }
    }
}

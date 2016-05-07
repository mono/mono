//------------------------------------------------------------------------------
// <copyright file="FullTrustAssembliesSection.cs" company="Microsoft">
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

    public sealed class FullTrustAssembliesSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propFullTrustAssemblies =
            new ConfigurationProperty(null, typeof(FullTrustAssemblyCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static FullTrustAssembliesSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propFullTrustAssemblies);
        }

        public FullTrustAssembliesSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public FullTrustAssemblyCollection FullTrustAssemblies {
            get {
                return GetFullTrustAssembliesCollection();
            }
        }

        private FullTrustAssemblyCollection GetFullTrustAssembliesCollection() {
            return (FullTrustAssemblyCollection)base[_propFullTrustAssemblies];
        }
    }
}

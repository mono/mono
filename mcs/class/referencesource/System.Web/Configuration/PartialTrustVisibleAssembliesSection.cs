//------------------------------------------------------------------------------
// <copyright file="PartialTrustVisibleAssembliesSection.cs" company="Microsoft">
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

    public sealed class PartialTrustVisibleAssembliesSection : ConfigurationSection
    {
        private static ConfigurationPropertyCollection _properties;

        private static readonly ConfigurationProperty _propPartialTrustVisibleAssemblies =
            new ConfigurationProperty(null, typeof(PartialTrustVisibleAssemblyCollection), null, ConfigurationPropertyOptions.IsDefaultCollection);

        static PartialTrustVisibleAssembliesSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propPartialTrustVisibleAssemblies);
        }

        public PartialTrustVisibleAssembliesSection() {
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("", IsDefaultCollection=true)]
        public PartialTrustVisibleAssemblyCollection PartialTrustVisibleAssemblies {
            get {
                return GetPartialTrustVisibleAssembliesCollection();
            }
        }

        private PartialTrustVisibleAssemblyCollection GetPartialTrustVisibleAssembliesCollection() {
            return (PartialTrustVisibleAssemblyCollection)base[_propPartialTrustVisibleAssemblies];
        }
    }
}

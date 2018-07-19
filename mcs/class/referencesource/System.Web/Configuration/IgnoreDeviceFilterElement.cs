//------------------------------------------------------------------------------
// <copyright file="IgnoreDeviceFilterElement.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Security.Permissions;
    using System.Text;
    using System.Threading;
    using System.Web.Util;
    using System.Web.UI;
    using System.Web.Compilation;
    using System.Web.Configuration;
    using System.Xml;

    public sealed class IgnoreDeviceFilterElement : ConfigurationElement {
        private static readonly ConfigurationElementProperty s_elemProperty = new ConfigurationElementProperty(new CallbackValidator(typeof(IgnoreDeviceFilterElement), ValidateElement));
        private static ConfigurationPropertyCollection _properties;

        #region Property Declarations
        private static readonly ConfigurationProperty _propName =
            new ConfigurationProperty("name",
                                        typeof(string),
                                        null,
                                        null,
                                        StdValidatorsAndConverters.NonEmptyStringValidator,
                                        ConfigurationPropertyOptions.IsRequired | ConfigurationPropertyOptions.IsKey);
        #endregion

        static IgnoreDeviceFilterElement() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propName);
        }

        internal IgnoreDeviceFilterElement() {
        }

        public IgnoreDeviceFilterElement(string name) {
            base[_propName] = name;
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "Can't modify the base class.")]
        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("name", IsRequired = true, IsKey = true)]
        [StringValidator(MinLength = 1)]
        public string Name {
            get {
                return (string)base[_propName];
            }
        }

        [SuppressMessage("Microsoft.Security", "CA2123:OverrideLinkDemandsShouldBeIdenticalToBase", Justification = "Can't modify the base class.")]
        protected override ConfigurationElementProperty ElementProperty {
            get {
                return s_elemProperty;
            }
        }

        static private void ValidateElement(object value) {
            Debug.Assert((value != null) && (value is IgnoreDeviceFilterElement));
            IgnoreDeviceFilterElement elem = (IgnoreDeviceFilterElement)value;
            if (System.Web.UI.Util.ContainsWhiteSpace(elem.Name)) {
                throw new ConfigurationErrorsException(
                    SR.GetString(SR.Space_attribute, "name"));
            }
        }
    }
}

//------------------------------------------------------------------------------
// <copyright file="SessionPageStateSection.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Web.Configuration {
    using System;
    using System.Xml;
    using System.Configuration;
    using System.Collections.Specialized;
    using System.Collections;
    using System.Globalization;
    using System.IO;
    using System.Text;
    using System.ComponentModel;
    using System.Web.Util;
    using System.Diagnostics;
    using System.Security.Permissions;

    public sealed class SessionPageStateSection : ConfigurationSection {
        private static ConfigurationPropertyCollection _properties;

        public const int DefaultHistorySize = 9; // 

        #region Property Declarations
        private static readonly ConfigurationProperty _propHistorySize =
            new ConfigurationProperty("historySize",
                                        typeof(int),
                                        DefaultHistorySize,
                                        null,
                                        StdValidatorsAndConverters.NonZeroPositiveIntegerValidator,
                                        ConfigurationPropertyOptions.None);
        #endregion

        static SessionPageStateSection() {
            // Property initialization
            _properties = new ConfigurationPropertyCollection();
            _properties.Add(_propHistorySize);
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return _properties;
            }
        }

        [ConfigurationProperty("historySize", DefaultValue = DefaultHistorySize)]
        [IntegerValidator(MinValue = 1)]
        public int HistorySize {
            get {
                return (int)base[_propHistorySize];
            }
            set {
                base[_propHistorySize] = value;
            }
        }
    }
}

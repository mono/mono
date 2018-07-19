//------------------------------------------------------------------------------
// <copyright file="WebUtilityElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration {
    using System;
    using System.Configuration;
    using System.ComponentModel;
    using System.Globalization;
    using System.Net;

    public sealed class WebUtilityElement : ConfigurationElement {

        private readonly ConfigurationProperty unicodeDecodingConformance =
            new ConfigurationProperty(ConfigurationStrings.UnicodeDecodingConformance, typeof(UnicodeDecodingConformance), UnicodeDecodingConformance.Auto, new EnumTypeConverter<UnicodeDecodingConformance>(), null,
                    ConfigurationPropertyOptions.None);

        private readonly ConfigurationProperty unicodeEncodingConformance =
            new ConfigurationProperty(ConfigurationStrings.UnicodeEncodingConformance, typeof(UnicodeEncodingConformance), UnicodeEncodingConformance.Auto, new EnumTypeConverter<UnicodeEncodingConformance>(), null,
                    ConfigurationPropertyOptions.None);

        private readonly ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        public WebUtilityElement() {
            this.properties.Add(this.unicodeDecodingConformance);
            this.properties.Add(this.unicodeEncodingConformance);
        }

        [ConfigurationProperty(ConfigurationStrings.UnicodeDecodingConformance, DefaultValue = UnicodeDecodingConformance.Auto)]
        public UnicodeDecodingConformance UnicodeDecodingConformance {
            get {
                return (UnicodeDecodingConformance)this[this.unicodeDecodingConformance];
            }
            set {
                this[this.unicodeDecodingConformance] = value;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.UnicodeEncodingConformance, DefaultValue = UnicodeEncodingConformance.Auto)]
        public UnicodeEncodingConformance UnicodeEncodingConformance {
            get {
                return (UnicodeEncodingConformance)this[this.unicodeEncodingConformance];
            }
            set {
                this[this.unicodeEncodingConformance] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties {
            get {
                return this.properties;
            }
        }

        private class EnumTypeConverter<TEnum> : TypeConverter where TEnum : struct {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                if (sourceType == typeof(string)) {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                string s = value as string;
                if (s != null) {
                    TEnum result;
                    if (Enum.TryParse(s, true /* ignoreCase */, out result)) {
                        return result;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}

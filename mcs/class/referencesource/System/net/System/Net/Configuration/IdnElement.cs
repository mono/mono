//------------------------------------------------------------------------------
// <copyright file="IdnElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Reflection;

    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Idn", Justification = "changing this would be a breaking change because the API has been present since v3.5")]
    public sealed class IdnElement : ConfigurationElement
    {
        internal const UriIdnScope EnabledDefaultValue = UriIdnScope.None;

        public IdnElement()
        {
            this.properties.Add(this.enabled);
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get{
                return this.properties;
            }
        }

        [ConfigurationProperty(CommonConfigurationStrings.Enabled, DefaultValue = EnabledDefaultValue)]
        public UriIdnScope Enabled
        {
            get { return (UriIdnScope)this[this.enabled]; }
            set { this[this.enabled] = value; }
        }

        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty enabled =
            new ConfigurationProperty(CommonConfigurationStrings.Enabled, typeof(UriIdnScope), 
                EnabledDefaultValue, new UriIdnScopeTypeConverter(), null, ConfigurationPropertyOptions.None);

        class UriIdnScopeTypeConverter : TypeConverter
        {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
            {
                if (sourceType == typeof(string)){
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }

            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
            {
                string s = value as string;
                if (s != null){
                    s = s.ToLower(CultureInfo.InvariantCulture);
                    switch (s){
                        case "all":
                            return UriIdnScope.All;
                        case "none":
                            return UriIdnScope.None;
                        case "allexceptintranet":
                            return UriIdnScope.AllExceptIntranet;
                    }
                }

                return base.ConvertFrom(context, culture, value);
            }
        }
    }
}

//------------------------------------------------------------------------------
// <copyright file="WebRequestModuleElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

namespace System.Net.Configuration
{
    using System;
    using System.Configuration;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class WebRequestModuleElement : ConfigurationElement
    {
        public WebRequestModuleElement()
        {
            this.properties.Add(this.prefix);
            this.properties.Add(this.type);
        }

        public WebRequestModuleElement(string prefix, string type) : this()
        {
            this.Prefix = prefix;
            this[this.type] = new TypeAndName(type);
        }
        
        public WebRequestModuleElement(string prefix, Type type) : this()
        {
            this.Prefix = prefix;
            this.Type = type;
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get 
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Prefix, IsRequired=true, IsKey = true)]
        public string Prefix
        {
            get { return (string)this[this.prefix]; }
            set { this[this.prefix] = value; }
        }

        [ConfigurationProperty(ConfigurationStrings.Type)]
        [TypeConverter(typeof(TypeTypeConverter))]
        public Type Type
        {
            get 
            {
                TypeAndName typeName = (TypeAndName)this[this.type];
                if (typeName != null)
                {
                    return typeName.type;
                }
                else
                {
                    return null;
                }
            }
            set { this[this.type] = new TypeAndName(value); }
        }

        internal string Key
        {
            get { return this.Prefix; }
        }
        
        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty prefix = 
            new ConfigurationProperty(ConfigurationStrings.Prefix, 
                                      typeof(string), 
                                      null, 
                                      ConfigurationPropertyOptions.IsKey);

        readonly ConfigurationProperty type =
            new ConfigurationProperty(ConfigurationStrings.Type, 
                                      typeof(TypeAndName), 
                                      null, 
                                      new TypeTypeConverter(), 
                                      null, 
                                      ConfigurationPropertyOptions.None);


        class TypeAndName
        {
            public TypeAndName(string name)
            {
                this.type = Type.GetType(name, true, true);
                this.name = name;
            }

            public TypeAndName(Type type)
            {
                this.type = type;
            }

            public override int GetHashCode()
            {
                return type.GetHashCode();
            }

            public override bool Equals(object comparand)
            {
                return type.Equals(((TypeAndName) comparand).type);
            }

            public readonly Type type;
            public readonly string name;
        }

        class TypeTypeConverter : TypeConverter {
            public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
                if (sourceType == typeof(string)) {
                    return true;
                }
                return base.CanConvertFrom(context, sourceType);
            }
        
            public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
                if (value is string) {
                    return new TypeAndName((string) value);
                }

                return base.ConvertFrom(context, culture, value);
            }

            public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {
                if (destinationType == typeof(string)) {
                    TypeAndName castedValue = (TypeAndName) value;
                    return castedValue.name == null ? castedValue.type.AssemblyQualifiedName : castedValue.name;
                }

                return base.ConvertTo(context, culture, value, destinationType);
            }
        }
    }
}


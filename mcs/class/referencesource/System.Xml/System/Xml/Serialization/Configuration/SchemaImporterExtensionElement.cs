//------------------------------------------------------------------------------
// <copyright file="SchemaImporterExtensionElement.cs" company="Microsoft Corporation">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// <owner current="true" primary="true">[....]</owner>                                                                
//------------------------------------------------------------------------------

namespace System.Xml.Serialization.Configuration
{
    using System;
    using System.Configuration;
    using System.ComponentModel;
    using System.Globalization;
    using System.Reflection;
    using System.Security.Permissions;

    public sealed class SchemaImporterExtensionElement : ConfigurationElement
    {
        public SchemaImporterExtensionElement()
        {
            this.properties.Add(this.name);
            this.properties.Add(this.type);
        }

        public SchemaImporterExtensionElement(string name, string type) : this()
        {
            this.Name = name;
            this[this.type] = new TypeAndName(type);
        }
        
        public SchemaImporterExtensionElement(string name, Type type) : this()
        {
            this.Name = name;
            this.Type = type;
        }

        [ConfigurationProperty(ConfigurationStrings.Name, IsRequired=true, IsKey = true)]
        public string Name
        {
            get { return (string)this[this.name]; }
            set { this[this.name] = value; }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get 
            {
                return this.properties;
            }
        }

        [ConfigurationProperty(ConfigurationStrings.Type, IsRequired=true, IsKey = false)]
        [TypeConverter(typeof(TypeTypeConverter))]
        public Type Type
        {
            get { return ((TypeAndName) this[this.type]).type; }
            set { this[this.type] = new TypeAndName(value); }
        }

        internal string Key
        {
            get { return this.Name; }
        }
        
        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();

        readonly ConfigurationProperty name = 
            new ConfigurationProperty(ConfigurationStrings.Name, typeof(string), null, 
                    ConfigurationPropertyOptions.IsKey);

        readonly ConfigurationProperty type =
            new ConfigurationProperty(ConfigurationStrings.Type, typeof(Type), null,
                    new TypeTypeConverter(), null, ConfigurationPropertyOptions.IsRequired);

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


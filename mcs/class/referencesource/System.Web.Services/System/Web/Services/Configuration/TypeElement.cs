//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration {
    using System;
    using System.Configuration;
    using System.ComponentModel;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;

    public sealed class TypeElement : ConfigurationElement {
        // These three constructors are used by the configuration system. 
        public TypeElement() : base() {
            this.properties.Add(this.type);
        }

        public TypeElement(string type) : this() {
            base[this.type] = new TypeAndName(type);
        }

        public TypeElement(Type type) : this(type.AssemblyQualifiedName) {
        }

        [ConfigurationProperty("type", IsKey = true)]
        [TypeConverter(typeof(TypeAndNameConverter))]
        public Type Type {
            get { return ((TypeAndName)base[this.type]).type; }
            set {
                if (value == null) {
                    throw new ArgumentNullException("value");
                }
                base[this.type] = new TypeAndName(value);
            }
        }
        protected override ConfigurationPropertyCollection Properties {
            get { return this.properties; }
        }


        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        readonly ConfigurationProperty type = new ConfigurationProperty("type", typeof(TypeAndName), null, new TypeAndNameConverter(), null, ConfigurationPropertyOptions.IsKey);
    }

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

    class TypeAndNameConverter : TypeConverter {
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

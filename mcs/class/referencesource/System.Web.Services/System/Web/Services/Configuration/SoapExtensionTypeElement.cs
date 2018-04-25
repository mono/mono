//------------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//------------------------------------------------------------------------------

namespace System.Web.Services.Configuration
{
    using System;
    using System.ComponentModel;
    using System.Configuration;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Security.Permissions;

    public sealed class SoapExtensionTypeElement : ConfigurationElement
    {
        // These three constructors are used by the configuration system. 
        public SoapExtensionTypeElement() : base()
        {
            this.properties.Add(this.group);
            this.properties.Add(this.priority);
            this.properties.Add(this.type);
        }

        public SoapExtensionTypeElement(string type, int priority, PriorityGroup group) : this()
        {
            this.Type = Type.GetType(type, true, true);
            this.Priority = priority;
            this.Group = group;
        }

        public SoapExtensionTypeElement(Type type, int priority, PriorityGroup group) : 
            this(type.AssemblyQualifiedName, priority, group)
        {
        }

        [ConfigurationProperty("group", IsKey = true, DefaultValue = PriorityGroup.Low)]
        public PriorityGroup Group
        {
            get { return (PriorityGroup)base[this.group]; }
            set
            {
                if (Enum.IsDefined(typeof(PriorityGroup), value))
                {
                    base[this.group] = value;
                }
                else
                {
                    throw new ArgumentException(Res.GetString(Res.Invalid_priority_group_value), "value");
                }
            }
        }

        [ConfigurationProperty("priority", IsKey = true, DefaultValue = 0)]
        [IntegerValidator(MinValue = 0)]
        public int Priority
        {
            get { return (int)base[this.priority]; }
            set { base[this.priority] = value; }
        }

        [ConfigurationProperty("type", IsKey = true)]
        [TypeConverter(typeof(TypeTypeConverter))]
        public Type Type
        {
            get { 
                return (Type)base[this.type];
            }
            set
            {
                base[this.type] = value;
            }
        }

        protected override ConfigurationPropertyCollection Properties
        {
            get { return this.properties; }
        }


        ConfigurationPropertyCollection properties = new ConfigurationPropertyCollection();
        readonly ConfigurationProperty group = new ConfigurationProperty("group", typeof(PriorityGroup), PriorityGroup.Low, new EnumConverter(typeof(PriorityGroup)), null, ConfigurationPropertyOptions.IsKey);
        readonly ConfigurationProperty priority = new ConfigurationProperty("priority", typeof(int), 0, null, new IntegerValidator( 0, int.MaxValue ), ConfigurationPropertyOptions.IsKey);
        readonly ConfigurationProperty type = new ConfigurationProperty("type", typeof(Type), null, new TypeTypeConverter(), null, ConfigurationPropertyOptions.IsKey);
    }

    class TypeTypeConverter : TypeAndNameConverter {
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType) {
            return base.CanConvertFrom(context, sourceType);
        }

        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value) {
            if (value is string) {
                TypeAndName baseValue = (TypeAndName)base.ConvertFrom(context, culture, value);
                return baseValue.type;
            }

            return base.ConvertFrom(context, culture, value);
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType) {

            if (destinationType == typeof(string)) {
                TypeAndName castedValue = new TypeAndName((Type)value);
                return base.ConvertTo(context, culture, castedValue, destinationType);
            }

            return base.ConvertTo(context, culture, value, destinationType);
        }
    }
}




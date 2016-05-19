// <copyright>
//   Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

namespace Microsoft.Activities.Presentation.Xaml
{
    using System;
    using System.Collections;
    using System.ComponentModel;
    using System.ComponentModel.Design.Serialization;
    using System.Globalization;
    using System.Reflection;

    // AttributeConverter is to convert some XAML-unfriendly attributes (without default ctor) to InstanceDescriptor for XAML serialization
    internal class AttributeConverter<TAttribute, TAttributeInfo> : TypeConverter
        where TAttribute : Attribute
        where TAttributeInfo : AttributeInfo<TAttribute>, new()
    {
        private static ConstructorInfo attributeConstructor = null;
        private TAttributeInfo attributeInfo = new TAttributeInfo();

        private ConstructorInfo Constructor
        {
            get
            {
                // no need to lock here because every thread will generate the same constructor info even in race condition
                // and cost to get the constructor is relative small
                if (AttributeConverter<TAttribute, TAttributeInfo>.attributeConstructor == null)
                {
                    AttributeConverter<TAttribute, TAttributeInfo>.attributeConstructor = this.attributeInfo.GetConstructor();
                }

                return AttributeConverter<TAttribute, TAttributeInfo>.attributeConstructor;
            }
        }

        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == typeof(InstanceDescriptor))
            {
                return true;
            }

            return base.CanConvertTo(context, destinationType);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            return false;
        }

        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            if (destinationType != typeof(InstanceDescriptor))
            {
                return base.ConvertTo(context, culture, value, destinationType);
            }

            TAttribute attribute = value as TAttribute;

            SharedFx.Assert(value != null, "The usage should be guaranteed by the XAML stack");

            ConstructorInfo constructor = this.Constructor;
            ICollection arguments = this.attributeInfo.GetConstructorArguments(attribute, ref constructor);
            return new InstanceDescriptor(constructor, arguments, this.attributeInfo.IsComplete);
        }
    }
}

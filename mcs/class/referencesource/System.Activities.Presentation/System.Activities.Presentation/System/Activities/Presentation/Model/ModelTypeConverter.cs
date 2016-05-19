//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Globalization;
    using System.Diagnostics.CodeAnalysis;

    // This is a type converter that wraps another type converter.  In the
    // editing model, we expose all properties as ModelItem objects, so we need
    // to unwrap them before handing them to a type converter.  This type
    // converter provides that unwrapping seamlessly.

    [SuppressMessage("XAML", "XAML1004", Justification = "This is internal, and is always available through TypeConverter.GetConverter, not used in xaml")]
    class ModelTypeConverter : TypeConverter
    {

        ModelTreeManager modelTreeManager;
        TypeConverter converter;

        internal ModelTypeConverter(ModelTreeManager modelTreeManager, TypeConverter converter)
        {
            this.modelTreeManager = modelTreeManager;
            this.converter = converter;
        }

        // Wraps the given type descriptor context with the set of this.modelTreeManagers
        // available in the editing context.
        ITypeDescriptorContext WrapContext(ITypeDescriptorContext context)
        {
            return new ModelTypeDescriptorContextWrapper(context, this.modelTreeManager);
        }

        // Returns true if the converter can convert from the given source type.
        public override bool CanConvertFrom(ITypeDescriptorContext context, Type sourceType)
        {
            if (sourceType == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("sourceType"));
            }
            return this.converter.CanConvertFrom(WrapContext(context), sourceType);
        }

        // Returns true if the converter can convert to the given target type.
        public override bool CanConvertTo(ITypeDescriptorContext context, Type destinationType)
        {
            if (destinationType == null)
            {
                throw FxTrace.Exception.AsError(new ArgumentNullException("destinationType"));
            }
            return this.converter.CanConvertTo(WrapContext(context), destinationType);
        }

        // Performs the actual conversion from one type to antother.  If the value provided
        // is a ModelItem, it will be unwrapped first.  The return value is a ModelItem.
        public override object ConvertFrom(ITypeDescriptorContext context, CultureInfo culture, object value)
        {
            ModelItem item = value as ModelItem;
            if (item != null)
            {
                value = item.GetCurrentValue();
            }
            object convertedValue = this.converter.ConvertFrom(WrapContext(context), culture, value);

            if (convertedValue != null)
            {
                convertedValue = this.modelTreeManager.CreateModelItem(null, convertedValue);
            }

            return convertedValue;
        }

        // Performs the actual conversion to another type.  If the value provided is an item, it will
        // be uwrapped first.  The return value is the raw data type.
        public override object ConvertTo(ITypeDescriptorContext context, CultureInfo culture, object value, Type destinationType)
        {
            ModelItem item = value as ModelItem;
            if (item != null)
            {
                value = item.GetCurrentValue();
            }
            if (value != null)
            {
                return this.converter.ConvertTo(WrapContext(context), culture, value, destinationType);
            }
            return null;
        }

        // Creates an instance of an object using a dictionary of property values.  The
        // return value is a wrapped model item.
        public override object CreateInstance(ITypeDescriptorContext context, IDictionary propertyValues)
        {
            object value = this.converter.CreateInstance(WrapContext(context), propertyValues);
            if (value != null)
            {
                value = this.modelTreeManager.CreateModelItem(null, value);
            }
            return value;
        }

        // Returns true if the CreateInstance method can be used to create new instances of
        // objects.
        public override bool GetCreateInstanceSupported(ITypeDescriptorContext context)
        {
            return this.converter.GetCreateInstanceSupported(WrapContext(context));
        }

        // Returns child properties for a type converter.  This will wrap all properties returned.
        public override PropertyDescriptorCollection GetProperties(ITypeDescriptorContext context, object value, Attribute[] attributes)
        {

            if (value == null)
            {
                throw FxTrace.Exception.AsError( new ArgumentNullException("value"));
            }

            ModelItem item = value as ModelItem;
            if (item != null)
            {
                value = item.GetCurrentValue();
            }

            PropertyDescriptorCollection props = this.converter.GetProperties(WrapContext(context), value, attributes);
            if (props != null && props.Count > 0)
            {

                if (item == null)
                {
                    // We will need the item for this object.
                    item = this.modelTreeManager.CreateModelItem(null, value);
                }

                // Search our item for each property and wrap it.  If
                // a property is not offered by the model, ommit it.

                List<PropertyDescriptor> newProps = new List<PropertyDescriptor>(props.Count);
                foreach (PropertyDescriptor p in props)
                {
                    ModelProperty modelProp = item.Properties.Find(p.Name);
                    if (modelProp != null)
                    {
                        newProps.Add(new ModelPropertyDescriptor(modelProp));
                    }
                }

                props = new PropertyDescriptorCollection(newProps.ToArray(), true);
            }

            return props;
        }

        // Returns true if GetProperties will return child properties for the
        // object.
        public override bool GetPropertiesSupported(ITypeDescriptorContext context)
        {
            return this.converter.GetPropertiesSupported(WrapContext(context));
        }

        // Returns a set of standard values this type converter offers.
        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context)
        {
            StandardValuesCollection values = this.converter.GetStandardValues(WrapContext(context));

            // Some type converters return null here, which isn't supposed to 
            // be allowed. Fix them

            object[] wrappedValues;

            if (values == null)
            {
                wrappedValues = new object[0];
            }
            else
            {
                wrappedValues = new object[values.Count];
                int idx = 0;
                foreach (object value in values)
                {
                    object wrappedValue;
                    if (value != null)
                    {
                        wrappedValue = this.modelTreeManager.CreateModelItem(null, value);
                    }
                    else
                    {
                        wrappedValue = value;
                    }

                    wrappedValues[idx++] = wrappedValue;
                }
            }

            return new StandardValuesCollection(wrappedValues);
        }

        // Returns true if the set of standard values cannot be customized.
        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return this.converter.GetStandardValuesExclusive(WrapContext(context));
        }

        // Returns true if this type converter offers a set of standard values.
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return this.converter.GetStandardValuesSupported(WrapContext(context));
        }

        // Returns true if the given value is a valid value for this type converter.
        public override bool IsValid(ITypeDescriptorContext context, object value)
        {
            ModelItem item = value as ModelItem;
            if (item != null)
            {
                value = item.GetCurrentValue();
            }
            return this.converter.IsValid(WrapContext(context), value);
        }
    }
}

//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.ComponentModel;

    // This is a property descriptor that wraps ModelProperty objects.
    // It is used when someone uses TypeDescriptor to ask for type information
    // on an editing model item.
    internal class ModelPropertyDescriptor : PropertyDescriptor
    {
        ModelProperty itemProperty;
        TypeConverter converter;

        internal ModelPropertyDescriptor(ModelProperty itemProperty)
            : base(itemProperty.Name, null)
        {
            this.itemProperty = itemProperty;
        }

        public override AttributeCollection Attributes
        {
            get { return this.itemProperty.Attributes; }
        }

        // Returns the type converter for this property.  Our property
        // descriptor wrapper is "complete" in that it always returns
        // editing model item objects.  Because of that, we must wrap
        // all type converters.
        public override TypeConverter Converter
        {
            get
            {
                if (this.converter == null)
                {
                    TypeConverter baseConverter = base.Converter;
                    IModelTreeItem propertyParent = this.itemProperty.Parent as IModelTreeItem;
                    this.converter = new ModelTypeConverter(propertyParent.ModelTreeManager, baseConverter);
                }

                return converter;
            }
        }


        public override bool IsBrowsable
        {
            get { return this.itemProperty.IsBrowsable; }
        }

        // Returns the type of object that defined this property.
        public override Type ComponentType
        {
            get { return this.itemProperty.Parent.ItemType; }
        }

        public override bool IsReadOnly
        {
            get { return this.itemProperty.IsReadOnly; }
        }

        // Returns the data type of the property.  
        public override Type PropertyType
        {

            get
            {
                return this.itemProperty.PropertyType;
            }

        }

        public override PropertyDescriptorCollection GetChildProperties(object instance, Attribute[] filter)
        {
            return base.GetChildProperties(instance, filter);
        }

        public override bool CanResetValue(object component)
        {
            return this.itemProperty.IsSet;
        }

        public override object GetEditor(Type editorBaseType)
        {
            // The new PropertyEntry PropertyValue editor model does not use this
            return null;
        }

        // Returns the current value of this property.
        // When the object is not primitive, enum, or string we always return the ModelItem wrapping it.
        // this enables nested binding in wpf still go through modelItems tree so taht we can intercept
        // the property sets e.g "{Binding Path=RootModel.ComplexProperty.Blah"}, since we return a ModelItem
        // for ComplexProperty we can still intercept sets made from Wpf controls to Blah even if ComplexProperty
        // type does not implement INotifyPropertyChanged.
        public override object GetValue(object component)
        {
            ModelItem value = this.itemProperty.Value;
            if (value == null)
            {
                return null;
            }
            Type itemType = value.ItemType;
            if (itemType.IsPrimitive || itemType.IsEnum || itemType.Equals(typeof(String)))
            {
                return value.GetCurrentValue();
            }
            return value;
        }

        public override void ResetValue(object component)
        {
            this.itemProperty.ClearValue();
        }

        // Sets the property value to the given value.  For
        // convenience, the value passed can either be an
        // item or a raw value.  In the latter case we will
        // wrap the value into an item for you. 
        public override void SetValue(object component, object value)
        {
            this.itemProperty.SetValue(value);
        }

        // Returns true if the value should be serialized to code.
        public override bool ShouldSerializeValue(object component)
        {
            // If the local value is set, see if the property supports
            // a ShouldSerialize on its property descriptor.  If it doesn't,
            // then we let the IsSet dictate the 'setness'.

            if (this.itemProperty.IsSet)
            {
                ModelPropertyImpl modelProp = this.itemProperty as ModelPropertyImpl;
                if (modelProp != null)
                {
                    return modelProp.PropertyDescriptor.ShouldSerializeValue(this.itemProperty.Parent.GetCurrentValue());
                }
                return true;
            }

            return false;
        }
    }
}

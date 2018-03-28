//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Xml.Linq;

    class DynamicActivityTypeDescriptor : ICustomTypeDescriptor
    {
        PropertyDescriptorCollection cachedProperties;
        Activity owner;

        public DynamicActivityTypeDescriptor(Activity owner)
        {
            this.owner = owner;
            this.Properties = new ActivityPropertyCollection(this);
        }

        public string Name
        {
            get;
            set;
        }

        public KeyedCollection<string, DynamicActivityProperty> Properties
        {
            get;
            private set;
        }

        public AttributeCollection GetAttributes()
        {
            return TypeDescriptor.GetAttributes(this.owner, true);
        }

        public string GetClassName()
        {
            if (this.Name != null)
            {
                return this.Name;
            }

            return TypeDescriptor.GetClassName(this.owner, true);
        }

        public string GetComponentName()
        {
            return TypeDescriptor.GetComponentName(this.owner, true);
        }

        public TypeConverter GetConverter()
        {
            return TypeDescriptor.GetConverter(this.owner, true);
        }

        public EventDescriptor GetDefaultEvent()
        {
            return TypeDescriptor.GetDefaultEvent(this.owner, true);
        }

        public PropertyDescriptor GetDefaultProperty()
        {
            return TypeDescriptor.GetDefaultProperty(this.owner, true);
        }

        public object GetEditor(Type editorBaseType)
        {
            return TypeDescriptor.GetEditor(this.owner, editorBaseType, true);
        }

        public EventDescriptorCollection GetEvents(Attribute[] attributes)
        {
            return TypeDescriptor.GetEvents(this.owner, attributes, true);
        }

        public EventDescriptorCollection GetEvents()
        {
            return TypeDescriptor.GetEvents(this.owner, true);
        }

        public PropertyDescriptorCollection GetProperties()
        {
            return GetProperties(null);
        }

        public PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            PropertyDescriptorCollection result = this.cachedProperties;
            if (result != null)
            {
                return result;
            }

            PropertyDescriptorCollection dynamicProperties;
            if (attributes != null)
            {
                dynamicProperties = TypeDescriptor.GetProperties(this.owner, attributes, true);
            }
            else
            {
                dynamicProperties = TypeDescriptor.GetProperties(this.owner, true);
            }

            // initial capacity is Properties + Name + Body 
            List<PropertyDescriptor> propertyDescriptors = new List<PropertyDescriptor>(this.Properties.Count + 2);
            for (int i = 0; i < dynamicProperties.Count; i++)
            {
                PropertyDescriptor dynamicProperty = dynamicProperties[i];
                if (dynamicProperty.IsBrowsable)
                {
                    propertyDescriptors.Add(dynamicProperty);
                }
            }

            foreach (DynamicActivityProperty property in Properties)
            {
                if (string.IsNullOrEmpty(property.Name)) 
                {
                    throw FxTrace.Exception.AsError(new ValidationException(SR.ActivityPropertyRequiresName(this.owner.DisplayName)));
                }            
                if (property.Type == null)
                {                
                    throw FxTrace.Exception.AsError(new ValidationException(SR.ActivityPropertyRequiresType(this.owner.DisplayName)));
                }
                propertyDescriptors.Add(new DynamicActivityPropertyDescriptor(property, this.owner.GetType()));
            }

            result = new PropertyDescriptorCollection(propertyDescriptors.ToArray());
            this.cachedProperties = result;
            return result;
        }

        public object GetPropertyOwner(PropertyDescriptor pd)
        {
            return this.owner;
        }

        class DynamicActivityPropertyDescriptor : PropertyDescriptor
        {
            AttributeCollection attributes;
            DynamicActivityProperty activityProperty;
            Type componentType;

            public DynamicActivityPropertyDescriptor(DynamicActivityProperty activityProperty, Type componentType)
                : base(activityProperty.Name, null)
            {
                this.activityProperty = activityProperty;
                this.componentType = componentType;
            }

            public override Type ComponentType
            {
                get
                {
                    return this.componentType;
                }
            }

            public override AttributeCollection Attributes
            {
                get
                {
                    if (this.attributes == null)
                    {
                        AttributeCollection inheritedAttributes = base.Attributes;
                        Collection<Attribute> propertyAttributes = this.activityProperty.Attributes;
                        Attribute[] totalAttributes = new Attribute[inheritedAttributes.Count + propertyAttributes.Count + 1];
                        inheritedAttributes.CopyTo(totalAttributes, 0);
                        propertyAttributes.CopyTo(totalAttributes, inheritedAttributes.Count);
                        totalAttributes[inheritedAttributes.Count + propertyAttributes.Count] = new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden);
                        this.attributes = new AttributeCollection(totalAttributes);
                    }
                    return this.attributes;
                }
            }

            public override bool IsReadOnly
            {
                get
                {
                    return false;
                }
            }

            public override Type PropertyType
            {
                get
                {
                    return this.activityProperty.Type;
                }
            }

            public override object GetValue(object component)
            {
                IDynamicActivity owner = component as IDynamicActivity;
                if (owner == null || !owner.Properties.Contains(this.activityProperty))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidDynamicActivityProperty(this.Name)));
                }

                return this.activityProperty.Value;                    
            }

            public override void SetValue(object component, object value)
            {
                IDynamicActivity owner = component as IDynamicActivity;
                if (owner == null || !owner.Properties.Contains(this.activityProperty))
                {
                    throw FxTrace.Exception.AsError(new InvalidOperationException(SR.InvalidDynamicActivityProperty(this.Name)));
                }

                this.activityProperty.Value = value;
            }

            public override bool CanResetValue(object component)
            {
                return false;
            }

            public override void ResetValue(object component)
            {
            }

            public override bool ShouldSerializeValue(object component)
            {
                return false;
            }

            protected override void FillAttributes(IList attributeList)
            {
                if (attributeList == null)
                {
                    throw FxTrace.Exception.ArgumentNull("attributeList");
                }

                attributeList.Add(new DesignerSerializationVisibilityAttribute(DesignerSerializationVisibility.Hidden));
            }
        }

        class ActivityPropertyCollection : KeyedCollection<string, DynamicActivityProperty>
        {
            DynamicActivityTypeDescriptor parent;

            public ActivityPropertyCollection(DynamicActivityTypeDescriptor parent)
                : base()
            {
                this.parent = parent;
            }

            protected override void InsertItem(int index, DynamicActivityProperty item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }

                if (this.Contains(item.Name))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.DynamicActivityDuplicatePropertyDetected(item.Name), "item"));
                }

                InvalidateCache();
                base.InsertItem(index, item);                
            }

            protected override void SetItem(int index, DynamicActivityProperty item)
            {
                if (item == null)
                {
                    throw FxTrace.Exception.ArgumentNull("item");
                }

                // We don't want self-assignment to throw. Note that if this[index] has the same
                // name as item, no other element in the collection can.
                if (!this[index].Name.Equals(item.Name) && this.Contains(item.Name))
                {
                    throw FxTrace.Exception.AsError(new ArgumentException(SR.DynamicActivityDuplicatePropertyDetected(item.Name), "item"));
                }

                InvalidateCache();
                base.SetItem(index, item);                
            }

            protected override void RemoveItem(int index)
            {
                InvalidateCache();
                base.RemoveItem(index);
            }

            protected override void ClearItems()
            {
                InvalidateCache();
                base.ClearItems();
            }

            protected override string GetKeyForItem(DynamicActivityProperty item)
            {
                return item.Name;
            }

            void InvalidateCache()
            {
                this.parent.cachedProperties = null;
            }
        }
    }
}



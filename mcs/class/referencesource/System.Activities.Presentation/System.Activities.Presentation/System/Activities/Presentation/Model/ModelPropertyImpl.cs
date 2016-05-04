//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Windows.Documents;

    // This class provides the implementation for a model property.
    // this intercepts sets /gets to the property and works with modeltreemanager
    // to keep the xaml in [....].

    class ModelPropertyImpl : ModelProperty
    {
        object defaultValue;
        ModelItem parent;
        PropertyDescriptor propertyDescriptor;
        bool isAttached = false;

        public ModelPropertyImpl(ModelItem parent, PropertyDescriptor propertyDescriptor, bool isAttached)
        {
            this.parent = parent;
            this.propertyDescriptor = propertyDescriptor;

            // using this a marker to indicate this hasnt been computed yet.
            this.defaultValue = this;
            this.isAttached = isAttached;
        }

        public override Type AttachedOwnerType
        {
            get
            {
                return this.parent.GetType();
            }
        }

        public override AttributeCollection Attributes
        {
            get
            {
                return this.propertyDescriptor.Attributes;
            }
        }

        public override ModelItemCollection Collection
        {
            get
            {
                return this.Value as ModelItemCollection;
            }
        }

        public override object ComputedValue
        {
            get
            {
                object computedValue = null;
                if (this.Value != null)
                {
                    computedValue = Value.GetCurrentValue();
                }
                return computedValue;
            }
            set
            {
                SetValue(value);
            }
        }

        public override System.ComponentModel.TypeConverter Converter
        {
            get
            {
                return new ModelTypeConverter(((IModelTreeItem)this.Parent).ModelTreeManager, propertyDescriptor.Converter);
            }
        }

        public override object DefaultValue
        {
            get
            {
                if (Object.ReferenceEquals(this.defaultValue, this))
                {
                    DefaultValueAttribute defaultValueAttribute = this.propertyDescriptor.Attributes[typeof(DefaultValueAttribute)] as DefaultValueAttribute;
                    this.defaultValue = (defaultValueAttribute == null) ? null : defaultValueAttribute.Value;
                }
                return this.defaultValue;
            }
        }

        public override ModelItemDictionary Dictionary
        {
            get
            {
                return Value as ModelItemDictionary;
            }
        }

        public override bool IsAttached
        {
            get
            {
                return this.isAttached;
            }
        }

        public override bool IsBrowsable
        {
            get
            {
                return propertyDescriptor.IsBrowsable;
            }
        }

        public override bool IsCollection
        {
            get { return this.Value is ModelItemCollection; }
        }

        public override bool IsDictionary
        {
            get { return this.Value is ModelItemDictionary; }
        }

        public override bool IsReadOnly
        {
            get
            {
                return propertyDescriptor.IsReadOnly;
            }
        }

        public override bool IsSet
        {
            get
            {
                return this.Value != null;
            }
        }

        public override string Name
        {
            get
            {
                return propertyDescriptor.Name;
            }
        }

        public override ModelItem Parent
        {
            get
            {
                return parent;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return propertyDescriptor.PropertyType;
            }
        }

        public override ModelItem Value
        {
            get
            {
                return ((IModelTreeItem)parent).ModelTreeManager.GetValue(this);
            }
        }

        internal PropertyDescriptor PropertyDescriptor
        {
            get
            {
                return propertyDescriptor;
            }
        }

        public override void ClearValue()
        {
            if (!this.IsReadOnly)
            {
                ((IModelTreeItem)parent).ModelTreeManager.ClearValue(this);
            }
        }

        public override ModelItem SetValue(object value)
        {
            return ((IModelTreeItem)parent).ModelTreeManager.SetValue(this, value);
        }

        internal override string Reference
        {
            get
            {
                return PropertyReferenceUtilities.GetPropertyReference(parent.GetCurrentValue(), this.Name);
            }
        }

        internal override void ClearReference()
        {
            this.SetReference(null);
        }

        internal override void SetReference(string sourceProperty)
        {
            PropertyReferenceChange change = new PropertyReferenceChange()
            {
                Owner = this.Parent,
                TargetProperty = this.Name,
                OldSourceProperty = this.Reference,
                NewSourceProperty = sourceProperty
            };

            ((IModelTreeItem)parent).ModelTreeManager.AddToCurrentEditingScope(change);
        }

        internal object SetValueCore(ModelItem newValueModelItem)
        {
            object newValueInstance = (newValueModelItem == null) ? null : newValueModelItem.GetCurrentValue();
            ModelItem oldValueModelItem = this.Value;
            IModelTreeItem parent = (IModelTreeItem)this.Parent;

            // update object instance
            this.PropertyDescriptor.SetValue(this.Parent.GetCurrentValue(), newValueInstance);

            if (oldValueModelItem != null && !this.isAttached)
            {
                parent.ModelPropertyStore.Remove(this.Name);
                parent.ModelTreeManager.OnPropertyEdgeRemoved(this.Name, this.Parent, oldValueModelItem);
            }

            if (newValueModelItem != null && !this.isAttached)
            {
                parent.ModelPropertyStore.Add(this.Name, newValueModelItem);
                parent.ModelTreeManager.OnPropertyEdgeAdded(this.Name, this.Parent, newValueModelItem);
            }

            // notify observers
            ((IModelTreeItem)this.Parent).OnPropertyChanged(this.Name);
            return newValueInstance;
        }
    }
}

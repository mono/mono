//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.ComponentModel;
    using System.Activities.Presentation.Model;

    class AttachedPropertyDescriptor : PropertyDescriptor
    {
        AttachedProperty AttachedProperty;
        ModelItem owner;

        public AttachedPropertyDescriptor(AttachedProperty AttachedProperty, ModelItem owner)
            : base(AttachedProperty.Name, null)
        {
            this.AttachedProperty = AttachedProperty;
            this.owner = owner;
        }

        public override AttributeCollection Attributes
        {
            get
            {
                List<Attribute> attributeList = new List<Attribute>();
                foreach (Attribute attr in TypeDescriptor.GetAttributes(this.PropertyType))
                {
                    attributeList.Add(attr);
                }
                BrowsableAttribute browsableAttribute = new BrowsableAttribute(this.IsBrowsable);
                attributeList.Add(browsableAttribute);
                return new AttributeCollection(attributeList.ToArray());
            }
        }

        public override Type ComponentType
        {
            get { return this.owner.ItemType; }
        }

        public override bool IsReadOnly
        {
            get
            {
                return this.AttachedProperty.IsReadOnly;
            }
        }

        public override Type PropertyType
        {
            get
            {
                return this.AttachedProperty.Type;
            }
        }

        public override bool IsBrowsable
        {
            get
            {
                return this.AttachedProperty.IsBrowsable;
            }
        }
        public override bool CanResetValue(object component)
        {
            return false;
        }

        public override object GetValue(object component)
        {

            return this.AttachedProperty.GetValue(owner);
        }

        public override void ResetValue(object component)
        {
            this.AttachedProperty.ResetValue(owner);
        }

        public override void SetValue(object component, object value)
        {
            this.AttachedProperty.SetValue(owner, value);
        }

        public override bool ShouldSerializeValue(object component)
        {
            return false;
        }
    }

}

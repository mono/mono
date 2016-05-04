//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------

namespace System.Activities.Presentation.Model
{

    using System;
    using System.ComponentModel;
    using System.Diagnostics;

    //
    // We need to provide a type descriptor context that can get to
    // services in the editing context.  This wrapper does that by
    // first checking the original set of services and then routing
    // to the given service provider (which is usually an editing context's
    // service manager).
    //
    internal class ModelTypeDescriptorContextWrapper : ITypeDescriptorContext
    {

        ITypeDescriptorContext context;
        ModelTreeManager modelTreeManager;

        internal ModelTypeDescriptorContextWrapper(ITypeDescriptorContext context, ModelTreeManager modelTreeManager)
        {

            this.context = context;
            this.modelTreeManager = modelTreeManager;
        }


        public IContainer Container
        {
            get { return this.context == null ? null : this.context.Container; }
        }

        public object Instance
        {
            get
            {
                object instance = null;

                if (this.context != null)
                {
                    instance = this.context.Instance;
                    ModelItem item = instance as ModelItem;
                    if (item != null)
                    {
                        instance = item.GetCurrentValue();
                    }
                }

                return instance;
            }
        }

        public PropertyDescriptor PropertyDescriptor
        {
            get
            {
                PropertyDescriptor desc = null;

                if (this.context != null)
                {
                    desc = this.context.PropertyDescriptor;
                    if (desc != null)
                    {
                        ModelItem item = this.context.Instance as ModelItem;
                        if (item != null)
                        {
                            desc = new WrappedPropertyDescriptor(desc, item);
                        }
                    }
                }

                return desc;
            }
        }


        public void OnComponentChanged()
        {
            if (this.context != null)
            {
                this.context.OnComponentChanged();
            }
        }

        public bool OnComponentChanging()
        {
            return this.context == null ? false : this.context.OnComponentChanging();
        }

        public object GetService(Type serviceType)
        {
            object service = null;

            if (this.context != null)
            {
                service = this.context.GetService(serviceType);
            }

            if (service == null)
            {
                service = this.modelTreeManager.Context.Services.GetService(serviceType);
            }


            return service;
        }


        // This property descriptor dynamically converts calls to
        // the original property descriptor, converting values on
        // the fly.
        class WrappedPropertyDescriptor : PropertyDescriptor
        {
            private PropertyDescriptor _parent;
            private ModelItem _item;

            internal WrappedPropertyDescriptor(PropertyDescriptor parent, ModelItem item)
                : base(parent)
            {
                _parent = parent;
                _item = item;
            }
            public override Type ComponentType
            { get { return _parent.ComponentType; } }
            public override bool IsReadOnly
            { get { return _parent.IsReadOnly; } }
            public override Type PropertyType
            { get { return _parent.PropertyType; } }

            public override bool CanResetValue(object component)
            {
                return _parent.CanResetValue(_item);
            }
            public override object GetValue(object component)
            {
                return _parent.GetValue(_item);
            }
            public override void ResetValue(object component)
            {
                _parent.ResetValue(_item);
            }
            public override void SetValue(object component, object value)
            {
                _parent.SetValue(_item, value);
            }
            public override bool ShouldSerializeValue(object component)
            {
                return _parent.ShouldSerializeValue(_item);
            }
        }
    }
}

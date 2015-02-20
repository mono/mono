//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System.Collections.Generic;
    using System.Activities.Presentation.View;
    using System.ComponentModel;
    using System.IO;
    using System.Linq;


    // This provides a container for model properties of a modelItem.
    // This uses the TypeDescriptor.GetProperties() instead of using reflection
    // to get the properties of a model item. So any model instance implementing ICustomTypeProvider 
    // is automatically taken care of.

    class ModelPropertyCollectionImpl : ModelPropertyCollection
    {
        ModelItem parent;
        bool createFakeModelProperties;

        public ModelPropertyCollectionImpl(ModelItem parent)
        {
            this.parent = parent;
            createFakeModelProperties = this.parent is FakeModelItemImpl;
        }

        public override IEnumerator<ModelProperty> GetEnumerator()
        {
            foreach (PropertyDescriptor propertyDescriptor in GetPropertyDescriptors())
            {
                yield return CreateProperty(parent, propertyDescriptor);
            }
        }

        protected override ModelProperty Find(System.Windows.DependencyProperty value, bool throwOnError)
        {
            // We dont support dependency properties.
            if (throwOnError)
            {
                throw FxTrace.Exception.AsError(new NotSupportedException());
            }
            else
            {
                return null;
            }
        }

        protected override ModelProperty Find(string name, bool throwOnError)
        {
            PropertyDescriptor propertyDescriptor = GetPropertyDescriptors()[name];
            if (propertyDescriptor != null)
            {
                return CreateProperty(parent, propertyDescriptor);
            }
            return null;
        }

        ModelProperty CreateProperty(ModelItem parent, PropertyDescriptor propertyDescriptor)
        {
            bool isAttached = propertyDescriptor is AttachedPropertyDescriptor;
            return this.createFakeModelProperties ?
                (ModelProperty)(new FakeModelPropertyImpl((FakeModelItemImpl)parent, propertyDescriptor)) :
                (ModelProperty)(new ModelPropertyImpl(parent, propertyDescriptor, isAttached));
        }

        PropertyDescriptorCollection GetPropertyDescriptors()
        {
            PropertyDescriptorCollection propertyDescriptors = PropertyDescriptorCollection.Empty;

            try
            {
                object instance = parent.GetCurrentValue();
                if (instance != null)
                {
                    if (!(instance is ICustomTypeDescriptor))
                    {
                        Type instanceType = instance.GetType();
                        if (instanceType.IsValueType)
                        {
                            propertyDescriptors = TypeDescriptor.GetProvider(instanceType).GetTypeDescriptor(instanceType).GetProperties();
                        }
                        else
                        {
                            propertyDescriptors = TypeDescriptor.GetProvider(instance).GetTypeDescriptor(instance).GetProperties();
                        }
                    }
                    else
                    {
                        propertyDescriptors = TypeDescriptor.GetProperties(instance);
                    }

                }

                // Add browsable attached properties 
                AttachedPropertiesService AttachedPropertiesService = this.parent.GetEditingContext().Services.GetService<AttachedPropertiesService>();
                if (AttachedPropertiesService != null)
                {

                    var browsableAttachedProperties = from attachedProperty in AttachedPropertiesService.GetAttachedProperties(this.parent.ItemType)
                                                      where (attachedProperty.IsBrowsable || attachedProperty.IsVisibleToModelItem)
                                                      select new AttachedPropertyDescriptor(attachedProperty, this.parent);

                    List<PropertyDescriptor> mergedProperties = new List<PropertyDescriptor>();
                    foreach (PropertyDescriptor propertyDescriptor in propertyDescriptors)
                    {
                        mergedProperties.Add(propertyDescriptor);
                    }
                    propertyDescriptors = new PropertyDescriptorCollection(mergedProperties.Concat(browsableAttachedProperties).ToArray(), true);

                }
            }
            catch (FileNotFoundException e)
            {
                EditingContext context = parent.GetEditingContext();
                if (context.Items.GetValue<ErrorItem>() == null)
                {
                    context.Items.SetValue(new ErrorItem { Message = e.Message, Details = e.ToString() });
                }
            }

            return propertyDescriptors;
        }
    }
}

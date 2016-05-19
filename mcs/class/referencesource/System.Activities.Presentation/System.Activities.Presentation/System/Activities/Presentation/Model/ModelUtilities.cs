//-----------------------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//-----------------------------------------------------------------------------

namespace System.Activities.Presentation.Model
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Runtime;

    // This class provides useful shared utility functions that are
    // needed by our ModelItemImpl class implementations.
    internal static class ModelUtilities
    {
        internal static bool IsSwitchCase(ModelItem modelItem)
        {
            if (IsModelItemKeyValuePair(modelItem.ItemType))
            {
                if (modelItem.Parent != null && //modelItem.Parent - ItemsCollection
                    modelItem.Parent.Parent != null && //modelItem.Parent.Parent - Cases
                    modelItem.Parent.Parent.Parent != null && //modelItem.Parent.Parent.Parent - Switch
                    modelItem.Parent.Parent.Parent.ItemType.IsGenericType &&
                    modelItem.Parent.Parent.Parent.ItemType.GetGenericTypeDefinition() == typeof(System.Activities.Statements.Switch<>))
                {
                    return true;
                }
            }
            return false;
        }

        internal static bool IsModelItemKeyValuePair(Type type)
        {
            Fx.Assert(type != null, "Parameter type is null!");
            return type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ModelItemKeyValuePair<,>);
        }

        // Returns a wrapped type converter for the given item.
        internal static TypeConverter GetConverter(IModelTreeItem item)
        {
            return GetConverter(item.ModelTreeManager, item.ModelItem);
        }

        // Returns a wrapped converter for the given item.
        internal static TypeConverter GetConverter(ModelTreeManager modelTreeManager, ModelItem item)
        {
            return new ModelTypeConverter(modelTreeManager, XamlUtilities.GetConverter(item.ItemType));
        }

        // Returns the default property on the item, or null if the item has
        internal static PropertyDescriptor GetDefaultProperty(ModelItem item)
        {
            DefaultPropertyAttribute propAttr = TypeDescriptor.GetAttributes(item.ItemType)[typeof(DefaultPropertyAttribute)] as DefaultPropertyAttribute;
            if (propAttr != null && !string.IsNullOrEmpty(propAttr.Name))
            {
                ModelProperty prop = item.Properties.Find(propAttr.Name);
                if (prop != null)
                {
                    return new ModelPropertyDescriptor(prop);
                }
            }
            return null;
        }

        // Wraps an item's properties in PropertyDescriptors and returns a
        // collection of them.
        internal static PropertyDescriptorCollection WrapProperties(ModelItem item)
        {
            List<PropertyDescriptor> descriptors = new List<PropertyDescriptor>();
            foreach (ModelProperty prop in item.Properties)
            {
                descriptors.Add(new ModelPropertyDescriptor(prop));
            }
            return new PropertyDescriptorCollection(descriptors.ToArray(), true);
        }


        internal static ModelItem ReverseFindFirst(ModelItem start, Predicate<ModelItem> matcher)
        {
            Fx.Assert(start != null, "start should not be null");
            Fx.Assert(matcher != null, "matcher should not be null");

            ModelItem result = null;
            ModelUtilities.ReverseTraverse(start, (ModelItem current) =>
            {
                if (matcher(current))
                {
                    result = current;
                    return false;
                }

                return true;
            });
            return result;
        }

        // Traverse model graph via ModelItem's parent. Stop traversing if shouldContinue returns false
        internal static void ReverseTraverse(ModelItem start, Predicate<ModelItem> shouldContinue)
        {
            Fx.Assert(start != null, "start should not be null");
            Fx.Assert(shouldContinue != null, "shouldContinue should not be null");

            HashSet<ModelItem> visited = new HashSet<ModelItem>();
            ModelItem current = start;

            while (current != null)
            {
                if (!shouldContinue(current))
                {
                    return;
                }

                visited.Add(current);
                current = current.Parent;
                if (visited.Contains(current))
                {
                    return;
                }
            }
        }
    }
}

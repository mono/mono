//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Text;
    using System.Windows.Markup;
    using System.Activities.Presentation.Model;
    using System.Activities.Presentation.PropertyEditing;

    // <summary>
    // Static class full of useful helpers
    // </summary>
    internal static class ModelUtilities
    {
        public static Type GetPropertyType(IEnumerable<ModelProperty> propertySet)
        {
            // all the ModelProperty should be of the same type, so returning the first type.
            foreach (ModelProperty property in propertySet)
            {
                return property.PropertyType;
            }
            return null;
        }

        // <summary>
        // Compares the name and Type of the specified ModelProperties,
        // returning true if they are equal.
        // </summary>
        // <param name="a">ModelProperty A</param>
        // <param name="b">ModelProperty B</param>
        // <returns>True if the names and Types of the specified ModelProperties
        // match, false otherwise.</returns>
        public static bool AreEquivalent(ModelProperty a, ModelProperty b)
        {
            return object.Equals(a.Name, b.Name) &&
                object.Equals(a.PropertyType, b.PropertyType);
        }

        // <summary>
        // Gets the underlying value object of the specified ModelProperty.  MarkupExtensions
        // (resources and such) will be returned as they are, with the exception of NullExtension,
        // which will be returned as null.
        // </summary>
        // <param name="property">ModelProperty to ---- open (can be null)</param>
        // <returns>Underlying value object, if any</returns>
        public static object GetSafeRawValue(ModelProperty property)
        {
            return GetSafeValue(property, false);
        }

        // <summary>
        // Gets the underlying computed value object of the specified ModelProperty.  MarkupExtensions
        // (resources and such) will be resolved into their final value.
        // </summary>
        // <param name="property">ModelProperty to ---- open (can be null)</param>
        // <returns>Underlying value object, if any</returns>
        public static object GetSafeComputedValue(ModelProperty property)
        {
            return GetSafeValue(property, true);
        }

        private static object GetSafeValue(ModelProperty property, bool resolveReferences)
        {
            if (property == null)
            {
                return null;
            }

            object value;

            // We have to special case TextBlock due to IAddChild behavior with Text and Inlines
            if (resolveReferences && !(typeof(System.Windows.Controls.TextBlock).IsAssignableFrom(property.Parent.ItemType) &&
                property.Name.Equals(System.Windows.Controls.TextBlock.TextProperty.Name)))
            {
                value = property.ComputedValue;
            }
            else
            {
                value = property.Value == null ? null : property.Value.GetCurrentValue();
            }

            if (value == null || value.GetType().Equals(typeof(NullExtension)))
            {
                return null;
            }

            return value;
        }

        // <summary>
        // Looks for the x:Name or Name property of the given PropertyValue and returns it if found.
        // Note: this method is expensive because it evaluates all the sub-properties of the given
        // PropertyValue.
        // </summary>
        // <param name="propertyValue">PropertyValue instance to look at</param>
        // <returns>Name if the PropertyValue defines one, null otherwise</returns>
        public static string GetPropertyName(PropertyValue propertyValue)
        {
            if (propertyValue == null)
            {
                return null;
            }

            if (propertyValue.HasSubProperties)
            {
                PropertyEntry nameProperty = propertyValue.SubProperties["Name"];
                if (nameProperty != null)
                {
                    return nameProperty.PropertyValue.StringValue;
                }
            }

            return null;
        }

        // <summary>
        // Returns ',' separated property name for sub-properties, going all the way
        // to the root ancestor in the property editing OM.  (ie. you get strings
        // such as 'ContextMenu,IsEnabled' instead of just 'IsEnabled'.
        // </summary>
        // <param name="property">Property to get the name of</param>
        // <returns>',' separated property name for sub-properties</returns>
        public static string GetSubPropertyHierarchyPath(PropertyEntry property)
        {
            if (property == null)
            {
                return null;
            }

            if (property.ParentValue == null)
            {
                return property.PropertyName;
            }

            StringBuilder sb = new StringBuilder();
            do
            {
                if (sb.Length > 0)
                {
                    sb.Insert(0, ',');
                }

                sb.Insert(0, property.PropertyName);
                property = property.ParentValue == null ? null : property.ParentValue.ParentProperty;

            } while (property != null && !(property is ModelPropertyIndexer));

            return sb.ToString();
        }

        // <summary>
        // Same as GetSubPropertyHierarchyPath(), but it looks up a cached version
        // of this path, if one exists, or calculates one from scratch and caches it
        // if it doesn't.
        // </summary>
        // <param name="property">Property to get the name of</param>
        // <returns>',' separated property name for sub-properties</returns>
        public static string GetCachedSubPropertyHierarchyPath(PropertyEntry property)
        {
            ModelPropertyEntry mpe = property as ModelPropertyEntry;
            return mpe == null ? GetSubPropertyHierarchyPath(property) : mpe.SubPropertyHierarchyPath;
        }

        // <summary>
        // Determines whether the specified type is implement generic Ilist interface.
        // </summary>
        // <param name="type">The type.</param>
        // <returns>
        // <c>true</c> if the specified type is implement generic Ilist interface;otherwise, <c>false</c>.
        // </returns>
        public static bool ImplementsIList(Type type)
        {
            bool ret = false;
            if (!type.IsGenericType)
            {
                ret = false;
            }
            Type[] interfaceTypes = type.GetInterfaces();
            foreach (Type interfaceType in interfaceTypes)
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(IList<>))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        // <summary>
        // Determines whether the specified type is implement generic ICollection interface.
        // </summary>
        // <param name="type">The type.</param>
        // <returns>
        // <c>true</c> if the specified type is implement generic ICollection interface;otherwise, <c>false</c>.
        // </returns>
        public static bool ImplementsICollection(Type type)
        {
            bool ret = false;
            if (!type.IsGenericType)
            {
                ret = false;
            }
            Type[] interfaceTypes = type.GetInterfaces();
            foreach (Type interfaceType in interfaceTypes)
            {
                if (interfaceType.IsGenericType &&
                    interfaceType.GetGenericTypeDefinition() == typeof(ICollection<>))
                {
                    ret = true;
                    break;
                }
            }
            return ret;
        }

        // <summary>
        // Tries to determine the common type ancestor of the specified types
        // </summary>
        // <param name="t1">Type 1</param>
        // <param name="t2">Type 2</param>
        // <returns>Common ancestor Type, if any</returns>
        public static Type GetCommonAncestor(Type t1, Type t2)
        {
            if (t1 == null || t2 == null)
            {
                return null;
            }

            if (t1 == typeof(object) || t2 == typeof(object))
            {
                return typeof(object);
            }

            if (t1.IsAssignableFrom(t2))
            {
                return t1;
            }

            while (t2 != typeof(object))
            {
                if (t2.IsAssignableFrom(t1))
                {
                    return t2;
                }

                t2 = t2.BaseType;
            }

            return typeof(object);
        }
    }
}

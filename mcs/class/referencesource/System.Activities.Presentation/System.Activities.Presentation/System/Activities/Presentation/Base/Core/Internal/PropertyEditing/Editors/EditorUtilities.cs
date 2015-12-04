//----------------------------------------------------------------
// Copyright (c) Microsoft Corporation.  All rights reserved.
//----------------------------------------------------------------
namespace System.Activities.Presentation.Internal.PropertyEditing.Editors 
{
    using System;
    using System.Collections;
    using System.Globalization;
    using System.Reflection;
    using System.Activities.Presentation.PropertyEditing;
    using System.Activities.Presentation.Internal.PropertyEditing.FromExpression.Framework.PropertyInspector;

    // <summary>
    // Collection of utilities used by the various value editors
    // </summary>
    internal static class EditorUtilities 
    {

        // Key = Type, Value = bool
        private static Hashtable _cachedLookups = new Hashtable();

        // <summary>
        // Checks to see whether the specified Type is concrete and has a default constructor.
        // That information if both returned and cached for future reference.
        //
        // NOTE: This method does not handle structs correctly because it will return FALSE
        // for struct types, which is incorrect.  However, this bug has its counter-part in
        // System.Activities.Presentation.dll where the default NewItemFactory only instantiates
        // non-struct classes.  Both of these need to be fixed at the same time because
        // they are used in conjunction.  However, MWD is currently locked.
        //
        // </summary>
        // <param name="type">Type to verify</param>
        // <returns>True if the specified type is concrete and has a default constructor,
        // false otherwise.</returns>
        public static bool IsConcreteWithDefaultCtor(Type type) 
        {

            object returnValue = _cachedLookups[type];
            if (returnValue == null) 
            {
                if (type == null || type.IsAbstract) 
                {
                    returnValue = false;
                }
                else 
                {
                    ConstructorInfo defaultCtor = type.GetConstructor(Type.EmptyTypes);
                    returnValue = (defaultCtor != null && defaultCtor.IsPublic);
                }

                _cachedLookups[type] = returnValue;
            }

            return (bool)returnValue;
        }

        // <summary>
        // Substitutes user-friendly display names for values of properties
        // </summary>
        // <param name="item">Item to attempt to identify</param>
        // <returns>String value for the item (guaranteed to be non-null)</returns>
        public static string GetDisplayName(object item) 
        {
            if (item == null)
            {
                return string.Empty;
            }

            // Display a user-friendly string for PropertyValues
            PropertyValue propertyValue = item as PropertyValue;
            if (propertyValue != null)
            {
                return PropertyValueToDisplayNameConverter.Instance.Convert(
                    propertyValue, typeof(string), null, CultureInfo.CurrentCulture).ToString();
            }

            // Display a user-friendly string for NewItemFactoryTypeModels
            NewItemFactoryTypeModel model = item as NewItemFactoryTypeModel;
            if (model != null)
            {
                return NewItemFactoryTypeModelToDisplayNameConverter.Instance.Convert(
                    model, typeof(string), null, CultureInfo.CurrentCulture).ToString();
            }

            // Otherwise, resort to ToString() implementation
            return item.ToString();
        }

        // <summary>
        // Tests whether a type t is a nullable enum type
        // </summary>
        // <param name="t">The type object to be tested</param>
        // <returns>A bool indicating the test result</returns>
        public static bool IsNullableEnumType(Type t)
        {
            if (t.IsGenericType && t.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                Type[] genericArgs = t.GetGenericArguments();
                if (genericArgs != null && genericArgs.Length == 1)
                {
                    return genericArgs[0].IsEnum;
                }
            }

            return false;
        }

        public const string NullString = "(null)";
    }
}

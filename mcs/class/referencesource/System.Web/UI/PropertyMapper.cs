//------------------------------------------------------------------------------
// <copyright file="PropertyMapper.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//------------------------------------------------------------------------------

/*
 * PropertyMapper.cs
 *
 * Copyright (c) 1999 Microsoft Corporation
 */

namespace System.Web.UI {
    using System;
    using System.Collections;
    using System.Reflection;
    using System.Web.Util;
    using System.ComponentModel;

    internal sealed class PropertyMapper {
        private const char PERSIST_CHAR = '-';
        private const char OM_CHAR = '.';
        private const string STR_OM_CHAR = ".";


        /*
         * Maps persisted attribute names to the object model equivalents.
         * This class should not be instantiated by itself.
         */
        private PropertyMapper() {
        }

        /*
         * Returns the PropertyInfo or FieldInfo corresponding to the
         * specified property name.
         */
        internal static MemberInfo GetMemberInfo(Type ctrlType, string name, out string nameForCodeGen) {

            Type currentType = ctrlType;
            PropertyInfo propInfo = null;
            FieldInfo fieldInfo = null;

            string mappedName = MapNameToPropertyName(name);
            nameForCodeGen = null;

            int startIndex = 0;
            while (startIndex < mappedName.Length) {   // parse thru dots of object model to locate PropertyInfo
                string propName;
                int index = mappedName.IndexOf(OM_CHAR, startIndex);

                if (index < 0) {
                    propName = mappedName.Substring(startIndex);
                    startIndex = mappedName.Length;
                }
                else {
                    propName = mappedName.Substring(startIndex, index - startIndex);
                    startIndex = index + 1;
                }

                BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static | BindingFlags.IgnoreCase;

                // 



                try {
                    propInfo = TargetFrameworkUtil.GetProperty(currentType, propName, flags);
                }
                catch (AmbiguousMatchException) {
                    flags |= BindingFlags.DeclaredOnly;
                    propInfo = TargetFrameworkUtil.GetProperty(currentType, propName, flags);
                }

                if (propInfo == null) {   // could not find a public property, look for a public field
                    fieldInfo = TargetFrameworkUtil.GetField(currentType, propName, flags);
                    if (fieldInfo == null) {
                        nameForCodeGen = null;
                        break;
                    }
                }

                propName = null;
                if (propInfo != null) {   // found a public property
                    currentType = propInfo.PropertyType;
                    propName = propInfo.Name;
                }
                else {   // found a public field
                    currentType = fieldInfo.FieldType;
                    propName = fieldInfo.Name;
                }

                // Throw if the type of not CLS-compliant (ASURT 83438)
                if (!IsTypeCLSCompliant(currentType)) {
                    throw new HttpException(SR.GetString(SR.Property_Not_ClsCompliant, name, ctrlType.FullName, currentType.FullName));
                }

                if (propName != null) {
                    if (nameForCodeGen == null)
                        nameForCodeGen = propName;
                    else
                        nameForCodeGen += STR_OM_CHAR + propName;
                }
            }

            if (propInfo != null)
                return propInfo;
            else
                return fieldInfo;
        }

        private static bool IsTypeCLSCompliant(Type type) {

            // We used to check this by looking up the CLSCompliantAttribute, but that was
            // way too inefficent.  So instead, we just juck for a few specific types
            if ((type == typeof(SByte)) ||
                (type == typeof(TypedReference)) ||
                (type == typeof(UInt16)) ||
                (type == typeof(UInt32)) ||
                (type == typeof(UInt64)) ||
                (type == typeof(UIntPtr))) {
                return false;
            }

            return true;
        }

        /*
         * Maps the specified persisted name to its object model equivalent.
         * The convention is to map all dashes to dots.
         * For example :  Font-Size maps to Font.Size
         *                HeaderStyle-Font-Name maps to HeaderStyle.Font.Name
         */
        internal static string MapNameToPropertyName(string attrName) {
            return attrName.Replace(PERSIST_CHAR,OM_CHAR);
        }

        internal static object LocatePropertyObject(object obj, string mappedName, out string propertyName, bool inDesigner) {
            object currentObject = obj;
            Type currentType = obj.GetType();
            propertyName = null;
            int index;
            int startIndex = 0;

            // step through the dots of the object model to extract the PropertyInfo
            // and object on which the property will be set
            while (startIndex < mappedName.Length) {
                index = mappedName.IndexOf(OM_CHAR, startIndex);

                // If we didn't find a separator, we're on the last piece
                if (index < 0)
                    break;

                // There is a sub property, so get its info and iterate

                propertyName = mappedName.Substring(startIndex, index - startIndex);
                startIndex = index + 1;

                currentObject = FastPropertyAccessor.GetProperty(currentObject, propertyName, inDesigner);

                if (currentObject == null)
                    return null;
            }

            // Avoid a useless call to Substring if possible
            if (startIndex > 0)
                propertyName = mappedName.Substring(startIndex);
            else
                propertyName = mappedName;

            return currentObject;
        }

        /*
         * Walks the object model using the mapped property name to get the
         * value of an instance's property.
         */
        internal static PropertyDescriptor GetMappedPropertyDescriptor(object obj, string mappedName, out object childObject, out string propertyName, bool inDesigner)
        {
            childObject = LocatePropertyObject(obj, mappedName, out propertyName, inDesigner);
            if (childObject == null) return null;
            PropertyDescriptorCollection properties = TargetFrameworkUtil.GetProperties(childObject);
            return properties[propertyName];
        }


        /*
         * Walks the object model using the mapped property name to set the
         * value of an instance's property.
         */
        internal static void SetMappedPropertyValue(object obj, string mappedName, object value, bool inDesigner) {
            string propertyName;
            object childObj = LocatePropertyObject(obj, mappedName, out propertyName, inDesigner);
            if (childObj == null) return;
            FastPropertyAccessor.SetProperty(childObj, propertyName, value, inDesigner);
        }

    }
}

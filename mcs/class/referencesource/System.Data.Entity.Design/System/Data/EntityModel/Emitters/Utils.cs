//---------------------------------------------------------------------
// <copyright file="Utils.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.EntityModel.SchemaObjectModel;
using System.Data.Entity.Design.Common;
using System.Diagnostics;
using System.Reflection;


namespace System.Data.EntityModel.Emitters
{
    /// <summary>
    /// Summary description for Utils
    /// </summary>
    internal static class Utils
    {
        #region Static Fields

        public const string AdoFrameworkNamespace = "System.Data.Objects";
        public const string AdoFrameworkDataClassesNamespace = "System.Data.Objects.DataClasses";
        public const string AdoFrameworkMetadataEdmNamespace = "System.Data.Metadata.Edm";
        public const string AdoEntityClientNamespace = "System.Data.EntityClient";

        public const string SetValidValueMethodName = "SetValidValue";
        public const string ReportPropertyChangingMethodName = "ReportPropertyChanging";
        public const string ReportPropertyChangedMethodName = "ReportPropertyChanged";
        public const string GetValidValueMethodName = "GetValidValue";
        public const string VerifyComplexObjectIsNotNullName = "VerifyComplexObjectIsNotNull";


        // to guarantee uniqueness these must all be unique, begin with and end with an underscore and not contain internal underscores
        private static string[] _privateMemberPrefixes = new string[(int)PrivateMemberPrefixId.Count]
        {
            "_",
            "_Initialize_",
            "PropertyInfo",
            "_pi",
        };

        // suffix that is added to field names to create a boolean field used to indicate whether or
        // not a complex property has been explicitly initialized 
        private static string _complexPropertyInitializedSuffix = "Initialized";
        private static List<KeyValuePair<string, Type>> _typeReservedNames = InitializeTypeReservedNames();

        /// <summary>
        /// Initialize some statics that cannot be initialized in member declaration...
        /// </summary>
        static List<KeyValuePair<string, Type>> InitializeTypeReservedNames()
        {
            Dictionary<string, Type> typeReservedNames = new Dictionary<string, Type>(StringComparer.Ordinal);
            BindingFlags bindingFlags = BindingFlags.FlattenHierarchy | BindingFlags.NonPublic | BindingFlags.Public 
                    | BindingFlags.Instance | BindingFlags.Static;
            foreach (MemberInfo member in TypeReference.ComplexTypeBaseClassType.GetMembers(bindingFlags))
            {
                if (ShouldReserveName(member))
                {
                    if (!typeReservedNames.ContainsKey(member.Name))
                    {
                        typeReservedNames.Add(member.Name, typeof(ComplexType));
                    }
                }
            }

            foreach (MemberInfo member in TypeReference.EntityTypeBaseClassType.GetMembers(bindingFlags))
            {
                if (ShouldReserveName(member))
                {
                    if (typeReservedNames.ContainsKey(member.Name))
                    {
                        if (typeReservedNames[member.Name] == typeof(ComplexType))
                        {
                            // apply to all types
                            typeReservedNames[member.Name] = null;
                        }
                    }
                    else
                    {
                        typeReservedNames.Add(member.Name, typeof(EntityType));
                    }
                }
            }

            List<KeyValuePair<string, Type>> pairs = new List<KeyValuePair<string, Type>>();
            foreach (KeyValuePair<string, Type> pair in typeReservedNames)
            {
                pairs.Add(pair);
            }

            return pairs;
        }

        private static bool ShouldReserveName(MemberInfo member)
        {
            if (member is EventInfo)
            {
                return ShouldReserveName((EventInfo)member);
            }
            else if(member is FieldInfo)
            {
                return ShouldReserveName((FieldInfo)member);
            }
            else if(member is MethodBase)
            {
                return ShouldReserveName((MethodBase)member);
            }
            else if(member is PropertyInfo)
            {
                return ShouldReserveName((PropertyInfo)member);
            }
            else
            {
                Debug.Assert(member is Type, "Did you add a new type of member?");
                return ShouldReserveName((Type)member);
            }

        }

        private static bool ShouldReserveName(EventInfo member)
        {
            bool hasNonPrivate = false;

            MethodInfo miAdd = member.GetAddMethod();
            if (miAdd != null)
            {
                hasNonPrivate |= ShouldReserveName(miAdd, false);
            }

            MethodInfo miRemove = member.GetRemoveMethod();
            if (miRemove != null)
            {
                hasNonPrivate |= ShouldReserveName(miRemove, false);
            }

            return hasNonPrivate;
        }

        private static bool ShouldReserveName(PropertyInfo member)
        {
            bool hasNonPrivate = false;

            MethodInfo miSet = member.GetSetMethod();
            if (miSet != null)
            {
                hasNonPrivate |= ShouldReserveName(miSet, false);
            }

            MethodInfo miGet = member.GetGetMethod();
            if (miGet != null)
            {
                hasNonPrivate |= ShouldReserveName(miGet, false);
            }

            return hasNonPrivate;
        }

        private static bool ShouldReserveName(FieldInfo member)
        {
            return !member.IsPrivate && !member.IsAssembly && 
                !member.IsSpecialName;
        }

        private static bool ShouldReserveName(MethodBase member, bool checkForSpecial)
        {
            return !member.IsPrivate && !member.IsAssembly &&
                (!checkForSpecial || !member.IsSpecialName);
        }

        private static bool ShouldReserveName(MethodBase member)
        {
            return ShouldReserveName(member, true);
        }

        private static bool ShouldReserveName(Type member)
        {
            // we don't want to keep types
            return false;
        }
        #endregion

        #region Public Methods
        /// <summary>
        /// 
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Globalization", "CA1308:NormalizeStringsToUppercase")]
        public static string CamelCase(string text)
        {
            if ( string.IsNullOrEmpty(text) )
                return text;

            if ( text.Length == 1 )
                return text[0].ToString(System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant();

            return text[0].ToString(System.Globalization.CultureInfo.InvariantCulture).ToLowerInvariant()+text.Substring(1);
        }

        public static string FixParameterName(string name)
        {
            // FxCop consider 'iD' as violation, we will change any property that is 'id'(case insensitive) to 'ID'
            if (StringComparer.OrdinalIgnoreCase.Equals(name, "id"))
            {
                // it is an abreviation not an acronym so it should be all lower case
                return "id";
            }
            return CamelCase(name);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="propName"></param>
        /// <returns></returns>
        public static string FieldNameFromPropName(string propName)
        {
            return PrivateMemberPrefix(PrivateMemberPrefixId.Field)+propName;
        }

        /// <summary>
        /// Generate the name of a field that is used to indicate whether a complex property has been explicitly initialized
        /// </summary>
        /// <param name="propName">Name of the property associated that with this field</param>
        /// <returns>Generated field name</returns>
        public static string ComplexPropertyInitializedNameFromPropName(string propName)
        {
            return FieldNameFromPropName(propName) + _complexPropertyInitializedSuffix;
        }
        
        /// <summary>
        /// get the prefix ussed for a private member
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public static string PrivateMemberPrefix(PrivateMemberPrefixId id)
        {
            return _privateMemberPrefixes[(int)id];
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FQAdoFrameworkName( string name )
        {
            return AdoFrameworkNamespace + "." + name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FQAdoFrameworkDataClassesName( string name )
        {
            return AdoFrameworkDataClassesNamespace + "." + name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">unqualifed name of the type</param>
        /// <returns></returns>
        public static string FQAdoFrameworkMetadataEdmName(string name)
        {
            return AdoFrameworkMetadataEdmNamespace + "." + name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string FQAdoEntityClientName(string name)
        {
            return AdoEntityClientNamespace + "." + name;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="element"></param>
        /// <param name="modelType"></param>
        /// <returns></returns>
        public static bool TryGetPrimitiveTypeKind(EdmType type, out PrimitiveTypeKind modelType )
        {
            if (!MetadataUtil.IsPrimitiveType(type))
            {
                // set it to something bogus because I have to
                modelType = PrimitiveTypeKind.Binary;
                return false;
            }

            modelType = ((PrimitiveType)type).PrimitiveTypeKind;
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public static string[] SplitName(string name)
        {
            Debug.Assert(!string.IsNullOrEmpty(name), "name parameter is null or empty");

            if ( name.Length > 0 && name[0] == '.' )
                return name.Substring(1).Split('.');

            return name.Split('.');
        }

        public static string GetFullyQualifiedCodeGenerationAttributeName(string attribute)
        {
            return XmlConstants.CodeGenerationSchemaNamespace + ":" + attribute;
        }

        /// <summary>
        /// check if a name is reserved for a type
        /// </summary>
        /// <param name="type">the object representing the schema type being defined</param>
        /// <param name="name">the member name</param>
        /// <returns>true if the name is reserved by the type</returns>
        public static bool DoesTypeReserveMemberName(StructuralType type, string name, StringComparison comparison)
        {
            Type reservingType = null;
            if (!TryGetReservedName(name,comparison, out reservingType))
            {
                return false;
            }

            // if reserving types is null it means the name is reserved for all types.
            if (reservingType == null)
            {
                return true;
            }

            return (reservingType == type.GetType());
        }

        public static bool TryGetReservedName(string name, StringComparison comparison, out Type applyToSpecificType)
        {
            applyToSpecificType = null;
            foreach(KeyValuePair<string, Type> pair in _typeReservedNames)
            {
                if (pair.Key.Equals(name, comparison))
                {
                    applyToSpecificType = pair.Value;
                    return true;
                }
            }

            return false;
        }


        #endregion
    }
}

//---------------------------------------------------------------------
// <copyright file="EntityDataSourceUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Data.Metadata.Edm;
using System.Data.Spatial;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace System.Web.UI.WebControls
{
    internal static class EntityDataSourceUtil
    {
        internal static readonly string EntitySqlElementAlias = "it";

        internal static T CheckArgumentNull<T>(T value, string parameterName) where T : class
        {
            if (null == value)
            {
                ThrowArgumentNullException(parameterName);
            }
            return value;
        }


        /// <summary>
        /// Indicates whether the given property name exists on the result.
        /// The result could be indicated by a wrapperCollection, an entitySet or a typeUsage,
        /// any of which could be null.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="wrapperCollection"></param>
        /// <param name="entitySet"></param>
        /// <param name="tu"></param>
        /// <returns></returns>
        internal static bool PropertyIsOnEntity(string propertyName, EntityDataSourceWrapperCollection wrapperCollection, EntitySet entitySet, TypeUsage tu)
        {
            bool propertyIsOnEntity = false;
            if (null != wrapperCollection)
            {
                // check for descriptor
                if (null != wrapperCollection.GetItemProperties(null).Find(propertyName, /*ignoreCase*/ false))
                {
                    propertyIsOnEntity = true;
                }
            }
            if (null != tu)
            {
                ReadOnlyMetadataCollection<EdmMember> members = null;
                switch (tu.EdmType.BuiltInTypeKind)
                {
                    case BuiltInTypeKind.RowType:
                        members = ((RowType)(tu.EdmType)).Members;
                        break;
                    case BuiltInTypeKind.EntityType:
                        members = ((EntityType)(tu.EdmType)).Members;
                        break;
                }
                if (null != members && members.Contains(propertyName))
                {
                    propertyIsOnEntity = true;
                }
            }
            if (null != entitySet)
            {
                if ( ((EntityType)(entitySet.ElementType)).Members.Contains(propertyName) )
                {
                    propertyIsOnEntity = true;
                }
            }
            return propertyIsOnEntity;
        }


        /// <summary>
        /// Returns the value set onto the Parameter named by propertyName.
        /// If the Paramter does not have a value, it returns null.
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="parameterCollection"></param>
        /// <param name="entityDataSource"></param>
        /// <returns></returns>
        internal static object GetParameterValue(string propertyName, ParameterCollection parameterCollection,
                                                 EntityDataSource entityDataSource)
        {
            if (null == parameterCollection) // ParameterCollection undefined
            {
                return null;
            }

            System.Collections.Specialized.IOrderedDictionary values = 
                parameterCollection.GetValues(entityDataSource.HttpContext, entityDataSource);

            foreach (object key in values.Keys)
            {
                string parameterName = key as string;
                if (null != parameterName && String.Equals(propertyName, parameterName, StringComparison.Ordinal))
                {
                    return values[parameterName];
                }
            }

            return null;
        }
        

        /// <summary>
        /// Get the System.Web.UI.WebControls.Parameter that matches the name in the given ParameterCollection
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="parameterCollection"></param>
        /// <returns></returns>
        internal static Parameter GetParameter(string propertyName, ParameterCollection parameterCollection)
        {
            if (null == parameterCollection)
            {
                return null;
            }

            foreach (Parameter p in parameterCollection)
            {
                if (String.Equals(p.Name, propertyName, StringComparison.Ordinal))
                {
                    return p;
                }
            }
            return null;
        }

        
        /// <summary>
        /// Validates that the keys in the update parameters all match property names on the entityWrapper.
        /// </summary>
        /// <param name="entityWrapper"></param>
        /// <param name="parameters"></param>
        internal static void ValidateWebControlParameterNames(EntityDataSourceWrapper entityWrapper, 
                                                     ParameterCollection parameters,
                                                     EntityDataSource owner)
        {
            Debug.Assert(null != entityWrapper, "entityWrapper should not be null");
            if (null != parameters)
            {
                PropertyDescriptorCollection entityProperties = entityWrapper.GetProperties();
                System.Collections.Specialized.IOrderedDictionary parmVals = parameters.GetValues(owner.HttpContext, owner);
                foreach (DictionaryEntry de in parmVals)
                {
                    string key = de.Key as string;
                    if (null == key || null == entityProperties.Find(key, false))
                    {
                        throw new InvalidOperationException(Strings.EntityDataSourceUtil_InsertUpdateParametersDontMatchPropertyNameOnEntity(key, entityWrapper.WrappedEntity.GetType().ToString()));
                    }
                }
            }
        }


        /// <summary>
        /// Verifies that the query's typeusage will not result in a polymorphic result.
        /// If the query would be restricted "is of only" using entityTypeFilter, then
        /// this check assumes the result will not be polymorphic.
        /// 
        /// This method is only called if the user specifies EntitySetName and updates are enabled.
        /// 
        /// Does nothing for RowTypes.
        /// </summary>
        /// <param name="typeUsage">The TypeUsage from the query</param>
        /// <param name="itemCollection"></param>
        /// <returns></returns>
        internal static void CheckNonPolymorphicTypeUsage(EntityType entityType,
                                                          ItemCollection ocItemCollection,
                                                          string entityTypeFilter)
        {
            CheckArgumentNull<ItemCollection>(ocItemCollection, "ocItemCollection");

            if (String.IsNullOrEmpty(entityTypeFilter))
            {
                List<EdmType> types = new List<EdmType>(EntityDataSourceUtil.GetTypeAndSubtypesOf(entityType, ocItemCollection, /*includeAbstractTypes*/true));
                if (entityType.BaseType != null ||
                    types.Count() > 1 || entityType.Abstract)
                {
                    throw new InvalidOperationException(Strings.EntityDataSourceUtil_EntityQueryCannotReturnPolymorphicTypes);
                }
            }

            return;
        }

        internal static IEnumerable<EdmType> GetTypeAndSubtypesOf(EntityType type, ReadOnlyCollection<GlobalItem> itemCollection, bool includeAbstractTypes)
        {
            if (includeAbstractTypes || !type.Abstract)
            {
                yield return type;
            }

            // Get entity sub-types
            foreach (EdmType subType in GetTypeAndSubtypesOf<EntityType>(type, itemCollection, includeAbstractTypes))
            {
                yield return subType;
            }

            // Get complex sub-types
            foreach (EdmType subType in GetTypeAndSubtypesOf<ComplexType>(type, itemCollection, includeAbstractTypes))
            {
                yield return subType;
            }
        }

        internal static bool IsTypeOrSubtypeOf(EntityType superType, EntityType derivedType, ReadOnlyCollection<GlobalItem> itemCollection)
        {
            IEnumerable types = GetTypeAndSubtypesOf(superType, itemCollection, false);
            foreach(EdmType type in types)
            {
                if (type == derivedType)
                {
                    return true;
                }
            }
            return false;
        }

        internal static Type GetClrType(MetadataWorkspace ocWorkspace, StructuralType edmType)
        {
            var oSpaceType = (StructuralType)ocWorkspace.GetObjectSpaceType(edmType);
            var objectItemCollection = (ObjectItemCollection)(ocWorkspace.GetItemCollection(DataSpace.OSpace));
            return objectItemCollection.GetClrType(oSpaceType);
        }

        internal static Type GetClrType(MetadataWorkspace ocWorkspace, EnumType edmType)
        {
            var oSpaceType = (EnumType)ocWorkspace.GetObjectSpaceType(edmType);
            var objectItemCollection = (ObjectItemCollection)(ocWorkspace.GetItemCollection(DataSpace.OSpace));
            return objectItemCollection.GetClrType(oSpaceType);
        }

        internal static ConstructorInfo GetConstructorInfo(Type type)
        {
            Debug.Assert(null != type, "type required");
            ConstructorInfo constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, System.Type.EmptyTypes, null);

            if (null == constructorInfo)
            {
                throw new InvalidOperationException(Strings.DefaultConstructorNotFound(type));
            }

            return constructorInfo;
        }

        internal static PropertyInfo GetPropertyInfo(Type type, string name)
        {
            Debug.Assert(null != type, "type required");
            Debug.Assert(null != name, "name required");

            PropertyInfo propertyInfo = type.GetProperty(name, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance, null, null, Type.EmptyTypes, null);

            if (null == propertyInfo)
            {
                throw new InvalidOperationException(Strings.PropertyNotFound(name, type));
            }

            return propertyInfo;
        }

        internal static object InitializeType(Type type)
        {
            ConstructorInfo constructorInfo = GetConstructorInfo(type);
            return constructorInfo.Invoke(new object[] { });
        }

        /// <summary>
        /// Given a data source column name, returns the corresponding Entity-SQL. If
        /// we are using the wrapper, we defer to the property descriptor to get
        /// the string. If there is no wrapper (or no corresponding property descriptor)
        /// we use the column name directly.
        /// </summary>
        /// <param name="columnName">Column name for which we produce a value expression.</param>
        /// <returns>Entity-SQL for column.</returns>
        internal static string GetEntitySqlValueForColumnName(string columnName, EntityDataSourceWrapperCollection wrapperCollection)
        {
            Debug.Assert(!String.IsNullOrEmpty(columnName), "columnName must be given");

            string result = null;

            if (wrapperCollection != null)
            {
                // use wrapper definition if it is available
                EntityDataSourceWrapperPropertyDescriptor descriptor =
                    wrapperCollection.GetItemProperties(null).Find(columnName, false) as EntityDataSourceWrapperPropertyDescriptor;
                if (null != descriptor)
                {
                    result = descriptor.Column.GetEntitySqlValue();
                }
            }

            // if descriptor does not provide SQL, create the default: it._columnName_
            if (null == result)
            {
                result = EntitySqlElementAlias + "." + QuoteEntitySqlIdentifier(columnName);
            }

            return result;
        }

        internal static Type ConvertTypeCodeToType(TypeCode typeCode)
        {
            switch (typeCode)
            {
                case TypeCode.Boolean: 
                    return typeof(Boolean); 

                case TypeCode.Byte:
                    return typeof(Byte);

                case TypeCode.Char:
                    return typeof(Char);

                case TypeCode.DateTime:
                    return typeof(DateTime);

                case TypeCode.DBNull:
                    return typeof(DBNull);

                case TypeCode.Decimal:
                    return typeof(Decimal);

                case TypeCode.Double:
                    return typeof(Double);

                case TypeCode.Empty:
                    return null;

                case TypeCode.Int16:
                    return typeof(Int16);

                case TypeCode.Int32:
                    return typeof(Int32);

                case TypeCode.Int64:
                    return typeof(Int64);

                case TypeCode.Object:
                    return typeof(Object);

                case TypeCode.SByte:
                    return typeof(SByte);

                case TypeCode.Single:
                    return typeof(Single);

                case TypeCode.String:
                    return typeof(String);

                case TypeCode.UInt16:
                    return typeof(UInt16);

                case TypeCode.UInt32:
                    return typeof(UInt32);

                case TypeCode.UInt64:
                    return typeof(UInt64);

                default:
                    throw new InvalidOperationException(Strings.EntityDataSourceUtil_UnableToConvertTypeCodeToType(typeCode.ToString()));
            }
        }

        /// <summary>
        /// Converts a DB type code to a CLR type, bypassing CLR type codes since there
        /// is not a sufficient mapping.
        /// </summary>
        /// <param name="dbType">The DB type to convert</param>
        /// <returns>The mapped CLR type</returns>
        internal static Type ConvertDbTypeToType(DbType dbType)
        {
            switch (dbType)
            {
                case DbType.AnsiString:
                case DbType.AnsiStringFixedLength:
                case DbType.String:
                case DbType.StringFixedLength:
                    return typeof(String);
                case DbType.Boolean:
                    return typeof(Boolean);
                case DbType.Byte:
                    return typeof(Byte);
                case DbType.VarNumeric:     // 
                case DbType.Currency:
                case DbType.Decimal:
                    return typeof(Decimal);
                case DbType.Date:
                case DbType.DateTime:
                case DbType.DateTime2: // new Katmai type
                    return typeof(DateTime);
                case DbType.Time:      // new Katmai type
                    return typeof(TimeSpan);
                case DbType.Double:
                    return typeof(Double);
                case DbType.Int16:
                    return typeof(Int16);
                case DbType.Int32:
                    return typeof(Int32);
                case DbType.Int64:
                    return typeof(Int64);
                case DbType.SByte:
                    return typeof(SByte);
                case DbType.Single:
                    return typeof(Single);
                case DbType.UInt16:
                    return typeof(UInt16);
                case DbType.UInt32:
                    return typeof(UInt32);
                case DbType.UInt64:
                    return typeof(UInt64);
                case DbType.Guid:
                    return typeof(Guid);
                case DbType.DateTimeOffset: // new Katmai type
                    return typeof(DateTimeOffset);
                case DbType.Binary:
                    return typeof(byte[]);
                case DbType.Object:
                default:
                    return typeof(Object);
            }
        }

        private static IEnumerable<EdmType> GetTypeAndSubtypesOf<T_EdmType>(EdmType type, ReadOnlyCollection<GlobalItem> itemCollection, bool includeAbstractTypes)
            where T_EdmType : EdmType
        {
            // Get the subtypes of the type from the WorkSpace
            T_EdmType specificType = type as T_EdmType;
            if (specificType != null)
            {

                IEnumerable<T_EdmType> typesInWorkSpace = itemCollection.OfType<T_EdmType>();
                foreach (T_EdmType typeInWorkSpace in typesInWorkSpace)
                {
                    if (specificType.Equals(typeInWorkSpace) == false && IsStrictSubtypeOf(typeInWorkSpace, specificType))
                    {
                        if (includeAbstractTypes || !typeInWorkSpace.Abstract)
                        {
                            yield return typeInWorkSpace;
                        }

                    }
                }
            }
            yield break;
        }

        // requires: firstType is not null
        // effects: if otherType is among the base types, return true, 
        // otherwise returns false.
        // when othertype is same as the current type, return false.
        private static bool IsStrictSubtypeOf(EdmType firstType, EdmType secondType)
        {
            Debug.Assert(firstType != null, "firstType should not be not null");
            if (secondType == null)
            {
                return false;
            }

            // walk up my type hierarchy list
            for (EdmType t = firstType.BaseType; t != null; t = t.BaseType)
            {
                if (t == secondType)
                    return true;
            }
            return false;
        }

        internal static bool NullCanBeAssignedTo(Type type)
        {
            Debug.Assert(null != type, "type required");
            return !type.IsValueType || IsNullableType(type, out type);
        }

        internal static bool IsNullableType(Type type, out Type underlyingType)
        {
            Debug.Assert(null != type, "type required");
            if (type.IsGenericType && type.GetGenericTypeDefinition().Equals(typeof(Nullable<>)))
            {
                underlyingType = type.GetGenericArguments()[0];
                return true;
            }
            underlyingType = null;
            return false;
        }

        internal static void ThrowArgumentNullException(string parameterName)
        {
            throw ArgumentNull(parameterName);
        }

        internal static ArgumentNullException ArgumentNull(string parameter)
        {
            ArgumentNullException e = new ArgumentNullException(parameter);
            return e;
        }

        internal static object ConvertType(object value, Type type, string paramName)
        {
            // NOTE: This method came from ObjectDataSource via LinqDataSource.
            // It has been changed to support better parsing of decimal values.
            string s = value as string;
            if (s != null)
            {
                // Get the type converter for the destination type
                TypeConverter converter = TypeDescriptor.GetConverter(type);
                if (converter != null)
                {
                    // Perform the conversion
                    try
                    {
                        // If the requested type is decimal or a spatial type, then first try to parse the string value.
                        // For decimal values we use the Decimal parsing which is able to handle comma thousands separators
                        // For spatial types we understand the string value returned in the format the .ToString() method 
                        // on DbGeometry or DbGeography would return it. If this doesn't work, or the requested value 
                        // is not decimal/DbGeometry/DbGeography, then we fall back on the type converter mechanism.
                        decimal decimalResult;
                        DbGeography geographyResult;
                        DbGeometry geometryResult;
                        if (type.IsAssignableFrom(typeof(Decimal)) && Decimal.TryParse(s, out decimalResult))
                        {
                            value = decimalResult;
                        }
                        else if (type.IsAssignableFrom(typeof(DbGeography)) && TryParseGeography(s, out geographyResult))
                        {
                            value = geographyResult;
                        }
                        else if (type.IsAssignableFrom(typeof(DbGeometry)) && TryParseGeometry(s, out geometryResult))
                        {
                            value = geometryResult;
                        }
                        else
                        {
                            value = converter.ConvertFromString(s);
                        }
                    }
                    catch (Exception) // ConvertFromString sometimes throws exceptions of actual type Exception!
                    {
                        // For Nullable types, we just get the type parameter since that makes a more readable exception message
                        string typeName;
                        if (type.IsGenericType && typeof(Nullable<>).IsAssignableFrom(type.GetGenericTypeDefinition()) && !type.ContainsGenericParameters)
                        {
                            Type[] types = type.GetGenericArguments();
                            Debug.Assert(types != null && types.Length == 1, "Nullable did not have a single generic type.");
                            typeName = types[0].FullName;
                        }
                        else
                        {
                            typeName = type.FullName;
                        }
                        throw new InvalidOperationException(Strings.EntityDataSourceUtil_UnableToConvertStringToType(paramName, typeName));
                    }
                }
            }

            // values for enum properties need to be cast to make sure nullable enums will work
            Type underlyingType = null;
            if (value != null && (type.IsEnum || (IsNullableType(type, out underlyingType) && underlyingType.IsEnum)))
            {
                value = Enum.ToObject(underlyingType ?? type, value);
            }

            return value;
        }

        /// <summary>
        /// Converts the string representation to DbGeography instance. A return value indicates if the conversion succeeded.
        /// </summary>
        /// <param name="stringValue">A geography string to convert.</param>
        /// <param name="result">If the conversion succeeds an instance of DbGeometry type created from <paramref name="result"/>; otherwise null.</param>
        /// <returns>true if <paramref name="stringValue"/> was converted successfully; otherwise false;</returns>
        /// <remarks>The <paramref name="stringValue"/> must be in the format returned by <see cref="DbGeometry.AsText()"/> method.</remarks>
        private static bool TryParseGeography(string stringValue, out DbGeography result)
        {
            return TryParseGeo<DbGeography>(stringValue, (geometryText, srid) => DbGeography.FromText(geometryText, srid), out result);
        }

        /// <summary>
        /// Converts the string representation to DbGeometry instance. A return value indicates if the conversion succeeded.
        /// </summary>
        /// <param name="stringValue">A geometry string to convert.</param>
        /// <param name="result">If the conversion succeeds an instance of DbGeometry type created from <paramref name="result"/>; otherwise null.</param>
        /// <returns>true if <paramref name="stringValue"/> was converted successfully; otherwise false;</returns>
        /// <remarks>The <paramref name="stringValue"/> must be in the format returned by <see cref="DbGeometry.ToString()"/> method.</remarks>
        private static bool TryParseGeometry(string stringValue, out DbGeometry result)
        {
            return TryParseGeo<DbGeometry>(stringValue, (geometryText, srid) => DbGeometry.FromText(geometryText, srid), out result);
        }

        /// <summary>
        /// Converts the string representation to DbGeometry or DbGeography instance. A return value indicates if the conversion succeeded.
        /// </summary>
        /// <typeparam name="T">Type to convert the <paramref name="stringValue"/>. Must be either DbGeometry or DbGeography.</typeparam>
        /// <param name="stringValue">A geometry string to convert.</param>
        /// <param name="createSpatialTypeInstanceFunc">Function invoked to create an instance of type T given SRID and geo text.</param>
        /// <param name="result">If the conversion succeeds an instance of DbGeometry or DbGeography type created from <paramref name="result"/>; otherwise null.</param>
        /// <returns>true if <paramref name="stringValue"/> was converted successfully; otherwise false;</returns>
        /// <remarks>The <paramref name="stringValue"/> must be in the format returned by .ToString() method of T.</remarks>
        private static bool TryParseGeo<T>(string stringValue, Func<string, int, T> createSpatialTypeInstanceFunc, out T result)
            where T : class
        {
            Debug.Assert(typeof(DbGeography).IsAssignableFrom(typeof(T)) || typeof(DbGeometry).IsAssignableFrom(typeof(T)), "This method should be called only for spatial type");
            Debug.Assert(createSpatialTypeInstanceFunc != null, "createSpatialTypeInstanceFunc != null");
            Debug.Assert(stringValue != null, "stringValue != null");

            int srid;
            string geometryText;

            if (TryParseSpatialString(stringValue, out srid, out geometryText))
            {
                try
                {
                    result = createSpatialTypeInstanceFunc(geometryText, srid) as T;                    
                    return true;
                }
                catch(Exception ex)
                { 
                    if(!IsCatchableExceptionType(ex))
                    {
                        throw;
                    }
                }
            }

            result = null;
            return false;
        }

        /// <summary>
        /// Retrieves SRID and geo text from <paramref name="stringValue"/> in format "SRID=4326;POINT (100 100)" .
        /// </summary>
        /// <param name="stringValue">String to parse.</param>
        /// <param name="srid">SRID retrieved from <paramref name="stringValue"/>.</param>
        /// <param name="geoText">Geo text retrieved from <paramref name="stringValue"/>.</param>
        /// <returns>true if it was possible to retrieve both SRID and geo text; otherwise false.</returns>
        private static bool TryParseSpatialString(string stringValue, out int srid, out string geoText)
        {
            Debug.Assert(stringValue != null, "stringValue != null");

            string[] components = stringValue.Split(';');

            // expected 2 semicolon separated components - SRID and well known text
            if (components.Length == 2)
            {
                if (components[0].StartsWith("SRID=", StringComparison.Ordinal))
                {
                    if (int.TryParse(components[0].Substring("SRID=".Length), NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out srid))
                    {
                        geoText = components[1];
                        return true;
                    }
                }
            }

            srid = int.MinValue;
            geoText = null;

            return false;
        }

        internal static void SetAllPropertiesWithVerification(EntityDataSourceWrapper entityWrapper, 
                                                              Dictionary<string, object> changedProperties, 
                                                              bool overwrite)
        {
            Dictionary<string, Exception> exceptions = null;
            entityWrapper.SetAllProperties(changedProperties, /*overwriteSameValue*/true, ref exceptions);

            if (null != exceptions)
            {
                // The EntityDataSourceValidationException has a property "InnerExceptions" that encapsulates
                // all of the failed property setters. The message from one of those errors is surfaced so that it
                // appears on the web page as a human-readable error like:
                //   "Error while setting property 'PropertyName': 'The value cannot be null.'."
                string key = exceptions.Keys.First();
                throw new EntityDataSourceValidationException(
                    Strings.EntityDataSourceView_DataConversionError(
                        key, exceptions[key].Message), exceptions);
            }
        }

        /// <summary>
        /// Get the Clr type for the primitive enum or complex type member. The member must not be null.
        /// </summary>
        internal static Type GetMemberClrType(MetadataWorkspace ocWorkspace, EdmMember member)
        {
            EntityDataSourceUtil.CheckArgumentNull(member, "member");

            EdmType memberType = member.TypeUsage.EdmType;

            Debug.Assert(EntityDataSourceUtil.IsScalar(memberType) ||
                memberType.BuiltInTypeKind == BuiltInTypeKind.ComplexType ||
                memberType.BuiltInTypeKind == BuiltInTypeKind.EntityType, "member type must be primitive, enum, entity or complex type");

            Type clrType;

            if (EntityDataSourceUtil.IsScalar(memberType))
            {
                clrType = memberType.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType ? 
                            ((PrimitiveType)memberType).ClrEquivalentType : 
                            GetClrType(ocWorkspace, (EnumType)memberType);

                if (!NullCanBeAssignedTo(clrType))
                {
                    Facet facet;
                    if (member.TypeUsage.Facets.TryGetValue("Nullable", true, out facet))
                    {
                        if ((bool)facet.Value)
                        {
                            clrType = MakeNullable(clrType);
                        }
                    }
                }
            }
            else
            {
                Debug.Assert(
                    memberType.BuiltInTypeKind == BuiltInTypeKind.EntityType || memberType.BuiltInTypeKind == BuiltInTypeKind.ComplexType,
                    "Complex or Entity type expected");

                clrType = GetClrType(ocWorkspace, (StructuralType)memberType);
            }

            return clrType;
        }

        internal static Type MakeNullable(Type type)
        {
            if (!NullCanBeAssignedTo(type))
            {
                type = typeof(Nullable<>).MakeGenericType(type);
            }
            return type;
        }
        
        /// <summary>
        /// Returns the collection of AssociationSetEnds for the relationships for this entity
        /// </summary>
        /// <param name="entitySet"></param>
        /// <param name="entityType"></param>
        /// <param name="forKey">If true, returns only the other ends with multiplicity 1. Ignores 1:0..1 relationships.</param>
        /// <returns></returns>
        internal static IEnumerable<AssociationSetEnd> GetReferenceEnds(EntitySet entitySet, EntityType entityType, bool forKey)
        {
            foreach (AssociationSet associationSet in entitySet.EntityContainer.BaseEntitySets.OfType<AssociationSet>())
            {
                Debug.Assert(associationSet.AssociationSetEnds.Count == 2, "non binary association?");
                AssociationSetEnd firstEnd = associationSet.AssociationSetEnds[0];
                AssociationSetEnd secondEnd = associationSet.AssociationSetEnds[1];

                // If both ends match, then we will return both ends
                if (IsReferenceEnd(entitySet, entityType, firstEnd, secondEnd, forKey))
                {
                    yield return secondEnd;
                }
                if (IsReferenceEnd(entitySet, entityType, secondEnd, firstEnd, forKey))
                {
                    yield return firstEnd;
                }
            }
        }

        /// <summary>
        /// Determine if the end is 'contained' in the source entity via a referential integrity constraint (e.g.,
        /// in a relationship from OrderDetail to Order where OrderDetail has the OrderId property, the association set end
        /// is contained in the order detail entity)
        /// </summary>
        private static bool IsContained(AssociationSetEnd end, out ReferentialConstraint constraint)
        {
            CheckArgumentNull(end, "end");

            AssociationEndMember endMember = end.CorrespondingAssociationEndMember;
            AssociationType associationType = (AssociationType)endMember.DeclaringType;

            constraint = null;
            bool result = false;

            if (null != associationType.ReferentialConstraints)
            {
                foreach (ReferentialConstraint candidate in associationType.ReferentialConstraints)
                {
                    if (candidate.FromRole.Name == endMember.Name)
                    {
                        constraint = candidate;
                        result = true;
                        break;
                    }
                }
            }

            return result;
        }

        internal static bool TryGetCorrespondingNavigationProperty(AssociationEndMember end, out NavigationProperty navigationProperty)
        {
            EntityType entityType = GetEntityType(GetOppositeEnd(end));

            // if there is a corresponding navigation property, use its name as the prefix
            navigationProperty = entityType.NavigationProperties
                .Where(np => np.ToEndMember == end)
                .SingleOrDefault(); // metadata is supposed to ensure this is non-ambiguous
            return null != navigationProperty;
        }

        internal static AssociationEndMember GetOppositeEnd(AssociationEndMember end)
        {
            return (AssociationEndMember)end.DeclaringType.Members.Where(m => m != end).Single();
        }

        /// <summary>
        /// A navigation ('fromEnd' -> 'toEnd') defines a reference end for 'entitySet' and 'entityType' if it 
        /// has multiplicity 0..1 or 1..1, is bound to the set, and has the appropriate type.
        /// 
        /// We omit 1..1:0..1 navigations assuming that the opposite end owns the relationship (since the foreign 
        /// key would need to point in the opposite direction.)
        /// </summary>
        private static bool IsReferenceEnd(EntitySet entitySet, EntityType entityType, AssociationSetEnd fromEnd, AssociationSetEnd toEnd, bool forKey)
        {
            EntityType fromType = GetEntityType(fromEnd);

            if (fromEnd.EntitySet == entitySet && (IsStrictSubtypeOf(entityType, fromType) || entityType == fromType))
            {
                RelationshipMultiplicity fromMult = fromEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity;
                RelationshipMultiplicity toMult = toEnd.CorrespondingAssociationEndMember.RelationshipMultiplicity;

                // If forKey is false (we are testing to see if this is a far end for a reference, not a key)
                //   then fromMult is ignored and all far-end 1 or 0..1 multiplicity ends are exposed.
                // If forKey is true, then we are asking about a reference end for the purpose of flattening.
                //   We do not flatten 1:0..1 relationships because of a limitation in the EDM.
                if (toMult == RelationshipMultiplicity.One ||
                    (toMult == RelationshipMultiplicity.ZeroOrOne && (!forKey || fromMult != RelationshipMultiplicity.One) ))
                {
                    return true;
                }
            }

            return false;
        }

        internal static bool IsScalar(EdmType type)
        {
            return type.BuiltInTypeKind == BuiltInTypeKind.PrimitiveType ||
                    type.BuiltInTypeKind == BuiltInTypeKind.EnumType;
        }

        internal static EntityType GetEntityType(AssociationSetEnd end)
        {
            return GetEntityType(end.CorrespondingAssociationEndMember);
        }

        internal static EntityType GetEntityType(AssociationEndMember end)
        {
            EntityType entityType = (EntityType)((RefType)end.TypeUsage.EdmType).ElementType;
            return entityType;
        }


        internal static string GetQualifiedEntitySetName(EntitySet entitySet)
        {
            EntityDataSourceUtil.CheckArgumentNull(entitySet, "entitySet");
            // ContainerName.EntitySetName
            return entitySet.EntityContainer.Name + "." + entitySet.Name;
        }

        internal static string QuoteEntitySqlIdentifier(string identifier)
        {
            return "[" + (identifier ?? string.Empty).Replace("]", "]]") + "]";
        }

        internal static string CreateEntitySqlTypeIdentifier(EdmType type)
        {
            // [_schema_namespace_name_].[_type_name_]
            // if the [_schema_namespace_name_] is null or empty, omit this part of the identifier
            // this can happen when the CLR type is defined outside of a namespace
            return (String.IsNullOrEmpty(type.NamespaceName) ? String.Empty : (QuoteEntitySqlIdentifier(type.NamespaceName) + "."))
                + QuoteEntitySqlIdentifier(type.Name);
        }

        internal static string CreateEntitySqlSetIdentifier(EntitySetBase set)
        {
            // [_container_name_].[_set_name_]
            return QuoteEntitySqlIdentifier(set.EntityContainer.Name) + "." + QuoteEntitySqlIdentifier(set.Name);
        }

        /// <summary>
        /// Determines which columns to expose for the given set and type. Includes
        /// flattened complex properties and 'reference' keys.
        /// </summary>
        /// <param name="csWorkspace">Used to determine 'interesting' members, or
        /// members whose values need to be maintained in ControlState</param>
        /// <param name="ocWorkspace">Used to get CLR mapping information for EDM
        /// types</param>
        /// <param name="entitySet">The set.</param>
        /// <param name="entityType">The type.</param>
        /// <returns>A map from display names to columns.</returns>
        internal static ReadOnlyCollection<EntityDataSourceColumn> GetNamedColumns(MetadataWorkspace csWorkspace, MetadataWorkspace ocWorkspace,
            EntitySet entitySet, EntityType entityType)
        {
            CheckArgumentNull(csWorkspace, "csWorkspace");
            CheckArgumentNull(ocWorkspace, "ocWorkspace");
            CheckArgumentNull(entitySet, "entitySet");
            CheckArgumentNull(entityType, "entityType");

            ReadOnlyCollection<EdmMember> interestingMembers = GetInterestingMembers(csWorkspace, entitySet, entityType);

            IEnumerable<EntityDataSourceColumn> columns = GetColumns(entitySet, entityType, ocWorkspace, interestingMembers);
            List<EntityDataSourceColumn> result = new List<EntityDataSourceColumn>();

            // give precedence to simple named columns (

            HashSet<string> usedNames = new HashSet<string>();
            foreach (EntityDataSourceColumn column in columns)
            {
                if (!column.IsHidden)
                {
                    // check that the column name has not been used
                    if (!usedNames.Add(column.DisplayName))
                    {
                        throw new InvalidOperationException(Strings.DisplayNameCollision(column.DisplayName));
                    }
                }
                result.Add(column);
            }

            return result.AsReadOnly();
        }

        private static ReadOnlyCollection<EdmMember> GetInterestingMembers(MetadataWorkspace csWorkspace, EntitySet entitySet, EntityType entityType)
        {
            // Note that this delegate is not used to determine whether reference columns are interesting. They
            // are intrinsically interesting and do not appear in this set.
            HashSet<EdmMember> interestingMembers = new HashSet<EdmMember>(
                csWorkspace.GetRelevantMembersForUpdate(entitySet, entityType, true));

            // keys are also interesting...
            foreach (EdmMember keyMember in entityType.KeyMembers)
            {
                interestingMembers.Add(keyMember);
            }

            ReadOnlyCollection<EdmMember> result = interestingMembers.ToList().AsReadOnly();

            return result;
        }

        private static IEnumerable<EntityDataSourceColumn> GetColumns(EntitySet entitySet, EntityType entityType,
            MetadataWorkspace ocWorkspace, ReadOnlyCollection<EdmMember> interestingMembers)
        {
            List<EntityDataSourceColumn> columns = new List<EntityDataSourceColumn>();

            // Primitive and complex properties
            EntityDataSourceMemberPath parent = null; // top-level properties are not qualified
            Dictionary<EdmProperty, EntityDataSourcePropertyColumn> entityProperties = AddPropertyColumns(columns, ocWorkspace, parent, entityType.Properties, interestingMembers);

            // Navigation reference properties
            AddReferenceNavigationColumns(columns, ocWorkspace, entitySet, entityType);

            // Reference key properties
            AddReferenceKeyColumns(columns, ocWorkspace, entitySet, entityType, entityProperties);

            return columns;
        }

        // Adds element to 'columns' for every element of 'properties'. Also returns a map from properties
        // at this level to the corresponding columns.
        private static Dictionary<EdmProperty, EntityDataSourcePropertyColumn> AddPropertyColumns(List<EntityDataSourceColumn> columns, MetadataWorkspace ocWorkspace, EntityDataSourceMemberPath parent, IEnumerable<EdmProperty> properties, ReadOnlyCollection<EdmMember> interestingMembers)
        {
            Dictionary<EdmProperty, EntityDataSourcePropertyColumn> result = new Dictionary<EdmProperty, EntityDataSourcePropertyColumn>();

            foreach (EdmProperty property in properties)
            {
                bool isLocallyInteresting = interestingMembers.Contains(property);

                EntityDataSourceMemberPath prefix = new EntityDataSourceMemberPath(ocWorkspace, parent, property, isLocallyInteresting);
                EdmType propertyType = property.TypeUsage.EdmType;

                // add column for this entity property
                EntityDataSourcePropertyColumn propertyColumn = new EntityDataSourcePropertyColumn(prefix);
                columns.Add(propertyColumn);
                result.Add(property, propertyColumn);

                if (propertyType.BuiltInTypeKind == BuiltInTypeKind.ComplexType)
                {
                    // add nested properties
                    // prepend the property name to the members of the complex type
                    AddPropertyColumns(columns, ocWorkspace, prefix, ((ComplexType)propertyType).Properties, interestingMembers);
                }
                // other property types are not currently supported (or possible in EF V1 for that matter)
            }

            return result;
        }

        private static void AddReferenceNavigationColumns(List<EntityDataSourceColumn> columns, MetadataWorkspace ocWorkspace, EntitySet entitySet, EntityType entityType)
        {
            foreach (AssociationSetEnd toEnd in GetReferenceEnds(entitySet, entityType, /*forKey*/false))
            {
                // Check for a navigation property
                NavigationProperty navigationProperty;
                if (TryGetCorrespondingNavigationProperty(toEnd.CorrespondingAssociationEndMember, out navigationProperty))
                {
                    Type clrToType = EntityDataSourceUtil.GetMemberClrType(ocWorkspace, navigationProperty);
                    EntityDataSourceReferenceValueColumn column = EntityDataSourceReferenceValueColumn.Create(clrToType, ocWorkspace, navigationProperty);
                    columns.Add(column);
                }
            }
        }

        private static void AddReferenceKeyColumns(List<EntityDataSourceColumn> columns, MetadataWorkspace ocWorkspace, EntitySet entitySet, EntityType entityType, Dictionary<EdmProperty, EntityDataSourcePropertyColumn> entityProperties)
        {
            foreach (AssociationSetEnd toEnd in GetReferenceEnds(entitySet, entityType, /*forKey*/true))
            {
                ReferentialConstraint constraint;
                bool isContained = EntityDataSourceUtil.IsContained(toEnd, out constraint);

                // Create a group for the end columns
                EntityType toType = EntityDataSourceUtil.GetEntityType(toEnd);
                Type clrToType = EntityDataSourceUtil.GetClrType(ocWorkspace, toType);
                
                EntityDataSourceReferenceGroup group = EntityDataSourceReferenceGroup.Create(clrToType, toEnd);

                // Create a column for every key
                foreach (EdmProperty keyMember in GetEntityType(toEnd).KeyMembers)
                {
                    EntityDataSourceColumn controllingColumn = null;
                    if (isContained)
                    {
                        // if this key is 'contained' in the entity, make the referential constrained
                        // property the principal for the column
                        int ordinalInConstraint = constraint.FromProperties.IndexOf(keyMember);

                        // find corresponding member in the current (dependent) entity
                        EdmProperty correspondingProperty = constraint.ToProperties[ordinalInConstraint];

                        controllingColumn = entityProperties[correspondingProperty];
                    }
                    columns.Add(new EntityDataSourceReferenceKeyColumn(ocWorkspace, group, keyMember, controllingColumn));
                }
            }
        }

        internal static void ValidateKeyPropertyValuesExist(EntityDataSourceWrapper entityWrapper, Dictionary<string, object> propertyValues)
        {
            foreach (var keyProperty in entityWrapper.Collection.AllPropertyDescriptors.Select(d => d.Column).OfType<EntityDataSourcePropertyColumn>().Where(c => c.IsKey))
            {
                if (!propertyValues.ContainsKey(keyProperty.DisplayName))
                {
                    throw new EntityDataSourceValidationException(Strings.EntityDataSourceView_NoKeyProperty);
                }
            }
        }

        static private readonly Type StackOverflowType = typeof(System.StackOverflowException);
        static private readonly Type OutOfMemoryType = typeof(System.OutOfMemoryException);
        static private readonly Type ThreadAbortType = typeof(System.Threading.ThreadAbortException);
        static private readonly Type NullReferenceType = typeof(System.NullReferenceException);
        static private readonly Type AccessViolationType = typeof(System.AccessViolationException);
        static private readonly Type SecurityType = typeof(System.Security.SecurityException);
        static private readonly Type AppDomainUnloadedType = typeof(System.AppDomainUnloadedException);
        static private readonly Type CannotUnloadAppDomainType = typeof(CannotUnloadAppDomainException);
        static private readonly Type BadImageFormatType = typeof(BadImageFormatException);
        static private readonly Type InvalidProgramType = typeof(InvalidProgramException);
        
        static private bool IsCatchableExceptionType(Exception e)
        {
            // a 'catchable' exception is defined by what it is not.
            Debug.Assert(e != null, "Unexpected null exception!");
            Type type = e.GetType();

            return ((type != StackOverflowType) &&
                     (type != OutOfMemoryType) &&
                     (type != ThreadAbortType) &&
                     (type != NullReferenceType) &&
                     (type != AccessViolationType) &&
                     (type != AppDomainUnloadedType) &&
                     (type != CannotUnloadAppDomainType) &&
                     (type != BadImageFormatType) &&
                     (type != InvalidProgramType) &&
                     !SecurityType.IsAssignableFrom(type));
        }
    }
}

//------------------------------------------------------------------------------
// <copyright file="ColumnMapFactory.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;
using System.Data.Common.Utils;
using System.Data.Entity;
using System.Data.Mapping;
using System.Data.Metadata.Edm;
using System.Data.Objects.ELinq;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// Factory methods for prescriptive column map patterns (includes default
    /// column maps for -- soon to be -- public materializer services and function
    /// mappings).
    /// </summary>
    internal static class ColumnMapFactory
    {
        /// <summary>
        /// Creates a column map for the given reader and function mapping.
        /// </summary>
        internal static CollectionColumnMap CreateFunctionImportStructuralTypeColumnMap(DbDataReader storeDataReader, FunctionImportMappingNonComposable mapping, int resultSetIndex, EntitySet entitySet, StructuralType baseStructuralType)
        {
            FunctionImportStructuralTypeMappingKB resultMapping = mapping.GetResultMapping(resultSetIndex);
            Debug.Assert(resultMapping != null);
            if (resultMapping.NormalizedEntityTypeMappings.Count == 0) // no explicit mapping; use default non-polymorphic reader
            {
                // if there is no mapping, create default mapping to root entity type or complex type
                Debug.Assert(!baseStructuralType.Abstract, "mapping loader must verify abstract types have explicit mapping");
               
                return CreateColumnMapFromReaderAndType(storeDataReader, baseStructuralType, entitySet, resultMapping.ReturnTypeColumnsRenameMapping);
            }
            
            // the section below deals with the polymorphic entity type mapping for return type
            EntityType baseEntityType = baseStructuralType as EntityType;
            Debug.Assert(null != baseEntityType, "We should have entity type here");

            // Generate column maps for all discriminators
            ScalarColumnMap[] discriminatorColumns = CreateDiscriminatorColumnMaps(storeDataReader, mapping, resultSetIndex);

            // Generate default maps for all mapped entity types
            var mappedEntityTypes = new HashSet<EntityType>(resultMapping.MappedEntityTypes);
            mappedEntityTypes.Add(baseEntityType); // make sure the base type is represented
            Dictionary<EntityType, TypedColumnMap> typeChoices = new Dictionary<EntityType, TypedColumnMap>(mappedEntityTypes.Count);
            ColumnMap[] baseTypeColumnMaps = null;
            foreach (EntityType entityType in mappedEntityTypes)
            {                
                ColumnMap[] propertyColumnMaps = GetColumnMapsForType(storeDataReader, entityType, resultMapping.ReturnTypeColumnsRenameMapping);
                EntityColumnMap entityColumnMap = CreateEntityTypeElementColumnMap(storeDataReader, entityType, entitySet, propertyColumnMaps, resultMapping.ReturnTypeColumnsRenameMapping);
                if (!entityType.Abstract)
                {
                    typeChoices.Add(entityType, entityColumnMap);
                }
                if (entityType == baseStructuralType)
                {
                    baseTypeColumnMaps = propertyColumnMaps;
                }
            }

            // NOTE: We don't have a null sentinel here, because the stored proc won't 
            //       return one anyway; we'll just presume the data's always there.
            MultipleDiscriminatorPolymorphicColumnMap polymorphicMap = new MultipleDiscriminatorPolymorphicColumnMap(TypeUsage.Create(baseStructuralType), baseStructuralType.Name, baseTypeColumnMaps, discriminatorColumns, typeChoices, (object[] discriminatorValues) => mapping.Discriminate(discriminatorValues, resultSetIndex));
            CollectionColumnMap collection = new SimpleCollectionColumnMap(baseStructuralType.GetCollectionType().TypeUsage, baseStructuralType.Name, polymorphicMap, null, null);
            return collection;
        }

        /// <summary>
        /// Build the collectionColumnMap from a store datareader, a type and an entitySet.
        /// </summary>
        /// <param name="storeDataReader"></param>
        /// <param name="edmType"></param>
        /// <param name="entitySet"></param>
        /// <returns></returns>
        internal static CollectionColumnMap CreateColumnMapFromReaderAndType(DbDataReader storeDataReader, EdmType edmType, EntitySet entitySet, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
        {
            Debug.Assert(Helper.IsEntityType(edmType) || null == entitySet, "The specified non-null EntitySet is incompatible with the EDM type specified.");

            // Next, build the ColumnMap directly from the edmType and entitySet provided.
            ColumnMap[] propertyColumnMaps = GetColumnMapsForType(storeDataReader, edmType, renameList);
            ColumnMap elementColumnMap = null;

            // NOTE: We don't have a null sentinel here, because the stored proc won't 
            //       return one anyway; we'll just presume the data's always there.
            if (Helper.IsRowType(edmType))
            {
                elementColumnMap = new RecordColumnMap(TypeUsage.Create(edmType), edmType.Name, propertyColumnMaps, null);
            }
            else if (Helper.IsComplexType(edmType))
            {
                elementColumnMap = new ComplexTypeColumnMap(TypeUsage.Create(edmType), edmType.Name, propertyColumnMaps, null);
            }
            else if (Helper.IsScalarType(edmType))
            {
                if (storeDataReader.FieldCount != 1)
                {
                    throw EntityUtil.CommandExecutionDataReaderFieldCountForScalarType();
                }
                elementColumnMap = new ScalarColumnMap(TypeUsage.Create(edmType), edmType.Name, 0, 0);
            }
            else if (Helper.IsEntityType(edmType))
            {
                elementColumnMap = CreateEntityTypeElementColumnMap(storeDataReader, edmType, entitySet, propertyColumnMaps, null/*renameList*/);
            }
            else
            {
                Debug.Assert(false, "unexpected edmType?");
            }
            CollectionColumnMap collection = new SimpleCollectionColumnMap(edmType.GetCollectionType().TypeUsage, edmType.Name, elementColumnMap, null, null);
            return collection;
        }

        /// <summary>
        /// Requires: a public type with a public, default constructor. Returns a column map initializing the type
        /// and all properties of the type with a public setter taking a primitive type and having a corresponding 
        /// column in the reader.
        /// </summary>
        internal static CollectionColumnMap CreateColumnMapFromReaderAndClrType(DbDataReader reader, Type type, MetadataWorkspace workspace)
        {
            Debug.Assert(null != reader);
            Debug.Assert(null != type);
            Debug.Assert(null != workspace);

            // we require a default constructor
            ConstructorInfo constructor = type.GetConstructor(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance,
                null, Type.EmptyTypes, null);
            if (type.IsAbstract || (null == constructor && !type.IsValueType))
            {
                throw EntityUtil.InvalidOperation(
                    Strings.ObjectContext_InvalidTypeForStoreQuery(type));
            }

            // build a LINQ expression used by result assembly to create results
            var memberInfo = new List<Tuple<MemberAssignment, int, EdmProperty>>();
            foreach (PropertyInfo prop in type.GetProperties(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance))
            {
                // for enums unwrap the type if nullable
                var propertyUnderlyingType = Nullable.GetUnderlyingType(prop.PropertyType) ?? prop.PropertyType;
                Type propType = propertyUnderlyingType.IsEnum ? propertyUnderlyingType.GetEnumUnderlyingType() : prop.PropertyType;

                EdmType modelType;
                int ordinal;
                if (TryGetColumnOrdinalFromReader(reader, prop.Name, out ordinal) &&
                    MetadataHelper.TryDetermineCSpaceModelType(propType, workspace, out modelType) &&
                    (Helper.IsScalarType(modelType)) &&
                    prop.CanWrite && prop.GetIndexParameters().Length == 0 && null != prop.GetSetMethod(/* nonPublic */true))
                {
                    memberInfo.Add(Tuple.Create(
                        Expression.Bind(prop, Expression.Parameter(prop.PropertyType, "placeholder")),
                        ordinal,
                        new EdmProperty(prop.Name, TypeUsage.Create(modelType))));
                }
            }
            // initialize members in the order in which they appear in the reader
            MemberInfo[] members = new MemberInfo[memberInfo.Count];
            MemberBinding[] memberBindings = new MemberBinding[memberInfo.Count];
            ColumnMap[] propertyMaps = new ColumnMap[memberInfo.Count];
            EdmProperty[] modelProperties = new EdmProperty[memberInfo.Count];
            int i = 0;
            foreach (var memberGroup in memberInfo.GroupBy(tuple => tuple.Item2).OrderBy(tuple => tuple.Key))
            {
                // make sure that a single column isn't contributing to multiple properties
                if (memberGroup.Count() != 1)
                {
                    throw EntityUtil.InvalidOperation(Strings.ObjectContext_TwoPropertiesMappedToSameColumn(
                        reader.GetName(memberGroup.Key), 
                        String.Join(", ", memberGroup.Select(tuple => tuple.Item3.Name).ToArray())));
                }

                var member = memberGroup.Single();
                MemberAssignment assignment = member.Item1;
                int ordinal = member.Item2;
                EdmProperty modelProp = member.Item3;
                
                members[i] = assignment.Member;
                memberBindings[i] = assignment;
                propertyMaps[i] = new ScalarColumnMap(modelProp.TypeUsage, modelProp.Name, 0, ordinal);
                modelProperties[i] = modelProp;
                i++;
            }
            NewExpression newExpr = null == constructor ? Expression.New(type) : Expression.New(constructor);
            MemberInitExpression init = Expression.MemberInit(newExpr, memberBindings);
            InitializerMetadata initMetadata = InitializerMetadata.CreateProjectionInitializer(
                (EdmItemCollection)workspace.GetItemCollection(DataSpace.CSpace), init, members);

            // column map (a collection of rows with InitializerMetadata markup)
            RowType rowType = new RowType(modelProperties, initMetadata);
            RecordColumnMap rowMap = new RecordColumnMap(TypeUsage.Create(rowType),
                "DefaultTypeProjection", propertyMaps, null);
            CollectionColumnMap collectionMap = new SimpleCollectionColumnMap(rowType.GetCollectionType().TypeUsage, 
                rowType.Name, rowMap, null, null);
            return collectionMap;
        }

        /// <summary>
        /// Build the entityColumnMap from a store datareader, a type and an entitySet and 
        /// a list ofproperties.
        /// </summary>
        /// <param name="storeDataReader"></param>
        /// <param name="edmType"></param>
        /// <param name="entitySet"></param>
        /// <param name="propertyColumnMaps"></param>
        /// <returns></returns>
        private static EntityColumnMap CreateEntityTypeElementColumnMap(
            DbDataReader storeDataReader, EdmType edmType, EntitySet entitySet,
            ColumnMap[] propertyColumnMaps, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
        {
            EntityType entityType = (EntityType)edmType;

            // The tricky part here is
            // that the KeyColumns list must point at the same ColumnMap(s) that 
            // the properties list points to, so we build a quick array of 
            // ColumnMap(s) that are indexed by their ordinal; then we can walk
            // the list of keyMembers, and find the ordinal in the reader, and 
            // pick the same ColumnMap for it.

            // Build the ordinal -> ColumnMap index
            ColumnMap[] ordinalToColumnMap = new ColumnMap[storeDataReader.FieldCount];

            foreach (ColumnMap propertyColumnMap in propertyColumnMaps)
            {
                int ordinal = ((ScalarColumnMap)propertyColumnMap).ColumnPos;
                ordinalToColumnMap[ordinal] = propertyColumnMap;
            }

            // Now build the list of KeyColumns;
            IList<EdmMember> keyMembers = entityType.KeyMembers;
            SimpleColumnMap[] keyColumns = new SimpleColumnMap[keyMembers.Count];

            int keyMemberIndex = 0;
            foreach (EdmMember keyMember in keyMembers)
            {
                int keyOrdinal = GetMemberOrdinalFromReader(storeDataReader, keyMember, edmType, renameList);

                Debug.Assert(keyOrdinal >= 0, "keyMember for entity is not found by name in the data reader?");

                ColumnMap keyColumnMap = ordinalToColumnMap[keyOrdinal];

                Debug.Assert(null != keyColumnMap, "keyMember for entity isn't in properties collection for the entity?");
                keyColumns[keyMemberIndex] = (SimpleColumnMap)keyColumnMap;
                keyMemberIndex++;
            }

            SimpleEntityIdentity entityIdentity = new SimpleEntityIdentity(entitySet, keyColumns);

            EntityColumnMap result = new EntityColumnMap(TypeUsage.Create(edmType), edmType.Name, propertyColumnMaps, entityIdentity);
            return result;
        }

        /// <summary>
        /// For a given edmType, build an array of scalarColumnMaps that map to the columns
        /// in the store datareader provided.  Note that we're hooking things up by name, not
        /// by ordinal position.
        /// </summary>
        /// <param name="storeDataReader"></param>
        /// <param name="edmType"></param>
        /// <returns></returns>
        private static ColumnMap[] GetColumnMapsForType(DbDataReader storeDataReader, EdmType edmType, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
        {
            // First get the list of properties; NOTE: we need to hook up the column by name, 
            // not by position.
            IBaseList<EdmMember> members = TypeHelpers.GetAllStructuralMembers(edmType);
            ColumnMap[] propertyColumnMaps = new ColumnMap[members.Count];

            int index = 0;
            foreach (EdmMember member in members)
            {
                if (!Helper.IsScalarType(member.TypeUsage.EdmType))
                {
                    throw EntityUtil.InvalidOperation(Strings.ADP_InvalidDataReaderUnableToMaterializeNonScalarType(member.Name, member.TypeUsage.EdmType.FullName));
                }

                int ordinal = GetMemberOrdinalFromReader(storeDataReader, member, edmType, renameList);

                propertyColumnMaps[index] = new ScalarColumnMap(member.TypeUsage, member.Name, 0, ordinal);
                index++;
            }
            return propertyColumnMaps;
        }

        private static ScalarColumnMap[] CreateDiscriminatorColumnMaps(DbDataReader storeDataReader, FunctionImportMappingNonComposable mapping, int resultIndex)
        {
            // choose an arbitrary type for discriminator columns -- the type is not
            // actually statically known
            EdmType discriminatorType =
                MetadataItem.EdmProviderManifest.GetPrimitiveType(PrimitiveTypeKind.String);
            TypeUsage discriminatorTypeUsage =
                TypeUsage.Create(discriminatorType);

            IList<string> discriminatorColumnNames = mapping.GetDiscriminatorColumns(resultIndex);
            ScalarColumnMap[] discriminatorColumns = new ScalarColumnMap[discriminatorColumnNames.Count];
            for (int i = 0; i < discriminatorColumns.Length; i++)
            {
                string columnName = discriminatorColumnNames[i];
                ScalarColumnMap columnMap = new ScalarColumnMap(discriminatorTypeUsage, columnName, 0,
                    GetDiscriminatorOrdinalFromReader(storeDataReader, columnName, mapping.FunctionImport));
                discriminatorColumns[i] = columnMap;
            }
            return discriminatorColumns;
        }

        /// <summary>
        /// Given a store datareader and a member of an edmType, find the column ordinal
        /// in the datareader with the name of the member.
        /// </summary>
        /// <param name="storeDataReader"></param>
        /// <param name="member"></param>
        /// <returns></returns>
        private static int GetMemberOrdinalFromReader(DbDataReader storeDataReader, EdmMember member, EdmType currentType, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
        {
            int result;
            string memberName = GetRenameForMember(member, currentType, renameList);

            if (!TryGetColumnOrdinalFromReader(storeDataReader, memberName, out result))
            {
                throw EntityUtil.CommandExecutionDataReaderMissingColumnForType(member, currentType);
            }
            return result;
        }

        private static string GetRenameForMember(EdmMember member, EdmType currentType, Dictionary<string, FunctionImportReturnTypeStructuralTypeColumnRenameMapping> renameList)
        {
            // if list is null,
            // or no rename mapping at all,
            // or partial rename and the member is not specified by the renaming
            // then we return the original member.Name
            // otherwise we return the mapped one
            return renameList == null || renameList.Count == 0 || !renameList.Any(m => m.Key == member.Name) ? member.Name : renameList[member.Name].GetRename(currentType);
        }

        /// <summary>
        /// Given a store datareader, a column name, find the column ordinal
        /// in the datareader with the name of the column.
        /// 
        /// We only have the functionImport provided to include it in the exception 
        /// message.
        /// </summary>
        /// <param name="storeDataReader"></param>
        /// <param name="columnName"></param>
        /// <param name="functionImport"></param>
        /// <returns></returns>
        private static int GetDiscriminatorOrdinalFromReader(DbDataReader storeDataReader, string columnName, EdmFunction functionImport)
        {
            int result;
            if (!TryGetColumnOrdinalFromReader(storeDataReader, columnName, out result))
            {
                throw EntityUtil.CommandExecutionDataReaderMissinDiscriminatorColumn(columnName, functionImport);
            }
            return result;
        }

        /// <summary>
        /// Given a store datareader and a column name, try to find the column ordinal
        /// in the datareader with the name of the column.
        /// </summary>
        /// <param name="storeDataReader"></param>
        /// <param name="columnName"></param>
        /// <param name="ordinal"></param>
        /// <returns>true if found, false otherwise.</returns>
        private static bool TryGetColumnOrdinalFromReader(DbDataReader storeDataReader, string columnName, out int ordinal)
        {
            if (0 == storeDataReader.FieldCount)
            {
                // If there are no fields, there can't be a match (this check avoids
                // an InvalidOperationException on the call to GetOrdinal)
                ordinal = default(int);
                return false;
            }

            // Wrap ordinal lookup for the member so that we can throw a nice exception.
            try
            {
                ordinal = storeDataReader.GetOrdinal(columnName);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                // No column matching the column name found
                ordinal = default(int);
                return false;
            }
        }
    }
}

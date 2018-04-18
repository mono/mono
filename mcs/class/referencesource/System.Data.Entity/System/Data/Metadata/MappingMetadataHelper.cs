//---------------------------------------------------------------------
// <copyright file="MappingMetadataHelper.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
namespace System.Data.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Common.Utils;
    using System.Data.Mapping;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Helps answer mapping questions since we don't have a good API for mapping information
    /// </summary>
    internal static class MappingMetadataHelper
    {

        internal static IEnumerable<StorageTypeMapping> GetMappingsForEntitySetAndType(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType)
        {
            Debug.Assert(entityType != null, "EntityType parameter should not be null.");
            StorageEntityContainerMapping containerMapping = GetEntityContainerMap(mappingCollection, container);
            StorageSetMapping extentMap = containerMapping.GetSetMapping(entitySet.Name);

            //The Set may have no mapping
            if (extentMap != null)
            {
                //for each mapping fragment of Type we are interested in within the given set
                //Check use of IsOfTypes in Code review
                foreach (StorageTypeMapping typeMap in extentMap.TypeMappings.Where(map => map.Types.Union(map.IsOfTypes).Contains(entityType)))
                {
                    yield return typeMap;
                }
            }
        }

       /// <summary>
       /// Returns all mapping fragments for the given entity set's types and their parent types.
       /// </summary>
        internal static IEnumerable<StorageTypeMapping> GetMappingsForEntitySetAndSuperTypes(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase childEntityType)
        {
            return MetadataHelper.GetTypeAndParentTypesOf(childEntityType, mappingCollection.EdmItemCollection, true /*includeAbstractTypes*/).SelectMany(
                edmType => 
                    edmType.EdmEquals(childEntityType) ? 
                      GetMappingsForEntitySetAndType(mappingCollection, container, entitySet, (edmType as EntityTypeBase))
                    : GetIsTypeOfMappingsForEntitySetAndType(mappingCollection, container, entitySet, (edmType as EntityTypeBase), childEntityType)
                ).ToList();
        }

        /// <summary>
        /// Returns mappings for the given set/type only if the mapping applies also to childEntittyType either via IsTypeOf or explicitly specifying multiple types in mapping fragments.
        /// </summary>
        private static IEnumerable<StorageTypeMapping> GetIsTypeOfMappingsForEntitySetAndType(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType, EntityTypeBase childEntityType)
        {
            foreach (var mapping in GetMappingsForEntitySetAndType(mappingCollection, container, entitySet, entityType))
            {
                if (mapping.IsOfTypes.Any(parentType => parentType.IsAssignableFrom(childEntityType)) || mapping.Types.Contains(childEntityType))
                {
                    yield return mapping;
                }
            }
        }      

        internal static IEnumerable<StorageEntityTypeModificationFunctionMapping> GetModificationFunctionMappingsForEntitySetAndType(StorageMappingItemCollection mappingCollection, EntityContainer container, EntitySetBase entitySet, EntityTypeBase entityType)
        {
            StorageEntityContainerMapping containerMapping = GetEntityContainerMap(mappingCollection, container);

            StorageSetMapping extentMap = containerMapping.GetSetMapping(entitySet.Name);
            StorageEntitySetMapping entitySetMapping = extentMap as StorageEntitySetMapping;

            //The Set may have no mapping
            if (entitySetMapping != null)
            {
                if (entitySetMapping != null) //could be association set mapping
                {
                    foreach (var v in entitySetMapping.ModificationFunctionMappings.Where(functionMap => functionMap.EntityType.Equals(entityType)))
                    {
                        yield return v;
                    }
                }
            }

        }

        internal static StorageEntityContainerMapping GetEntityContainerMap(StorageMappingItemCollection mappingCollection, EntityContainer entityContainer)
        {
            ReadOnlyCollection<StorageEntityContainerMapping> entityContainerMaps = mappingCollection.GetItems<StorageEntityContainerMapping>();
            StorageEntityContainerMapping entityContainerMap = null;
            foreach (StorageEntityContainerMapping map in entityContainerMaps)
            {
                if ((entityContainer.Equals(map.EdmEntityContainer))
                    || (entityContainer.Equals(map.StorageEntityContainer)))
                {
                    entityContainerMap = map;
                    break;
                }
            }
            if (entityContainerMap == null)
            {
                throw new MappingException(System.Data.Entity.Strings.Mapping_NotFound_EntityContainer(entityContainer.Name));
            }
            return entityContainerMap;
        }
    }
}

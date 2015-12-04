//---------------------------------------------------------------------
// <copyright file="DefaultObjectMappingItemCollection.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>

// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Mapping {

    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// The class creates a default OCMapping between a TypeMetadata in O space
    /// and an TypeMetadata in Edm space. The loader expects that for each member in 
    /// C space type there exists a member in O space type that has the same name. The member maps will be stored in
    /// C space member order.    
    /// </summary>
    internal class DefaultObjectMappingItemCollection : MappingItemCollection {
        #region Constructors
        /// <summary>
        /// Constrcutor to create an instance of DefaultObjectMappingItemCollection.
        /// To start with we will create a Schema under which maps will be created.
        /// </summary>
        /// <param name="edmCollection"></param>
        /// <param name="objectCollection"></param>
        public DefaultObjectMappingItemCollection(EdmItemCollection edmCollection,
            ObjectItemCollection objectCollection) : base(DataSpace.OCSpace)
        {
            EntityUtil.CheckArgumentNull(edmCollection, "edmCollection");
            EntityUtil.CheckArgumentNull(objectCollection, "objectCollection");
            this.m_edmCollection = edmCollection;
            this.m_objectCollection = objectCollection;
            LoadPrimitiveMaps();
        }
        #endregion

        #region Fields
        private ObjectItemCollection m_objectCollection;
        private EdmItemCollection m_edmCollection;
        private Dictionary<string, int> clrTypeIndexes = new Dictionary<string, int>(StringComparer.Ordinal); //Indexes into the type mappings collection based on clr type name
        private Dictionary<string, int> cdmTypeIndexes = new Dictionary<string, int>(StringComparer.Ordinal); //Indexes into the type mappings collection based on clr type name
        #endregion

        #region Methods
        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal override Map GetMap(string identity, DataSpace typeSpace, bool ignoreCase)
        {
            Map map;
            if (!this.TryGetMap(identity, typeSpace, ignoreCase, out map))
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.Mapping_Object_InvalidType(identity));
            }
            return map;
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="ignoreCase">true for case-insensitive lookup</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(string identity, DataSpace typeSpace, bool ignoreCase, out Map map)
        {
            EdmType cdmType = null;
            EdmType clrType = null;
            if (typeSpace == DataSpace.CSpace)
            {
                if (ignoreCase)
                {
                    // Get the correct casing of the identity first if we are asked to do ignore case
                    if (!m_edmCollection.TryGetItem<EdmType>(identity, true, out cdmType))
                    {
                        map = null;
                        return false;
                    }

                    identity = cdmType.Identity;
                }

                int index;
                if (cdmTypeIndexes.TryGetValue(identity, out index))
                {
                    map = (Map)this[index];
                    return true;
                }

                if (cdmType != null ||
                    m_edmCollection.TryGetItem<EdmType>(identity, ignoreCase, out cdmType))
                {
                    // If the mapping is not already loaded, then get the mapping ospace type
                    m_objectCollection.TryGetOSpaceType(cdmType, out clrType);
                }
            }
            else if (typeSpace == DataSpace.OSpace)
            {
                if (ignoreCase)
                {
                    // Get the correct casing of the identity first if we are asked to do ignore case
                    if (!m_objectCollection.TryGetItem<EdmType>(identity, true, out clrType))
                    {
                        map = null;
                        return false;
                    }

                    identity = clrType.Identity;
                }

                int index;
                if (clrTypeIndexes.TryGetValue(identity, out index))
                {
                    map = (Map)this[index];
                    return true;
                }

                if (clrType != null ||
                    m_objectCollection.TryGetItem<EdmType>(identity, ignoreCase, out clrType))
                {
                    // If the mapping is not already loaded, get the mapping cspace type
                    string cspaceTypeName = ObjectItemCollection.TryGetMappingCSpaceTypeIdentity(clrType);
                    m_edmCollection.TryGetItem<EdmType>(cspaceTypeName, out cdmType);
                }
            }

            if ((clrType == null) || (cdmType == null))
            {
                map = null;
                return false;
            }
            else
            {
                map = this.GetDefaultMapping(cdmType, clrType);
                return true;
            }
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <exception cref="ArgumentException"> Thrown if mapping space is not valid</exception>
        internal override Map GetMap(string identity, DataSpace typeSpace)
        {
            return this.GetMap(identity, typeSpace, false /*ignoreCase*/);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="identity">identity of the type</param>
        /// <param name="typeSpace">The dataspace that the type for which map needs to be returned belongs to</param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(string identity, DataSpace typeSpace, out Map map)
        {
            return this.TryGetMap(identity, typeSpace, false /*ignoreCase*/, out map);
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        internal override Map GetMap(GlobalItem item)
        {
            EntityUtil.CheckArgumentNull(item, "item");
            Map map;
            if (!this.TryGetMap(item, out map))
            {
                throw new InvalidOperationException(System.Data.Entity.Strings.Mapping_Object_InvalidType(item.Identity));
            }
            return map;
        }

        /// <summary>
        /// Search for a Mapping metadata with the specified type key.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="map"></param>
        /// <returns>Returns false if no match found.</returns>
        internal override bool TryGetMap(GlobalItem item, out Map map)
        {
            if (item == null)
            {
                map = null;
                return false;
            }

            DataSpace typeSpace = item.DataSpace;

            //For transient types just create a map on fly and return
            EdmType edmType = item as EdmType;
            if (edmType != null)
            {
                if (Helper.IsTransientType(edmType))
                {
                    map = GetOCMapForTransientType(edmType, typeSpace);
                    if (map != null)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                }
            }
            return this.TryGetMap(item.Identity, typeSpace, out map);
        }

        /// <summary>
        /// The method creates a default mapping between two TypeMetadatas - one in 
        /// C space and one in O space. The precondition for calling this method is that
        /// the type in Object space contains the members with the same name as those of defined in
        /// C space. It is not required the otherway.
        /// </summary>
        /// <param name="cdmType"></param>
        /// <param name="clrType"></param>
        private Map GetDefaultMapping(EdmType cdmType, EdmType clrType) {
            Debug.Assert((cdmType != null) && (clrType != null));
            return LoadObjectMapping(cdmType, clrType, this);
        }

        private Map GetOCMapForTransientType(EdmType edmType, DataSpace typeSpace)
        {
            Debug.Assert(typeSpace == DataSpace.CSpace || typeSpace == DataSpace.OSpace || Helper.IsRowType(edmType) || Helper.IsCollectionType(edmType));
            EdmType clrType = null;
            EdmType cdmType = null;
            int index = -1;
            if (typeSpace != DataSpace.OSpace)
            {
                if (cdmTypeIndexes.TryGetValue(edmType.Identity, out index))
                {
                    return (Map)this[index];
                }
                else
                {
                    cdmType = edmType;
                    clrType = ConvertCSpaceToOSpaceType(edmType);
                }
            }
            else if (typeSpace == DataSpace.OSpace)
            {
                if (clrTypeIndexes.TryGetValue(edmType.Identity, out index))
                {
                    return (Map)this[index];
                }
                else
                {
                    clrType = edmType;
                    cdmType = ConvertOSpaceToCSpaceType(clrType);
                }
            }

            ObjectTypeMapping typeMapping = new ObjectTypeMapping(clrType, cdmType);
            if (BuiltInTypeKind.RowType == edmType.BuiltInTypeKind)
            {
                RowType clrRowType = (RowType)clrType;
                RowType edmRowType = (RowType)cdmType;

                Debug.Assert(clrRowType.Properties.Count == edmRowType.Properties.Count, "Property count mismatch");
                for (int idx = 0; idx < clrRowType.Properties.Count; idx++)
                {
                    typeMapping.AddMemberMap(new ObjectPropertyMapping(edmRowType.Properties[idx], clrRowType.Properties[idx]));
                }
            }
            if ( (!cdmTypeIndexes.ContainsKey(cdmType.Identity)) && (!clrTypeIndexes.ContainsKey(clrType.Identity)) )
            {
            AddInternalMapping(typeMapping);
            }
            return typeMapping;
        }

        /// <summary>Convert CSpace TypeMetadata into OSpace TypeMetadata</summary>
        /// <param name="cdmType"></param>
        /// <returns>OSpace type metadata</returns>
        private EdmType ConvertCSpaceToOSpaceType(EdmType cdmType)
        {
            EdmType clrType = null;

            if (Helper.IsCollectionType(cdmType))
            {
                EdmType elemType = ConvertCSpaceToOSpaceType(((CollectionType)cdmType).TypeUsage.EdmType);
                clrType = new CollectionType(elemType);
            }
            else if (Helper.IsRowType(cdmType))
            {
                List<EdmProperty> clrProperties = new List<EdmProperty>();
                foreach (EdmProperty column in ((RowType)cdmType).Properties)
                {
                    EdmType clrPropertyType = ConvertCSpaceToOSpaceType(column.TypeUsage.EdmType);
                    EdmProperty clrProperty = new EdmProperty(column.Name, TypeUsage.Create(clrPropertyType));
                    clrProperties.Add(clrProperty);
                }
                clrType = new RowType(clrProperties, ((RowType)cdmType).InitializerMetadata);
            }
            else if (Helper.IsRefType(cdmType))
            {
                clrType = new RefType((EntityType)ConvertCSpaceToOSpaceType(((RefType)cdmType).ElementType));
            }
            else if (Helper.IsPrimitiveType(cdmType))
            {
                clrType = m_objectCollection.GetMappedPrimitiveType(((PrimitiveType)cdmType).PrimitiveTypeKind);
            }
            else
            {
                clrType = ((ObjectTypeMapping)GetMap(cdmType)).ClrType;
            }
            Debug.Assert((null != clrType), "null converted clr type");
            return clrType;
        }

        /// <summary>Convert CSpace TypeMetadata into OSpace TypeMetadata</summary>
        /// <param name="clrType"></param>
        /// <returns>OSpace type metadata</returns>
        private EdmType ConvertOSpaceToCSpaceType(EdmType clrType)
        {
            EdmType cdmType = null;

            if (Helper.IsCollectionType(clrType))
            {
                EdmType elemType = ConvertOSpaceToCSpaceType(((CollectionType)clrType).TypeUsage.EdmType);
                cdmType = new CollectionType(elemType);
            }
            else if (Helper.IsRowType(clrType))
            {
                List<EdmProperty> cdmProperties = new List<EdmProperty>();
                foreach (EdmProperty column in ((RowType)clrType).Properties)
                {
                    EdmType cdmPropertyType = ConvertOSpaceToCSpaceType(column.TypeUsage.EdmType);
                    EdmProperty cdmPorperty = new EdmProperty(column.Name, TypeUsage.Create(cdmPropertyType));
                    cdmProperties.Add(cdmPorperty);
                }
                cdmType = new RowType(cdmProperties, ((RowType)clrType).InitializerMetadata);
            }
            else if (Helper.IsRefType(clrType))
            {
                cdmType = new RefType((EntityType)(ConvertOSpaceToCSpaceType(((RefType)clrType).ElementType)));
            }
            else
            {
                cdmType = ((ObjectTypeMapping)GetMap(clrType)).EdmType;
            }
            Debug.Assert((null != cdmType), "null converted clr type");
            return cdmType;
        }

        /// <summary>
        /// checks if the schemaKey refers to the primitive OC mapping schema and if true, 
        /// loads the maps between primitive types
        /// </summary>
        /// <returns>returns the loaded schema if the schema key refers to a primitive schema</returns>
        private void LoadPrimitiveMaps() {
            // Get all the primitive types from the CSpace and create OCMaps for it
            IEnumerable<PrimitiveType> cspaceTypes = m_edmCollection.GetPrimitiveTypes();
            foreach (PrimitiveType type in cspaceTypes)
            {
                PrimitiveType ospaceType = m_objectCollection.GetMappedPrimitiveType(type.PrimitiveTypeKind);
                Debug.Assert(ospaceType != null, "all primitive type must have been loaded");
                this.AddInternalMapping(new ObjectTypeMapping(ospaceType, type));
            }
        }

        // Add to the cache. If it is already present, then throw an exception
        private void AddInternalMapping(ObjectTypeMapping objectMap)
        {
            string clrName = objectMap.ClrType.Identity;
            string cdmName = objectMap.EdmType.Identity;
            int currIndex = this.Count;
            //Always assume that the first Map for an associated map being added is
            //the default map for primitive type. Similarly, row and collection types can collide
            //because their components are primitive types. For other types,
            //there should be only one map
            if (clrTypeIndexes.ContainsKey(clrName))
            {
                if (BuiltInTypeKind.PrimitiveType != objectMap.ClrType.BuiltInTypeKind &&
                    BuiltInTypeKind.RowType != objectMap.ClrType.BuiltInTypeKind &&
                    BuiltInTypeKind.CollectionType != objectMap.ClrType.BuiltInTypeKind)
                {
                    throw new MappingException(System.Data.Entity.Strings.Mapping_Duplicate_Type(clrName));
                }
            }
            else
            {
                clrTypeIndexes.Add(clrName, currIndex);
            }
            if (cdmTypeIndexes.ContainsKey(cdmName))
            {
                if (BuiltInTypeKind.PrimitiveType != objectMap.EdmType.BuiltInTypeKind &&
                    BuiltInTypeKind.RowType != objectMap.EdmType.BuiltInTypeKind &&
                    BuiltInTypeKind.CollectionType != objectMap.EdmType.BuiltInTypeKind)
                {
                    throw new MappingException(System.Data.Entity.Strings.Mapping_Duplicate_Type(clrName));
                }
            }
            else
            {
                cdmTypeIndexes.Add(cdmName, currIndex);
            }
            objectMap.DataSpace = DataSpace.OCSpace;
            base.AddInternal (objectMap);
        }

        /// <summary>
        /// The method fills up the children of ObjectMapping. It goes through the
        /// members in CDM type and finds the member in Object space with the same name 
        /// and creates a member map between them. These member maps are added
        /// as children of the object mapping.
        /// </summary>
        /// <param name="cdmType"></param>
        /// <param name="objectType"></param>
        /// <param name="ocItemCollection"></param>
        internal static ObjectTypeMapping LoadObjectMapping(EdmType cdmType, EdmType objectType, DefaultObjectMappingItemCollection ocItemCollection)
        {
            Dictionary<string, ObjectTypeMapping> typeMappings = new Dictionary<string, ObjectTypeMapping>(StringComparer.Ordinal);
            ObjectTypeMapping typeMapping = LoadObjectMapping(cdmType, objectType, ocItemCollection, typeMappings);

            // If DefaultOCMappingItemCollection is not null, add all the type mappings to the item collection
            if (ocItemCollection != null)
            {
                foreach (ObjectTypeMapping map in typeMappings.Values)
                {
                    ocItemCollection.AddInternalMapping(map);
                }
            }

            return typeMapping;
        }

        private static ObjectTypeMapping LoadObjectMapping(EdmType edmType, EdmType objectType, DefaultObjectMappingItemCollection ocItemCollection,
            Dictionary<string, ObjectTypeMapping> typeMappings)
        {
            Debug.Assert((edmType != null) && (objectType != null));
            Debug.Assert((edmType.BuiltInTypeKind == objectType.BuiltInTypeKind), "The BuiltInTypeKind must be same in LoadObjectMapping");

            if (Helper.IsEnumType(edmType) ^ Helper.IsEnumType(objectType))
            {
                throw new MappingException(System.Data.Entity.Strings.Mapping_EnumTypeMappingToNonEnumType(edmType.FullName, objectType.FullName));
            }

            // Check if both the types are abstract or both of them are not
            if (edmType.Abstract != objectType.Abstract)
            {
                throw new MappingException(System.Data.Entity.Strings.Mapping_AbstractTypeMappingToNonAbstractType(edmType.FullName, objectType.FullName));
            }

            ObjectTypeMapping objectTypeMapping = new ObjectTypeMapping(objectType, edmType);
            typeMappings.Add(edmType.FullName, objectTypeMapping);

            if (Helper.IsEntityType(edmType) || Helper.IsComplexType(edmType))
            {
                LoadEntityTypeOrComplexTypeMapping(objectTypeMapping, edmType, objectType, ocItemCollection, typeMappings);
            }
            else if (Helper.IsEnumType(edmType))
            {
                ValidateEnumTypeMapping((EnumType)edmType, (EnumType)objectType);
            }
            else
            {
                Debug.Assert(Helper.IsAssociationType(edmType));

                LoadAssociationTypeMapping(objectTypeMapping, edmType, objectType, ocItemCollection, typeMappings);
            }

            return objectTypeMapping;
        }

        /// <summary>
        /// Tries and get the mapping ospace member for the given edmMember and the ospace type
        /// </summary>
        /// <param name="edmMember"></param>
        /// <param name="objectType"></param>
        /// <returns></returns
        private static EdmMember GetObjectMember(EdmMember edmMember, StructuralType objectType)
        {
            // Assuming that we will have a single member in O-space for a member in C space
            EdmMember objectMember;
            if (!objectType.Members.TryGetValue(edmMember.Name, false/*ignoreCase*/, out objectMember))
            {
                throw new MappingException(System.Data.Entity.Strings.Mapping_Default_OCMapping_Clr_Member(
                          edmMember.Name, edmMember.DeclaringType.FullName, objectType.FullName));
            }

            return objectMember;
        }

        private static void ValidateMembersMatch(EdmMember edmMember, EdmMember objectMember)
        {
            Debug.Assert(edmMember.DeclaringType.DataSpace == DataSpace.CSpace, "the cspace member is not on a cspace type");
            Debug.Assert(objectMember.DeclaringType.DataSpace == DataSpace.OSpace, "the ospace member is not on a cspace type");

            // Make sure the property type is the same
            if (edmMember.BuiltInTypeKind != objectMember.BuiltInTypeKind)
            {
                throw new MappingException(System.Data.Entity.Strings.Mapping_Default_OCMapping_MemberKind_Mismatch(
                          edmMember.Name, edmMember.DeclaringType.FullName, edmMember.BuiltInTypeKind,
                          objectMember.Name, objectMember.DeclaringType.FullName, objectMember.BuiltInTypeKind));
            }

            // Make sure the member type is the same
            if (edmMember.TypeUsage.EdmType.BuiltInTypeKind != objectMember.TypeUsage.EdmType.BuiltInTypeKind)
            {
                // use EntityRes.GetString(EntityRes. instead of Strings. because the generated method does not
                // include all string parameters (6 rather than 8)
                throw new MappingException(EntityRes.GetString(EntityRes.Mapping_Default_OCMapping_Member_Type_Mismatch,
                    edmMember.TypeUsage.EdmType.Name, edmMember.TypeUsage.EdmType.BuiltInTypeKind, edmMember.Name, edmMember.DeclaringType.FullName,
                    objectMember.TypeUsage.EdmType.Name, objectMember.TypeUsage.EdmType.BuiltInTypeKind, objectMember.Name, objectMember.DeclaringType.FullName));
            }

            if (Helper.IsPrimitiveType(edmMember.TypeUsage.EdmType))
            {
                PrimitiveType memberType = Helper.GetSpatialNormalizedPrimitiveType(edmMember.TypeUsage.EdmType);

                //We expect the CLR prmitive type and their corresponding EDM primitive types to have the same primitive type kind( atleast for now)
                if (memberType.PrimitiveTypeKind != ((PrimitiveType)objectMember.TypeUsage.EdmType).PrimitiveTypeKind)
                {
                    
                    throw new MappingException(System.Data.Entity.Strings.Mapping_Default_OCMapping_Invalid_MemberType(
                              edmMember.TypeUsage.EdmType.FullName, edmMember.Name, edmMember.DeclaringType.FullName,
                              objectMember.TypeUsage.EdmType.FullName, objectMember.Name, objectMember.DeclaringType.FullName));
                }
            }
            else if (Helper.IsEnumType(edmMember.TypeUsage.EdmType))
            {
                Debug.Assert(
                    Helper.IsEnumType(objectMember.TypeUsage.EdmType), 
                    "Both types are expected to by EnumTypes. For non-matching types we should have already thrown.");

                ValidateEnumTypeMapping((EnumType)edmMember.TypeUsage.EdmType, (EnumType)objectMember.TypeUsage.EdmType);
            }
            else
            {
                EdmType edmMemberType;
                EdmType objectMemberType;

                if (BuiltInTypeKind.AssociationEndMember == edmMember.BuiltInTypeKind)
                {
                    edmMemberType = ((RefType)edmMember.TypeUsage.EdmType).ElementType;
                    objectMemberType = ((RefType)objectMember.TypeUsage.EdmType).ElementType;
                }
                else if (BuiltInTypeKind.NavigationProperty == edmMember.BuiltInTypeKind &&
                         Helper.IsCollectionType(edmMember.TypeUsage.EdmType))
                {
                    edmMemberType = ((CollectionType)edmMember.TypeUsage.EdmType).TypeUsage.EdmType;
                    objectMemberType = ((CollectionType)objectMember.TypeUsage.EdmType).TypeUsage.EdmType;
                }
                else
                {
                    edmMemberType = edmMember.TypeUsage.EdmType;
                    objectMemberType = objectMember.TypeUsage.EdmType;
                }

                if (edmMemberType.Identity != ObjectItemCollection.TryGetMappingCSpaceTypeIdentity(objectMemberType))
                {
                    throw new MappingException(System.Data.Entity.Strings.Mapping_Default_OCMapping_Invalid_MemberType(
                              edmMember.TypeUsage.EdmType.FullName, edmMember.Name, edmMember.DeclaringType.FullName,
                              objectMember.TypeUsage.EdmType.FullName, objectMember.Name, objectMember.DeclaringType.FullName));
                }
            }

        }

        /// <summary>
        /// Validates the scalar property on the cspace side and ospace side and creates a new 
        /// ObjectPropertyMapping, if everything maps property
        /// </summary>
        /// <param name="edmProperty"></param>
        /// <param name="objectProperty"></param>
        /// <returns></returns>
        private static ObjectPropertyMapping LoadScalarPropertyMapping(EdmProperty edmProperty, EdmProperty objectProperty)
        {
            Debug.Assert(
                Helper.IsScalarType(edmProperty.TypeUsage.EdmType), 
                "Only edm scalar properties expected");
            Debug.Assert(
                Helper.IsScalarType(objectProperty.TypeUsage.EdmType), 
                "Only object scalar properties expected");

            return new ObjectPropertyMapping(edmProperty, objectProperty);
        }

        /// <summary>
        /// Load the entity type or complex type mapping
        /// </summary>
        /// <param name="objectMapping"></param>
        /// <param name="edmType"></param>
        /// <param name="objectType"></param>
        /// <param name="ocItemCollection">
        /// <param name="typeMappings"></param></param>
        private static void LoadEntityTypeOrComplexTypeMapping(ObjectTypeMapping objectMapping, EdmType edmType, EdmType objectType,
            DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
        {
            Debug.Assert(edmType.BuiltInTypeKind == BuiltInTypeKind.EntityType ||
                    edmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType, 
                    "Expected Type Encountered in LoadEntityTypeOrComplexTypeMapping");
            Debug.Assert((edmType.BuiltInTypeKind == objectType.BuiltInTypeKind), "The BuiltInTypeKind must be same in LoadEntityTypeOrComplexTypeMapping");

            StructuralType cdmStructuralType = (StructuralType)edmType;
            StructuralType objectStructuralType = (StructuralType)objectType;

            ValidateAllMembersAreMapped(cdmStructuralType, objectStructuralType);

            //Go through the CDMMembers and find the corresponding member in Object space
            //and create a member map.
            foreach (EdmMember edmMember in cdmStructuralType.Members)
            {

                EdmMember objectMember = GetObjectMember(edmMember, objectStructuralType);
                ValidateMembersMatch(edmMember, objectMember);

                if (Helper.IsEdmProperty(edmMember))
                {

                    EdmProperty edmPropertyMember = (EdmProperty)edmMember;
                    EdmProperty edmPropertyObject = (EdmProperty)objectMember;

                    //Depending on the type of member load the member mapping i.e. For complex
                    //members we have to go in and load the child members of the Complex type.
                    if (Helper.IsComplexType(edmMember.TypeUsage.EdmType))
                    {
                        objectMapping.AddMemberMap(
                            LoadComplexMemberMapping(edmPropertyMember, edmPropertyObject, ocItemCollection, typeMappings));
                    }
                    else
                    {
                        objectMapping.AddMemberMap(
                            LoadScalarPropertyMapping(edmPropertyMember, edmPropertyObject));
                    }
                }
                else
                {
                    Debug.Assert(edmMember.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty, "Unexpected Property type encountered");

                    // For navigation properties, we need to make sure the relationship type on the navigation property is mapped
                    NavigationProperty navigationProperty = (NavigationProperty)edmMember;
                    NavigationProperty objectNavigationProperty = (NavigationProperty)objectMember;
                    LoadTypeMapping(navigationProperty.RelationshipType, objectNavigationProperty.RelationshipType, ocItemCollection, typeMappings);

                    objectMapping.AddMemberMap(new ObjectNavigationPropertyMapping(navigationProperty, objectNavigationProperty));
                }

            }
        }

        private static void ValidateAllMembersAreMapped(StructuralType cdmStructuralType, StructuralType objectStructuralType)
        {
            Debug.Assert(cdmStructuralType.BuiltInTypeKind == objectStructuralType.BuiltInTypeKind, "the types must be the same");

            // error if they don't have the same required members, or if
            // some object concepts don't exist in cspace (it is ok if the ospace is missing some cspace concepts)
            if (cdmStructuralType.Members.Count != objectStructuralType.Members.Count)
            {
                throw new MappingException(System.Data.Entity.Strings.Mapping_Default_OCMapping_Member_Count_Mismatch(
                          cdmStructuralType.FullName, objectStructuralType.FullName));
            }

            foreach (EdmMember member in objectStructuralType.Members)
            {

                if(!cdmStructuralType.Members.Contains(member.Identity))
                {
                    throw new MappingException(System.Data.Entity.Strings.Mapping_Default_OCMapping_Clr_Member2(
                            member.Name, objectStructuralType.FullName, cdmStructuralType.FullName));
                }
            }
        }

        /// <summary>
        /// Validates whether CSpace enum type and OSpace enum type match.
        /// </summary>
        /// <param name="edmEnumType">CSpace enum type.</param>
        /// <param name="objectEnumType">OSpace enum type.</param>
        private static void ValidateEnumTypeMapping(EnumType edmEnumType, EnumType objectEnumType)
        {
            Debug.Assert(edmEnumType != null, "edmEnumType != null");
            Debug.Assert(Helper.IsPrimitiveType(edmEnumType.UnderlyingType));
            Debug.Assert(Helper.IsSupportedEnumUnderlyingType(edmEnumType.UnderlyingType.PrimitiveTypeKind));

            Debug.Assert(objectEnumType != null, "objectEnumType != null");
            Debug.Assert(Helper.IsPrimitiveType(objectEnumType.UnderlyingType));
            Debug.Assert(Helper.IsSupportedEnumUnderlyingType(objectEnumType.UnderlyingType.PrimitiveTypeKind));

            if (edmEnumType.UnderlyingType.PrimitiveTypeKind != objectEnumType.UnderlyingType.PrimitiveTypeKind)
            {
                throw new MappingException(
                    System.Data.Entity.Strings.Mapping_Enum_OCMapping_UnderlyingTypesMismatch(
                        edmEnumType.UnderlyingType.Name, 
                        edmEnumType.FullName,
                        objectEnumType.UnderlyingType.Name,
                        objectEnumType.FullName));
            }

            // EnumMember.Value is just a number so sorting by value is faster than by the name. 
            // The drawback is that there can be multiple members with the same value. To break 
            // the tie we need to sort by name after sorting by value.
            var edmEnumTypeMembersSortedEnumerator =
                edmEnumType.Members.OrderBy(m => Convert.ToInt64(m.Value, CultureInfo.InvariantCulture)).ThenBy(m => m.Name).GetEnumerator();
            var objectEnumTypeMembersSortedEnumerator =
                objectEnumType.Members.OrderBy(m => Convert.ToInt64(m.Value, CultureInfo.InvariantCulture)).ThenBy(m => m.Name).GetEnumerator();

            if (edmEnumTypeMembersSortedEnumerator.MoveNext())
            {
                while (objectEnumTypeMembersSortedEnumerator.MoveNext())
                {
                    if (edmEnumTypeMembersSortedEnumerator.Current.Name == objectEnumTypeMembersSortedEnumerator.Current.Name &&
                        edmEnumTypeMembersSortedEnumerator.Current.Value.Equals(objectEnumTypeMembersSortedEnumerator.Current.Value))
                    {
                        if (!edmEnumTypeMembersSortedEnumerator.MoveNext())
                        {
                            return;
                        }
                    }
                }

                throw new MappingException(
                    System.Data.Entity.Strings.Mapping_Enum_OCMapping_MemberMismatch(
                        objectEnumType.FullName,
                        edmEnumTypeMembersSortedEnumerator.Current.Name,
                        edmEnumTypeMembersSortedEnumerator.Current.Value,
                        edmEnumType.FullName));
            }
        }

        /// <summary>
        /// Loads Association Type Mapping
        /// </summary>
        /// <param name="objectMapping"></param>
        /// <param name="edmType"></param>
        /// <param name="objectType"></param>
        /// <param name="ocItemCollection"></param>
        /// <param name="typeMappings"></param>
        private static void LoadAssociationTypeMapping(ObjectTypeMapping objectMapping, EdmType edmType, EdmType objectType,
            DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
        {
            Debug.Assert(edmType.BuiltInTypeKind == BuiltInTypeKind.AssociationType, "Expected Type Encountered in LoadAssociationTypeMapping");
            Debug.Assert((edmType.BuiltInTypeKind == objectType.BuiltInTypeKind), "The BuiltInTypeKind must be same in LoadAssociationTypeMapping");

            AssociationType association = (AssociationType)edmType;
            AssociationType objectAssociation = (AssociationType)objectType;

            foreach (AssociationEndMember edmEnd in association.AssociationEndMembers)
            {
                AssociationEndMember objectEnd = (AssociationEndMember)GetObjectMember(edmEnd, objectAssociation);
                ValidateMembersMatch(edmEnd, objectEnd);

                if (edmEnd.RelationshipMultiplicity != objectEnd.RelationshipMultiplicity)
                {
                    throw new MappingException(System.Data.Entity.Strings.Mapping_Default_OCMapping_MultiplicityMismatch(
                        edmEnd.RelationshipMultiplicity, edmEnd.Name, association.FullName,
                        objectEnd.RelationshipMultiplicity, objectEnd.Name, objectAssociation.FullName));
                }

                Debug.Assert(edmEnd.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.RefType, "Ends must be of Ref type");

                // GetMap for the entity types for the ends of the relationship type to make sure
                // the entity type mentioned are valid
                LoadTypeMapping(((RefType)edmEnd.TypeUsage.EdmType).ElementType,
                                ((RefType)objectEnd.TypeUsage.EdmType).ElementType, ocItemCollection, typeMappings);

                objectMapping.AddMemberMap(new ObjectAssociationEndMapping(edmEnd, objectEnd));
            }
        }

        /// <summary>
        /// The method loads the EdmMember mapping for complex members.
        /// It goes through the CDM members of the Complex Cdm type and
        /// tries to find the corresponding members in Complex Clr type.
        /// </summary>
        /// <param name="containingEdmMember"></param>
        /// <param name="containingClrMember"></param>
        /// <param name="ocItemCollection"></param>
        /// <param name="typeMappings"></param>
        /// <returns></returns>
        private static ObjectComplexPropertyMapping LoadComplexMemberMapping(EdmProperty containingEdmMember, EdmProperty containingClrMember,
            DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
        {
            Debug.Assert(containingEdmMember.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType, "edm member declaringType must be of complexType");
            Debug.Assert(containingClrMember.TypeUsage.EdmType.BuiltInTypeKind == BuiltInTypeKind.ComplexType, "clr member declaringType must be of complexType");

            ComplexType edmComplexType = (ComplexType)containingEdmMember.TypeUsage.EdmType;
            ComplexType objectComplexType = (ComplexType)containingClrMember.TypeUsage.EdmType;

            // Get the type mapping for the complex type
            ObjectTypeMapping complexTypeMapping = LoadTypeMapping(edmComplexType, objectComplexType, ocItemCollection, typeMappings);

            //Go through the CDMMembers and find the corresponding member in Object space
            //and create a member map.
            return new ObjectComplexPropertyMapping(containingEdmMember, containingClrMember, complexTypeMapping);
        }

        private static ObjectTypeMapping LoadTypeMapping(EdmType edmType, EdmType objectType, 
            DefaultObjectMappingItemCollection ocItemCollection, Dictionary<string, ObjectTypeMapping> typeMappings)
        {
            ObjectTypeMapping objectTypeMapping;

            //First, check in the type mappings to find out if the mapping is already present
            if (typeMappings.TryGetValue(edmType.FullName, out objectTypeMapping))
            {
                return objectTypeMapping;
            }

            if (ocItemCollection != null)
            {
                ObjectTypeMapping typeMapping;

                if (ocItemCollection.ContainsMap(edmType, out typeMapping))
                {
                    return (ObjectTypeMapping)typeMapping;
                }
            }

            // If the type mapping is not already loaded, then load it
            return LoadObjectMapping(edmType, objectType, ocItemCollection, typeMappings);
        }

        private bool ContainsMap(GlobalItem cspaceItem, out ObjectTypeMapping map)
        {
            Debug.Assert(cspaceItem.DataSpace == DataSpace.CSpace, "ContainsMap: It must be a CSpace item");
            int index;
            if (cdmTypeIndexes.TryGetValue(cspaceItem.Identity, out index))
            {
                map = (ObjectTypeMapping)this[index];
                return true;
            }

            map = null;
            return false;
        }

        #endregion
    }
}

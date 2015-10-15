//---------------------------------------------------------------------
// <copyright file="StructuredTypeInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections;
using System.Collections.Generic;
//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

using System.Globalization;
using System.Linq;
using System.Data.Common;
using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{

    /// <summary>
    /// The type flattener module is part of the structured type elimination phase,
    /// and is largely responsible for "flattening" record and nominal types into 
    /// flat record types. Additionally, for nominal types, this module produces typeid
    /// values that can be used later to interpret the input data stream.
    ///
    /// The goal of this module is to load up information about type and entityset metadata
    /// used in the ITree. This module is part of the "StructuredTypeElimination" phase, 
    /// and provides information to help in this process. 
    /// 
    /// This module itself is broken down into multiple parts.
    /// 
    /// (*) Loading type information: We walk the query tree to identify all references
    ///     to structured types and entity sets
    /// 
    /// (*) Processing entitysets: We walk the list of entitysets, and assign ids to each
    ///     entityset. We also create a map of id->entityset metadata in this phase.
    /// 
    /// (*) Processing types: We then walk the list of types, and process each type. This, 
    ///     in turn, is also broken into multiple parts:
    /// 
    ///     * Populating the Type Map: we walk the list of reference types and add each of
    ///       them to our typeMap, along with their base types.
    /// 
    ///     * TypeId assignment: We assign typeids to each nominal (complextype/entitytype).
    ///       This typeid is based on a dewey encoding. The typeid of a type is typically
    ///       the typeid of its supertype suffixed by the subtype number of this type within
    ///       its supertype. This encoding is intended to support easy type matching 
    ///       later on in the query - both for exact (IS OF ONLY) and inexact (IS OF) matches.
    /// 
    ///     * Type flattening: We then "explode"/"flatten" each structured type - refs, 
    ///       entity types, complex types and record types. The result is a flattened type
    ///       where every single property of the resulting type is a primitive/scalar type
    ///       (Note: UDTs are considered to be scalar types). Additional information may also
    ///       be encoded as a type property. For example, a typeid property is added (if
    ///       necessary) to complex/entity types to help discriminate polymorphic instances.
    ///       An EntitySetId property is added to ref and entity type attributes to help
    ///       determine the entity set that a given entity instance comes from.
    ///       As part of type flattening, we keep track of additional information that allows
    ///       us to map easily from the original property to the properties in the new type
    ///
    /// The final result of this processing is an object that contains:
    /// 
    ///  * a TypeInfo (extra type information) for each structured type in the query
    ///  * a map from typeid value to type. To be used later by result assembly
    ///  * a map between entitysetid value and entityset. To be used later by result assembly 
    ///
    /// NOTE: StructuredTypeInfo is probably not the best name for this class, since
    ///       it doesn't derive from TypeInfo but rather manages a collection of them.
    ///       I don't have a better name, but if you come up with one change this.
    /// 
    /// </summary>
    internal class StructuredTypeInfo
    {

        #region private state

        private md.TypeUsage m_stringType;
        private md.TypeUsage m_intType;
        private Dictionary<md.TypeUsage, TypeInfo> m_typeInfoMap;
        private bool m_typeInfoMapPopulated;
        private md.EntitySet[] m_entitySetIdToEntitySetMap; //used as a Dictionary with the index as key
        private Dictionary<md.EntitySet, int> m_entitySetToEntitySetIdMap;
        // A mapping from entity types to the "single" entityset (in the query) that can
        // produce instances of that entity. If there are multiple entitysets of the
        // same type, or "free-floating" entity constructors in the query, then 
        // the corresponding entry is null
        private Dictionary<md.EntityTypeBase, md.EntitySet> m_entityTypeToEntitySetMap;
        private Dictionary<md.EntitySetBase, ExplicitDiscriminatorMap> m_discriminatorMaps;
        private RelPropertyHelper m_relPropertyHelper;
        private HashSet<string> m_typesNeedingNullSentinel;
        #endregion

        #region constructor

        private StructuredTypeInfo(HashSet<string> typesNeedingNullSentinel)
        {

            // 




            m_typeInfoMap = new Dictionary<md.TypeUsage, TypeInfo>(TypeUsageEqualityComparer.Instance);
            m_typeInfoMapPopulated = false;
            m_typesNeedingNullSentinel = typesNeedingNullSentinel;
        }

        #endregion

        #region Process driver

        /// <summary>
        /// Process Driver
        /// </summary>
        /// <param name="itree"></param>
        /// <param name="referencedTypes">structured types referenced in the query</param>
        /// <param name="referencedEntitySets">entitysets referenced in the query</param>
        /// <param name="freeFloatingEntityConstructorTypes">entity types that have "free-floating" entity constructors</param>
        /// <param name="discriminatorMaps">information on optimized discriminator patterns for entity sets</param>
        /// <param name="relPropertyHelper">helper for rel properties</param>
        /// <param name="typesNeedingNullSentinel">which types need a null sentinel</param>
        /// <param name="structuredTypeInfo"></param>
        internal static void Process(Command itree,
            HashSet<md.TypeUsage> referencedTypes,
            HashSet<md.EntitySet> referencedEntitySets,
            HashSet<md.EntityType> freeFloatingEntityConstructorTypes,
            Dictionary<md.EntitySetBase, DiscriminatorMapInfo> discriminatorMaps,
            RelPropertyHelper relPropertyHelper,
            HashSet<string> typesNeedingNullSentinel,
            out StructuredTypeInfo structuredTypeInfo)
        {
            structuredTypeInfo = new StructuredTypeInfo(typesNeedingNullSentinel);
            structuredTypeInfo.Process(itree, referencedTypes, referencedEntitySets, freeFloatingEntityConstructorTypes, discriminatorMaps, relPropertyHelper);
        }

        /// <summary>
        /// Fills the StructuredTypeInfo instance from the itree provided.
        /// </summary>
        /// <param name="itree"></param>
        /// <param name="referencedTypes">referenced structured types</param>
        /// <param name="referencedEntitySets">referenced entitysets</param>
        /// <param name="freeFloatingEntityConstructorTypes">free-floating entityConstructor types</param>
        /// <param name="discriminatorMaps">discriminator information for entity sets mapped using TPH pattern</param>
        /// <param name="relPropertyHelper">helper for rel properties</param>
        private void Process(Command itree,
            HashSet<md.TypeUsage> referencedTypes,
            HashSet<md.EntitySet> referencedEntitySets,
            HashSet<md.EntityType> freeFloatingEntityConstructorTypes,
            Dictionary<md.EntitySetBase, DiscriminatorMapInfo> discriminatorMaps,
            RelPropertyHelper relPropertyHelper)
        {
            PlanCompiler.Assert(null != itree, "null itree?");

            m_stringType = itree.StringType;
            m_intType = itree.IntegerType;
            m_relPropertyHelper = relPropertyHelper;

            ProcessEntitySets(referencedEntitySets, freeFloatingEntityConstructorTypes);
            ProcessDiscriminatorMaps(discriminatorMaps);
            ProcessTypes(referencedTypes);
        }

        #endregion

        #region "public" properties

        /// <summary>
        /// Mapping from entitysetid-s to entitysets
        /// </summary>
        internal md.EntitySet[] EntitySetIdToEntitySetMap
        {
            get
            {
                return m_entitySetIdToEntitySetMap;
            }
        }

        #endregion

        #region "public" methods
        /// <summary>
        /// Get a helper for rel properties
        /// </summary>
        internal RelPropertyHelper RelPropertyHelper
        {
            get { return m_relPropertyHelper; }
        }
        /// <summary>
        /// Gets the "single" entityset that stores instances of this type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal md.EntitySet GetEntitySet(md.EntityTypeBase type)
        {
            md.EntitySet set;
            md.EntityTypeBase rootType = GetRootType(type);
            if (!m_entityTypeToEntitySetMap.TryGetValue(rootType, out set))
            {
                return null;
            }
            return set;
        }

        /// <summary>
        /// Get the entitysetid value for a given entityset
        /// </summary>
        /// <param name="e">the entityset</param>
        /// <returns>entitysetid value</returns>
        internal int GetEntitySetId(md.EntitySet e)
        {
            int result = 0;

            if (!m_entitySetToEntitySetIdMap.TryGetValue(e, out result))
            {
                PlanCompiler.Assert(false, "no such entity set?");
            }
            return result;
        }

        /// <summary>
        /// Gets entity sets referenced by the query.
        /// </summary>
        /// <returns>entity sets</returns>
        internal Common.Utils.Set<md.EntitySet> GetEntitySets()
        {
            return new Common.Utils.Set<md.EntitySet>(m_entitySetIdToEntitySetMap).MakeReadOnly();
        }

        /// <summary>
        /// Find the TypeInfo entry for a type. For non-structured types, we always
        /// return null. For structured types, we return the entry in the typeInfoMap. 
        /// If we don't find one, and the typeInfoMap has already been populated, then we
        /// assert
        /// </summary>
        /// <param name="type">the type to look up</param>
        /// <returns>the typeinfo for the type (null if we couldn't find one)</returns>
        internal TypeInfo GetTypeInfo(md.TypeUsage type)
        {
            if (!TypeUtils.IsStructuredType(type))
            {
                return null;
            }
            TypeInfo typeInfo = null;
            if (!m_typeInfoMap.TryGetValue(type, out typeInfo))
            {
                PlanCompiler.Assert(!TypeUtils.IsStructuredType(type) || !m_typeInfoMapPopulated,
                    "cannot find typeInfo for type " + type);
            }
            return typeInfo;
        }

        #endregion

        #region private methods

        #region EntitySet processing methods

        /// <summary>
        /// Add a new entry to the entityTypeToSet map
        /// </summary>
        /// <param name="entityType">entity type</param>
        /// <param name="entitySet">entityset producing this type</param>
        private void AddEntityTypeToSetEntry(md.EntityType entityType, md.EntitySet entitySet)
        {
            md.EntitySet other;
            md.EntityTypeBase rootType = GetRootType(entityType);
            bool hasSingleEntitySet = true;

            if (entitySet == null)
            {
                hasSingleEntitySet = false;
            }
            else if (m_entityTypeToEntitySetMap.TryGetValue(rootType, out other))
            {
                if (other != entitySet)
                {
                    hasSingleEntitySet = false;
                }
            }

            if (hasSingleEntitySet)
            {
                m_entityTypeToEntitySetMap[rootType] = entitySet;
            }
            else
            {
                m_entityTypeToEntitySetMap[rootType] = null;
            }
        }

        /// <summary>
        /// Handle any relevant processing for entity sets
        /// <param name="referencedEntitySets">referenced entitysets</param>
        /// <param name="freeFloatingEntityConstructorTypes">free-floating entity constructor types</param>
        /// </summary>
        private void ProcessEntitySets(HashSet<md.EntitySet> referencedEntitySets, HashSet<md.EntityType> freeFloatingEntityConstructorTypes)
        {
            AssignEntitySetIds(referencedEntitySets);

            //
            // set up the entity-type to set map
            //
            m_entityTypeToEntitySetMap = new Dictionary<md.EntityTypeBase, md.EntitySet>();
            foreach (md.EntitySet e in referencedEntitySets)
            {
                AddEntityTypeToSetEntry(e.ElementType, e);
            }
            foreach (md.EntityType t in freeFloatingEntityConstructorTypes)
            {
                AddEntityTypeToSetEntry(t, null);
            }
        }

        /// <summary>
        /// Handle discriminator maps (determine which can safely be used in the query)
        /// </summary>
        private void ProcessDiscriminatorMaps(Dictionary<md.EntitySetBase, DiscriminatorMapInfo> discriminatorMaps)
        {
            // Only use custom type discrimination where a type has a single entity set. Where
            // there are multiple sets, discriminator properties and flattened representations
            // may be incompatible.
            Dictionary<md.EntitySetBase, ExplicitDiscriminatorMap> filteredMaps = null;
            if (null != discriminatorMaps)
            {
                filteredMaps = new Dictionary<md.EntitySetBase, ExplicitDiscriminatorMap>(discriminatorMaps.Count, discriminatorMaps.Comparer);
                foreach (KeyValuePair<md.EntitySetBase, DiscriminatorMapInfo> setMapPair in discriminatorMaps)
                {
                    md.EntitySetBase set = setMapPair.Key;
                    ExplicitDiscriminatorMap map = setMapPair.Value.DiscriminatorMap;
                    if (null != map)
                    {
                        md.EntityTypeBase rootType = GetRootType(set.ElementType);
                        bool hasOneSet = GetEntitySet(rootType) != null;
                        if (hasOneSet)
                        {
                            filteredMaps.Add(set, map);
                        }
                    }
                }
                if (filteredMaps.Count == 0)
                {
                    // don't bother keeping the dictionary if it's empty
                    filteredMaps = null;
                }
            }
            m_discriminatorMaps = filteredMaps;
        }

        /// <summary>
        /// Assign ids to each entityset in the query
        /// <param name="referencedEntitySets">referenced entitysets</param>
        /// </summary>
        private void AssignEntitySetIds(HashSet<md.EntitySet> referencedEntitySets)
        {
            m_entitySetIdToEntitySetMap = new md.EntitySet[referencedEntitySets.Count];
            m_entitySetToEntitySetIdMap = new Dictionary<md.EntitySet, int>();

            int id = 0;
            foreach (md.EntitySet e in referencedEntitySets)
            {
                if (m_entitySetToEntitySetIdMap.ContainsKey(e))
                {
                    continue;
                }
                m_entitySetIdToEntitySetMap[id] = e;
                m_entitySetToEntitySetIdMap[e] = id;
                id++;
            }
        }

        #endregion

        #region Type processing methods

        /// <summary>
        /// Process all types in the query
        /// </summary>
        /// <param name="referencedTypes">referenced types</param>
        private void ProcessTypes(HashSet<md.TypeUsage> referencedTypes)
        {
            // Build up auxilliary information for each type
            PopulateTypeInfoMap(referencedTypes);
            // Assign typeids to all nominal types
            AssignTypeIds();
            // Then "explode" all types
            ExplodeTypes();
        }

        #region Populating TypeInfo Map

        /// <summary>
        /// Build up auxilliary information for each referenced type in the query
        /// </summary>
        /// <param name="referencedTypes"></param>
        private void PopulateTypeInfoMap(HashSet<md.TypeUsage> referencedTypes)
        {
            foreach (md.TypeUsage t in referencedTypes)
            {
                CreateTypeInfoForType(t);
            }
            m_typeInfoMapPopulated = true;
        }

        /// <summary>
        /// Tries to lookup custom discriminator map for the given type (applies to EntitySets with
        /// TPH discrimination pattern)
        /// </summary>
        private bool TryGetDiscriminatorMap(md.EdmType type, out ExplicitDiscriminatorMap discriminatorMap)
        {
            discriminatorMap = null;

            // check that there are actually discriminator maps available
            if (null == m_discriminatorMaps)
            {
                return false;
            }

            // must be an entity type...
            if (type.BuiltInTypeKind != md.BuiltInTypeKind.EntityType)
            {
                return false;
            }

            // get root entity type (discriminator maps are mapped from the root)
            md.EntityTypeBase rootEntityType = GetRootType((md.EntityType)type);

            // find entity set
            md.EntitySet entitySet;
            if (!m_entityTypeToEntitySetMap.TryGetValue(rootEntityType, out entitySet))
            {
                return false;
            }

            // free floating entity constructors are stored with a null EntitySet
            if (entitySet == null)
            {
                return false;
            }

            // look for discriminator map
            return m_discriminatorMaps.TryGetValue(entitySet, out discriminatorMap);
        }

        /// <summary>
        /// Create a TypeInfo (if necessary) for the type, and add it to the TypeInfo map
        /// </summary>
        /// <param name="type">the type to process</param>
        private void CreateTypeInfoForType(md.TypeUsage type)
        {
            //
            // peel off all collection wrappers
            //
            while (TypeUtils.IsCollectionType(type))
            {
                type = TypeHelpers.GetEdmType<md.CollectionType>(type).TypeUsage;
            }

            // Only add "structured" types
            if (TypeUtils.IsStructuredType(type))
            {
                // check for discriminator map...
                ExplicitDiscriminatorMap discriminatorMap;
                TryGetDiscriminatorMap(type.EdmType, out discriminatorMap);

                CreateTypeInfoForStructuredType(type, discriminatorMap);
            }
        }

        /// <summary>
        /// Add a new entry to the map. If an entry already exists, then this function
        /// simply returns the existing entry. Otherwise a new entry is created. If 
        /// the type has a supertype, then we ensure that the supertype also exists in
        /// the map, and we add our info to the supertype's list of subtypes
        /// </summary>
        /// <param name="type">New type to add</param>
        /// <param name="discriminatorMap">type discriminator map</param>
        /// <returns>The TypeInfo for this type</returns>
        private TypeInfo CreateTypeInfoForStructuredType(md.TypeUsage type, ExplicitDiscriminatorMap discriminatorMap)
        {
            TypeInfo typeInfo;

            PlanCompiler.Assert(TypeUtils.IsStructuredType(type), "expected structured type. Found " + type);

            // Return existing entry, if one is available
            typeInfo = GetTypeInfo(type);
            if (typeInfo != null)
            {
                return typeInfo;
            }

            // Ensure that my supertype has been added to the map. 
            TypeInfo superTypeInfo = null;
            md.RefType refType;
            if (type.EdmType.BaseType != null)
            {

                superTypeInfo = CreateTypeInfoForStructuredType(md.TypeUsage.Create(type.EdmType.BaseType), discriminatorMap);
            }
            // 
            // Handle Ref types also in a similar fashion
            //
            else if (TypeHelpers.TryGetEdmType<md.RefType>(type, out refType))
            {
                md.EntityType entityType = refType.ElementType as md.EntityType;
                if (entityType != null && entityType.BaseType != null)
                {
                    md.TypeUsage baseRefType = TypeHelpers.CreateReferenceTypeUsage(entityType.BaseType as md.EntityType);
                    superTypeInfo = CreateTypeInfoForStructuredType(baseRefType, discriminatorMap);
                }
            }

            //
            // Add the types of my properties to the TypeInfo map
            // 
            foreach (md.EdmMember m in TypeHelpers.GetDeclaredStructuralMembers(type))
            {
                CreateTypeInfoForType(m.TypeUsage);
            }

            // 
            // Get the types of the rel properties also
            //
            {
                md.EntityTypeBase entityType;
                if (TypeHelpers.TryGetEdmType<md.EntityTypeBase>(type, out entityType))
                {
                    foreach (RelProperty p in m_relPropertyHelper.GetDeclaredOnlyRelProperties(entityType))
                    {
                        CreateTypeInfoForType(p.ToEnd.TypeUsage);
                    }
                }
            }


            // Now add myself to the map
            typeInfo = TypeInfo.Create(type, superTypeInfo, discriminatorMap);
            m_typeInfoMap.Add(type, typeInfo);

            return typeInfo;
        }

        #endregion

        #region Assigning TypeIds

        /// <summary>
        /// Assigns typeids to each type in the map. 
        /// We walk the map looking only for "root" types, and call the function
        /// above to process root types. All other types will be handled in that
        /// function
        /// </summary>
        private void AssignTypeIds()
        {
            int typeNum = 0;

            foreach (KeyValuePair<md.TypeUsage, TypeInfo> kv in m_typeInfoMap)
            {
                // See if there is a declared discriminator value for this column
                if (kv.Value.RootType.DiscriminatorMap != null)
                {
                    // find discriminator value for type
                    var entityType = (md.EntityType)kv.Key.EdmType;
                    kv.Value.TypeId = kv.Value.RootType.DiscriminatorMap.GetTypeId(entityType);
                }

                // Only handle root types. The call below will ensure that all the 
                // subtypes are appropriately tagged
                else if (kv.Value.IsRootType && (md.TypeSemantics.IsEntityType(kv.Key) || md.TypeSemantics.IsComplexType(kv.Key)))
                {
                    AssignRootTypeId(kv.Value, String.Format(CultureInfo.InvariantCulture, "{0}X", typeNum));
                    typeNum++;
                }
            }
        }

        /// <summary>
        /// Assign a typeid to a root type
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <param name="typeId"></param>
        private void AssignRootTypeId(TypeInfo typeInfo, string typeId)
        {
            typeInfo.TypeId = typeId;
            AssignTypeIdsToSubTypes(typeInfo);
        }

        /// <summary>
        /// Assigns typeids to each subtype of the current type. 
        /// Assertion: the current type has already had a typeid assigned to it.
        /// </summary>
        /// <param name="typeInfo">The current type</param>
        private void AssignTypeIdsToSubTypes(TypeInfo typeInfo)
        {
            // Now walk through all my subtypes, and assign their typeids
            int mySubTypeNum = 0;
            foreach (TypeInfo subtype in typeInfo.ImmediateSubTypes)
            {
                AssignTypeId(subtype, mySubTypeNum);
                mySubTypeNum++;
            }
        }

        /// <summary>
        /// Assign a typeid to a non-root type.
        /// Assigns typeids to a non-root type based on a dewey encoding scheme. 
        /// The typeid will be the typeId of the supertype suffixed by a 
        /// local identifier for the type.  
        /// </summary>
        /// <param name="typeInfo">the non-root type</param>
        /// <param name="subtypeNum">position in the subtype list</param>
        private void AssignTypeId(TypeInfo typeInfo, int subtypeNum)
        {
            typeInfo.TypeId = String.Format(CultureInfo.InvariantCulture, "{0}{1}X", typeInfo.SuperType.TypeId, subtypeNum);
            AssignTypeIdsToSubTypes(typeInfo);
        }

        #endregion

        #region Flattening/Exploding types
        /// <summary>
        /// A type needs a type-id property if it is an entity type or a complex tpe that
        /// has subtypes.
        /// Coming soon: relax the "need subtype" requirement (ie) any entity/complex type will
        /// have a typeid
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        private bool NeedsTypeIdProperty(TypeInfo typeInfo)
        {
            return typeInfo.ImmediateSubTypes.Count > 0 && !md.TypeSemantics.IsReferenceType(typeInfo.Type);
        }

        /// <summary>
        /// A type needs a null-sentinel property if it is an row type that was projected
        /// at the top level of the query; we capture that information in the preprocessor
        /// and pass it in here.
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        private bool NeedsNullSentinelProperty(TypeInfo typeInfo)
        {
            return m_typesNeedingNullSentinel.Contains(typeInfo.Type.EdmType.Identity);
        }

        /// <summary>
        /// The type needs an entitysetidproperty, if it is either an entity type
        /// or a reference type, AND we cannot determine that there is only entityset
        /// in the query that could be producing instances of this entity
        /// </summary>
        /// <param name="typeInfo"></param>
        /// <returns></returns>
        private bool NeedsEntitySetIdProperty(TypeInfo typeInfo)
        {
            md.EntityType entityType;
            md.RefType refType = typeInfo.Type.EdmType as md.RefType;
            if (refType != null)
            {
                entityType = refType.ElementType as md.EntityType;
            }
            else
            {
                entityType = typeInfo.Type.EdmType as md.EntityType;
            }
            bool result = ((entityType != null) && (GetEntitySet(entityType) == null));
            return result;
        }

        /// <summary>
        /// "Explode" each type in the dictionary. (ie) for each type, get a flattened
        /// list of all its members (including special cases for the typeid)
        /// </summary>
        private void ExplodeTypes()
        {
            // Walk through the list of types, and only process the supertypes, since
            // The ExplodeType method will ensure that all the subtypes are appropriately
            // tagged
            foreach (KeyValuePair<md.TypeUsage, TypeInfo> kv in m_typeInfoMap)
            {
                if (kv.Value.IsRootType)
                {
                    ExplodeType(kv.Value);
                }
            }
        }

        /// <summary>
        /// "Explode" a type.  (ie) produce a flat record type with one property for each
        /// scalar property (top-level or nested) of the original type.
        /// Really deals with structured types, but also 
        /// peels off collection wrappers
        /// </summary>
        /// <param name="type">the type to explode</param>
        /// <returns>the typeinfo for this type (with the explosion)</returns>
        private TypeInfo ExplodeType(md.TypeUsage type)
        {
            if (TypeUtils.IsStructuredType(type))
            {
                TypeInfo typeInfo = GetTypeInfo(type);
                ExplodeType(typeInfo);
                return typeInfo;
            }

            if (TypeUtils.IsCollectionType(type))
            {
                md.TypeUsage elementType = TypeHelpers.GetEdmType<md.CollectionType>(type).TypeUsage;
                ExplodeType(elementType);
                return null;
            }
            return null;
        }

        /// <summary>
        /// Type Explosion - simply delegates to the root type
        /// </summary>
        /// <param name="typeInfo">type info</param>
        private void ExplodeType(TypeInfo typeInfo)
        {
            ExplodeRootStructuredType(typeInfo.RootType);
        }

        /// <summary>
        /// "Explode" a root type. (ie) add each member of the type to a flat list of 
        /// members for the supertype. 
        /// 
        /// Type explosion works in a DFS style model. We first walk through the
        /// list of properties for the current type, and "flatten" out the properties
        /// that are themselves "structured". We then target each subtype (recursively)
        /// and perform the same kind of processing. 
        /// 
        /// Consider a very simple case:
        /// 
        ///   Q = (z1 int, z2 date)
        ///   Q2: Q = (z3 string)  -- Q2 is a subtype of Q
        ///   T = (a int, b Q, c date)
        ///   S: T = (d int)  -- read as S is a subtype of T
        /// 
        /// The result of flattening T (and S) will be
        /// 
        ///   (a int, b.z1 int, b.z2 date, b.z3 string, c date, d int)
        /// </summary>
        /// <param name="rootType">the root type to explode</param>        
        private void ExplodeRootStructuredType(RootTypeInfo rootType)
        {
            // Already done??
            if (rootType.FlattenedType != null)
            {
                return;
            }

            //
            // Special handling for root types. Add any special
            // properties that are needed - TypeId, EntitySetId, etc
            //
            if (NeedsTypeIdProperty(rootType))
            {
                rootType.AddPropertyRef(TypeIdPropertyRef.Instance);
                // check for discriminator map; if one exists, use custom discriminator member; otherwise, use default
                if (null != rootType.DiscriminatorMap)
                {
                    rootType.TypeIdKind = TypeIdKind.UserSpecified;
                    rootType.TypeIdType = md.Helper.GetModelTypeUsage(rootType.DiscriminatorMap.DiscriminatorProperty);
                }
                else
                {
                    rootType.TypeIdKind = TypeIdKind.Generated;
                    rootType.TypeIdType = m_stringType;
                }
            }
            if (NeedsEntitySetIdProperty(rootType))
            {
                rootType.AddPropertyRef(EntitySetIdPropertyRef.Instance);
            }
            if (NeedsNullSentinelProperty(rootType))
            {
                rootType.AddPropertyRef(NullSentinelPropertyRef.Instance);
            }

            //
            // Then add members from each type in the hierarchy (including 
            // the root type)
            //
            ExplodeRootStructuredTypeHelper(rootType);

            //
            // For entity types, add all the rel-properties now. Note that rel-properties
            // are added after the regular properties of all subtypes
            //
            if (md.TypeSemantics.IsEntityType(rootType.Type))
            {
                AddRelProperties(rootType);
            }

            //
            // We've now gotten all the relevant properties
            // Now let's create a new record type
            //
            CreateFlattenedRecordType(rootType);
        }

        /// <summary>
        /// Helper for ExplodeType. 
        /// Walks through each member introduced by the current type, and
        /// adds it onto the "flat" record type being constructed. 
        /// We then walk through all subtypes of this type, and process those as
        /// well.
        /// Special handling for Refs: we only add the keys; there is no
        /// need to handle subtypes (since they won't be introducing anything
        /// different)
        /// </summary>
        /// <param name="typeInfo">type in the type hierarchy</param>
        private void ExplodeRootStructuredTypeHelper(TypeInfo typeInfo)
        {
            RootTypeInfo rootType = typeInfo.RootType;

            // Identify the members of this type. For Refs, use the key properties
            // of the target entity type. For all other types, simply use the type
            // members
            IEnumerable typeMembers = null;
            md.RefType refType;
            if (TypeHelpers.TryGetEdmType<md.RefType>(typeInfo.Type, out refType))
            {
                //
                // If this is not the root type, then don't bother adding the keys.
                // the root type has already done this
                //
                if (!typeInfo.IsRootType)
                {
                    return;
                }
                typeMembers = refType.ElementType.KeyMembers;
            }
            else
            {
                typeMembers = TypeHelpers.GetDeclaredStructuralMembers(typeInfo.Type);
            }

            // Walk through all the members of the type
            foreach (md.EdmMember p in typeMembers)
            {
                TypeInfo propertyType = ExplodeType(p.TypeUsage);

                //
                // If we can't find a TypeInfo for this property's type, then it must
                // be a scalar type or a collection type. In either case, we'll 
                // build up a SimplePropertyRef
                //
                if (propertyType == null)
                {
                    rootType.AddPropertyRef(new SimplePropertyRef(p));
                }
                else
                {
                    //
                    // We're dealing with a structured type again. Create NestedPropertyRef
                    // for each property of the nested type
                    //
                    foreach (PropertyRef nestedPropInfo in propertyType.PropertyRefList)
                    {
                        rootType.AddPropertyRef(nestedPropInfo.CreateNestedPropertyRef(p));
                    }
                }
            }

            // 
            // Process all subtypes now
            //
            foreach (TypeInfo subTypeInfo in typeInfo.ImmediateSubTypes)
            {
                ExplodeRootStructuredTypeHelper(subTypeInfo);
            }
        }

        /// <summary>
        /// Add the list of rel-properties for this type
        /// </summary>
        /// <param name="typeInfo">the type to process</param>
        private void AddRelProperties(TypeInfo typeInfo)
        {

            md.EntityTypeBase entityType = (md.EntityTypeBase)typeInfo.Type.EdmType;

            //
            // Walk through each rel-property defined for this specific type, 
            // and add a corresponding property-ref
            //
            foreach (RelProperty p in m_relPropertyHelper.GetDeclaredOnlyRelProperties(entityType))
            {
                md.EdmType refType = p.ToEnd.TypeUsage.EdmType;
                TypeInfo refTypeInfo = GetTypeInfo(p.ToEnd.TypeUsage);

                //
                // We're dealing with a structured type again - flatten this out
                // as well
                //
                ExplodeType(refTypeInfo);

                foreach (PropertyRef nestedPropInfo in refTypeInfo.PropertyRefList)
                {
                    typeInfo.RootType.AddPropertyRef(nestedPropInfo.CreateNestedPropertyRef(p));
                }
            }

            // 
            // Process all subtypes now
            //
            foreach (TypeInfo subTypeInfo in typeInfo.ImmediateSubTypes)
            {
                AddRelProperties(subTypeInfo);
            }
        }

        /// <summary>
        /// Create the flattened record type for the type.
        /// Walk through the list of property refs, and creates a new field
        /// (which we name as "F1", "F2" etc.) with the required property type.
        /// 
        /// We then produce a mapping from the original property (propertyRef really)
        /// to the new property for use in later modules.
        /// 
        /// Finally, we identify the TypeId and EntitySetId property if they exist
        /// </summary>
        /// <param name="type"></param>
        private void CreateFlattenedRecordType(RootTypeInfo type)
        {
            //
            // If this type corresponds to an entity type, and that entity type
            // has no subtypes, and that that entity type has no complex properties
            // then simply use the name from that property
            //
            bool usePropertyNamesFromUnderlyingType;
            if (md.TypeSemantics.IsEntityType(type.Type) &&
                type.ImmediateSubTypes.Count == 0)
            {
                usePropertyNamesFromUnderlyingType = true;
            }
            else
            {
                usePropertyNamesFromUnderlyingType = false;
            }


            // Build the record type
            List<KeyValuePair<string, md.TypeUsage>> fieldList = new List<KeyValuePair<string, md.TypeUsage>>();
            HashSet<string> fieldNames = new HashSet<string>();
            int nextFieldId = 0;
            foreach (PropertyRef p in type.PropertyRefList)
            {
                string fieldName = null;
                if (usePropertyNamesFromUnderlyingType)
                {
                    SimplePropertyRef simpleP = p as SimplePropertyRef;
                    if (simpleP != null)
                    {
                        fieldName = simpleP.Property.Name;
                    }
                }
               
                if (fieldName == null)
                {
                    fieldName = "F" + nextFieldId.ToString(CultureInfo.InvariantCulture);
                    nextFieldId++;
                }
                
                // Deal with collisions
                while (fieldNames.Contains(fieldName))
                {
                    fieldName = "F" + nextFieldId.ToString(CultureInfo.InvariantCulture);
                    nextFieldId++;
                }

                md.TypeUsage propertyType = GetPropertyType(type, p);
                fieldList.Add(new KeyValuePair<string, md.TypeUsage>(fieldName, propertyType));
                fieldNames.Add(fieldName);
            }

            type.FlattenedType = TypeHelpers.CreateRowType(fieldList);

            // Now build up the property map
            IEnumerator<PropertyRef> origProps = type.PropertyRefList.GetEnumerator();
            foreach (md.EdmProperty p in type.FlattenedType.Properties)
            {
                if (!origProps.MoveNext())
                {
                    PlanCompiler.Assert(false, "property refs count and flattened type member count mismatch?");
                }
                type.AddPropertyMapping(origProps.Current, p);
            }
        }

        /// <summary>
        /// Get the "new" type corresponding to the input type. For structured types,
        /// we return the flattened record type.
        /// For collections of structured type, we return a new collection type of the corresponding flattened
        /// type.
        /// For enum types we return the underlying type of the enum type.
        /// For strong spatial types we return the union type that includes the strong spatial type.
        /// For everything else, we return the input type
        /// </summary>
        /// <param name="type">the original type</param>
        /// <returns>the new type (if any)</returns>
        private md.TypeUsage GetNewType(md.TypeUsage type)
        {
            if (TypeUtils.IsStructuredType(type))
            {
                TypeInfo typeInfo = GetTypeInfo(type);
                return typeInfo.FlattenedTypeUsage;
            }
            md.TypeUsage elementType;
            if (TypeHelpers.TryGetCollectionElementType(type, out elementType))
            {
                md.TypeUsage newElementType = GetNewType(elementType);
                if (newElementType.EdmEquals(elementType))
                {
                    return type;
                }
                else
                {
                    return TypeHelpers.CreateCollectionTypeUsage(newElementType);
                }
            }

            if (TypeUtils.IsEnumerationType(type))
            {
                return TypeHelpers.CreateEnumUnderlyingTypeUsage(type);
            }
            
            if (md.TypeSemantics.IsStrongSpatialType(type))
            {
                return TypeHelpers.CreateSpatialUnionTypeUsage(type);
            }

            // simple scalar
            return type;
        }

        /// <summary>
        /// Get the datatype for a propertyRef. The only concrete classes that we 
        /// handle are TypeIdPropertyRef, and BasicPropertyRef. 
        /// AllPropertyRef is illegal here.
        /// For BasicPropertyRef, we simply pick up the type from the corresponding
        /// property. For TypeIdPropertyRef, we use "string" as the default type
        /// or the discriminator property type where one is available.
        /// </summary>
        /// <param name="typeInfo">typeinfo of the current type</param>
        /// <param name="p">current property ref</param>
        /// <returns>the datatype of the property</returns>
        private md.TypeUsage GetPropertyType(RootTypeInfo typeInfo, PropertyRef p)
        {
            md.TypeUsage result = null;

            PropertyRef innerProperty = null;
            // Get the "leaf" property first
            while (p is NestedPropertyRef)
            {
                NestedPropertyRef npr = (NestedPropertyRef)p;
                p = npr.OuterProperty;
                innerProperty = npr.InnerProperty;
            }

            if (p is TypeIdPropertyRef)
            {
                //
                // Get to the innermost type that specifies this typeid (the entity type),
                // get the datatype for the typeid column from that type
                //
                if (innerProperty != null && innerProperty is SimplePropertyRef)
                {
                    md.TypeUsage innerType = ((SimplePropertyRef)innerProperty).Property.TypeUsage;
                    TypeInfo innerTypeInfo = GetTypeInfo(innerType);
                    result = innerTypeInfo.RootType.TypeIdType;
                }
                else
                {
                    result = typeInfo.TypeIdType;
                }
            }
            else if (p is EntitySetIdPropertyRef || p is NullSentinelPropertyRef)
            {
                result = m_intType;
            }
            else if (p is RelPropertyRef)
            {
                result = (p as RelPropertyRef).Property.ToEnd.TypeUsage;
            }
            else
            {
                SimplePropertyRef simpleP = p as SimplePropertyRef;
                if (simpleP != null)
                {
                    result = md.Helper.GetModelTypeUsage(simpleP.Property);
                }
            }

            result = GetNewType(result);
            PlanCompiler.Assert(null != result, "unrecognized property type?");
            return result;
        }

        #endregion

        #endregion

        #region utils
        /// <summary>
        /// Get the root entity type for a type
        /// </summary>
        /// <param name="type">entity type</param>
        /// <returns></returns>
        private static md.EntityTypeBase GetRootType(md.EntityTypeBase type)
        {
            while (type.BaseType != null)
            {
                type = (md.EntityTypeBase)type.BaseType;
            }
            return type;
        }
        #endregion
        #endregion
    }
}

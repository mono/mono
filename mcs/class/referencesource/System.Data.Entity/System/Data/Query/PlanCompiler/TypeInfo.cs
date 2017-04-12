//---------------------------------------------------------------------
// <copyright file="TypeInfo.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

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

using System.Data.Common;
using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{

    /// <summary>
    /// The kind of type-id in use
    /// </summary>
    internal enum TypeIdKind
    {
        UserSpecified = 0,
        Generated
    }

    /// <summary>
    /// The TypeInfo class encapsulates various pieces of information about a type.
    /// The most important of these include the "flattened" record type - corresponding
    /// to the type, and the TypeId field for nominal types
    /// </summary>
    internal class TypeInfo
    {

        #region private state
        private readonly md.TypeUsage m_type;    // the type
        private object m_typeId;            // the type's Id, assigned by the StructuredTypeInfo processing.
        private List<TypeInfo> m_immediateSubTypes;  // the list of children below this type in it's type hierarchy.
        private readonly TypeInfo m_superType;       // the type one level up in this types type hierarchy -- the base type.
        private readonly RootTypeInfo m_rootType;    // the top-most type in this types type hierarchy
        #endregion

        #region Constructors and factory methods

        /// <summary>
        /// Creates type information for a type
        /// </summary>
        /// <param name="type"></param>
        /// <param name="superTypeInfo"></param>
        /// <returns></returns>
        internal static TypeInfo Create(md.TypeUsage type, TypeInfo superTypeInfo, ExplicitDiscriminatorMap discriminatorMap)
        {
            TypeInfo result;
            if (superTypeInfo == null)
            {
                result = new RootTypeInfo(type, discriminatorMap);
            }
            else
            {
                result = new TypeInfo(type, superTypeInfo);
            }
            return result;
        }

        protected TypeInfo(md.TypeUsage type, TypeInfo superType)
        {
            m_type = type;
            m_immediateSubTypes = new List<TypeInfo>();
            m_superType = superType;
            if (superType != null)
            {
                // Add myself to my supertype's list of subtypes
                superType.m_immediateSubTypes.Add(this);
                // my supertype's root type is mine as well
                m_rootType = superType.RootType;
            }
        }

        #endregion

        #region "public" properties for all types

        /// <summary>
        /// Is this the root type?
        /// True for entity, complex types and ref types, if this is the root of the
        /// hierarchy. 
        /// Always true for Record types
        /// </summary>
        internal bool IsRootType
        {
            get
            {
                return m_rootType == null;
            }
        }

        /// <summary>
        /// the types that derive from this type
        /// </summary>
        internal List<TypeInfo> ImmediateSubTypes
        {
            get
            {
                return m_immediateSubTypes;
            }
        }

        /// <summary>
        /// the immediate parent type of this type.
        /// </summary>
        internal TypeInfo SuperType
        {
            get
            {
                return m_superType;
            }
        }

        /// <summary>
        /// the top most type in the hierarchy.
        /// </summary>
        internal RootTypeInfo RootType
        {
            get
            {
                return m_rootType ?? (RootTypeInfo)this;
            }
        }
        /// <summary>
        /// The metadata type
        /// </summary>
        internal md.TypeUsage Type
        {
            get
            {
                return m_type;
            }
        }

        /// <summary>
        /// The typeid value for this type - only applies to nominal types
        /// </summary>
        internal object TypeId
        {
            get
            {
                return m_typeId;
            }
            set
            {
                m_typeId = value;
            }
        }

        #endregion

        #region "public" properties for root types

        // These properties are actually stored on the RootType but we let
        // let folks use the TypeInfo class as the proxy to get to them.
        // Essentially, they are mostly sugar to simplify coding.
        //
        // For example:
        //
        // You could either write:
        //
        //      typeinfo.RootType.FlattenedType
        //
        // or you can write:
        //
        //      typeinfo.FlattenedType
        //

        /// <summary>
        /// Flattened record version of the type
        /// </summary>
        internal virtual md.RowType FlattenedType
        {
            get
            {
                return RootType.FlattenedType;
            }
        }

        /// <summary>
        /// TypeUsage that encloses the Flattened record version of the type
        /// </summary>
        internal virtual md.TypeUsage FlattenedTypeUsage
        {
            get
            {
                return RootType.FlattenedTypeUsage;
            }
        }

        /// <summary>
        /// Get the property describing the entityset (if any)
        /// </summary>
        internal virtual md.EdmProperty EntitySetIdProperty
        {
            get
            {
                return RootType.EntitySetIdProperty;
            }
        }

        /// <summary>
        /// Does this type have an entitySetId property
        /// </summary>
        internal bool HasEntitySetIdProperty
        {
            get
            {
                return RootType.EntitySetIdProperty != null;
            }
        }

        /// <summary>
        /// Get the nullSentinel property (if any)
        /// </summary>
        internal virtual md.EdmProperty NullSentinelProperty
        {
            get
            {
                return RootType.NullSentinelProperty;
            }
        }

        /// <summary>
        /// Does this type have a nullSentinel property?
        /// </summary>
        internal bool HasNullSentinelProperty
        {
            get
            {
                return RootType.NullSentinelProperty != null;
            }
        }

        /// <summary>
        /// The typeid property in the flattened type - applies only to nominal types
        /// this will be used as the type discriminator column.
        /// </summary>
        internal virtual md.EdmProperty TypeIdProperty
        {
            get
            {
                return RootType.TypeIdProperty;
            }
        }

        /// <summary>
        /// Does this type need a typeid property? (Needed for complex types and entity types in general)
        /// </summary>
        internal bool HasTypeIdProperty
        {
            get
            {
                return RootType.TypeIdProperty != null;
            }
        }

        /// <summary>
        /// All the properties of this type.
        /// </summary>
        internal virtual IEnumerable<PropertyRef> PropertyRefList
        {
            get
            {
                return RootType.PropertyRefList;
            }
        }

        /// <summary>
        /// Get the new property for the supplied propertyRef
        /// </summary>
        /// <param name="propertyRef">property reference (on the old type)</param>
        /// <returns></returns>
        internal md.EdmProperty GetNewProperty(PropertyRef propertyRef)
        {
            md.EdmProperty property;
            bool result = TryGetNewProperty(propertyRef, true, out property);
            Debug.Assert(result, "Should have thrown if the property was not found");
            return property;
        }

        /// <summary>
        /// Try get the new property for the supplied propertyRef
        /// </summary>
        /// <param name="propertyRef">property reference (on the old type)</param>
        /// <param name="throwIfMissing">throw if the property is not found</param>
        /// <param name="newProperty">the corresponding property on the new type</param>
        /// <returns></returns>
        internal bool TryGetNewProperty(PropertyRef propertyRef, bool throwIfMissing, out md.EdmProperty newProperty)
        {
            return this.RootType.TryGetNewProperty(propertyRef, throwIfMissing, out newProperty);
        }

        /// <summary>
        /// Get the list of "key" properties (in the flattened type)
        /// </summary>
        /// <returns>the key property equivalents in the flattened type</returns>
        internal IEnumerable<PropertyRef> GetKeyPropertyRefs()
        {
            md.EntityTypeBase entityType = null;
            md.RefType refType = null;
            if (TypeHelpers.TryGetEdmType<md.RefType>(m_type, out refType))
            {
                entityType = refType.ElementType;
            }
            else
            {
                entityType = TypeHelpers.GetEdmType<md.EntityTypeBase>(m_type);
            }

            // Walk through the list of keys of the entity type, and find their analogs in the
            // "flattened" type
            foreach (md.EdmMember p in entityType.KeyMembers)
            {
                // Eventually this could be RelationshipEndMember, but currently only properties are suppported as key members
                PlanCompiler.Assert(p is md.EdmProperty, "Non-EdmProperty key members are not supported");
                SimplePropertyRef spr = new SimplePropertyRef(p);
                yield return spr;
            }
        }

        /// <summary>
        /// Get the list of "identity" properties in the flattened type.
        /// The identity properties include the entitysetid property, followed by the
        /// key properties
        /// </summary>
        /// <returns>List of identity properties</returns>
        internal IEnumerable<PropertyRef> GetIdentityPropertyRefs()
        {
            if (this.HasEntitySetIdProperty)
            {
                yield return EntitySetIdPropertyRef.Instance;
            }
            foreach (PropertyRef p in this.GetKeyPropertyRefs())
            {
                yield return p;
            }
        }

        /// <summary>
        /// Get the list of all properties in the flattened type
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<PropertyRef> GetAllPropertyRefs()
        {
            foreach (PropertyRef p in this.PropertyRefList)
            {
                yield return p;
            }
        }

        /// <summary>
        /// Get the list of all properties in the flattened type
        /// </summary>
        /// <returns></returns>
        internal IEnumerable<md.EdmProperty> GetAllProperties()
        {
            foreach (md.EdmProperty m in this.FlattenedType.Properties)
            {
                yield return m;
            }
        }

        /// <summary>
        /// Gets all types in the hierarchy rooted at this.
        /// </summary>
        internal List<TypeInfo> GetTypeHierarchy()
        {
            List<TypeInfo> result = new List<TypeInfo>();
            GetTypeHierarchy(result);
            return result;
        }

        /// <summary>
        /// Adds all types in the hierarchy to the given list.
        /// </summary>
        private void GetTypeHierarchy(List<TypeInfo> result)
        {
            result.Add(this);
            foreach (TypeInfo subType in this.ImmediateSubTypes)
            {
                subType.GetTypeHierarchy(result);
            }
        }
        #endregion
    }

    /// <summary>
    /// A subclass of the TypeInfo class above that only represents information
    /// about "root" types
    /// </summary>
    internal class RootTypeInfo : TypeInfo
    {

        #region private state
        private readonly List<PropertyRef> m_propertyRefList;
        private readonly Dictionary<PropertyRef, md.EdmProperty> m_propertyMap;
        private md.EdmProperty m_nullSentinelProperty;
        private md.EdmProperty m_typeIdProperty;
        private TypeIdKind m_typeIdKind;
        private md.TypeUsage m_typeIdType;
        private readonly ExplicitDiscriminatorMap m_discriminatorMap;
        private md.EdmProperty m_entitySetIdProperty;
        private md.RowType m_flattenedType;
        private md.TypeUsage m_flattenedTypeUsage;
        #endregion

        #region Constructor

        /// <summary>
        /// Constructor for a root type
        /// </summary>
        /// <param name="type"></param>
        internal RootTypeInfo(md.TypeUsage type, ExplicitDiscriminatorMap discriminatorMap)
            : base(type, null)
        {
            PlanCompiler.Assert(type.EdmType.BaseType == null, "only root types allowed here");

            m_propertyMap = new Dictionary<PropertyRef, md.EdmProperty>();
            m_propertyRefList = new List<PropertyRef>();
            m_discriminatorMap = discriminatorMap;
            m_typeIdKind = TypeIdKind.Generated;
        }

        #endregion

        #region "public" surface area

        /// <summary>
        /// Kind of the typeid column (if any)
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal TypeIdKind TypeIdKind
        {
            get { return m_typeIdKind; }
            set { m_typeIdKind = value; }
        }

        /// <summary>
        /// Datatype of the typeid column (if any)
        /// </summary>
        internal md.TypeUsage TypeIdType
        {
            get { return m_typeIdType; }
            set { m_typeIdType = value; }
        }
        /// <summary>
        /// Add a mapping from the propertyRef (of the old type) to the 
        /// corresponding property in the new type.
        /// 
        /// NOTE: Only to be used by StructuredTypeInfo
        /// </summary>
        /// <param name="propertyRef"></param>
        /// <param name="newProperty"></param>
        internal void AddPropertyMapping(PropertyRef propertyRef, md.EdmProperty newProperty)
        {
            m_propertyMap[propertyRef] = newProperty;
            if (propertyRef is TypeIdPropertyRef)
            {
                m_typeIdProperty = newProperty;
            }
            else if (propertyRef is EntitySetIdPropertyRef)
            {
                m_entitySetIdProperty = newProperty;
            }
            else if (propertyRef is NullSentinelPropertyRef)
            {
                m_nullSentinelProperty = newProperty;
            }
        }

        /// <summary>
        /// Adds a new property reference to the list of desired properties
        /// NOTE: Only to be used by StructuredTypeInfo
        /// </summary>
        /// <param name="propertyRef"></param>
        internal void AddPropertyRef(PropertyRef propertyRef)
        {
            m_propertyRefList.Add(propertyRef);
        }

        /// <summary>
        /// Flattened record version of the type
        /// </summary>
        internal new md.RowType FlattenedType
        {
            get
            {
                return m_flattenedType;
            }
            set
            {
                m_flattenedType = value;
                m_flattenedTypeUsage = md.TypeUsage.Create(value);
            }
        }

        /// <summary>
        /// TypeUsage that encloses the Flattened record version of the type
        /// </summary>
        internal new md.TypeUsage FlattenedTypeUsage
        {
            get
            {
                return m_flattenedTypeUsage;
            }
        }

        /// <summary>
        /// Gets map information for types mapped using simple discriminator pattern.
        /// </summary>
        internal ExplicitDiscriminatorMap DiscriminatorMap
        {
            get
            {
                return m_discriminatorMap;
            }
        }

        /// <summary>
        /// Get the property describing the entityset (if any)
        /// </summary>
        internal new md.EdmProperty EntitySetIdProperty
        {
            get
            {
                return m_entitySetIdProperty;
            }
        }

        internal new md.EdmProperty NullSentinelProperty
        {
            get
            {
                return m_nullSentinelProperty;
            }
        }

        /// <summary>
        /// Get the list of property refs for this type
        /// </summary>
        internal new IEnumerable<PropertyRef> PropertyRefList
        {
            get
            {
                return m_propertyRefList;
            }
        }

        /// <summary>
        /// Determines the offset for structured types in Flattened type. For instance, if the original type is of the form:
        /// 
        ///     { int X, ComplexType Y }
        ///     
        /// and the flattened type is of the form:
        /// 
        ///     { int X, Y_ComplexType_Prop1, Y_ComplexType_Prop2 }
        ///     
        /// GetNestedStructureOffset(Y) returns 1
        /// </summary>
        /// <param name="property">Complex property.</param>
        /// <returns>Offset.</returns>
        internal int GetNestedStructureOffset(PropertyRef property)
        {
            // m_propertyRefList contains every element of the flattened type
            for (int i = 0; i < m_propertyRefList.Count; i++)
            {
                NestedPropertyRef nestedPropertyRef = m_propertyRefList[i] as NestedPropertyRef;

                // match offset of the first element of the complex type property
                if (null != nestedPropertyRef && nestedPropertyRef.InnerProperty.Equals(property))
                {
                    return i;
                }
            }
            PlanCompiler.Assert(false, "no complex structure " + property + " found in TypeInfo");
            // return something so that the compiler doesn't complain
            return default(int);
        }

        /// <summary>
        /// Try get the new property for the supplied propertyRef
        /// </summary>
        /// <param name="propertyRef">property reference (on the old type)</param>
        /// <param name="throwIfMissing">throw if the property is not found</param>
        /// <param name="property">the corresponding property on the new type</param>
        /// <returns></returns>
        internal new bool TryGetNewProperty(PropertyRef propertyRef, bool throwIfMissing, out md.EdmProperty property)
        {
            bool result = m_propertyMap.TryGetValue(propertyRef, out property);
            if (throwIfMissing && !result)
            {
                {
                    PlanCompiler.Assert(false, "Unable to find property " + propertyRef.ToString() + " in type " + this.Type.EdmType.Identity);
                }
            }
            return result;
        }

        /// <summary>
        /// The typeid property in the flattened type - applies only to nominal types
        /// this will be used as the type discriminator column.
        /// </summary>
        internal new md.EdmProperty TypeIdProperty
        {
            get
            {
                return m_typeIdProperty;
            }
        }

        #endregion
    }
}

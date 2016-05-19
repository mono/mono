//---------------------------------------------------------------------
// <copyright file="TypeSemantics.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System.Collections.Generic;
using System.Data.Common;
using System.Linq;
using System.Diagnostics;
using objectModel = System.Collections.ObjectModel;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Provides type semantics service, type operations and type predicates for the EDM type system.
    /// </summary>
    /// <remarks>
    /// For detailed functional specification, see "The EDP Type System.docx" and "edm.spec.doc".
    /// Notes:
    /// 1) The notion of 'type' for the sake of type operation semantics is based on TypeUsage, i.e., EdmType *plus* facets.
    /// 
    /// 2) EDM built-in primitive types are defined by the EDM Provider Manifest.
    /// 
    /// 3) SubType and Promotable are similar notions however subtyping is stricter than promotability. Subtyping is used for mapping 
    ///    validation while Promotability is used in query, update expression static type validation.
    /// </remarks>
    internal static class TypeSemantics
    {
        #region Fields
        //
        // cache commom super type closure
        //
        static private objectModel.ReadOnlyCollection<PrimitiveType>[,] _commonTypeClosure;
        #endregion

        //
        // 'Public' Interface
        //
        #region 'Public' Interface

        /// <summary>
        /// Determines whether two types are exactly equal.
        /// For row types, this INCLUDES property names as well as property types.
        /// </summary>
        /// <param name="type1">The first type to compare.</param>
        /// <param name="type2">The second type to compare.</param>
        /// <returns>If the two types are structurally equal, <c>true</c>; otherwise <c>false</c>.</returns>
        internal static bool IsEqual(TypeUsage type1, TypeUsage type2)
        {
            return CompareTypes(type1, type2, false /*equivalenceOnly*/);
        }

        /// <summary>
        /// Determines if the two types are structurally equivalent.
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <remarks>
        /// Equivalence for nomimal types is based on lexical identity and structural equivalence for structural types.
        /// Structural equivalence for row types is based only on equivalence of property types, property names are ignored.
        /// </remarks>
        /// <returns>true if equivalent, false otherwise</returns>
        internal static bool IsStructurallyEqual(TypeUsage fromType, TypeUsage toType)
        {
            return CompareTypes(fromType, toType, true /*equivalenceOnly*/);
        }

        /// <summary>
        /// determines if two types are equivalent or if fromType is promotable to toType 
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns>true if fromType equivalent or promotable to toType, false otherwise</returns>
        internal static bool IsStructurallyEqualOrPromotableTo(TypeUsage fromType, TypeUsage toType)
        {
            return IsStructurallyEqual(fromType, toType) ||
                   IsPromotableTo(fromType, toType);
        }

        /// <summary>
        /// determines if two types are equivalent or if fromType is promotable to toType 
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns>true if fromType equivalent or promotable to toType, false otherwise</returns>
        internal static bool IsStructurallyEqualOrPromotableTo(EdmType fromType, EdmType toType)
        {
            return IsStructurallyEqualOrPromotableTo(TypeUsage.Create(fromType), TypeUsage.Create(toType));
        }

        /// <summary>
        /// determines if subType is equal to or a sub-type of superType.
        /// </summary>
        /// <param name="subType"></param>
        /// <param name="superType"></param>
        /// <returns>true if subType is equal to or a sub-type of superType, false otherwise</returns>
        internal static bool IsSubTypeOf(TypeUsage subType, TypeUsage superType)
        {
            Debug.Assert(subType != null, "subType must not be NULL");
            Debug.Assert(superType != null, "superType must not be NULL");

            if (subType.EdmEquals(superType))
            {
                return true;
            }

            if (Helper.IsPrimitiveType(subType.EdmType) && Helper.IsPrimitiveType(superType.EdmType))
            {
                return IsPrimitiveTypeSubTypeOf(subType, superType);
            }

            return subType.IsSubtypeOf(superType);
        }

        /// <summary>
        /// determines if subType EdmType is a sub-type of superType EdmType.
        /// </summary>
        /// <param name="subEdmType"></param>
        /// <param name="superEdmType"></param>
        /// <returns>true if subType is a sub-type of superType, false otherwise</returns>
        internal static bool IsSubTypeOf(EdmType subEdmType, EdmType superEdmType)
        {
            return subEdmType.IsSubtypeOf(superEdmType);
        }

        /// <summary>
        /// Determines if fromType is promotable to toType.
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns>true if fromType is promotable to toType, false otherwise</returns>
        internal static bool IsPromotableTo(TypeUsage fromType, TypeUsage toType)
        {
            Debug.Assert(fromType != null, "fromType must not be NULL");
            Debug.Assert(toType != null, "toType must not be NULL");

            if (toType.EdmType.EdmEquals(fromType.EdmType))
            {
                return true;
            }

            if (Helper.IsPrimitiveType(fromType.EdmType) && Helper.IsPrimitiveType(toType.EdmType))
            {
                return IsPrimitiveTypePromotableTo(fromType,
                                                   toType);
            }
            else if (Helper.IsCollectionType(fromType.EdmType) && Helper.IsCollectionType(toType.EdmType))
            {
                return IsPromotableTo(TypeHelpers.GetElementTypeUsage(fromType),
                                      TypeHelpers.GetElementTypeUsage(toType));
            }
            else if (Helper.IsEntityTypeBase(fromType.EdmType) && Helper.IsEntityTypeBase(toType.EdmType))
            {
                return fromType.EdmType.IsSubtypeOf(toType.EdmType);
            }
            else if (Helper.IsRefType(fromType.EdmType) && Helper.IsRefType(toType.EdmType))
            {
                return IsPromotableTo(TypeHelpers.GetElementTypeUsage(fromType),
                                      TypeHelpers.GetElementTypeUsage(toType));
            }
            else if (Helper.IsRowType(fromType.EdmType) && Helper.IsRowType(toType.EdmType))
            {
                return IsPromotableTo((RowType)fromType.EdmType,
                                      (RowType)toType.EdmType);
            }

            return false;
        }

        /// <summary>
        /// Flattens composite transient type down to nominal type leafs.
        /// </summary>
        internal static IEnumerable<TypeUsage> FlattenType(TypeUsage type)
        {
            Func<TypeUsage, bool> isLeaf = t => !Helper.IsTransientType(t.EdmType);

            Func<TypeUsage, IEnumerable<TypeUsage>> getImmediateSubNodes = 
                t =>
                {
                    if (Helper.IsCollectionType(t.EdmType) || Helper.IsRefType(t.EdmType))
                    {
                        return new[] { TypeHelpers.GetElementTypeUsage(t) };
                    }
                    else if (Helper.IsRowType(t.EdmType))
                    {
                        return ((RowType)t.EdmType).Properties.Select(p => p.TypeUsage);
                    }
                    else
                    {
                        Debug.Fail("cannot enumerate subnodes of a leaf node");
                        return new TypeUsage[] { };
                    }
                };

            return Common.Utils.Helpers.GetLeafNodes<TypeUsage>(type, isLeaf, getImmediateSubNodes);
        }

        /// <summary>
        /// determines if fromType can be casted to toType.
        /// </summary>
        /// <param name="fromType">Type to cast from.</param>
        /// <param name="toType">Type to cast to.</param>
        /// <returns><c>true</c> if <paramref name="fromType"/> can be casted to <paramref name="toType" />; <c>false</c> otherwise.</returns>
        /// <remarks>
        /// Cast rules:
        /// - primitive types can be casted to other primitive types
        /// - primitive types can be casted to enum types
        /// - enum types can be casted to primitive types
        /// - enum types cannot be casted to other enum types except for casting to the same type
        /// </remarks>
        internal static bool IsCastAllowed(TypeUsage fromType, TypeUsage toType)
        {
            Debug.Assert(fromType != null, "fromType != null");
            Debug.Assert(toType != null, "toType != null");

            return
                (Helper.IsPrimitiveType(fromType.EdmType) && Helper.IsPrimitiveType(toType.EdmType)) ||
                (Helper.IsPrimitiveType(fromType.EdmType) && Helper.IsEnumType(toType.EdmType)) ||
                (Helper.IsEnumType(fromType.EdmType) && Helper.IsPrimitiveType(toType.EdmType)) ||
                (Helper.IsEnumType(fromType.EdmType) && Helper.IsEnumType(toType.EdmType) && fromType.EdmType.Equals(toType.EdmType));
        }

        /// <summary>
        /// Determines if a common super type (LUB) exists between type1 and type2.
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <param name="commonType"></param>
        /// <returns>
        /// true if a common super type between type1 and type2 exists and out commonType represents the common super type.
        /// false otherwise along with commonType as null
        /// </returns>
        internal static bool TryGetCommonType(TypeUsage type1, TypeUsage type2, out TypeUsage commonType)
        {
            Debug.Assert(type1 != null, "type1 must not be NULL");
            Debug.Assert(type2 != null, "type2 must not be NULL");

            commonType = null;

            if (type1.EdmEquals(type2))
            {
                commonType = ForgetConstraints(type2);
                return true;
            }

            if (Helper.IsPrimitiveType(type1.EdmType) && Helper.IsPrimitiveType(type2.EdmType))
            {
                return TryGetCommonPrimitiveType(type1, type2, out commonType);
            }

            EdmType commonEdmType;
            if (TryGetCommonType(type1.EdmType, type2.EdmType, out commonEdmType))
            {
                commonType = ForgetConstraints(TypeUsage.Create(commonEdmType));
                return true;
            }

            commonType = null;
            return false;
        }

        /// <summary>
        /// Gets a Common super-type of type1 and type2 if one exists. null otherwise.
        /// </summary>
        /// <param name="type1"></param>
        /// <param name="type2"></param>
        /// <returns></returns>
        internal static TypeUsage GetCommonType(TypeUsage type1, TypeUsage type2)
        {
            TypeUsage commonType = null;
            if (TypeSemantics.TryGetCommonType(type1, type2, out commonType))
            {
                return commonType;
            }
            return null;
        }

        /// <summary>
        /// determines if an EdmFunction is an aggregate function
        /// </summary>
        /// <param name="function"></param>
        /// <returns></returns>
        internal static bool IsAggregateFunction(EdmFunction function)
        {
            return function.AggregateAttribute;
        }

        /// <summary>
        /// determines if fromType can be cast to toType. this operation is valid only
        /// if fromtype and totype are polimorphic types.
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        internal static bool IsValidPolymorphicCast(TypeUsage fromType, TypeUsage toType)
        {
            if (!IsPolymorphicType(fromType) || !IsPolymorphicType(toType))
            {
                return false;
            }
            return (IsStructurallyEqual(fromType, toType) || IsSubTypeOf(fromType, toType) || IsSubTypeOf(toType, fromType));
        }

        /// <summary>
        /// determines if fromEdmType can be cast to toEdmType. this operation is valid only
        /// if fromtype and totype are polimorphic types.
        /// </summary>
        /// <param name="fromType"></param>
        /// <param name="toType"></param>
        /// <returns></returns>
        internal static bool IsValidPolymorphicCast(EdmType fromEdmType, EdmType toEdmType)
        {
            return IsValidPolymorphicCast(TypeUsage.Create(fromEdmType), TypeUsage.Create(toEdmType));
        }

        /// <summary>
        /// Determines if the <param ref="type" /> is a structural nominal type, i.e., EntityType or ComplexType
        /// </summary>
        /// <param name="type">Type to be checked.</param>
        /// <returns><c>true</c> if the <param ref="type" /> is a nominal type. <c>false</c> otherwise.</returns>
        static internal bool IsNominalType(TypeUsage type)
        {
            Debug.Assert(!IsEnumerationType(type), "Implicit cast/Softcast is not allowed for enums so we should never see enum type here.");

            return IsEntityType(type) || IsComplexType(type);
        }

        /// <summary>
        /// determines if type is a collection type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsCollectionType(TypeUsage type)
        {
            return Helper.IsCollectionType(type.EdmType);
        }

        /// <summary>
        /// determines if type is a complex type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsComplexType(TypeUsage type)
        {
            return (BuiltInTypeKind.ComplexType == type.EdmType.BuiltInTypeKind);
        }

        /// <summary>
        /// determines if type is an EntityType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsEntityType(TypeUsage type)
        {
            return Helper.IsEntityType(type.EdmType);
        }

        /// <summary>
        /// determines if type is a Relationship Type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsRelationshipType(TypeUsage type)
        {
            return (BuiltInTypeKind.AssociationType == type.EdmType.BuiltInTypeKind);
        }

        /// <summary>
        /// determines if type is of EnumerationType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsEnumerationType(TypeUsage type)
        {
            Debug.Assert(type != null, "type != null");

            return Helper.IsEnumType(type.EdmType);
        }

        /// <summary>
        /// determines if <paramref name="type"/> is primitive or enumeration type
        /// </summary>
        /// <param name="type">Type to verify.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is primitive or enumeration type. <c>false</c> otherwise.</returns>
        internal static bool IsScalarType(TypeUsage type)
        {
            return IsScalarType(type.EdmType);
        }

        /// <summary>
        /// determines if <paramref name="type"/> is primitive or enumeration type
        /// </summary>
        /// <param name="type">Type to verify.</param>
        /// <returns><c>true</c> if <paramref name="type"/> is primitive or enumeration type. <c>false</c> otherwise.</returns>
        internal static bool IsScalarType(EdmType type)
        {
            Debug.Assert(type != null, "type != null");

            return Helper.IsPrimitiveType(type) || Helper.IsEnumType(type);
        }

        /// <summary>
        /// Determines if type is a numeric type, i.e., is one of:
        /// Byte, Int16, Int32, Int64, Decimal, Single or Double
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsNumericType(TypeUsage type)
        {
            return (IsIntegerNumericType(type) || IsFixedPointNumericType(type) || IsFloatPointNumericType(type));
        }

        /// <summary>
        /// Determines if type is an integer numeric type, i.e., is one of: Byte, Int16, Int32, Int64
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsIntegerNumericType(TypeUsage type)
        {
            PrimitiveTypeKind typeKind;
            if (TypeHelpers.TryGetPrimitiveTypeKind(type, out typeKind))
            {
                switch (typeKind)
                {
                    case PrimitiveTypeKind.Byte:
                    case PrimitiveTypeKind.Int16:
                    case PrimitiveTypeKind.Int32:
                    case PrimitiveTypeKind.Int64:
                    case PrimitiveTypeKind.SByte:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines if type is an fixed point numeric type, i.e., is one of: Decimal
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsFixedPointNumericType(TypeUsage type)
        {
            PrimitiveTypeKind typeKind;
            if (TypeHelpers.TryGetPrimitiveTypeKind(type, out typeKind))
            {
                return (typeKind == PrimitiveTypeKind.Decimal);
            }

            return false;
        }

        /// <summary>
        /// Determines if type is an float point numeric type, i.e., is one of: Single or Double.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsFloatPointNumericType(TypeUsage type)
        {
            PrimitiveTypeKind typeKind;
            if (TypeHelpers.TryGetPrimitiveTypeKind(type, out typeKind))
            {
                return (typeKind == PrimitiveTypeKind.Double || typeKind == PrimitiveTypeKind.Single);
            }
            return false;
        }

        /// <summary>
        /// Determines if type is an unsigned integer numeric type, i.e., is Byte
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsUnsignedNumericType(TypeUsage type)
        {
            PrimitiveTypeKind typeKind;
            if (TypeHelpers.TryGetPrimitiveTypeKind(type, out typeKind))
            {
                switch (typeKind)
                {
                    case PrimitiveTypeKind.Byte:
                        return true;

                    default:
                        return false;
                }
            }
            return false;
        }

        /// <summary>
        /// determines if type is a polimorphic type, ie, EntityType or ComplexType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsPolymorphicType(TypeUsage type)
        {
            return (IsEntityType(type) || IsComplexType(type));
        }

        /// <summary>
        /// determines if type is of Boolean Kind
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsBooleanType(TypeUsage type)
        {
            return TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Boolean);
        }

        /// <summary>
        /// determines if type is a primitive/scalar type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsPrimitiveType(TypeUsage type)
        {
            return Helper.IsPrimitiveType(type.EdmType);
        }

        /// <summary>
        /// determines if type is a primitive type of given primitiveTypeKind
        /// </summary>
        /// <param name="type"></param>
        /// <param name="primitiveType"></param>
        /// <returns></returns>
        internal static bool IsPrimitiveType(TypeUsage type, PrimitiveTypeKind primitiveTypeKind)
        {
            PrimitiveTypeKind typeKind;
            if (TypeHelpers.TryGetPrimitiveTypeKind(type, out typeKind))
            {
                return (typeKind == primitiveTypeKind);
            }
            return false;
        }

        /// <summary>
        /// determines if type is a RowType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsRowType(TypeUsage type)
        {
            return Helper.IsRowType(type.EdmType);
        }

        /// <summary>
        /// determines if type is a ReferenceType
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsReferenceType(TypeUsage type)
        {
            return Helper.IsRefType(type.EdmType);
        }

        /// <summary>
        /// determines if type is a spatial type
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsSpatialType(TypeUsage type)
        {
            return Helper.IsSpatialType(type);
        }

        /// <summary>
        /// determines if type is a strong spatial type (i.e., a spatial type, but not one of the two spatial union types)
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsStrongSpatialType(TypeUsage type)
        {
            return IsPrimitiveType(type) && Helper.IsStrongSpatialTypeKind(((PrimitiveType)type.EdmType).PrimitiveTypeKind);
        }

        /// <summary>
        /// determines if type is a structural type, ie, EntityType, ComplexType, RowType or ReferenceType.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsStructuralType(TypeUsage type)
        {
            return Helper.IsStructuralType(type.EdmType);
        }

        /// <summary>
        /// determines if edmMember is part of the key of it's defining type.
        /// </summary>
        /// <param name="member"></param>
        /// <returns></returns>
        internal static bool IsPartOfKey(EdmMember edmMember)
        {
            if (Helper.IsRelationshipEndMember(edmMember))
            {
                return ((RelationshipType)edmMember.DeclaringType).KeyMembers.Contains(edmMember);
            }

            if (!Helper.IsEdmProperty(edmMember))
            {
                return false;
            }

            if (Helper.IsEntityTypeBase(edmMember.DeclaringType))
            {
                return ((EntityTypeBase)edmMember.DeclaringType).KeyMembers.Contains(edmMember);
            }

            return false;
        }

        /// <summary>
        /// determines if type is Nullable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static bool IsNullable(TypeUsage type)
        {
            Facet nullableFacet;
            if (type.Facets.TryGetValue(EdmProviderManifest.NullableFacetName, false, out nullableFacet))
            {
                return (bool)nullableFacet.Value;
            }
            return true;
        }

        /// <summary>
        /// determines if edmMember is Nullable.
        /// </summary>
        /// <param name="edmMember"></param>
        /// <returns></returns>
        internal static bool IsNullable(EdmMember edmMember)
        {
            return IsNullable(edmMember.TypeUsage);
        }

        /// <summary>
        /// determines if given type is equal-comparable.
        /// </summary>
        /// <param name="type"></param>
        /// <returns>true if equal-comparable, false otherwise</returns>
        internal static bool IsEqualComparable(TypeUsage type)
        {
            return IsEqualComparable(type.EdmType);
        }

        /// <summary>
        /// Determines if type1 is equal-comparable to type2.
        /// in order for type1 and type2 to be equal-comparable, they must be
        /// individualy equal-comparable and have a common super-type.
        /// </summary>
        /// <param name="type1">an instance of a TypeUsage</param>
        /// <param name="type2">an instance of a TypeUsage</param>
        /// <returns><c>true</c> if type1 and type2 are equal-comparable, <c>false</c> otherwise</returns>
        internal static bool IsEqualComparableTo(TypeUsage type1, TypeUsage type2)
        {
            if (IsEqualComparable(type1) && IsEqualComparable(type2))
            {
                return HasCommonType(type1, type2);
            }
            return false;
        }

        /// <summary>
        /// Determines if given type is order-comparable
        /// </summary>
        /// <param name="typeUsage"></param>
        /// <returns></returns>
        internal static bool IsOrderComparable(TypeUsage type)
        {
            Debug.Assert(null != type, "type must not be null");
            return IsOrderComparable(type.EdmType);
        }

        /// Determines if type1 is order-comparable to type2.
        /// in order for type1 and type2 to be order-comparable, they must be
        /// individualy order-comparable and have a common super-type.
        /// </summary>
        /// <param name="type1">an instance of a TypeUsage</param>
        /// <param name="type2">an instance of a TypeUsage</param>
        /// <returns><c>true</c> if type1 and type2 are order-comparable, <c>false</c> otherwise</returns>
        internal static bool IsOrderComparableTo(TypeUsage type1, TypeUsage type2)
        {
            if (IsOrderComparable(type1) && IsOrderComparable(type2))
            {
                return HasCommonType(type1, type2);
            }
            return false;
        }

        /// <summary>
        /// Removes facets that are not type constraints.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        internal static TypeUsage ForgetConstraints(TypeUsage type)
        {
            if (Helper.IsPrimitiveType(type.EdmType))
            {
                return EdmProviderManifest.Instance.ForgetScalarConstraints(type);
            }
            return type;
        }

        [System.Diagnostics.Conditional("DEBUG")]
        static internal void AssertTypeInvariant(string message, Func<bool> assertPredicate)
        {
            System.Diagnostics.Debug.Assert(assertPredicate(),
                                            "Type invariant check FAILED\n" + message);
        }
        #endregion // Internal interface

        //
        // Private Interface
        //
        #region Private Interface

        #region Subtyping
        private static bool IsPrimitiveTypeSubTypeOf(TypeUsage fromType, TypeUsage toType)
        {
            Debug.Assert(fromType != null, "fromType must not be null");
            Debug.Assert(Helper.IsPrimitiveType(fromType.EdmType), "fromType must be primitive type");
            Debug.Assert(toType != null, "toType must not be null");
            Debug.Assert(Helper.IsPrimitiveType(toType.EdmType), "toType must be primitive type");

            if (!IsSubTypeOf((PrimitiveType)fromType.EdmType, (PrimitiveType)toType.EdmType))
            {
                return false;
            }

            return true;
        }

        private static bool IsSubTypeOf(PrimitiveType subPrimitiveType, PrimitiveType superPrimitiveType)
        {
            if (object.ReferenceEquals(subPrimitiveType, superPrimitiveType))
            {
                return true;
            }

            if (Helper.AreSameSpatialUnionType(subPrimitiveType, superPrimitiveType))
            {
                return true;
            }

            objectModel.ReadOnlyCollection<PrimitiveType> superTypes = EdmProviderManifest.Instance.GetPromotionTypes(subPrimitiveType);

            return (-1 != superTypes.IndexOf(superPrimitiveType));
        }

        #endregion // Subtyping

        #region Promotability
        private static bool IsPromotableTo(RowType fromRowType, RowType toRowType)
        {
            Debug.Assert(fromRowType != null && toRowType != null);

            if (fromRowType.Properties.Count != toRowType.Properties.Count)
            {
                return false;
            }

            for (int i = 0; i < fromRowType.Properties.Count; i++)
            {
                if (!IsPromotableTo(fromRowType.Properties[i].TypeUsage, toRowType.Properties[i].TypeUsage))
                {
                    return false;
                }
            }

            return true;
        }

        private static bool IsPrimitiveTypePromotableTo(TypeUsage fromType, TypeUsage toType)
        {
            Debug.Assert(fromType != null, "fromType must not be null");
            Debug.Assert(Helper.IsPrimitiveType(fromType.EdmType), "fromType must be primitive type");
            Debug.Assert(toType != null, "toType must not be null");
            Debug.Assert(Helper.IsPrimitiveType(toType.EdmType), "toType must be primitive type");

            if (!IsSubTypeOf((PrimitiveType)fromType.EdmType, (PrimitiveType)toType.EdmType))
            {
                return false;
            }

            return true;
        }

        #endregion // promotability

        #region Common Super-Type
        private static bool TryGetCommonType(EdmType edmType1, EdmType edmType2, out EdmType commonEdmType)
        {
            Debug.Assert(edmType1 != null && edmType2 != null);

            if (edmType2 == edmType1)
            {
                commonEdmType = edmType1;
                return true;
            }

            if (Helper.IsPrimitiveType(edmType1) && Helper.IsPrimitiveType(edmType2))
            {
                return TryGetCommonType((PrimitiveType)edmType1,
                                                 (PrimitiveType)edmType2,
                                                 out commonEdmType);
            }

            else if (Helper.IsCollectionType(edmType1) && Helper.IsCollectionType(edmType2))
            {
                return TryGetCommonType((CollectionType)edmType1,
                                                  (CollectionType)edmType2,
                                                  out commonEdmType);
            }

            else if (Helper.IsEntityTypeBase(edmType1) && Helper.IsEntityTypeBase(edmType2))
            {
                return TryGetCommonBaseType(edmType1,
                                            edmType2,
                                            out commonEdmType);
            }

            else if (Helper.IsRefType(edmType1) && Helper.IsRefType(edmType2))
            {
                return TryGetCommonType((RefType)edmType1,
                                           (RefType)edmType2,
                                           out commonEdmType);
            }

            else if (Helper.IsRowType(edmType1) && Helper.IsRowType(edmType2))
            {
                return TryGetCommonType((RowType)edmType1,
                                           (RowType)edmType2,
                                           out commonEdmType);
            }
            else
            {
                commonEdmType = null;
                return false;
            }
        }

        private static bool TryGetCommonPrimitiveType(TypeUsage type1, TypeUsage type2, out TypeUsage commonType)
        {
            Debug.Assert(type1 != null, "type1 must not be null");
            Debug.Assert(Helper.IsPrimitiveType(type1.EdmType), "type1 must be primitive type");
            Debug.Assert(type2 != null, "type2 must not be null");
            Debug.Assert(Helper.IsPrimitiveType(type2.EdmType), "type2 must be primitive type");

            commonType = null;

            if (IsPromotableTo(type1, type2))
            {
                commonType = ForgetConstraints(type2);
                return true;
            }

            if (IsPromotableTo(type2, type1))
            {
                commonType = ForgetConstraints(type1);
                return true;
            }

            objectModel.ReadOnlyCollection<PrimitiveType> superTypes = GetPrimitiveCommonSuperTypes((PrimitiveType)type1.EdmType,
                                                                                                    (PrimitiveType)type2.EdmType);
            if (superTypes.Count == 0)
            {
                return false;
            }

            commonType = TypeUsage.CreateDefaultTypeUsage(superTypes[0]);
            return null != commonType;
        }

        private static bool TryGetCommonType(PrimitiveType primitiveType1, PrimitiveType primitiveType2, out EdmType commonType)
        {
            commonType = null;

            if (IsSubTypeOf(primitiveType1, primitiveType2))
            {
                commonType = primitiveType2;
                return true;
            }

            if (IsSubTypeOf(primitiveType2, primitiveType1))
            {
                commonType = primitiveType1;
                return true;
            }

            objectModel.ReadOnlyCollection<PrimitiveType> superTypes = GetPrimitiveCommonSuperTypes(primitiveType1, primitiveType2);
            if (superTypes.Count > 0)
            {
                commonType = superTypes[0];
                return true;
            }

            return false;
        }

        private static bool TryGetCommonType(CollectionType collectionType1, CollectionType collectionType2, out EdmType commonType)
        {
            TypeUsage commonTypeUsage = null;
            if (!TryGetCommonType(collectionType1.TypeUsage, collectionType2.TypeUsage, out commonTypeUsage))
            {
                commonType = null;
                return false;
            }

            commonType = new CollectionType(commonTypeUsage);
            return true;
        }

        private static bool TryGetCommonType(RefType refType1, RefType reftype2, out EdmType commonType)
        {
            Debug.Assert(refType1.ElementType != null && reftype2.ElementType != null);

            if (!TryGetCommonType(refType1.ElementType, reftype2.ElementType, out commonType))
            {
                return false;
            }

            commonType = new RefType((EntityType)commonType);
            return true;
        }

        private static bool TryGetCommonType(RowType rowType1, RowType rowType2, out EdmType commonRowType)
        {
            if (rowType1.Properties.Count != rowType2.Properties.Count ||
                rowType1.InitializerMetadata != rowType2.InitializerMetadata)
            {
                commonRowType = null;
                return false;
            }

            // find a common type for every property
            List<EdmProperty> commonProperties = new List<EdmProperty>();
            for (int i = 0; i < rowType1.Properties.Count; i++)
            {
                TypeUsage columnCommonTypeUsage;
                if (!TryGetCommonType(rowType1.Properties[i].TypeUsage, rowType2.Properties[i].TypeUsage, out columnCommonTypeUsage))
                {
                    commonRowType = null;
                    return false;
                }

                commonProperties.Add(new EdmProperty(rowType1.Properties[i].Name, columnCommonTypeUsage));
            }

            commonRowType = new RowType(commonProperties, rowType1.InitializerMetadata);
            return true;
        }

        private static bool TryGetCommonBaseType(EdmType type1, EdmType type2, out EdmType commonBaseType)
        {
            // put all the other base types in a dictionary
            Dictionary<EdmType, byte> otherBaseTypes = new Dictionary<EdmType, byte>();
            for (EdmType ancestor = type2; ancestor != null; ancestor = ancestor.BaseType)
            {
                otherBaseTypes.Add(ancestor, 0);
            }

            // walk up the ancestor chain, and see if any of them are 
            // common to the otherTypes ancestors
            for (EdmType ancestor = type1; ancestor != null; ancestor = ancestor.BaseType)
            {

                if (otherBaseTypes.ContainsKey(ancestor))
                {
                    commonBaseType = ancestor;
                    return true;
                }
            }

            commonBaseType = null;
            return false;
        }

        private static bool HasCommonType(TypeUsage type1, TypeUsage type2)
        {
            return (null != TypeHelpers.GetCommonTypeUsage(type1, type2));
        }
        #endregion // common super-type helpers

        #region Comparability
        /// <summary>
        /// Determines if the given edmType is equal comparable. Consult "EntitySql Language Specification", 
        /// section 7 - Comparison and Dependent Operations for details.
        /// </summary>
        /// <param name="edmType">an instance of an EdmType</param>
        /// <returns>true if edmType is equal-comparable, false otherwise</returns>
        private static bool IsEqualComparable(EdmType edmType)
        {
            if (Helper.IsPrimitiveType(edmType) || Helper.IsRefType(edmType) || Helper.IsEntityType(edmType) || Helper.IsEnumType(edmType))
            {
                return true;
            }
            else if (Helper.IsRowType(edmType))
            {
                RowType rowType = (RowType)edmType;
                foreach (EdmProperty rowProperty in rowType.Properties)
                {
                    if (!IsEqualComparable(rowProperty.TypeUsage))
                    {
                        return false;
                    }
                }
                return true;
            }
            return false;
        }

        /// Determines if the given edmType is order comparable. Consult "EntitySql Language Specification", 
        /// section 7 - Comparison and Dependent Operations for details.
        /// </summary>
        /// <param name="edmType">an instance of an EdmType</param>
        /// <returns>true if edmType is order-comparable, false otherwise</returns>
        private static bool IsOrderComparable(EdmType edmType)
        {
            // only primitive and enum types are assumed to be order-comparable though they 
            // may still fail during runtime depending on the provider specific behavior
            return Helper.IsScalarType(edmType);
        }
        #endregion

        #region Private Helpers

        private static bool CompareTypes(TypeUsage fromType, TypeUsage toType, bool equivalenceOnly)
        {
            Debug.Assert(fromType != null && toType != null);

            // If the type usages are the same reference, they are equal.
            if (object.ReferenceEquals(fromType, toType))
            {
                return true;
            }

            if (fromType.EdmType.BuiltInTypeKind != toType.EdmType.BuiltInTypeKind)
            {
                return false;
            }

            //
            // Ensure structural evaluation for Collection, Ref and Row types
            //
            if (fromType.EdmType.BuiltInTypeKind == BuiltInTypeKind.CollectionType)
            {
                // Collection Type: Just compare the Element types
                return CompareTypes(((CollectionType)fromType.EdmType).TypeUsage,
                                                ((CollectionType)toType.EdmType).TypeUsage,
                                                equivalenceOnly);
            }
            else if (fromType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RefType)
            {
                // Both are Reference Types, so compare the referenced Entity types
                return ((RefType)fromType.EdmType).ElementType.EdmEquals(((RefType)toType.EdmType).ElementType);
            }
            else if (fromType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType)
            {
                // Row Types
                RowType fromRow = (RowType)fromType.EdmType;
                RowType toRow = (RowType)toType.EdmType;
                // Both are RowTypes, so compare the structure.
                // The number of properties must be the same.
                if (fromRow.Properties.Count != toRow.Properties.Count)
                {
                    return false;
                }

                // Compare properties. For an equivalence comparison, only
                // property types must match, otherwise names and types must match.
                for (int idx = 0; idx < fromRow.Properties.Count; idx++)
                {
                    EdmProperty fromProp = fromRow.Properties[idx];
                    EdmProperty toProp = toRow.Properties[idx];

                    if (!equivalenceOnly && (fromProp.Name != toProp.Name))
                    {
                        return false;
                    }

                    if (!CompareTypes(fromProp.TypeUsage, toProp.TypeUsage, equivalenceOnly))
                    {
                        return false;
                    }
                }

                return true;
            }

            //
            // compare non-transient type usages - simply compare the edm types instead
            //
            return fromType.EdmType.EdmEquals(toType.EdmType);
        }

        /// <summary>
        /// Computes the closure of common super types of the set of predefined edm primitive types
        /// This is done only once and cached as opposed to previous implementation that was computing
        /// this for every new pair of types.
        /// </summary>
        private static void ComputeCommonTypeClosure()
        {
            if (null != _commonTypeClosure)
            {
                return;
            }

            objectModel.ReadOnlyCollection<PrimitiveType>[,] commonTypeClosure = new objectModel.ReadOnlyCollection<PrimitiveType>[EdmConstants.NumPrimitiveTypes, EdmConstants.NumPrimitiveTypes];
            for (int i = 0; i < EdmConstants.NumPrimitiveTypes; i++)
            {
                commonTypeClosure[i, i] = Helper.EmptyPrimitiveTypeReadOnlyCollection;
            }

            objectModel.ReadOnlyCollection<PrimitiveType> primitiveTypes = EdmProviderManifest.Instance.GetStoreTypes();

            for (int i = 0; i < EdmConstants.NumPrimitiveTypes; i++)
            {
                for (int j = 0; j < i; j++)
                {
                    commonTypeClosure[i, j] = Intersect(EdmProviderManifest.Instance.GetPromotionTypes(primitiveTypes[i]),
                                                        EdmProviderManifest.Instance.GetPromotionTypes(primitiveTypes[j]));

                    commonTypeClosure[j, i] = commonTypeClosure[i, j];
                }
            }

            TypeSemantics.AssertTypeInvariant("Common Type closure is incorrect",
                                                delegate()
                                                {
                                                    for (int i = 0; i < EdmConstants.NumPrimitiveTypes; i++)
                                                    {
                                                        for (int j = 0; j < EdmConstants.NumPrimitiveTypes; j++)
                                                        {
                                                            if (commonTypeClosure[i, j] != commonTypeClosure[j, i])
                                                                return false;
                                                            if (i == j && commonTypeClosure[i, j].Count != 0)
                                                                return false;
                                                        }
                                                    }
                                                    return true;
                                                });

            System.Threading.Interlocked.CompareExchange<objectModel.ReadOnlyCollection<PrimitiveType>[,]>(ref _commonTypeClosure, commonTypeClosure, null);
        }

        /// <summary>
        /// returns the intersection of types.
        /// </summary>
        /// <param name="types1"></param>
        /// <param name="types2"></param>
        /// <returns></returns>
        private static objectModel.ReadOnlyCollection<PrimitiveType> Intersect(IList<PrimitiveType> types1, IList<PrimitiveType> types2)
        {
            List<PrimitiveType> commonTypes = new List<PrimitiveType>();
            for (int i = 0; i < types1.Count; i++)
            {
                if (types2.Contains(types1[i]))
                {
                    commonTypes.Add(types1[i]);
                }
            }

            if (0 == commonTypes.Count)
            {
                return Helper.EmptyPrimitiveTypeReadOnlyCollection;
            }

            return new objectModel.ReadOnlyCollection<PrimitiveType>(commonTypes);
        }

        /// <summary>
        /// Returns the list of common super types of two primitive types.
        /// </summary>
        /// <param name="primitiveType1"></param>
        /// <param name="primitiveType2"></param>
        /// <returns></returns>
        private static objectModel.ReadOnlyCollection<PrimitiveType> GetPrimitiveCommonSuperTypes(PrimitiveType primitiveType1, PrimitiveType primitiveType2)
        {
            ComputeCommonTypeClosure();
            return _commonTypeClosure[(int)primitiveType1.PrimitiveTypeKind, (int)primitiveType2.PrimitiveTypeKind];
        }
        #endregion // Private Helpers

        #endregion // Private interface
    }
}

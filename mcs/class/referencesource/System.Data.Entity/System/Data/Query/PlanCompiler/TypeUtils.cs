//---------------------------------------------------------------------
// <copyright file="TypeUtils.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
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

using System.Data.Common;
using md = System.Data.Metadata.Edm;

//
// This module contains a few utility functions that make it easier to operate
// with type metadata
//

namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// This class is used as a Comparer for Types all through the PlanCompiler.
    /// It has a pretty strict definition of type equality - which pretty much devolves
    /// to equality of the "Identity" of the Type (not the TypeUsage).
    /// 
    /// NOTE: Unlike other parts of the query pipeline, record types follow 
    /// a much stricter equality condition here - the field names must be the same, and 
    /// the field types must be equal.
    /// 
    /// NOTE: Primitive types are considered equal, if their Identities are equal. This doesn't
    /// take into account any of the facets that are represented external to the type (size, for instance). 
    /// Again, this is different from other parts of  the query pipeline; and we're much stricter here
    /// 
    /// </summary>
    sealed internal class TypeUsageEqualityComparer : IEqualityComparer<md.TypeUsage>
    {
        private TypeUsageEqualityComparer() { }
        internal static readonly TypeUsageEqualityComparer Instance = new TypeUsageEqualityComparer();

        #region IEqualityComparer<TypeUsage> Members

        public bool Equals(System.Data.Metadata.Edm.TypeUsage x, System.Data.Metadata.Edm.TypeUsage y)
        {
            if (x == null || y == null)
            {
                return false;
            }

            return TypeUsageEqualityComparer.Equals(x.EdmType, y.EdmType);
        }

        public int GetHashCode(System.Data.Metadata.Edm.TypeUsage obj)
        {
            return obj.EdmType.Identity.GetHashCode();
        }

        #endregion

        internal static bool Equals(md.EdmType x, md.EdmType y)
        {
            return x.Identity.Equals(y.Identity);
        }
    }

    internal static class TypeUtils
    {
        /// <summary>
        /// Is this type a UDT? (ie) a structural type supported by the store
        /// </summary>
        /// <param name="type">the type in question</param>
        /// <returns>true, if the type was a UDT</returns>
        internal static bool IsUdt(md.TypeUsage type)
        {
            return IsUdt(type.EdmType);
        }

        /// <summary>
        /// Is this type a UDT? (ie) a structural type supported by the store
        /// </summary>
        /// <param name="type">the type in question</param>
        /// <returns>true, if the type was a UDT</returns>
        internal static bool IsUdt(md.EdmType type)
        {
#if UDT_SUPPORT
            // Ideally this should be as simple as:
            // return TypeUsage.HasExtendedAttribute(type, MetadataConstants.UdtAttribute);
            // The definition below is 'Type is a ComplexType defined in the store'.
            return (BuiltInTypeKind.ComplexType == type.BuiltInTypeKind &&
                    TypeHelpers.HasExtendedAttribute(type, MetadataConstants.TargetAttribute));
#else
            return false;
#endif
        }

        /// <summary>
        /// Is this a structured type? 
        /// Note: Structured, in this context means structured outside the server. 
        /// UDTs for instance, are considered to be scalar types - all WinFS types,
        /// would by this argument, be scalar types.
        /// </summary>
        /// <param name="type">The type to check</param>
        /// <returns>true, if the type is a structured type</returns>
        internal static bool IsStructuredType(md.TypeUsage type)
        {
            return (md.TypeSemantics.IsReferenceType(type) ||
                    md.TypeSemantics.IsRowType(type) ||
                    md.TypeSemantics.IsEntityType(type) ||
                    md.TypeSemantics.IsRelationshipType(type) ||
                    (md.TypeSemantics.IsComplexType(type) && !IsUdt(type)));
        }

        /// <summary>
        /// Is this type a collection type?
        /// </summary>
        /// <param name="type">the current type</param>
        /// <returns>true, if this is a collection type</returns>
        internal static bool IsCollectionType(md.TypeUsage type)
        {
            return md.TypeSemantics.IsCollectionType(type);
        }

        /// <summary>
        /// Is this type an enum type?
        /// </summary>
        /// <param name="type">the current type</param>
        /// <returns>true, if this is an enum type</returns>
        internal static bool IsEnumerationType(md.TypeUsage type)
        {
            return md.TypeSemantics.IsEnumerationType(type);
        }

        /// <summary>
        /// Create a new collection type based on the supplied element type
        /// </summary>
        /// <param name="elementType">element type of the collection</param>
        /// <returns>the new collection type</returns>
        internal static md.TypeUsage CreateCollectionType(md.TypeUsage elementType)
        {
            return TypeHelpers.CreateCollectionTypeUsage(elementType);
        }
    }
}

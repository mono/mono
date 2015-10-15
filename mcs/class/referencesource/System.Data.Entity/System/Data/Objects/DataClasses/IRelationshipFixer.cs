//---------------------------------------------------------------------
// <copyright file="IRelationshipFixer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       mirszy
// @backupOwner sparra
//---------------------------------------------------------------------

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// Internal interface used to provide a non-typed way to store a reference to an object
    /// that knows the type and cardinality of the source end of a relationship
    /// </summary>
    internal interface IRelationshipFixer
    {
        /// <summary>
        /// Used during relationship fixup when the source end of the relationship is not
        /// yet in the relationships list, and needs to be created
        /// </summary>
        /// <param name="navigation">RelationshipNavigation to be set on new RelatedEnd</param>
        /// <param name="relationshipManager">RelationshipManager to use for creating the new end</param>
        /// <returns>Reference to the new collection or reference on the other end of the relationship</returns>
        RelatedEnd CreateSourceEnd(RelationshipNavigation navigation, RelationshipManager relationshipManager);            
    }
}

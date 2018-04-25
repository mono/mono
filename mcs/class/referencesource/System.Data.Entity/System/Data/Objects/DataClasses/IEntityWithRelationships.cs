//---------------------------------------------------------------------
// <copyright file="IEntityWithRelationships.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       sparra
// @backupOwner barryfr
//---------------------------------------------------------------------
using System.Data.Objects;
using System.Reflection;

namespace System.Data.Objects.DataClasses
{
    /// <summary>
    /// Interface that a data class must implement if exposes relationships    
    /// </summary>
    public interface IEntityWithRelationships
    {
        /// <summary>
        /// The RelationshipManager property is used for elationship fixup.  
        /// Classes that expose relationships must implement this property
        /// by constructing and setting RelationshipManager in their constructor.
        /// The implementation of this property should use the static method RelationshipManager.Create 
        /// to create a new RelationshipManager when needed. Once created, it is expected that this
        /// object will be stored on the entity and will be provided through this property.
        /// </summary>        
        RelationshipManager RelationshipManager { get; }
    }
}

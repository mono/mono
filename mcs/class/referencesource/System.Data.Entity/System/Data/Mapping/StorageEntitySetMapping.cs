//---------------------------------------------------------------------
// <copyright file="StorageEntitySetMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Mapping {
    /// <summary>
    /// Represents the Mapping metadata for an EnitytSet in CS space.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    /// This class represents the metadata for the EntitySetMapping elements in the
    /// above example. And it is possible to access the EntityTypeMaps underneath it.
    /// </example>
    internal class StorageEntitySetMapping : StorageSetMapping {
        #region Constructors
        /// <summary>
        /// Construct a EntitySet mapping object
        /// </summary>
        /// <param name="extent">EntitySet metadata object</param>
        /// <param name="entityContainerMapping">The entity Container Mapping that contains this Set mapping</param>
        internal StorageEntitySetMapping(EntitySet extent, StorageEntityContainerMapping entityContainerMapping)
            : base(extent, entityContainerMapping) {
            m_modificationFunctionMappings = new List<StorageEntityTypeModificationFunctionMapping>();
            m_implicitlyMappedAssociationSetEnds = new List<AssociationSetEnd>();
        }
        #endregion

        #region Fields
        private readonly List<StorageEntityTypeModificationFunctionMapping> m_modificationFunctionMappings;
        private readonly List<AssociationSetEnd> m_implicitlyMappedAssociationSetEnds;
        #endregion

        #region Properties
        /// <summary>
        /// Gets all function mappings for this entity set.
        /// </summary>
        internal IList<StorageEntityTypeModificationFunctionMapping> ModificationFunctionMappings {
            get { return m_modificationFunctionMappings.AsReadOnly(); }
        }

        /// <summary>
        /// Gets all association sets that are implicitly "covered" through function mappings.
        /// </summary>
        internal IList<AssociationSetEnd> ImplicitlyMappedAssociationSetEnds {
            get { return m_implicitlyMappedAssociationSetEnds.AsReadOnly(); }
        }


        /// <summary>
        /// Whether the EntitySetMapping has empty content
        /// Returns true if there are no Function Maps and no table Mapping fragments
        /// </summary>
        internal override bool HasNoContent {
            get {
                if (m_modificationFunctionMappings.Count != 0) {
                    return false;
                }
                return base.HasNoContent;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        internal override void Print(int index) {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("EntitySetMapping");
            sb.Append("   ");
            sb.Append("Name:");
            sb.Append(this.Set.Name);
            if (this.QueryView != null)
            {
                sb.Append("   ");
                sb.Append("Query View:");
                sb.Append(this.QueryView);
            }
            Console.WriteLine(sb.ToString());
            foreach (StorageTypeMapping typeMapping in TypeMappings) {
                typeMapping.Print(index+5);
            }
            foreach (StorageEntityTypeModificationFunctionMapping m in m_modificationFunctionMappings)
            {
                m.Print(index + 10);
            }
        }

        /// <summary>
        /// Requires:
        /// - Function mapping refers to a sub-type of this entity set's element type
        /// - Function mappings for types are not redundantly specified
        /// Adds a new function mapping for this class.
        /// </summary>
        /// <param name="modificationFunctionMapping">Function mapping to add. May not be null.</param>
        internal void AddModificationFunctionMapping(StorageEntityTypeModificationFunctionMapping modificationFunctionMapping) {
            AssertModificationFunctionMappingInvariants(modificationFunctionMapping);

            m_modificationFunctionMappings.Add(modificationFunctionMapping);

            // check if any association sets are indirectly mapped within this function mapping
            // through association navigation bindings
            if (null != modificationFunctionMapping.DeleteFunctionMapping)
            {
                m_implicitlyMappedAssociationSetEnds.AddRange(modificationFunctionMapping.DeleteFunctionMapping.CollocatedAssociationSetEnds);
            }
            if (null != modificationFunctionMapping.InsertFunctionMapping)
            {
                m_implicitlyMappedAssociationSetEnds.AddRange(modificationFunctionMapping.InsertFunctionMapping.CollocatedAssociationSetEnds);
            }
            if (null != modificationFunctionMapping.UpdateFunctionMapping)
            {
                m_implicitlyMappedAssociationSetEnds.AddRange(modificationFunctionMapping.UpdateFunctionMapping.CollocatedAssociationSetEnds);
            }
        }

        [Conditional("DEBUG")]
        internal void AssertModificationFunctionMappingInvariants(StorageEntityTypeModificationFunctionMapping modificationFunctionMapping) {
            Debug.Assert(null != modificationFunctionMapping, "modification function mapping must not be null");
            Debug.Assert(modificationFunctionMapping.EntityType.Equals(this.Set.ElementType) ||
                Helper.IsSubtypeOf(modificationFunctionMapping.EntityType, this.Set.ElementType),
                "attempting to add a modification function mapping with the wrong entity type");
            foreach (StorageEntityTypeModificationFunctionMapping existingMapping in m_modificationFunctionMappings) {
                Debug.Assert(!existingMapping.EntityType.Equals(modificationFunctionMapping.EntityType),
                    "modification function mapping already exists for this type");
            }
        }
        #endregion
    }
}

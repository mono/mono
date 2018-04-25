//---------------------------------------------------------------------
// <copyright file="StorageAssociationSetMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping {
    /// <summary>
    /// Represents the Mapping metadata for an AssociationSet in CS space.
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
    /// This class represents the metadata for the AssociationSetMapping elements in the
    /// above example. And it is possible to access the AssociationTypeMap underneath it.
    /// There will be only one TypeMap under AssociationSetMap.
    /// </example>
    internal class StorageAssociationSetMapping : StorageSetMapping {
        #region Constructors
        /// <summary>
        /// Construct a new AssociationSetMapping object
        /// </summary>
        /// <param name="extent">Represents the Association Set Metadata object. Will
        ///                      change this to Extent instead of MemberMetadata.</param>
        /// <param name="entityContainerMapping">The entityContainerMapping mapping that contains this Set mapping</param>
        internal StorageAssociationSetMapping(AssociationSet extent, StorageEntityContainerMapping entityContainerMapping)
            : base(extent, entityContainerMapping) {
        }
        #endregion

        #region Fields
        private StorageAssociationSetModificationFunctionMapping m_modificationFunctionMapping;
        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets function mapping information for this association set. May be null.
        /// </summary>
        internal StorageAssociationSetModificationFunctionMapping ModificationFunctionMapping {
            get { return m_modificationFunctionMapping; }
            set { m_modificationFunctionMapping = value; }
        }

        internal EntitySetBase StoreEntitySet
        {
            get
            {
                if ((this.TypeMappings.Count != 0) && (this.TypeMappings.First().MappingFragments.Count != 0))
                {
                    return this.TypeMappings.First().MappingFragments.First().TableSet;
                }
                return null;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        /// <param name="index"></param>
        internal override void Print(int index) {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("AssociationSetMapping");
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
                typeMapping.Print(index + 5);
            }
            if(m_modificationFunctionMapping != null)
            {
                m_modificationFunctionMapping.Print(index + 5);
            }
        }
        #endregion
    }
}

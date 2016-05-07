//---------------------------------------------------------------------
// <copyright file="StoragePropertyMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping {
    /// <summary>
    /// Mapping metadata for all types of property mappings.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap
    ///           --ScalarPropertyMap
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap
    ///           --ComplexPropertyMap
    ///             --ScalarPropertyMap
    ///             --ScalarProperyMap
    ///           --ScalarPropertyMap
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap
    ///             --ScalarProperyMap
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap
    /// This class represents the metadata for all property map elements in the 
    /// above example. This includes the scalar property maps, complex property maps
    /// and end property maps.
    /// </example>
    internal abstract class StoragePropertyMapping {
        #region Constructors
        /// <summary>
        /// Construct a new EdmProperty mapping object
        /// </summary>
        /// <param name="cdmMember">The PropertyMetadata object that represents the member for which mapping is being specified</param>
        internal StoragePropertyMapping(EdmProperty cdmMember) {
            this.m_cdmMember = cdmMember;
        }
        #endregion

        #region Fields
        /// <summary>
        /// EdmProperty metadata representing the Cdm member for which the mapping is specified.
        /// </summary>
        EdmProperty m_cdmMember;
        #endregion

        #region Properties
        /// <summary>
        /// The PropertyMetadata object that represents the member for which mapping is being specified
        /// </summary>
        internal virtual EdmProperty EdmProperty
        {
            get {
                return this.m_cdmMember;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        /// <param name="index"></param>
        internal virtual void Print(int index) {
        }
        #endregion
    }
}

//---------------------------------------------------------------------
// <copyright file="StorageComplexPropertyMapping.cs" company="Microsoft">
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

namespace System.Data.Mapping {
    /// <summary>
    /// Mapping metadata for Complex properties.
    /// </summary>
    /// <example>
    /// For Example if conceptually you could represent the CS MSL file as following
    /// --Mapping 
    ///   --EntityContainerMapping ( CNorthwind-->SNorthwind )
    ///     --EntitySetMapping
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///       --EntityTypeMapping
    ///         --MappingFragment
    ///           --EntityKey
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --ComplexPropertyMap
    ///             --ComplexTypeMapping
    ///               --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///             --ComplexTypeMapping
    ///               --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///               --DiscriminatorProperyMap ( constant value-->SMemberMetadata )
    ///           --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///     --AssociationSetMapping 
    ///       --AssociationTypeMapping
    ///         --MappingFragment
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    ///             --ScalarProperyMap ( CMemberMetadata-->SMemberMetadata )
    ///           --EndPropertyMap
    ///             --ScalarPropertyMap ( CMemberMetadata-->SMemberMetadata )
    /// This class represents the metadata for all the complex property map elements in the 
    /// above example. ComplexPropertyMaps contain ComplexTypeMaps which define mapping based 
    /// on the type of the ComplexProperty in case of inheritance.
    /// </example>
    internal class StorageComplexPropertyMapping : StoragePropertyMapping {
        #region Constructors
        /// <summary>
        /// Construct a new Complex Property mapping object
        /// </summary>
        /// <param name="cdmMember">The MemberMetadata object that represents this Complex member</param>
        internal StorageComplexPropertyMapping(EdmProperty cdmMember)
            : base(cdmMember) {
            this.m_typeMappings = new List<StorageComplexTypeMapping>();
        }
        #endregion

        #region Fields
        /// <summary>
        /// Set of type mappings that make up the EdmProperty mapping.
        /// </summary>
        private List<StorageComplexTypeMapping> m_typeMappings;
        #endregion

        #region Properties
        ///// <summary>
        ///// The property Metadata object for which the mapping is represented.
        ///// </summary>
        //internal EdmProperty ComplexProperty {
        //    get {
        //        return this.EdmProperty;
        //    }
        //}

        /// <summary>
        /// TypeMappings that make up this property.
        /// </summary>
        internal ReadOnlyCollection<StorageComplexTypeMapping> TypeMappings
        {
            get
            {
                return this.m_typeMappings.AsReadOnly();
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add type mapping as a child under this Property Mapping
        /// </summary>
        /// <param name="typeMapping"></param>
        internal void AddTypeMapping(StorageComplexTypeMapping typeMapping)
        {
            this.m_typeMappings.Add(typeMapping);
        }

        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        /// <param name="index"></param>
        internal override void Print(int index) {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("ComplexPropertyMapping");
            sb.Append("   ");
            if (this.EdmProperty != null) {
                sb.Append("Name:");
                sb.Append(this.EdmProperty.Name);
                sb.Append("   ");
            }
            Console.WriteLine(sb.ToString());
            foreach (StorageComplexTypeMapping typeMapping in TypeMappings)
            {
                typeMapping.Print(index + 5);
            }

        }
        #endregion
    }
}

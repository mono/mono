//---------------------------------------------------------------------
// <copyright file="StorageMappingFragment.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Mapping {
    /// <summary>
    /// Represents the metadata for mapping fragment.
    /// A set of mapping fragments makes up the Set mappings( EntitySet, AssociationSet or CompositionSet )
    /// Each MappingFragment provides mapping for those properties of a type that map to a single table.
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
    /// This class represents the metadata for all the mapping fragment elements in the 
    /// above example. Users can access all the top level constructs of 
    /// MappingFragment element like EntityKey map, Property Maps, Discriminator
    /// property through this mapping fragment class.
    /// </example>
    internal class StorageMappingFragment {
        #region Constructors
        /// <summary>
        /// Construct a new Mapping Fragment object
        /// </summary>
        /// <param name="tableExtent"></param>
        /// <param name="typeMapping"></param>
        internal StorageMappingFragment(EntitySet tableExtent, StorageTypeMapping typeMapping, bool distinctFlag) {
            Debug.Assert(tableExtent != null, "Table should not be null when constructing a Mapping Fragment");
            Debug.Assert(typeMapping != null, "TypeMapping should not be null when constructing a Mapping Fragment");
            this.m_tableExtent = tableExtent;
            this.m_typeMapping = typeMapping;
            this.m_isSQueryDistinct = distinctFlag;
        }
        #endregion

        #region Fields
        /// <summary>
        /// Table extent from which the properties are mapped under this fragment.
        /// </summary>
        EntitySet m_tableExtent;
        /// <summary>
        /// Type mapping under which this mapping fragment exists.
        /// </summary>
        StorageTypeMapping m_typeMapping;
        /// <summary>
        /// Condition property mappings for this mapping fragment.
        /// </summary>
        Dictionary<EdmProperty, StoragePropertyMapping> m_conditionProperties = new Dictionary<EdmProperty, StoragePropertyMapping>(EqualityComparer<EdmProperty>.Default);
        /// <summary>
        /// All the other properties .
        /// </summary>
        List<StoragePropertyMapping> m_properties = new List<StoragePropertyMapping>();
        /// <summary>
        /// Line Number for MappingFragment element start tag.
        /// </summary>
        int m_startLineNumber;
        /// <summary>
        /// Line position for MappingFragment element start tag.
        /// </summary>
        int m_startLinePosition;
        bool m_isSQueryDistinct;
        #endregion

        #region Properties
        /// <summary>
        /// The table from which the properties are mapped in this fragment
        /// </summary>
        internal EntitySet TableSet {
            get {
                return this.m_tableExtent;
            }
        }

        internal bool IsSQueryDistinct
        {
            get { return m_isSQueryDistinct; }
        }

        /// <summary>
        /// Returns all the property mappings defined in the complex type mapping
        /// including Properties and Condition Properties
        /// </summary>
        internal ReadOnlyCollection<StoragePropertyMapping> AllProperties
        {
            get
            {
                List<StoragePropertyMapping> properties = new List<StoragePropertyMapping>();
                properties.AddRange(m_properties);
                properties.AddRange(m_conditionProperties.Values);
                return properties.AsReadOnly();
            }
        }

        /// <summary>
        /// Returns all the property mappings defined in the complex type mapping
        /// including Properties and Condition Properties
        /// </summary>
        internal ReadOnlyCollection<StoragePropertyMapping> Properties
        {
            get
            {
                return m_properties.AsReadOnly();
            }
        }
        
        /// <summary>
        /// Line Number in MSL file where the Mapping Fragment Element's Start Tag is present.
        /// </summary>
        internal int StartLineNumber {
            get {
                return m_startLineNumber;
            }
            set {
                m_startLineNumber = value;
            }
        }

        /// <summary>
        /// Line Position in MSL file where the Mapping Fragment Element's Start Tag is present.
        /// </summary>
        internal int StartLinePosition {
            get {
                return m_startLinePosition;
            }
            set {
                m_startLinePosition = value;
            }
        }

        /// <summary>
        /// File URI of the MSL file
        /// </summary>
        //This should not be stored on the Fragment. Probably it should go on schema.
        //But this requires some thinking before we can finally decide where it should go.
        internal string SourceLocation {
            get {
                return this.m_typeMapping.SetMapping.EntityContainerMapping.SourceLocation;
            }
        }        
        #endregion

        #region Methods
        /// <summary>
        /// Add a property mapping as a child of this mapping fragment
        /// </summary>
        /// <param name="prop">child property mapping to be added</param>
        internal void AddProperty(StoragePropertyMapping prop)
        {
            this.m_properties.Add(prop);
        }

        /// <summary>
        /// Add a condition property mapping as a child of this complex property mapping
        /// Condition Property Mapping specifies a Condition either on the C side property or S side property.
        /// </summary>
        /// <param name="conditionPropertyMap">The mapping that needs to be added</param>
        internal void AddConditionProperty(StorageConditionPropertyMapping conditionPropertyMap, Action<EdmMember> duplicateMemberConditionError)
        {
            //Same Member can not have more than one Condition with in the 
            //same Mapping Fragment.
            EdmProperty conditionMember = (conditionPropertyMap.EdmProperty != null) ? conditionPropertyMap.EdmProperty : conditionPropertyMap.ColumnProperty;
            Debug.Assert(conditionMember != null);
            if (!m_conditionProperties.ContainsKey(conditionMember))
            {
                m_conditionProperties.Add(conditionMember, conditionPropertyMap);
            }
            else
            {
                duplicateMemberConditionError(conditionMember);
            }
        }

        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        /// <param name="index"></param>
        internal virtual void Print(int index) {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("MappingFragment");
            sb.Append("   ");
            sb.Append("Table Name:");
            sb.Append(this.m_tableExtent.Name);

            Console.WriteLine(sb.ToString());
            foreach (StorageConditionPropertyMapping conditionMap in m_conditionProperties.Values)
                (conditionMap).Print(index + 5);
            foreach (StoragePropertyMapping propertyMapping in m_properties)
            {
                propertyMapping.Print(index + 5);
            }
        }
        #endregion
    }
}

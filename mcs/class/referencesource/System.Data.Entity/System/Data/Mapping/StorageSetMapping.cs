//---------------------------------------------------------------------
// <copyright file="StorageSetMapping.cs" company="Microsoft">
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
using System.Data.Common.Utils;
using System.Diagnostics;
using System.Linq;

namespace System.Data.Mapping {

    using Triple = Pair<EntitySetBase, Pair<EntityTypeBase, bool>>;

    /// <summary>
    /// Represents the Mapping metadata for an Extent in CS space.
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
    /// This class represents the metadata for all the extent map elements in the 
    /// above example namely EntitySetMapping, AssociationSetMapping and CompositionSetMapping.
    /// The SetMapping elements that are children of the EntityContainerMapping element
    /// can be accessed through the properties on this type.
    /// </example>
    internal abstract class StorageSetMapping
    {
        #region Constructors
        /// <summary>
        /// Construct the new StorageSetMapping object.
        /// </summary>
        /// <param name="extent">Extent metadata object</param>
        /// <param name="entityContainerMapping">The EntityContainer mapping that contains this extent mapping</param>
        internal StorageSetMapping(EntitySetBase extent, StorageEntityContainerMapping entityContainerMapping) {
            this.m_entityContainerMapping = entityContainerMapping;
            this.m_extent = extent;
            this.m_typeMappings = new List<StorageTypeMapping>();
        }
        #endregion

        #region Fields
        /// <summary>
        /// The EntityContainer mapping that contains this extent mapping.
        /// </summary>
        private StorageEntityContainerMapping m_entityContainerMapping;
        /// <summary>
        /// The extent for which this mapping represents.
        /// </summary>
        private EntitySetBase m_extent;
        /// <summary>
        /// Set of type mappings that make up the Set Mapping.
        /// Unless this is a EntitySetMapping with inheritance,
        /// you would have a single type mapping per set.
        /// </summary>
        private List<StorageTypeMapping> m_typeMappings;
        /// <summary>
        /// User defined Query View for the EntitySet.
        /// </summary>
        private string m_queryView;
        /// <summary>
        /// Line Number for Set Mapping element start tag.
        /// </summary>
        private int m_startLineNumber;
        /// <summary>
        /// Line position for Set Mapping element start tag.
        /// </summary>
        private int m_startLinePosition;
        /// <summary>
        /// Has modificationfunctionmapping for set mapping.
        /// </summary>
        private bool m_hasModificationFunctionMapping;
        /// <summary>
        /// Stores type-Specific user-defined QueryViews.
        /// </summary>
        private Dictionary<Triple, string> m_typeSpecificQueryViews = new Dictionary<Triple, string>(Triple.PairComparer.Instance);

        #endregion

        #region Properties
        /// <summary>
        /// The set for which this mapping is for
        /// </summary>
        internal EntitySetBase Set
        {
            get {
                return this.m_extent;
            }
        }

        ///// <summary>
        ///// TypeMappings that make up this set type.
        ///// For AssociationSet and CompositionSet there will be one type (at least that's what
        ///// we expect as of now). EntitySet could have mappings for multiple Entity types.
        ///// </summary>
        internal ReadOnlyCollection<StorageTypeMapping> TypeMappings
        {
            get
            {
                return this.m_typeMappings.AsReadOnly();
            }
        }

        internal StorageEntityContainerMapping EntityContainerMapping 
        {
            get 
            { 
                return m_entityContainerMapping; 
            }
        }

        /// <summary>
        /// Whether the SetMapping has empty content
        /// Returns true if there no table Mapping fragments
        /// </summary>
        internal virtual bool HasNoContent 
        {
            get
            {
                if (QueryView != null)
                {
                    return false;
                }
                foreach (StorageTypeMapping typeMap in TypeMappings)
                {
                    foreach (StorageMappingFragment mapFragment in typeMap.MappingFragments)
                    {
                        foreach (StoragePropertyMapping propertyMap in mapFragment.AllProperties)
                        {
                            return false;
                        }

                    }
                }
                return true;
            }
        }

        internal string QueryView
        {
            get { return m_queryView; }
            set { m_queryView = value; }
        }

        /// <summary>
        /// Line Number in MSL file where the Set Mapping Element's Start Tag is present.
        /// </summary>
        internal int StartLineNumber
        {
            get
            {
                return m_startLineNumber;
            }
            set
            {
                m_startLineNumber = value;
            }
        }

        /// <summary>
        /// Line Position in MSL file where the Set Mapping Element's Start Tag is present.
        /// </summary>
        internal int StartLinePosition
        {
            get
            {
                return m_startLinePosition;
            }
            set
            {
                m_startLinePosition = value;
            }
        }

        internal bool HasModificationFunctionMapping
        {
            get
            {
                return m_hasModificationFunctionMapping;
            }
            set
            {
                m_hasModificationFunctionMapping = value;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Add type mapping as a child under this SetMapping
        /// </summary>
        /// <param name="typeMapping"></param>
        internal void AddTypeMapping(StorageTypeMapping typeMapping)
        {
            this.m_typeMappings.Add(typeMapping);
        }

        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        internal abstract void Print(int index);


        internal bool ContainsTypeSpecificQueryView(Triple key)
        {
            return m_typeSpecificQueryViews.ContainsKey(key);
        }

        /// <summary>
        /// Stores a type-specific user-defiend QueryView so that it can be loaded
        /// into StorageMappingItemCollection's view cache.
        /// </summary>
        internal void AddTypeSpecificQueryView(Triple key, string viewString)
        {
            Debug.Assert(!m_typeSpecificQueryViews.ContainsKey(key), "Query View already present for the given Key");
            m_typeSpecificQueryViews.Add(key, viewString);
        }

        internal ReadOnlyCollection<Triple> GetTypeSpecificQVKeys()
        {
            return new ReadOnlyCollection<Triple>(m_typeSpecificQueryViews.Keys.ToList());
        }

        internal string GetTypeSpecificQueryView(Triple key)
        {
            Debug.Assert(m_typeSpecificQueryViews.ContainsKey(key));
            return m_typeSpecificQueryViews[key];
        }

        #endregion
    }
}

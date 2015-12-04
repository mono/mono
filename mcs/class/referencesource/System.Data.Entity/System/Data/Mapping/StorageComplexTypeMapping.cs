//---------------------------------------------------------------------
// <copyright file="StorageComplexTypeMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....], [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Data.Metadata.Edm;
using System.Diagnostics;

namespace System.Data.Mapping {
    /// <summary>
    /// Mapping metadata for Complex Types.
    /// </summary>
    internal class StorageComplexTypeMapping {
        #region Constructors
        /// <summary>
        /// Construct a new Complex Property mapping object
        /// </summary>
        /// <param name="isPartial">Whether the property mapping representation is 
        ///                         totally represented in this table mapping fragment or not. </param>
        internal StorageComplexTypeMapping(bool isPartial) {
            m_isPartial = isPartial;
        }
        #endregion

        #region Fields
        Dictionary<string, StoragePropertyMapping> m_properties = new Dictionary<string, StoragePropertyMapping>(StringComparer.Ordinal);  //child property mappings that make up this complex property
        Dictionary<EdmProperty, StoragePropertyMapping> m_conditionProperties = new Dictionary<EdmProperty, StoragePropertyMapping>(EqualityComparer<EdmProperty>.Default);  //Condition property mappings for this complex type
        bool m_isPartial;  //Whether the property mapping representation is 
        //totally represented in this table mapping fragment or not.
        private Dictionary<string, ComplexType> m_types = new Dictionary<string, ComplexType>(StringComparer.Ordinal);  //Types for which the mapping holds true for.
        private Dictionary<string, ComplexType> m_isOfTypes = new Dictionary<string, ComplexType>(StringComparer.Ordinal);  //Types for which the mapping holds true for
        // not only the type specified but the sub-types of that type as well.        
        #endregion

        #region Properties
        /// <summary>
        /// a list of TypeMetadata that this mapping holds true for.
        /// </summary>
        internal ReadOnlyCollection<ComplexType> Types {
            get {
                return new List<ComplexType>(m_types.Values).AsReadOnly();
            }
        }

        /// <summary>
        /// a list of TypeMetadatas for which the mapping holds true for
        /// not only the type specified but the sub-types of that type as well.        
        /// </summary>
        internal ReadOnlyCollection<ComplexType> IsOfTypes {
            get {
                return new List<ComplexType>(m_isOfTypes.Values).AsReadOnly();
            }
        }

        /// <summary>
        /// List of child properties that make up this complex property
        /// </summary>
        internal ReadOnlyCollection<StoragePropertyMapping> Properties {
            get {
                return new List<StoragePropertyMapping>(m_properties.Values).AsReadOnly();
            }
        }

        /// <summary>
        /// Returns all the property mappings defined in the complex type mapping
        /// including Properties and Condition Properties
        /// </summary>
        internal ReadOnlyCollection<StoragePropertyMapping> AllProperties {
            get {
                List<StoragePropertyMapping> properties = new List<StoragePropertyMapping>();
                properties.AddRange(m_properties.Values);
                properties.AddRange(m_conditionProperties.Values);
                return properties.AsReadOnly();
            }
        }

        ///// <summary>
        ///// Whether the property mapping representation is 
        ///// totally represented in this table mapping fragment or not.
        ///// </summary>
        //internal bool IsPartial {
        //    get {
        //        return m_isPartial;
        //    }
        //}
        #endregion

        #region Methods
        /// <summary>
        /// Add a Type to the list of types that this mapping is valid for
        /// </summary>
        internal void AddType(ComplexType type) {
            m_types.Add(type.FullName, type);
        }

        /// <summary>
        /// Add a Type to the list of Is-Of types that this mapping is valid for
        /// </summary>
        internal void AddIsOfType(ComplexType type) {
            m_isOfTypes.Add(type.FullName, type);
        }

        /// <summary>
        /// Add a property mapping as a child of this complex property mapping
        /// </summary>
        /// <param name="prop">The mapping that needs to be added</param>
        internal void AddProperty(StoragePropertyMapping prop) {
            m_properties.Add(prop.EdmProperty.Name, prop);
        }

        /// <summary>
        /// Add a condition property mapping as a child of this complex property mapping
        /// Condition Property Mapping specifies a Condition either on the C side property or S side property.
        /// </summary>
        /// <param name="conditionPropertyMap">The Condition Property mapping that needs to be added</param>
        internal void AddConditionProperty(StorageConditionPropertyMapping conditionPropertyMap, Action<EdmMember> duplicateMemberConditionError)
        {
            //Same Member can not have more than one Condition with in the 
            //same Complex Type.
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
        /// The method finds the type in which the member with the given name exists
        /// form the list of IsOfTypes and Type.
        /// </summary>
        /// <param name="memberName"></param>
        internal ComplexType GetOwnerType(string memberName) {
            foreach (ComplexType type in m_types.Values) {                
                EdmMember tempMember;
                if ((type.Members.TryGetValue(memberName, false, out tempMember)) && (tempMember is EdmProperty))
                {
                    return type;
                }
            }

            foreach (ComplexType type in m_isOfTypes.Values)
            {
                EdmMember tempMember;
                if ((type.Members.TryGetValue(memberName, false, out tempMember)) && (tempMember is EdmProperty))
                {
                    return type;
                }
            }
            return null;
        }

        /// <summary>
        /// This method is primarily for debugging purposes.
        /// Will be removed shortly.
        /// </summary>
        /// <param name="index"></param>
        internal void Print(int index) {
            StorageEntityContainerMapping.GetPrettyPrintString(ref index);
            StringBuilder sb = new StringBuilder();
            sb.Append("ComplexTypeMapping");
            sb.Append("   ");
            if (m_isPartial) {
                sb.Append("IsPartial:True");
            }
            sb.Append("   ");
            foreach (ComplexType type in m_types.Values) {
                sb.Append("Types:");
                sb.Append(type.FullName);
                sb.Append("   ");
            }
            foreach (ComplexType type in m_isOfTypes.Values) {
                sb.Append("Is-Of Types:");
                sb.Append(type.FullName);
                sb.Append("   ");
            }
            Console.WriteLine(sb.ToString());
            foreach (StorageConditionPropertyMapping conditionMap in m_conditionProperties.Values)
                (conditionMap).Print(index + 5);
            foreach (StoragePropertyMapping propertyMapping in Properties) {
                propertyMapping.Print(index + 5);
            }
        }
        #endregion
    }
}

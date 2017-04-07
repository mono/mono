//---------------------------------------------------------------------
// <copyright file="ObjectTypeMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Mapping {

    using System;
    using System.Collections.Generic;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Represents the metadata for OCObjectMapping.
    /// </summary>
    internal class ObjectTypeMapping : Map {
        #region Constructors
        /// <summary>
        /// Construct a new ObjectTypeMapping object
        /// </summary>
        /// <param name="clrType"></param>
        /// <param name="cdmType"></param>
        internal ObjectTypeMapping(EdmType clrType, EdmType cdmType) {
            Debug.Assert(clrType.BuiltInTypeKind == cdmType.BuiltInTypeKind, "BuiltInTypeKind must be the same for both types");
            this.m_clrType = clrType;
            this.m_cdmType = cdmType;
            identity = clrType.Identity + ObjectMslConstructs.IdentitySeperator + cdmType.Identity;

            if (Helper.IsStructuralType(cdmType))
            {
                m_memberMapping = new Dictionary<string, ObjectMemberMapping>(((StructuralType)cdmType).Members.Count);
            }
            else
            {
                m_memberMapping = EmptyMemberMapping;
            }
        }
        #endregion

        #region Fields
        #region Internal
        private readonly EdmType m_clrType;  //type on the Clr side that is being mapped
        private readonly EdmType m_cdmType;  //type on the Cdm side that is being mapped
        private readonly string identity;
        private readonly Dictionary<string, ObjectMemberMapping> m_memberMapping; //Indexes into the member mappings collection based on clr member name

        private static readonly Dictionary<string, ObjectMemberMapping> EmptyMemberMapping
            = new Dictionary<string, ObjectMemberMapping>(0);
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// Gets the type kind for this item
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind {
            get { return BuiltInTypeKind.MetadataItem; }
        }

        /// <summary>
        /// The reference to the Clr type in Metadata
        /// that participates in this mapping instance
        /// </summary>
        internal EdmType ClrType {
            get {
                return this.m_clrType;
            }
        }

        /// <summary>
        /// The reference to the Cdm type in Metadata
        /// that participates in this mapping instance
        /// </summary>
        internal override MetadataItem EdmItem {
            get {
                return this.EdmType;
            }
        }

        /// <summary>
        /// The reference to the Cdm type in Metadata
        /// that participates in this mapping instance
        /// </summary>
        internal EdmType EdmType {
            get {
                return this.m_cdmType;
            }
        }

        /// <summary>
        /// Returns the Identity of ObjectTypeMapping.
        /// The identity for an Object Type Map is the concatenation of
        /// CLR Type Idntity + ':' + CDM Type Identity
        /// </summary>
        internal override string Identity {
            get {
                return identity;
            }
        }

        #endregion

        #region Methods
        /// <summary>
        /// get a MemberMap for the member name specified
        /// </summary>
        /// <param name="cdmPropertyName">the name of the CDM member for which map needs to be retrieved</param>
        internal ObjectPropertyMapping GetPropertyMap(String propertyName)
        {
            ObjectMemberMapping memberMapping = GetMemberMap(propertyName, false /*ignoreCase*/);

            if (memberMapping != null &&
                memberMapping.MemberMappingKind == MemberMappingKind.ScalarPropertyMapping ||
                memberMapping.MemberMappingKind == MemberMappingKind.ComplexPropertyMapping)
            {
                return (ObjectPropertyMapping)memberMapping;
            }

            return null;
        }

        /// <summary>
        /// Add a member mapping as a child of this object mapping
        /// </summary>
        /// <param name="property">child property mapping to be added</param>
        internal void AddMemberMap(ObjectMemberMapping memberMapping) {
            Debug.Assert(memberMapping.ClrMember.Name == memberMapping.EdmMember.Name,
                "Both clrmember and edmMember name must be the same");
            //Check to see if either the Clr member or the Cdm member specified in this 
            //type has already been mapped.
            Debug.Assert(!m_memberMapping.ContainsKey(memberMapping.EdmMember.Name));
            Debug.Assert(!Type.ReferenceEquals(m_memberMapping, EmptyMemberMapping),
                "Make sure you don't add anything to the static emtpy member mapping");
            m_memberMapping.Add(memberMapping.EdmMember.Name, memberMapping);
        }

        /// <summary>
        /// Returns the member map for the given clr member
        /// </summary>
        /// <param name="clrPropertyName"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        internal ObjectMemberMapping GetMemberMapForClrMember(string clrMemberName, bool ignoreCase)
        {
            return GetMemberMap(clrMemberName, ignoreCase);
        }

        /// <summary>
        /// returns the member mapping for the given member
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="ignoreCase"></param>
        /// <returns></returns>
        private ObjectMemberMapping GetMemberMap(string propertyName, bool ignoreCase)
        {
            EntityUtil.CheckStringArgument(propertyName, "propertyName");
            ObjectMemberMapping memberMapping = null;

            if (!ignoreCase)
            {
                //First get the index of the member map from the clr indexs
                m_memberMapping.TryGetValue(propertyName, out memberMapping);
            }
            else
            {
                foreach (KeyValuePair<string, ObjectMemberMapping> keyValuePair in m_memberMapping)
                {
                    if (keyValuePair.Key.Equals(propertyName, StringComparison.OrdinalIgnoreCase))
                    {
                        if (memberMapping != null)
                        {
                            throw new MappingException(System.Data.Entity.Strings.Mapping_Duplicate_PropertyMap_CaseInsensitive(
                                propertyName));
                        }
                        memberMapping = keyValuePair.Value;
                    }
                }
            }

            return memberMapping;
        }

        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return this.Identity;
        }
        #endregion
    }
}

//---------------------------------------------------------------------
// <copyright file="ObjectMemberMapping.cs" company="Microsoft">
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
    /// Mapping metadata for all OC member maps.
    /// </summary>
    internal abstract class ObjectMemberMapping
    {
        #region Constructors
        /// <summary>
        /// Constrcut a new member mapping metadata object
        /// </summary>
        /// <param name="edmMember"></param>
        /// <param name="clrMember"></param>
        protected ObjectMemberMapping(EdmMember edmMember, EdmMember clrMember)
        {
            System.Diagnostics.Debug.Assert(edmMember.BuiltInTypeKind == clrMember.BuiltInTypeKind, "BuiltInTypeKind must be the same");
            this.m_edmMember = edmMember;
            this.m_clrMember = clrMember;
        }
        #endregion

        #region Fields
        #region Internal
        EdmMember m_edmMember;  //EdmMember metadata representing the Cdm member for which the mapping is specified
        EdmMember m_clrMember;  //EdmMember metadata representing the Clr member for which the mapping is specified
        #endregion
        #endregion

        #region Properties
        /// <summary>
        /// The PropertyMetadata object that represents the Cdm member for which mapping is being specified
        /// </summary>
        internal EdmMember EdmMember
        {
            get
            {
                return this.m_edmMember;
            }
        }

        /// <summary>
        /// The PropertyMetadata object that represents the Clr member for which mapping is being specified
        /// </summary>
        internal EdmMember ClrMember
        {
            get
            {
                return this.m_clrMember;
            }
        }

        /// <summary>
        /// Returns the member mapping kind
        /// </summary>
        internal abstract MemberMappingKind MemberMappingKind { get; }
        #endregion
    }

    /// <summary>
    /// Represents the various kind of member mapping
    /// </summary>
    internal enum MemberMappingKind
    {
        ScalarPropertyMapping = 0,

        NavigationPropertyMapping = 1,

        AssociationEndMapping =2,

        ComplexPropertyMapping = 3,
    }
}

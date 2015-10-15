//---------------------------------------------------------------------
// <copyright file="ObjectAssociationEndMapping.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft

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
    internal class ObjectAssociationEndMapping: ObjectMemberMapping
    {
        #region Constructors
        /// <summary>
        /// Constrcut a new AssociationEnd member mapping metadata object
        /// </summary>
        /// <param name="edmAssociationEnd"></param>
        /// <param name="clrAssociationEnd"></param>
        internal ObjectAssociationEndMapping(AssociationEndMember edmAssociationEnd, AssociationEndMember clrAssociationEnd)
            : base(edmAssociationEnd, clrAssociationEnd)
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// return the member mapping kind
        /// </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get
            {
                return MemberMappingKind.AssociationEndMapping;
            }
        }
        #endregion
    }
}

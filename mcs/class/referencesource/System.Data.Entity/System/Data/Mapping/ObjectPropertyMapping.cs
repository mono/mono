//---------------------------------------------------------------------
// <copyright file="ObjectPropertyMapping.cs" company="Microsoft">
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
    internal class ObjectPropertyMapping: ObjectMemberMapping
    {
        #region Constructors
        /// <summary>
        /// Constrcut a new member mapping metadata object
        /// </summary>
        /// <param name="edmProperty"></param>
        /// <param name="clrProperty"></param>
        internal ObjectPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty) :
            base(edmProperty, clrProperty)
        {
        }
        #endregion

        #region Properties

        /// <summary>
        /// The PropertyMetadata object that represents the Clr member for which mapping is being specified
        /// </summary>
        internal EdmProperty ClrProperty
        {
            get
            {
                return (EdmProperty)this.ClrMember;
            }
        }

        /// <summary>
        /// return the member mapping kind
        /// </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get
            {
                return MemberMappingKind.ScalarPropertyMapping;
            }
        }
        #endregion
    }
}

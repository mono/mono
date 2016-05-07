//---------------------------------------------------------------------
// <copyright file="ObjectComplexPropertyMapping.cs" company="Microsoft">
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
using System.Xml;
using System.Data.Metadata.Edm;

namespace System.Data.Mapping {
    /// <summary>
    /// Mapping metadata for complex member maps.
    /// </summary>
    internal class ObjectComplexPropertyMapping : ObjectPropertyMapping
    {
        #region Constructors
        /// <summary>
        /// Constrcut a new member mapping metadata object
        /// </summary>
        /// <param name="edmProperty"></param>
        /// <param name="clrProperty"></param>
        /// <param name="complexTypeMapping"></param>
        internal ObjectComplexPropertyMapping(EdmProperty edmProperty, EdmProperty clrProperty, ObjectTypeMapping complexTypeMapping)
            : base(edmProperty, clrProperty)
        {
            m_objectTypeMapping = complexTypeMapping;
        }
        #endregion

        #region Fields
        private readonly ObjectTypeMapping m_objectTypeMapping;
        #endregion

        #region Properties
        /// <summary>
        /// return the member mapping kind
        /// </summary>
        internal override MemberMappingKind MemberMappingKind
        {
            get
            {
                return MemberMappingKind.ComplexPropertyMapping;
            }
        }
        #endregion
    }
}

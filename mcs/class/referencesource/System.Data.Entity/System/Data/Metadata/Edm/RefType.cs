//---------------------------------------------------------------------
// <copyright file="RefType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class representing a ref type
    /// </summary>
    public sealed class RefType : EdmType
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing a RefType object with the entity type it references
        /// </summary>
        /// <param name="entityType">The entity type that this ref type references</param>
        /// <exception cref="System.ArgumentNullException">Thrown if entityType argument is null</exception>
        internal RefType(EntityType entityType)
            : base(GetIdentity(EntityUtil.GenericCheckArgumentNull(entityType, "entityType")), 
            EdmConstants.TransientNamespace, entityType.DataSpace)
        {
            _elementType = entityType;
            SetReadOnly();
        }
        #endregion

        #region Fields
        private readonly EntityTypeBase _elementType;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.RefType; } }

        /// <summary>
        /// The entity type that this ref type references
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EntityTypeBase, false)]
        public EntityTypeBase ElementType
        {
            get
            {
                return _elementType;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Constructs the name of the collection type
        /// </summary>
        /// <param name="entityTypeBase">The entity type base that this ref type refers to</param>
        /// <returns>The identity of the resulting ref type</returns>
        private static string GetIdentity(EntityTypeBase entityTypeBase)
        {
            StringBuilder builder = new StringBuilder(50);
            builder.Append("reference[");
            entityTypeBase.BuildIdentity(builder);
            builder.Append("]");
            return builder.ToString();
        }
        #endregion
    }
}

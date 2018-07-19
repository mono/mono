//---------------------------------------------------------------------
// <copyright file="CollectionType.cs" company="Microsoft">
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
    /// Represents the Edm Collection Type
    /// </summary>
    public sealed class CollectionType : EdmType
    {
        #region Constructors
        /// <summary>
        /// The constructor for constructing a CollectionType object with the element type it contains
        /// </summary>
        /// <param name="elementType">The element type that this collection type contains</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument elementType is null</exception>
        internal CollectionType(EdmType elementType)
            : this(TypeUsage.Create(elementType))
        {
            this.DataSpace = elementType.DataSpace;
        }

        /// <summary>
        /// The constructor for constructing a CollectionType object with the element type (as a TypeUsage) it contains
        /// </summary>
        /// <param name="elementType">The element type that this collection type contains</param>
        /// <exception cref="System.ArgumentNullException">Thrown if the argument elementType is null</exception>
        internal CollectionType(TypeUsage elementType)
            : base(GetIdentity(EntityUtil.GenericCheckArgumentNull(elementType, "elementType")), 
                    EdmConstants.TransientNamespace, elementType.EdmType.DataSpace)
        {
            _typeUsage = elementType;
            SetReadOnly();
        }
        #endregion

        #region Fields
        private readonly TypeUsage _typeUsage;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.CollectionType; } }

        /// <summary>
        /// The type of the element that this collection type contains
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.TypeUsage, false)]
        public TypeUsage TypeUsage
        {
            get
            {
                return _typeUsage;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Constructs the name of the collection type
        /// </summary>
        /// <param name="typeUsage">The typeusage for the element type that this collection type refers to</param>
        /// <returns>The identity of the resulting collection type</returns>
        private static string GetIdentity(TypeUsage typeUsage)
        {
            StringBuilder builder = new StringBuilder(50);
            builder.Append("collection[");
            typeUsage.BuildIdentity(builder);
            builder.Append("]");
            return builder.ToString();
        }

        /// <summary>
        /// Override EdmEquals to support value comparison of TypeUsage property
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override bool EdmEquals(MetadataItem item)
        {
            // short-circuit if this and other are reference equivalent
            if (Object.ReferenceEquals(this, item)) { return true; }

            // check type of item
            if (null == item || BuiltInTypeKind.CollectionType != item.BuiltInTypeKind) { return false; }
            CollectionType other = (CollectionType)item;

            // compare type usage
            return this.TypeUsage.EdmEquals(other.TypeUsage);
        }
        #endregion
    }
}

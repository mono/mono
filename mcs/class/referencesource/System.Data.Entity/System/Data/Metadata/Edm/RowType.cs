//---------------------------------------------------------------------
// <copyright file="RowType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Data.Common;
using System.Text;
using System.Data.Objects.ELinq;
using System.Threading;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the Edm Row Type
    /// </summary>
    public sealed class RowType : StructuralType
    {
        private ReadOnlyMetadataCollection<EdmProperty> _properties;
        private readonly InitializerMetadata _initializerMetadata;
    
        #region Constructors
        /// <summary>
        /// Initializes a new instance of RowType class with the given list of members
        /// </summary>
        /// <param name="properties">properties for this row type</param>
        /// <exception cref="System.ArgumentException">Thrown if any individual property in the passed in properties argument is null</exception>
        internal RowType(IEnumerable<EdmProperty> properties)
            : this(properties, null)
        {
        }

        /// <summary>
        /// Initializes a RowType with the given members and initializer metadata 
        /// </summary>
        internal RowType(IEnumerable<EdmProperty> properties, InitializerMetadata initializerMetadata)
            : base(GetRowTypeIdentityFromProperties(CheckProperties(properties), initializerMetadata), EdmConstants.TransientNamespace, (DataSpace)(-1))
        {
            // Initialize the properties. 
            if (null != properties)
            {
                foreach (EdmProperty property in properties)
                {
                    this.AddProperty(property);
                }
            }

            _initializerMetadata = initializerMetadata;

            // Row types are immutable, so now that we're done initializing, set it
            // to be read-only.
            SetReadOnly();
        }


        #endregion

        #region Properties
        /// <summary>
        /// Gets or sets LINQ initializer Metadata for this row type. If there is no associated
        /// initializer type, value is null.
        /// </summary>
        internal InitializerMetadata InitializerMetadata
        {
            get { return _initializerMetadata; }
        }

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.RowType; } }

        /// <summary>
        /// Returns the list of properties for this row type
        /// </summary>
        /// <summary>
        /// Returns just the properties from the collection
        /// of members on this type
        /// </summary>
        public ReadOnlyMetadataCollection<EdmProperty> Properties
        {
            get
            {
                Debug.Assert(IsReadOnly, "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to readonly");
                if (null == _properties)
                {
                    Interlocked.CompareExchange(ref _properties,
                        new FilteredReadOnlyMetadataCollection<EdmProperty, EdmMember>(
                            this.Members, Helper.IsEdmProperty), null);
                }
                return _properties;
            }
        }


        /// <summary>
        /// Adds a property
        /// </summary>
        /// <param name="property">The property to add</param>
        private void AddProperty(EdmProperty property)
        {
            EntityUtil.GenericCheckArgumentNull(property, "property");
            AddMember(property);
        }

        /// <summary>
        /// Validates a EdmMember object to determine if it can be added to this type's 
        /// Members collection. If this method returns without throwing, it is assumed
        /// the member is valid. 
        /// </summary>
        /// <param name="member">The member to validate</param>
        /// <exception cref="System.ArgumentException">Thrown if the member is not a EdmProperty</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(Helper.IsEdmProperty(member), "Only members of type Property may be added to Row types.");
        }

        /// <summary>
        /// Calculates the row type identity that would result from 
        /// a given set of properties.
        /// </summary>
        /// <param name="properties">The properties that determine the row type's structure</param>
        /// <param name="initializerMetadata">Metadata describing materialization of this row type</param>
        /// <returns>A string that identifies the row type</returns>
        private static string GetRowTypeIdentityFromProperties(IEnumerable<EdmProperty> properties, InitializerMetadata initializerMetadata)
        {
            // The row type identity is formed as follows:
            // "rowtype[" + a comma-separated list of property identities + "]"
            StringBuilder identity = new StringBuilder("rowtype[");

            if (null != properties)
            {
                int i = 0;
                // For each property, append the type name and facets.
                foreach (EdmProperty property in properties)
                {
                    if (i > 0)
                    {
                        identity.Append(",");
                    }
                    identity.Append("(");
                    identity.Append(property.Name);
                    identity.Append(",");
                    property.TypeUsage.BuildIdentity(identity);
                    identity.Append(")");
                    i++;
                }
            }
            identity.Append("]");

            if (null != initializerMetadata)
            {
                identity.Append(",").Append(initializerMetadata.Identity);
            }

            return identity.ToString();
        }


        private static IEnumerable<EdmProperty> CheckProperties(IEnumerable<EdmProperty> properties)
        {
            if (null != properties)
            {
                int i = 0;
                foreach (EdmProperty prop in properties)
                {
                    if (prop == null)
                    {
                        throw EntityUtil.CollectionParameterElementIsNull("properties");
                    }
                    i++;
                }

                /*
                if (i < 1)
                {
                    throw EntityUtil.ArgumentOutOfRange("properties");
                }
                 */

            }
            return properties;
        }
        #endregion

        #region Methods
        /// <summary>
        /// EdmEquals override verifying the equivalence of all members and their type usages.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal override bool EdmEquals(MetadataItem item)
        {
            // short-circuit if this and other are reference equivalent
            if (Object.ReferenceEquals(this, item)) { return true; }

            // check type of item
            if (null == item || BuiltInTypeKind.RowType != item.BuiltInTypeKind) { return false; }
            RowType other = (RowType)item;

            // check each row type has the same number of members
            if (this.Members.Count != other.Members.Count) { return false; }

            // verify all members are equivalent
            for (int ordinal = 0; ordinal < this.Members.Count; ordinal++)
            {
                EdmMember thisMember = this.Members[ordinal];
                EdmMember otherMember = other.Members[ordinal];

                // if members are different, return false
                if (!thisMember.EdmEquals(otherMember) ||
                    !thisMember.TypeUsage.EdmEquals(otherMember.TypeUsage))
                {
                    return false;
                }
            }

            return true;
        }
        #endregion
    }
}

//---------------------------------------------------------------------
// <copyright file="AssociationType.cs" company="Microsoft">
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
using System.Threading;
using System.Diagnostics;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the EDM Association Type
    /// </summary>
    public sealed class AssociationType : RelationshipType
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of Association Type with the given name, namespace, version and ends
        /// </summary>
        /// <param name="name">name of the association type</param>
        /// <param name="namespaceName">namespace of the association type</param>
        /// <param name="foreignKey">is this a foreign key (FK) relationship?</param>
        /// <param name="dataSpace">dataSpace in which this AssociationType belongs to</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either the name, namespace or version attributes are null</exception>
        internal AssociationType(string name,
                                 string namespaceName,
                                 bool foreignKey,
                                 DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
            _referentialConstraints = new ReadOnlyMetadataCollection<ReferentialConstraint>(new MetadataCollection<ReferentialConstraint>());
            _isForeignKey = foreignKey;
        }
        #endregion

        #region Fields
        private readonly ReadOnlyMetadataCollection<ReferentialConstraint> _referentialConstraints;
        private FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember> _associationEndMembers;
        private readonly bool _isForeignKey;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.AssociationType; } }

        /// <summary>
        /// Returns the list of ends for this association type
        /// </summary>
        public ReadOnlyMetadataCollection<AssociationEndMember> AssociationEndMembers
        {
            get
            {
                Debug.Assert(IsReadOnly, "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to read-only");
                if (null == _associationEndMembers)
                {
                    Interlocked.CompareExchange(ref _associationEndMembers,
                        new FilteredReadOnlyMetadataCollection<AssociationEndMember, EdmMember>(
                            this.Members, Helper.IsAssociationEndMember), null);
                }
                return _associationEndMembers;
            }
        }

        /// <summary>
        /// Returns the list of constraints for this association type
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.ReferentialConstraint, true)]
        public ReadOnlyMetadataCollection<ReferentialConstraint> ReferentialConstraints
        {
            get
            {
                return _referentialConstraints;
            }
        }

        /// <summary>
        /// Indicates whether this is a foreign key relationship.
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.Boolean, false)]
        public bool IsForeignKey
        {
            get
            {
                return _isForeignKey;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Validates a EdmMember object to determine if it can be added to this type's 
        /// Members collection. If this method returns without throwing, it is assumed
        /// the member is valid. 
        /// </summary>
        /// <param name="member">The member to validate</param>
        /// <exception cref="System.ArgumentException">Thrown if the member is not an AssociationEndMember</exception>
        internal override void ValidateMemberForAdd(EdmMember member)
        {
            Debug.Assert(
                (member is AssociationEndMember),
                "Only members of type AssociationEndMember may be added to Association definitions.");
        }

        /// <summary>
        /// Sets this item to be read-only, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                this.ReferentialConstraints.Source.SetReadOnly();
            }
        }

        /// <summary>
        /// Add the given referential constraint to the collection of referential constraints
        /// </summary>
        /// <param name="referentialConstraint"></param>
        internal void AddReferentialConstraint(ReferentialConstraint referentialConstraint)
        {
            this.ReferentialConstraints.Source.Add(referentialConstraint);
        }
        #endregion
    }
}

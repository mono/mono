//---------------------------------------------------------------------
// <copyright file="AssociationSet.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class for representing an Association set
    /// </summary>
    public sealed class AssociationSet : RelationshipSet
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of AssocationSet with the given name and the association type
        /// </summary>
        /// <param name="name">The name of the Assocation set</param>
        /// <param name="associationType">The association type of the entities that this associationship set type contains</param>
        internal AssociationSet(string name, AssociationType associationType)
            : base(name, null, null, null, associationType)
        {
        }
        #endregion

        #region Fields
        private readonly ReadOnlyMetadataCollection<AssociationSetEnd> _associationSetEnds =
            new ReadOnlyMetadataCollection<AssociationSetEnd>(new MetadataCollection<AssociationSetEnd>());
        #endregion

        #region Properties
        /// <summary>
        /// Returns the association type associated with this association set
        /// </summary>
        public new AssociationType ElementType
        {
            get
            {
                return (AssociationType)base.ElementType;
            }
        }

        /// <summary>
        /// Returns the ends of the association set
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.AssociationSetEnd, true)]
        public ReadOnlyMetadataCollection<AssociationSetEnd> AssociationSetEnds
        {
            get
            {
                return _associationSetEnds;
            }
        }

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.AssociationSet; } }
        #endregion

        #region Methods
        /// <summary>
        /// Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!this.IsReadOnly)
            {
                base.SetReadOnly();
                this.AssociationSetEnds.Source.SetReadOnly();
            }
        }

        /// <summary>
        /// Adds the given end to the collection of ends
        /// </summary>
        /// <param name="associationSetEnd"></param>
        internal void AddAssociationSetEnd(AssociationSetEnd associationSetEnd)
        {
            this.AssociationSetEnds.Source.Add(associationSetEnd);
        }
        #endregion
    }
}

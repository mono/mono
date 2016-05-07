//---------------------------------------------------------------------
// <copyright file="AssociationSetEnd.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Text;


namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Class representing a AssociationSet End
    /// </summary>
    public sealed class AssociationSetEnd : MetadataItem
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of AssocationSetEnd
        /// </summary>
        /// <param name="entitySet">Entity set that this end refers to</param>
        /// <param name="parentSet">The association set which this belongs to</param>
        /// <param name="endMember">The end member of the association set which this is an instance of</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either the role,entitySet, parentSet or endMember arguments are null </exception>
        internal AssociationSetEnd(EntitySet entitySet, AssociationSet parentSet, AssociationEndMember endMember)
        {
            _entitySet = EntityUtil.GenericCheckArgumentNull(entitySet, "entitySet");
            _parentSet = EntityUtil.GenericCheckArgumentNull(parentSet, "parentSet");
            _endMember = EntityUtil.GenericCheckArgumentNull(endMember, "endMember");
        }
        #endregion

        #region Fields
        private readonly EntitySet _entitySet;
        private readonly AssociationSet _parentSet;
        private readonly AssociationEndMember _endMember;
        #endregion

        #region Properties
        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.AssociationSetEnd; } }

        /// <summary>
        /// The parent association set for this AssociationSetEnd.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if the value passed in for the setter is null </exception>
        /// <exception cref="System.InvalidOperationException">Thrown if Setter is called when the AssociationSetEnd instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.AssociationSet, false)]
        public AssociationSet ParentAssociationSet
        {
            get
            {
                return _parentSet;
            }
        }

        /// <summary>
        /// The EndMember which this AssociationSetEnd corresponds to.
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if the value passed in for the setter is null </exception>
        /// <exception cref="System.InvalidOperationException">Thrown if Setter is called when the AssociationSetEnd instance is in ReadOnly state</exception>
        [MetadataProperty(BuiltInTypeKind.AssociationEndMember, false)]
        public AssociationEndMember CorrespondingAssociationEndMember
        {
            get
            {
                return _endMember;
            }
        }

        /// <summary>
        /// Name of the end
        /// </summary>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        public string Name
        {
            get
            {
                return CorrespondingAssociationEndMember.Name;
            }
        }

        /// <summary>
        /// Name of the end role
        /// </summary>
        /// <exception cref="System.ArgumentNullException">Thrown if the value passed in for the setter is null </exception>
        /// <exception cref="System.InvalidOperationException">Thrown if Setter is called when the AssociationSetEnd instance is in ReadOnly state</exception>
        [MetadataProperty(PrimitiveTypeKind.String, false)]
        [Obsolete("This property is going away, please use the Name property instead")]
        public string Role
        {
            get
            {
                return Name;
            }
        }

        /// <summary>
        /// Returns the entity set referred by this end role
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EntitySet, false)]
        public EntitySet EntitySet
        {
            get
            {
                return _entitySet;
            }
        }

        /// <summary>
        /// Gets the identity of this item
        /// </summary>
        internal override string Identity
        {
            get
            {
                return this.Name;
            }
        }
        #endregion

        #region Methods
        /// <summary>
        /// Overriding System.Object.ToString to provide better String representation 
        /// for this type.
        /// </summary>
        public override string ToString()
        {
            return Name;
        }

        /// <summary>
        /// Sets this item to be readonly, once this is set, the item will never be writable again.
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();

                AssociationSet parentAssociationSet = ParentAssociationSet;
                if (parentAssociationSet != null)
                {
                    parentAssociationSet.SetReadOnly();
                }

                AssociationEndMember endMember = CorrespondingAssociationEndMember;
                if (endMember != null)
                {
                    endMember.SetReadOnly();
                }

                EntitySet entitySet = EntitySet;
                if (entitySet != null)
                {
                    entitySet.SetReadOnly();
                }
            }
        }
        #endregion
    }
}

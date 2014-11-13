//---------------------------------------------------------------------
// <copyright file="StructuralType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Common;
using System.Text;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the Structural Type
    /// </summary>
    public abstract class StructuralType : EdmType
    {
        #region Constructors
        /// <summary>
        /// Internal parameterless constructor for bootstrapping edmtypes
        /// </summary>
        internal StructuralType()
        {
            _members = new MemberCollection(this);
            _readOnlyMembers = _members.AsReadOnlyMetadataCollection();
        }

        /// <summary>
        /// Initializes a new instance of Structural Type with the given members
        /// </summary>
        /// <param name="name">name of the structural type</param>
        /// <param name="namespaceName">namespace of the structural type</param>
        /// <param name="version">version of the structural type</param>
        /// <param name="dataSpace">dataSpace in which this edmtype belongs to</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal StructuralType(string name, string namespaceName, DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
            _members = new MemberCollection(this);
            _readOnlyMembers = _members.AsReadOnlyMetadataCollection();
        }
        #endregion

        #region Fields
        private readonly MemberCollection _members;
        private readonly ReadOnlyMetadataCollection<EdmMember> _readOnlyMembers;
        #endregion

        #region Properties

        /// <summary>
        /// Returns the collection of members. 
        /// </summary>
        [MetadataProperty(BuiltInTypeKind.EdmMember, true)]
        public ReadOnlyMetadataCollection<EdmMember> Members
        {
            get
            {
                return _readOnlyMembers;
            }
        }
        #endregion

        #region Methods

        /// <summary>
        /// Get the declared only members of a particular type
        /// </summary>
        internal ReadOnlyMetadataCollection<T> GetDeclaredOnlyMembers<T>() where T : EdmMember
        {
            return _members.GetDeclaredOnlyMembers<T>();
        }

        /// <summary>
        /// Validates the types and sets the readOnly property to true. Once the type is set to readOnly,
        /// it can never be changed. 
        /// </summary>
        internal override void SetReadOnly()
        {
            if (!IsReadOnly)
            {
                base.SetReadOnly();
                this.Members.Source.SetReadOnly();
            }
        }

        /// <summary>
        /// Validates a EdmMember object to determine if it can be added to this type's 
        /// Members collection. If this method returns without throwing, it is assumed
        /// the member is valid.
        /// </summary>
        /// <param name="member">The member to validate</param>
        internal abstract void ValidateMemberForAdd(EdmMember member);

        /// <summary>
        /// Adds a member to this type
        /// </summary>
        /// <param name="member">The member to add</param>
        internal void AddMember(EdmMember member)
        {
            EntityUtil.GenericCheckArgumentNull(member, "member");
            Util.ThrowIfReadOnly(this);
            Debug.Assert(this.DataSpace == member.TypeUsage.EdmType.DataSpace || this.BuiltInTypeKind == BuiltInTypeKind.RowType, "Wrong member type getting added in structural type");

            //Since we set the DataSpace of the RowType to be -1 in the constructor, we need to initialize it
            //as and when we add members to it
            if (BuiltInTypeKind.RowType == this.BuiltInTypeKind)
            {
                // Do this only when you are adding the first member
                if (_members.Count == 0)
                {
                    this.DataSpace = member.TypeUsage.EdmType.DataSpace;
                }
                // We need to build types that span across more than one space. For such row types, we set the 
                // DataSpace to -1
                else if (this.DataSpace != (DataSpace)(-1) && member.TypeUsage.EdmType.DataSpace != this.DataSpace)
                {
                    this.DataSpace = (DataSpace)(-1);
                }
            }
            this._members.Add(member);
        }
        #endregion
    }
}

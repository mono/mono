//---------------------------------------------------------------------
// <copyright file="RelationshipType.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------
using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;
using System.Threading;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents the Relationship type
    /// </summary>
    public abstract class RelationshipType : EntityTypeBase
    {
        private ReadOnlyMetadataCollection<RelationshipEndMember> _relationshipEndMembers;

        #region Constructors
        /// <summary>
        /// Initializes a new instance of relationship type
        /// </summary>
        /// <param name="name">name of the relationship type</param>
        /// <param name="namespaceName">namespace of the relationship type</param>
        /// <param name="version">version of the relationship type</param>
        /// <param name="dataSpace">dataSpace in which this edmtype belongs to</param>
        /// <exception cref="System.ArgumentNullException">Thrown if either name, namespace or version arguments are null</exception>
        internal RelationshipType(string name,
                                  string namespaceName,
                                  DataSpace dataSpace)
            : base(name, namespaceName, dataSpace)
        {
        }
        #endregion

        #region Properties
        /// <summary>
        /// Returns the list of ends for this relationship type
        /// </summary>
        public ReadOnlyMetadataCollection<RelationshipEndMember> RelationshipEndMembers
        {
            get
            {
                Debug.Assert(IsReadOnly, "this is a wrapper around this.Members, don't call it during metadata loading, only call it after the metadata is set to readonly");
                if (null == _relationshipEndMembers)
                {
                    FilteredReadOnlyMetadataCollection<RelationshipEndMember, EdmMember> relationshipEndMembers = new FilteredReadOnlyMetadataCollection<RelationshipEndMember, EdmMember>(
                                this.Members, Helper.IsRelationshipEndMember);
                    Interlocked.CompareExchange(ref _relationshipEndMembers, relationshipEndMembers, null);
                }
                return _relationshipEndMembers;
            }
        }
        #endregion
    }
}

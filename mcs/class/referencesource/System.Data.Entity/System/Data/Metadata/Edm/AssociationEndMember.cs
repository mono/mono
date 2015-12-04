//---------------------------------------------------------------------
// <copyright file="AssociationEndMember.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner       [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Data.Objects.DataClasses;

namespace System.Data.Metadata.Edm
{
    /// <summary>
    /// Represents a end of a Association Type
    /// </summary>
    public sealed class AssociationEndMember : RelationshipEndMember
    {
        #region Constructors
        /// <summary>
        /// Initializes a new instance of AssociationEndMember
        /// </summary>
        /// <param name="name">name of the association end member</param>
        /// <param name="endRefType">Ref type that this end refers to </param>
        /// <param name="multiplicity">multiplicity of the end</param>
        internal AssociationEndMember(string name,
                                    RefType endRefType,
                                    RelationshipMultiplicity multiplicity)
            : base(name, endRefType, multiplicity)
        {
        }
        #endregion

        /// <summary>
        /// Returns the kind of the type
        /// </summary>
        public override BuiltInTypeKind BuiltInTypeKind { get { return BuiltInTypeKind.AssociationEndMember; } }

        private Func<RelationshipManager, RelatedEnd, RelatedEnd> _getRelatedEndMethod = null;

        /// <summary>cached dynamic method to set a CLR property value on a CLR instance</summary> 
        internal Func<RelationshipManager, RelatedEnd, RelatedEnd> GetRelatedEnd
        {
            get { return _getRelatedEndMethod; }
            set
            {
                System.Diagnostics.Debug.Assert(null != value, "clearing GetRelatedEndMethod");
                // It doesn't matter which delegate wins, but only one should be jitted
                Interlocked.CompareExchange(ref _getRelatedEndMethod, value, null);
            }
        }

    }
}

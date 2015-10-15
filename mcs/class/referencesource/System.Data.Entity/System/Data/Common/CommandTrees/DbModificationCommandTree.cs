//---------------------------------------------------------------------
// <copyright file="DbModificationCommandTree.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft, Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;
using System.Data.Common.Utils;
using System.Linq;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Represents a DML operation expressed as a canonical command tree
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbModificationCommandTree : DbCommandTree
    {
        private readonly DbExpressionBinding _target;
        private System.Collections.ObjectModel.ReadOnlyCollection<DbParameterReferenceExpression> _parameters;

        internal DbModificationCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target)
            : base(metadata, dataSpace)
        {
            EntityUtil.CheckArgumentNull(target, "target");

            this._target = target;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the target table for the DML operation.
        /// </summary>
        public DbExpressionBinding Target
        {
            get
            {
                return _target;
            }
        }

        /// <summary>
        /// Returns true if this modification command returns a reader (for instance, to return server generated values)
        /// </summary>
        internal abstract bool HasReader
        {
            get;
        }

        internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
        {
            if (this._parameters == null)
            {
                this._parameters = ParameterRetriever.GetParameters(this);
            }
            return this._parameters.Select(p => new KeyValuePair<string, TypeUsage>(p.ParameterName, p.ResultType));
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            if (this.Target != null)
            {
                dumper.Dump(this.Target, "Target");
            }
        }
    }
}

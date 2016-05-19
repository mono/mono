//---------------------------------------------------------------------
// <copyright file="DbUpdateCommandTree.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....], [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;
using System.Data.Common.Utils;
using System.Diagnostics;

using ReadOnlyModificationClauses = System.Collections.ObjectModel.ReadOnlyCollection<System.Data.Common.CommandTrees.DbModificationClause>;  // System.Data.Common.ReadOnlyCollection conflicts

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Represents a single-row update operation expressed as a canonical command tree.
    /// When the <see cref="Returning"/> property is set, the command returns a reader; otherwise,
    /// it returns a scalar indicating the number of rows affected.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbUpdateCommandTree : DbModificationCommandTree
    {
        private readonly DbExpression _predicate;
        private readonly DbExpression _returning;
        private readonly ReadOnlyModificationClauses _setClauses;

        internal DbUpdateCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, DbExpression predicate, ReadOnlyModificationClauses setClauses, DbExpression returning)
            : base(metadata, dataSpace, target)
        {
            EntityUtil.CheckArgumentNull(predicate, "predicate");
            EntityUtil.CheckArgumentNull(setClauses, "setClauses");
            // returning is allowed to be null

            this._predicate = predicate;
            this._setClauses = setClauses;
            this._returning = returning;
        }

        /// <summary>
        /// Gets the list of update set clauses that define the update operation.
        /// </summary>
        public IList<DbModificationClause> SetClauses
        {
            get
            {
                return _setClauses;
            }
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> that specifies a projection of results to be returned based on the modified rows.
        /// If null, indicates no results should be returned from this command.
        /// </summary>
        /// <remarks>
        /// The returning projection includes only the following elements:
        /// <list>
        /// <item>NewInstance expression</item>
        /// <item>Property expression</item>
        /// </list>
        /// </remarks>
        public DbExpression Returning
        {
            get
            {
                return _returning;
            }
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> that specifies the predicate used to determine which members of the target collection should be updated.
        /// </summary>
        /// <remarks>
        /// The predicate includes only the following elements:
        /// <list>
        /// <item>Equality expression</item>
        /// <item>Constant expression</item>
        /// <item>IsNull expression</item>
        /// <item>Property expression</item>
        /// <item>Reference expression to the target</item>
        /// <item>And expression</item>
        /// <item>Or expression</item>
        /// <item>Not expression</item>
        /// </list>
        /// </remarks>
        public DbExpression Predicate
        {
            get
            {   
                return _predicate;
            }
        }

        internal override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Update; }
        }

        internal override bool HasReader
        {
            get { return null != Returning; }
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            base.DumpStructure(dumper);

            if (this.Predicate != null)
            {
                dumper.Dump(this.Predicate, "Predicate");
            }

            dumper.Begin("SetClauses", null);
            foreach (DbModificationClause clause in this.SetClauses)
            {
                if (null != clause)
                {
                    clause.DumpStructure(dumper);
                }
            }
            dumper.End("SetClauses");

            dumper.Dump(this.Returning, "Returning");
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this);
        }
    }
}

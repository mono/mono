//---------------------------------------------------------------------
// <copyright file="DbDeleteCommandTree.cs" company="Microsoft">
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

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Represents a single row delete operation expressed as a canonical command tree.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbDeleteCommandTree : DbModificationCommandTree
    {
        private readonly DbExpression _predicate;

        internal DbDeleteCommandTree(MetadataWorkspace metadata, DataSpace dataSpace, DbExpressionBinding target, DbExpression predicate)
            : base(metadata, dataSpace, target)
        {
            EntityUtil.CheckArgumentNull(predicate, "predicate");

            this._predicate = predicate;
        }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> that specifies the predicate used to determine which members of the target collection should be deleted.
        /// </summary>
        /// <remarks>
        /// The predicate can include only the following elements:
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
            get { return DbCommandTreeKind.Delete; }
        }

        internal override bool HasReader
        {
            get 
            {
                // a delete command never returns server-gen values, and
                // therefore never returns a reader
                return false; 
            }
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            base.DumpStructure(dumper);

            if (this.Predicate != null)
            {
                dumper.Dump(this.Predicate, "Predicate");
            }
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this); 
        }
    }
}

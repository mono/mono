//---------------------------------------------------------------------
// <copyright file="DbModificationClause.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;

using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;
using System.Data.Common.Utils;
using System.Diagnostics;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Specifies a single clause in an insert or update modification operation, see
    /// <see cref="DbInsertCommandTree.SetClauses"/> and <see cref="DbUpdateCommandTree.SetClauses"/>
    /// </summary>
    /// <remarks>
    /// An abstract base class allows the possibility of patterns other than
    /// Property = Value in future versions, e.g.,
    /// <code>
    /// update Foo
    /// set ComplexTypeColumn.Bar()
    /// where Id = 2
    /// </code>
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbModificationClause
    {
        internal DbModificationClause()
        {
        }

        // Effects: describes the contents of this clause using the given dumper
        internal abstract void DumpStructure(ExpressionDumper dumper);

        // Effects: produces a tree node describing this clause, recursively producing nodes
        // for child expressions using the given expression visitor
        internal abstract TreeNode Print(DbExpressionVisitor<TreeNode> visitor);
    }
}

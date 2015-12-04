//---------------------------------------------------------------------
// <copyright file="GroupPartitionExpr.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents GROUPPARTITION(expr) expression.
    /// </summary>
    internal sealed class GroupPartitionExpr : GroupAggregateExpr
    {
        private readonly Node _argExpr;

        /// <summary>
        /// Initializes GROUPPARTITION expression node.
        /// </summary>
        internal GroupPartitionExpr(DistinctKind distinctKind, Node refArgExpr)
            : base(distinctKind)
        {
            _argExpr = refArgExpr;
        }

        /// <summary>
        /// Return GROUPPARTITION argument expression.
        /// </summary>
        internal Node ArgExpr
        {
            get { return _argExpr; }
        }
    }
}

//---------------------------------------------------------------------
// <copyright file="AliasedExpr.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// AST node for an aliased expression.
    /// </summary>
    internal sealed class AliasedExpr : Node
    {
        private readonly Node _expr;
        private readonly Identifier _alias;

        /// <summary>
        /// Constructs an aliased expression node.
        /// </summary>
        internal AliasedExpr(Node expr, Identifier alias)
        {
            Debug.Assert(expr != null, "expr != null");
            Debug.Assert(alias != null, "alias != null");

            if (String.IsNullOrEmpty(alias.Name))
            {
                throw EntityUtil.EntitySqlError(alias.ErrCtx, System.Data.Entity.Strings.InvalidEmptyIdentifier);
            }

            _expr = expr;
            _alias = alias;
        }

        /// <summary>
        /// Constructs an aliased expression node with null alias.
        /// </summary>
        internal AliasedExpr(Node expr)
        {
            Debug.Assert(expr != null, "expr != null");

            _expr = expr;
        }

        internal Node Expr
        {
            get { return _expr; }
        }

        /// <summary>
        /// Returns expression alias identifier, or null if not aliased.
        /// </summary>
        internal Identifier Alias
        {
            get { return _alias; }
        }
    }
}

//---------------------------------------------------------------------
// <copyright file="ParenExpr.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;
    using System.Diagnostics;

    /// <summary>
    /// Represents a paren expression ast node.
    /// </summary>
    internal sealed class ParenExpr : Node
    {
        private readonly AST.Node _expr;

        /// <summary>
        /// Initializes paren expression.
        /// </summary>
        internal ParenExpr(AST.Node expr)
        {
            Debug.Assert(expr != null, "expr != null");
            _expr = expr;
        }

        /// <summary>
        /// Returns the parenthesized expression.
        /// </summary>
        internal AST.Node Expr
        {
            get { return _expr; }
        }
    }
}

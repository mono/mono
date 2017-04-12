//---------------------------------------------------------------------
// <copyright file="ConstructorExpr.cs" company="Microsoft">
//     Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;
    using System.Globalization;
    using System.Collections;
    using System.Collections.Generic;

    /// <summary>
    /// Represents Row contructor expression.
    /// </summary>
    internal sealed class RowConstructorExpr : Node
    {
        private readonly NodeList<AliasedExpr> _exprList;

        internal RowConstructorExpr(NodeList<AliasedExpr> exprList)
        {
            _exprList = exprList;
        }

        /// <summary>
        /// Returns list of elements as aliased expressions.
        /// </summary>
        internal NodeList<AliasedExpr> AliasedExprList
        {
            get { return _exprList; }
        }
    }

    /// <summary>
    /// Represents multiset constructor expression.
    /// </summary>
    internal sealed class MultisetConstructorExpr : Node
    {
        private readonly NodeList<Node> _exprList;

        internal MultisetConstructorExpr(NodeList<Node> exprList)
        {
            _exprList = exprList;
        }

        /// <summary>
        /// Returns list of elements as alias expressions.
        /// </summary>
        internal NodeList<Node> ExprList
        {
            get { return _exprList; }
        }
    }
}

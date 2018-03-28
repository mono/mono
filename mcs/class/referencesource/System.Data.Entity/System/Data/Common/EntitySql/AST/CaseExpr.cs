//---------------------------------------------------------------------
// <copyright file="CaseExpr.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
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
    /// Represents the Seached Case Expression - CASE WHEN THEN [ELSE] END.
    /// </summary>
    internal sealed class CaseExpr : Node
    {
        private readonly NodeList<WhenThenExpr> _whenThenExpr;
        private readonly Node _elseExpr;

        /// <summary>
        /// Initializes case expression without else sub-expression.
        /// </summary>
        /// <param name="whenThenExpr">whenThen expression list</param>
        internal CaseExpr(NodeList<WhenThenExpr> whenThenExpr)
            : this(whenThenExpr, null)
        {
        }

        /// <summary>
        /// Initializes case expression with else sub-expression.
        /// </summary>
        /// <param name="whenThenExpr">whenThen expression list</param>
        /// <param name="elseExpr">else expression</param>
        internal CaseExpr(NodeList<WhenThenExpr> whenThenExpr, Node elseExpr)
        {
            _whenThenExpr = whenThenExpr;
            _elseExpr = elseExpr;
        }

        /// <summary>
        /// Returns the list of WhenThen expressions.
        /// </summary>
        internal NodeList<WhenThenExpr> WhenThenExprList
        {
            get { return _whenThenExpr; }
        }

        /// <summary>
        /// Returns the optional Else expression.
        /// </summary>
        internal Node ElseExpr
        {
            get { return _elseExpr; }
        }
    }

    /// <summary>
    /// Represents the when then sub expression.
    /// </summary>
    internal class WhenThenExpr : Node
    {
        private readonly Node _whenExpr;
        private readonly Node _thenExpr;

        /// <summary>
        /// Initializes WhenThen sub-expression.
        /// </summary>
        /// <param name="whenExpr">When expression</param>
        /// <param name="thenExpr">Then expression</param>
        internal WhenThenExpr(Node whenExpr, Node thenExpr)
        {
            _whenExpr = whenExpr;
            _thenExpr = thenExpr;
        }

        /// <summary>
        /// Returns When expression.
        /// </summary>
        internal Node WhenExpr
        {
            get { return _whenExpr; }
        }

        /// <summary>
        /// Returns Then Expression.
        /// </summary>
        internal Node ThenExpr
        {
            get { return _thenExpr; }
        }
    }
}

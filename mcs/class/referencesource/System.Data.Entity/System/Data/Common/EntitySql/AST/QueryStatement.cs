//---------------------------------------------------------------------
// <copyright file="QueryStatement.cs" company="Microsoft">
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
    /// Represents query statement AST. 
    /// </summary>
    internal sealed class QueryStatement : Statement
    {
        private readonly NodeList<FunctionDefinition> _functionDefList;
        private readonly Node _expr;

        /// <summary>
        /// Initializes query statement.
        /// </summary>
        /// <param name="functionDefList">optional function definitions</param>
        /// <param name="statement">query top level expression</param>
        internal QueryStatement(NodeList<FunctionDefinition> functionDefList, Node expr)
        {
            _functionDefList = functionDefList;
            _expr = expr;
        }

        /// <summary>
        /// Returns optional function defintions. May be null.
        /// </summary>
        internal NodeList<FunctionDefinition> FunctionDefList
        {
            get { return _functionDefList; }
        }

        /// <summary>
        /// Returns query top-level expression.
        /// </summary>
        internal Node Expr
        {
            get { return _expr; }
        }
    }
}

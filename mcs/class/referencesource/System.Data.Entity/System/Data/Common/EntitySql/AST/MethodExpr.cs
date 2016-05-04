//---------------------------------------------------------------------
// <copyright file="MethodExpr.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
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
    using System.Data.Common.CommandTrees;
    using System.Diagnostics;

    /// <summary>
    /// Represents invocation expression: expr(...)
    /// </summary>
    internal sealed class MethodExpr : GroupAggregateExpr
    {
        private readonly Node _expr;
        private readonly NodeList<Node> _args;
        private readonly NodeList<RelshipNavigationExpr> _relationships;

        /// <summary>
        /// Initializes method ast node.
        /// </summary>
        internal MethodExpr(Node expr,
                            DistinctKind distinctKind,
                            NodeList<Node> args) : this (expr, distinctKind, args, null)
        { }

        /// <summary>
        /// Intializes a method ast node with relationships.
        /// </summary>
        internal MethodExpr(Node expr,
                            DistinctKind distinctKind,
                            NodeList<Node> args,
                            NodeList<RelshipNavigationExpr> relationships) : base(distinctKind)
        {
            Debug.Assert(expr != null, "expr != null");
            Debug.Assert(args == null || args.Count > 0, "args must be null or a non-empty list");

            _expr = expr;
            _args = args;
            _relationships = relationships;
        }

        /// <summary>
        /// For the following expression: "a.b.c.Foo()", returns "a.b.c.Foo".
        /// </summary>
        internal Node Expr
        {
            get { return _expr; }
        }

        /// <summary>
        /// Argument list.
        /// </summary>
        internal NodeList<Node> Args
        {
            get { return _args; }
        }

        /// <summary>
        /// True if there are associated relationship expressions.
        /// </summary>
        internal bool HasRelationships
        {
            get { return null != _relationships && _relationships.Count > 0; }
        }

        /// <summary>
        /// Optional relationship list.
        /// </summary>
        internal NodeList<RelshipNavigationExpr> Relationships
        {
            get { return _relationships; }
        }
    }
}

//---------------------------------------------------------------------
// <copyright file="BuiltInExpr.cs" company="Microsoft">
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
    using System.Diagnostics;

    /// <summary>
    /// Defines the function class of builtin expressions.
    /// </summary>
    internal enum BuiltInKind
    {
        And,
        Or,
        Not,

        Cast,
        OfType,
        Treat,
        IsOf,

        Union,
        UnionAll,
        Intersect,
        Overlaps,
        AnyElement,
        Element,
        Except,
        Exists,
        Flatten,
        In,
        NotIn,
        Distinct,

        IsNull,
        IsNotNull,

        Like,

        Equal,
        NotEqual,
        LessEqual,
        LessThan,
        GreaterThan,
        GreaterEqual,

        Plus,
        Minus,
        Multiply,
        Divide,
        Modulus,
        UnaryMinus,
        UnaryPlus,

        Between,
        NotBetween
    }

    /// <summary>
    /// Represents a builtin expression ast node.
    /// </summary>
    internal sealed class BuiltInExpr : Node
    {
        private BuiltInExpr(BuiltInKind kind, string name)
        {
            Kind = kind;
            Name = name.ToUpperInvariant();
        }

        internal BuiltInExpr(BuiltInKind kind, string name, Node arg1)
            : this(kind, name)
        {
            ArgCount = 1;
            Arg1 = arg1;
        }

        internal BuiltInExpr(BuiltInKind kind, string name, Node arg1, Node arg2)
            : this(kind, name)
        {
            ArgCount = 2;
            Arg1 = arg1;
            Arg2 = arg2;
        }

        internal BuiltInExpr(BuiltInKind kind, string name, Node arg1, Node arg2, Node arg3)
            : this(kind, name)
        {
            ArgCount = 3;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
        }

        internal BuiltInExpr(BuiltInKind kind, string name, Node arg1, Node arg2, Node arg3, Node arg4)
            : this(kind, name)
        {
            ArgCount = 4;
            Arg1 = arg1;
            Arg2 = arg2;
            Arg3 = arg3;
            Arg4 = arg4;
        }

        internal readonly BuiltInKind Kind;
        internal readonly string Name;

        internal readonly int ArgCount;
        internal readonly Node Arg1;
        internal readonly Node Arg2;
        internal readonly Node Arg3;
        internal readonly Node Arg4;
    }
}

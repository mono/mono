//---------------------------------------------------------------------
// <copyright file="RefExpr.cs" company="Microsoft">
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
    /// Represents REF(expr) expression.
    /// </summary>
    internal sealed class RefExpr : Node
    {
        private readonly Node _argExpr;

        /// <summary>
        /// Initializes REF expression node.
        /// </summary>
        internal RefExpr(Node refArgExpr)
        {
            _argExpr = refArgExpr;
        }

        /// <summary>
        /// Return ref argument expression.
        /// </summary>
        internal Node ArgExpr
        {
            get { return _argExpr; }
        }
    }

    /// <summary>
    /// Represents DEREF(epxr) expression.
    /// </summary>
    internal sealed class DerefExpr : Node
    {
        private Node _argExpr;

        /// <summary>
        /// Initializes DEREF expression node.
        /// </summary>
        internal DerefExpr(Node derefArgExpr)
        {
            _argExpr = derefArgExpr;
        }

        /// <summary>
        /// Ieturns ref argument expression.
        /// </summary>
        internal Node ArgExpr
        {
            get { return _argExpr; }
        }
    }
}

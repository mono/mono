//------------------------------------------------------------------------------
// <copyright file="CreateRefExpr.cs" company="Microsoft">
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
    /// Represents CREATEREF(entitySet, keys) expression.
    /// </summary>
    internal sealed class CreateRefExpr : Node
    {
        private readonly Node _entitySet;
        private readonly Node _keys;
        private readonly Node _typeIdentifier;

        /// <summary>
        /// Initializes CreateRefExpr.
        /// </summary>
        /// <param name="entitySet">expression representing the entity set</param>
        internal CreateRefExpr(Node entitySet, Node keys) : this(entitySet, keys, null)
        { }

        /// <summary>
        /// Initializes CreateRefExpr.
        /// </summary>
        internal CreateRefExpr(Node entitySet, Node keys, Node typeIdentifier)
        {
            _entitySet = entitySet;
            _keys = keys;
            _typeIdentifier = typeIdentifier;
        }

        /// <summary>
        /// Returns the expression for the entity set.
        /// </summary>
        internal Node EntitySet
        {
            get { return _entitySet; }
        }

        /// <summary>
        /// Returns the expression for the keys.
        /// </summary>
        internal Node Keys
        {
            get { return _keys; }
        }

        /// <summary>
        /// Gets optional typeidentifier. May be null.
        /// </summary>
        internal Node TypeIdentifier
        {
            get { return _typeIdentifier; }
        }
    }

    /// <summary>
    /// Represents KEY(expr) expression.
    /// </summary>
    internal class KeyExpr : Node
    {
        private readonly Node _argExpr;

        /// <summary>
        /// Initializes KEY expression.
        /// </summary>
        internal KeyExpr(Node argExpr)
        {
            _argExpr = argExpr;
        }

        /// <summary>
        /// Returns KEY argument expression.
        /// </summary>
        internal Node ArgExpr
        {
            get { return _argExpr; }
        }
    }
}

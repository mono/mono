//---------------------------------------------------------------------
// <copyright file="NavigationExpr.cs" company="Microsoft">
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
    /// Represents a relationship navigation operator NAVIGATE(sourceRefExpr, Relationship-Type-Name [,ToEndName [,FromEndName]]).
    /// Also used in WITH RELATIONSHIP clause as RELATIONSHIP(targetRefExpr, Relationship-Type-Name [,FromEndName [,ToEndName]]).
    /// </summary>
    internal sealed class RelshipNavigationExpr : Node
    {
        private readonly Node _refExpr;
        private readonly Node _relshipTypeName;
        private readonly Identifier _toEndIdentifier;
        private readonly Identifier _fromEndIdentifier;

        /// <summary>
        /// Initializes relationship navigation expression.
        /// </summary>
        internal RelshipNavigationExpr(Node refExpr, Node relshipTypeName, Identifier toEndIdentifier, Identifier fromEndIdentifier)
        {
            _refExpr = refExpr;
            _relshipTypeName = relshipTypeName;
            _toEndIdentifier = toEndIdentifier;
            _fromEndIdentifier = fromEndIdentifier;
        }

        /// <summary>
        /// Entity reference expression.
        /// </summary>
        internal Node RefExpr
        {
            get { return _refExpr; }
        }

        /// <summary>
        /// Relship type name.
        /// </summary>
        internal Node TypeName
        {
            get { return _relshipTypeName; }
        }

        /// <summary>
        /// TO end identifier.
        /// </summary>
        internal Identifier ToEndIdentifier
        {
            get { return _toEndIdentifier; }
        }

        /// <summary>
        /// FROM end identifier.
        /// </summary>
        internal Identifier FromEndIdentifier
        {
            get { return _fromEndIdentifier; }
        }
    }
}

//---------------------------------------------------------------------
// <copyright file="DotExpr.cs" company="Microsoft">
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
    using System.Diagnostics;

    /// <summary>
    /// Represents dotExpr: expr.Identifier
    /// </summary>
    internal sealed class DotExpr : Node
    {
        private readonly Node _leftExpr;
        private readonly Identifier _identifier;
        private bool? _isMultipartIdentifierComputed;
        private string[] _names;

        /// <summary>
        /// initializes 
        /// </summary>
        internal DotExpr(Node leftExpr, Identifier id)
        {
            _leftExpr = leftExpr;
            _identifier = id;
        }

        /// <summary>
        /// For the following expression: "a.b.c.d", Left returns "a.b.c".
        /// </summary>
        internal Node Left
        {
            get { return _leftExpr; }
        }

        /// <summary>
        /// For the following expression: "a.b.c.d", Identifier returns "d".
        /// </summary>
        internal Identifier Identifier
        {
            get { return _identifier; }
        }

        /// <summary>
        /// Returns true if all parts of this expression are identifiers like in "a.b.c",
        /// false for expressions like "FunctionCall().a.b.c".
        /// </summary>
        internal bool IsMultipartIdentifier(out string[] names)
        {
            if (_isMultipartIdentifierComputed.HasValue)
            {
                names = _names;
                return _isMultipartIdentifierComputed.Value;
            }

            _names = null;
            Identifier leftIdenitifier = _leftExpr as Identifier;
            if (leftIdenitifier != null)
            {
                _names = new string[] { leftIdenitifier.Name, _identifier.Name };
            }

            DotExpr leftDotExpr = _leftExpr as DotExpr;
            string[] leftNames;
            if (leftDotExpr != null && leftDotExpr.IsMultipartIdentifier(out leftNames))
            {
                _names = new string[leftNames.Length + 1];
                leftNames.CopyTo(_names, 0);
                _names[_names.Length - 1] = _identifier.Name;
            }

            Debug.Assert(_names == null || _names.Length > 0, "_names must be null or non-empty");

            _isMultipartIdentifierComputed = _names != null;
            names = _names;
            return _isMultipartIdentifierComputed.Value;
        }
    }
}

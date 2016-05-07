//---------------------------------------------------------------------
// <copyright file="FunctionDefinition.cs" company="Microsoft">
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

    /// <summary>
    /// Represents an ast node for an inline function definition.
    /// </summary>
    internal sealed class FunctionDefinition : Node
    {
        private readonly Identifier _name;
        private readonly NodeList<PropDefinition> _paramDefList;
        private readonly Node _body;
        private readonly int _startPosition;
        private readonly int _endPosition;

        /// <summary>
        /// Initializes function definition using the name, the optional argument definitions and the body expression.
        /// </summary>
        internal FunctionDefinition(Identifier name, NodeList<PropDefinition> argDefList, Node body, int startPosition, int endPosition)
        {
            this._name = name;
            this._paramDefList = argDefList;
            this._body = body;
            this._startPosition = startPosition;
            this._endPosition = endPosition;
        }

        /// <summary>
        /// Returns function name.
        /// </summary>
        internal string Name
        {
            get { return this._name.Name; }
        }

        /// <summary>
        /// Returns optional parameter definition list. May be null.
        /// </summary>
        internal NodeList<PropDefinition> Parameters
        {
            get { return this._paramDefList; }
        }

        /// <summary>
        /// Returns function body.
        /// </summary>
        internal Node Body
        {
            get { return this._body; }
        }

        /// <summary>
        /// Returns start position of the function definition in the command text.
        /// </summary>
        internal int StartPosition
        {
            get { return this._startPosition; }
        }

        /// <summary>
        /// Returns end position of the function definition in the command text.
        /// </summary>
        internal int EndPosition
        {
            get { return this._endPosition; }
        }
    }
}

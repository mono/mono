//---------------------------------------------------------------------
// <copyright file="QueryParameter.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql.AST
{
    using System;

    /// <summary>
    /// Represents an ast node for a query parameter.
    /// </summary>
    internal sealed class QueryParameter : Node
    {
        private readonly string _name;

        /// <summary>
        /// Initializes parameter
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.EntityException">Thrown if the parameter name does not conform to the expected format</exception>
        /// </remarks>
        internal QueryParameter(string parameterName, string query, int inputPos)
            : base(query, inputPos)
        {
            _name = parameterName.Substring(1);

            //
            // valid parameter format is: @({LETTER})(_|{LETTER}|{DIGIT})*
            //
            if (_name.StartsWith("_", StringComparison.OrdinalIgnoreCase) || Char.IsDigit(_name, 0))
            {
                throw EntityUtil.EntitySqlError(ErrCtx, System.Data.Entity.Strings.InvalidParameterFormat(_name));
            }
        }

        /// <summary>
        /// Returns parameter parameterName (without @ sign).
        /// </summary>
        internal string Name
        {
            get { return _name; }
        }
    }
}

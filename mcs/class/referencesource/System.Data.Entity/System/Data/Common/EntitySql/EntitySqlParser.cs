//---------------------------------------------------------------------
// <copyright file="EntitySqlParser.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Common.CommandTrees;
    using System.Data.Common.EntitySql;
    using System.Data.Common.Utils;
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// Public Entity SQL Parser class.
    /// </summary>
    public sealed class EntitySqlParser
    {
        private readonly Perspective _perspective;

        /// <summary>
        /// Construct a parser bound to the specified workspace with the specified perspective.
        /// </summary>
        internal EntitySqlParser(Perspective perspective)
        {
            Debug.Assert(null != perspective, "null perspective?");
            _perspective = perspective;                
        }

        /// <summary>
        /// Parse the specified <paramref name="query"/> with the specified <paramref name="parameters"/>.
        /// </summary>
        /// <param name="query">EntitySQL query to be parsed.</param>
        /// <param name="parameters">optional query parameters</param>
        /// <returns><see cref="ParseResult"/> containing <see cref="DbCommandTree"/> and information describing inline function definitions if any.</returns>
        public ParseResult Parse(string query, params DbParameterReferenceExpression[] parameters)
        {
            EntityUtil.CheckArgumentNull(query, "query");
            if (parameters != null)
            {
                IEnumerable<DbParameterReferenceExpression> paramsEnum = parameters;
                EntityUtil.CheckArgumentContainsNull(ref paramsEnum, "parameters");
            }

            var result = CqlQuery.Compile(query, _perspective, null /* parser options - use default */, parameters);
            return result;
        }

        /// <summary>
        /// Parse a specific query with a specific set variables and produce a <see cref="DbLambda"/>.
        /// </summary>
        public DbLambda ParseLambda(string query, params DbVariableReferenceExpression[] variables)
        {
            EntityUtil.CheckArgumentNull(query, "query");
            if (variables != null)
            {
                IEnumerable<DbVariableReferenceExpression> varsEnum = variables;
                EntityUtil.CheckArgumentContainsNull(ref varsEnum, "variables");
            }

            DbLambda result = CqlQuery.CompileQueryCommandLambda(query, _perspective, null /* parser options - use default */, null /* parameters */, variables);

            return result;
        }
    }
}

//------------------------------------------------------------------------------
// <copyright file="EntitySqlQueryCacheKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//------------------------------------------------------------------------------

namespace System.Data.Common.QueryCache
{
    using System;
    using System.Data.Objects;
    using System.Diagnostics;

    /// <summary>
    /// Represents an Entity-SQL-based ObjectQuery Cache key context
    /// </summary>
    internal sealed class EntitySqlQueryCacheKey : QueryCacheKey
    {
        /// <summary>
        /// Aggregate hashcode based the hashcode of the properties of this cache key
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        /// The name of the default container in effect when the Entity-SQL text was parsed
        /// (affects whether or not the text can be successfully parsed)
        /// </summary>
        private string _defaultContainer;

        /// <summary>
        /// Entity Sql statement
        /// </summary>
        private readonly string _eSqlStatement;

        /// <summary>
        /// Parameter collection token
        /// </summary>
        private readonly string _parametersToken;

        /// <summary>
        /// Number of parameters
        /// </summary>
        private readonly int _parameterCount;

        /// <summary>
        /// Concatenated representation of the Include span paths
        /// </summary>
        private readonly string _includePathsToken;

        /// <summary>
        /// The merge option in effect
        /// </summary>
        private readonly MergeOption _mergeOption;

        /// <summary>
        /// Result type affects assembly plan.
        /// </summary>
        private readonly Type _resultType;

        /// <summary>
        /// Creates a new instance of ObjectQueryCacheKey given a entityCommand instance
        /// </summary>
        /// <param name="defaultContainerName">The default container name in effect when parsing the query (may be null)</param>
        /// <param name="eSqlStatement">The Entity-SQL text of the query</param>
        /// <param name="parameterCount">The number of parameters to the query</param>
        /// <param name="parametersToken">A string representation of the parameters to the query (may be null)</param>
        /// <param name="includePathsToken">A string representation of the Include span paths in effect (may be null)</param>
        /// <param name="mergeOption">The merge option in effect. Required for result assembly.</param>
        internal EntitySqlQueryCacheKey(string defaultContainerName,
                                     string eSqlStatement,
                                     int parameterCount,
                                     string parametersToken,
                                     string includePathsToken,
                                     MergeOption mergeOption,
                                     Type resultType)
            : base()
        {
            Debug.Assert(null != eSqlStatement, "eSqlStatement must not be null");

            _defaultContainer = defaultContainerName;
            _eSqlStatement = eSqlStatement;
            _parameterCount = parameterCount;
            _parametersToken = parametersToken;
            _includePathsToken = includePathsToken;
            _mergeOption = mergeOption;
            _resultType = resultType;

            int combinedHash = _eSqlStatement.GetHashCode() ^
                               _mergeOption.GetHashCode();

            if (_parametersToken != null)
            {
                combinedHash ^= _parametersToken.GetHashCode();
            }

            if (_includePathsToken != null)
            {
                combinedHash ^= _includePathsToken.GetHashCode();
            }

            if (_defaultContainer != null)
            {
                combinedHash ^= _defaultContainer.GetHashCode();
            }

            _hashCode = combinedHash;
        }

        /// <summary>
        /// Determines equality of two cache keys based on cache context values
        /// </summary>
        /// <param name="otherObject"></param>
        /// <returns></returns>
        public override bool Equals(object otherObject)
        {
            Debug.Assert(null != otherObject, "otherObject must not be null");
            if (typeof(EntitySqlQueryCacheKey) != otherObject.GetType())
            {
                return false;
            }

            EntitySqlQueryCacheKey otherObjectQueryCacheKey = (EntitySqlQueryCacheKey)otherObject;

            // also use result type...
            return (_parameterCount == otherObjectQueryCacheKey._parameterCount) &&
                   (_mergeOption == otherObjectQueryCacheKey._mergeOption) &&
                    Equals(otherObjectQueryCacheKey._defaultContainer, _defaultContainer) &&
                    Equals(otherObjectQueryCacheKey._eSqlStatement, _eSqlStatement) &&
                    Equals(otherObjectQueryCacheKey._includePathsToken, _includePathsToken) &&
                    Equals(otherObjectQueryCacheKey._parametersToken, _parametersToken) &&
                    Equals(otherObjectQueryCacheKey._resultType, _resultType);
        }

        /// <summary>
        /// Returns the hashcode for this cache key
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Returns a string representation of the state of this cache key
        /// </summary>
        /// <returns>
        /// A string representation that includes query text, parameter information, include path information
        /// and merge option information about this cache key.
        /// </returns>
        public override string ToString()
        {
            return String.Join("|", new string[] { _defaultContainer, _eSqlStatement, _parametersToken, _includePathsToken, Enum.GetName(typeof(MergeOption), _mergeOption) });
        }

    }
}

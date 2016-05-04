//------------------------------------------------------------------------------
// <copyright file="LinqQueryCacheKey.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner venkatja
//------------------------------------------------------------------------------

namespace System.Data.Common.QueryCache
{
    using System;
    using System.Data.Objects;
    using System.Diagnostics;

    /// <summary>
    /// Represents an ELinq-based ObjectQuery Cache key context
    /// </summary>
    internal sealed class LinqQueryCacheKey : QueryCacheKey
    {
        /// <summary>
        /// Aggregate hashcode based the hashcode of the properties of this cache key
        /// </summary>
        private readonly int _hashCode;

        /// <summary>
        /// DbExpression key
        /// </summary>
        private readonly string _expressionKey;

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
        /// Flag indicating if the C# behavior should be used for null comparisons
        /// </summary>
        private readonly bool _useCSharpNullComparisonBehavior;

        /// <summary>
        /// Creates a new instance of LinqQueryCacheKey.
        /// </summary>
        /// <param name="expressionKey">The DbExpression key of the linq query</param>
        /// <param name="parameterCount">The number of parameters to the query</param>
        /// <param name="parametersToken">A string representation of the parameters to the query (may be null)</param>
        /// <param name="includePathsToken">A string representation of the Include span paths in effect (may be null)</param>
        /// <param name="mergeOption">The merge option in effect. Required for result assembly.</param>
        /// <param name="useCSharpNullComparisonBehavior">Flag indicating if the C# behavior should be used for null comparisons</param>
        /// <param name="resultType">The type of each result item - for a given query as a CLR type instance</param>
        internal LinqQueryCacheKey(string expressionKey,
                                   int parameterCount,
                                   string parametersToken,
                                   string includePathsToken,
                                   MergeOption mergeOption,
                                   bool useCSharpNullComparisonBehavior,
                                   Type resultType)
            : base()
        {
            Debug.Assert(null != expressionKey, "expressionKey must not be null");

            _expressionKey = expressionKey;
            _parameterCount = parameterCount;
            _parametersToken = parametersToken;
            _includePathsToken = includePathsToken;
            _mergeOption = mergeOption;
            _resultType = resultType;
            _useCSharpNullComparisonBehavior = useCSharpNullComparisonBehavior;

            int combinedHash = _expressionKey.GetHashCode() ^
                               _mergeOption.GetHashCode();

            if (_parametersToken != null)
            {
                combinedHash ^= _parametersToken.GetHashCode();
            }

            if (_includePathsToken != null)
            {
                combinedHash ^= _includePathsToken.GetHashCode();
            }

            combinedHash ^= _useCSharpNullComparisonBehavior.GetHashCode();

            _hashCode = combinedHash;
        }

        /// <summary>
        /// Determines equality of two cache keys based on cache context values
        /// </summary>
        public override bool Equals(object otherObject)
        {
            Debug.Assert(null != otherObject, "otherObject must not be null");
            if (typeof(LinqQueryCacheKey) != otherObject.GetType())
            {
                return false;
            }

            LinqQueryCacheKey otherObjectQueryCacheKey = (LinqQueryCacheKey)otherObject;

            // also use result type...
            return (_parameterCount == otherObjectQueryCacheKey._parameterCount) &&
                   (_mergeOption == otherObjectQueryCacheKey._mergeOption) &&
                    Equals(otherObjectQueryCacheKey._expressionKey, _expressionKey) &&
                    Equals(otherObjectQueryCacheKey._includePathsToken, _includePathsToken) &&
                    Equals(otherObjectQueryCacheKey._parametersToken, _parametersToken) &&
                    Equals(otherObjectQueryCacheKey._resultType, _resultType) &&
                    Equals(otherObjectQueryCacheKey._useCSharpNullComparisonBehavior, _useCSharpNullComparisonBehavior);
        }

        /// <summary>
        /// Returns the hashcode for this cache key
        /// </summary>
        public override int GetHashCode()
        {
            return _hashCode;
        }

        /// <summary>
        /// Returns a string representation of the state of this cache key
        /// </summary>
        public override string ToString()
        {
            return String.Join("|", new string[] { _expressionKey, _parametersToken, _includePathsToken, Enum.GetName(typeof(MergeOption), _mergeOption), _useCSharpNullComparisonBehavior.ToString() });
        }
    }
}

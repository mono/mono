//---------------------------------------------------------------------
// <copyright file="Aggregates.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Data.Common;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;
using System.Diagnostics;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Aggregates are pseudo-expressions. They look and feel like expressions, but 
    /// are severely restricted in where they can appear - only in the aggregates clause
    /// of a group-by expression.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbAggregate
    {
        private readonly DbExpressionList _args;
        private readonly TypeUsage _type;

        internal DbAggregate(TypeUsage resultType, DbExpressionList arguments)
        {
            Debug.Assert(resultType != null, "DbAggregate.ResultType cannot be null");
            Debug.Assert(arguments != null, "DbAggregate.Arguments cannot be null");
            Debug.Assert(arguments.Count == 1, "DbAggregate requires a single argument");

            this._type = resultType;
            this._args = arguments;
        }

        /// <summary>
        /// Gets the result type of this aggregate
        /// </summary>
        public TypeUsage ResultType
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets the list of expressions that define the arguments to the aggregate.
        /// </summary>
        public IList<DbExpression> Arguments { get { return _args; } }
    }

    /// <summary>
    /// The aggregate type that corresponds to exposing the collection of elements that comprise a group
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbGroupAggregate : DbAggregate
    {
        internal DbGroupAggregate(TypeUsage resultType, DbExpressionList arguments)
            : base(resultType, arguments)
        {
        }
    }

    /// <summary>
    /// The aggregate type that corresponds to the invocation of an aggregate function.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbFunctionAggregate : DbAggregate
    {
        private bool _distinct;
        EdmFunction _aggregateFunction;

        internal DbFunctionAggregate(TypeUsage resultType, DbExpressionList arguments, EdmFunction function, bool isDistinct)
            : base(resultType, arguments)
        {
            Debug.Assert(function != null, "DbFunctionAggregate.Function cannot be null");

            _aggregateFunction = function;
            _distinct = isDistinct;
        }

        /// <summary>
        /// Gets a value indicating whether the aggregate function is applied in a distinct fashion
        /// </summary>
        public bool Distinct { get { return _distinct; } }

        /// <summary>
        /// Gets the method metadata that specifies the aggregate function to invoke.
        /// </summary>
        public EdmFunction Function { get { return _aggregateFunction; } }
    }
}

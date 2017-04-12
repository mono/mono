//---------------------------------------------------------------------
// <copyright file="RelationalExpressions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;

using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Represents an apply operation, which is the invocation of the specified functor for each element in the specified input set.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbApplyExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpressionBinding _apply;

        internal DbApplyExpression(DbExpressionKind applyKind, TypeUsage resultRowCollectionTypeUsage, DbExpressionBinding input, DbExpressionBinding apply)
            : base(applyKind, resultRowCollectionTypeUsage)
        {
            Debug.Assert(input != null, "DbApplyExpression input cannot be null");
            Debug.Assert(input != null, "DbApplyExpression apply cannot be null");
            Debug.Assert(DbExpressionKind.CrossApply == applyKind || DbExpressionKind.OuterApply == applyKind, "Invalid DbExpressionKind for DbApplyExpression");

            _input = input;
            _apply = apply;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the functor that is invoked for each element in the input set.
        /// </summary>
        public DbExpressionBinding Apply { get { return _apply;  } }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input { get { return _input; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the removal of duplicate elements from the specified set operand.
    /// </summary>
    /// <remarks>
    /// DbDistinctExpression requires that its argument has a collection result type
    /// with an element type that is equality comparable.
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbDistinctExpression : DbUnaryExpression
    {
        internal DbDistinctExpression(TypeUsage resultType, DbExpression argument)
            : base(DbExpressionKind.Distinct, resultType, argument)
        {
            Debug.Assert(TypeSemantics.IsCollectionType(argument.ResultType), "DbDistinctExpression argument must have a collection result type");
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the conversion of the specified set operand to a singleton.
    /// If the set is empty the conversion will return null, otherwise the conversion will return one of the elements in the set.
    /// </summary>
    /// <remarks>
    /// DbElementExpression requires that its argument has a collection result type
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbElementExpression : DbUnaryExpression
    {
        private bool _singlePropertyUnwrapped;

        internal DbElementExpression(TypeUsage resultType, DbExpression argument)
            : base(DbExpressionKind.Element, resultType, argument)
        {
            this._singlePropertyUnwrapped = false;
        }

        internal DbElementExpression(TypeUsage resultType, DbExpression argument, bool unwrapSingleProperty)
            : base(DbExpressionKind.Element, resultType, argument)
        {
            this._singlePropertyUnwrapped = unwrapSingleProperty;
        }

        /// <summary>
        /// Is the result type of the element equal to the result type of the single property 
        /// of the element of its operand?
        /// </summary>
        internal bool IsSinglePropertyUnwrapped { get { return _singlePropertyUnwrapped; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the set subtraction operation between the left and right operands.
    /// </summary>
    /// <remarks>
    /// DbExceptExpression requires that its arguments have a common collection result type
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbExceptExpression : DbBinaryExpression
    {
        internal DbExceptExpression(TypeUsage resultType, DbExpression left, DbExpression right)
            : base(DbExpressionKind.Except, resultType, left, right)
        {
            Debug.Assert(object.ReferenceEquals(resultType, left.ResultType), "DbExceptExpression result type should be result type of left argument");
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents a predicate applied to an input set to produce the set of elements that satisfy the predicate.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbFilterExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpression _predicate;

        internal DbFilterExpression(TypeUsage resultType, DbExpressionBinding input, DbExpression predicate)
            : base(DbExpressionKind.Filter, resultType)
        {
            Debug.Assert(input != null, "DbFilterExpression input cannot be null");
            Debug.Assert(predicate != null, "DbBFilterExpression predicate cannot be null");
            Debug.Assert(TypeSemantics.IsPrimitiveType(predicate.ResultType, PrimitiveTypeKind.Boolean), "DbFilterExpression predicate must have a Boolean result type");
            
            _input = input;
            _predicate = predicate;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input { get { return _input; } }
        
        /// <summary>
        /// Gets the <see cref="DbExpression"/> that specifies the predicate used to filter the input set.
        /// </summary>
        public DbExpression Predicate { get { return _predicate; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents a group by operation, which is a grouping of the elements in the input set based on the specified key expressions followed by the application of the specified aggregates.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbGroupByExpression : DbExpression
    {
        private readonly DbGroupExpressionBinding _input;
        private readonly DbExpressionList _keys;
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<DbAggregate> _aggregates;

        internal DbGroupByExpression(TypeUsage collectionOfRowResultType,
                                     DbGroupExpressionBinding input,
                                     DbExpressionList groupKeys,
                                     System.Collections.ObjectModel.ReadOnlyCollection<DbAggregate> aggregates)
            : base(DbExpressionKind.GroupBy, collectionOfRowResultType)
        {
            Debug.Assert(input != null, "DbGroupExpression input cannot be null");
            Debug.Assert(groupKeys != null, "DbGroupExpression keys cannot be null");
            Debug.Assert(aggregates != null, "DbGroupExpression aggregates cannot be null");
            Debug.Assert(groupKeys.Count > 0 || aggregates.Count > 0, "At least one key or aggregate is required");

            _input = input;
            _keys = groupKeys;
            _aggregates = aggregates;
        }

        /// <summary>
        /// Gets the <see cref="DbGroupExpressionBinding"/> that specifies the input set and provides access to the set element and group element variables.
        /// </summary>
        public DbGroupExpressionBinding Input { get { return _input; } }

        /// <summary>
        /// Gets an <see cref="DbExpression"/> list that provides grouping keys.
        /// </summary>
        public IList<DbExpression> Keys { get { return _keys; } }

        /// <summary>
        /// Gets an <see cref="DbAggregate"/> list that provides the aggregates to apply.
        /// </summary>
        public IList<DbAggregate> Aggregates { get { return _aggregates; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the set intersection operation between the left and right operands.
    /// </summary>
    /// <remarks>
    /// DbIntersectExpression requires that its arguments have a common collection result type
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbIntersectExpression : DbBinaryExpression
    {
        internal DbIntersectExpression(TypeUsage resultType, DbExpression left, DbExpression right)
            : base(DbExpressionKind.Intersect, resultType, left, right)
        {
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }
    
    /// <summary>
    /// Represents an unconditional join operation between the given collection arguments
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbCrossJoinExpression : DbExpression
    {
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<DbExpressionBinding> _inputs;

        internal DbCrossJoinExpression(TypeUsage collectionOfRowResultType, System.Collections.ObjectModel.ReadOnlyCollection<DbExpressionBinding> inputs)
            : base(DbExpressionKind.CrossJoin, collectionOfRowResultType)
       {
           Debug.Assert(inputs != null, "DbCrossJoin inputs cannot be null");
           Debug.Assert(inputs.Count >= 2, "DbCrossJoin requires at least two inputs");

           _inputs = inputs;
        }
        
        /// <summary>
        /// Gets an <see cref="DbExpressionBinding"/> list that provide the input sets to the join.
        /// </summary>
        public IList<DbExpressionBinding> Inputs { get { return _inputs; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }
   
    /// <summary>
    /// Represents an inner, left outer or full outer join operation between the given collection arguments on the specified join condition.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbJoinExpression : DbExpression
    {
        private readonly DbExpressionBinding _left;
        private readonly DbExpressionBinding _right;
        private readonly DbExpression _condition;

        internal DbJoinExpression(DbExpressionKind joinKind, TypeUsage collectionOfRowResultType, DbExpressionBinding left, DbExpressionBinding right, DbExpression condition)
            : base(joinKind, collectionOfRowResultType)
        {
            Debug.Assert(left != null, "DbJoinExpression left cannot be null");
            Debug.Assert(right != null, "DbJoinExpression right cannot be null");
            Debug.Assert(condition != null, "DbJoinExpression condition cannot be null");
            Debug.Assert(DbExpressionKind.InnerJoin == joinKind ||
                         DbExpressionKind.LeftOuterJoin == joinKind ||
                         DbExpressionKind.FullOuterJoin == joinKind,
                         "Invalid DbExpressionKind specified for DbJoinExpression");

            _left = left;
            _right = right;
            _condition = condition;
        }
        
        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> provides the left input.
        /// </summary>
        public DbExpressionBinding Left { get { return _left; } }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> provides the right input.
        /// </summary>
        public DbExpressionBinding Right { get { return _right; } }
                
        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the join condition to apply.
        /// </summary>
        public DbExpression JoinCondition { get { return _condition; } }
        
        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the restriction of the number of elements in the Argument collection to the specified Limit value.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbLimitExpression : DbExpression
    {
        private readonly DbExpression _argument;
        private readonly DbExpression _limit;
        private readonly bool _withTies;

        internal DbLimitExpression(TypeUsage resultType, DbExpression argument, DbExpression limit, bool withTies)
            : base(DbExpressionKind.Limit, resultType)
        {
            Debug.Assert(argument != null, "DbLimitExpression argument cannot be null");
            Debug.Assert(limit != null, "DbLimitExpression limit cannot be null");
            Debug.Assert(object.ReferenceEquals(resultType, argument.ResultType), "DbLimitExpression result type must be the result type of the argument");
                        
            this._argument = argument;
            this._limit = limit;
            this._withTies = withTies;
        }

        /// <summary>
        /// Gets the expression that specifies the input collection.
        /// </summary>
        public DbExpression Argument { get { return this._argument; } }
        
        /// <summary>
        /// Gets the expression that specifies the limit on the number of elements returned from the input collection.
        /// </summary>
        public DbExpression Limit { get { return this._limit; } }
        
        /// <summary>
        /// Gets whether the limit operation will include tied results, which could produce more results than specifed by the Limit value if ties are present.
        /// </summary>
        public bool WithTies { get { return _withTies; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents the projection of a given set of values over the specified input set.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbProjectExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpression _projection;

        internal DbProjectExpression(TypeUsage resultType, DbExpressionBinding input, DbExpression projection)
            : base(DbExpressionKind.Project, resultType)
        {
            Debug.Assert(input != null, "DbProjectExpression input cannot be null");
            Debug.Assert(projection != null, "DbProjectExpression projection cannot be null");

            this._input = input;
            this._projection = projection;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input { get { return _input; } }
        
        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the projection.
        /// </summary>
        public DbExpression Projection { get { return _projection; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents a quantifier operation of the specified kind (Any, All) over the elements of the specified input set.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbQuantifierExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly DbExpression _predicate;

        internal DbQuantifierExpression(DbExpressionKind kind, TypeUsage booleanResultType, DbExpressionBinding input, DbExpression predicate)
            : base(kind, booleanResultType)
        {
            Debug.Assert(input != null, "DbQuantifierExpression input cannot be null");
            Debug.Assert(predicate != null, "DbQuantifierExpression predicate cannot be null");
            Debug.Assert(TypeSemantics.IsPrimitiveType(booleanResultType, PrimitiveTypeKind.Boolean), "DbQuantifierExpression must have a Boolean result type");
            Debug.Assert(TypeSemantics.IsPrimitiveType(predicate.ResultType, PrimitiveTypeKind.Boolean), "DbQuantifierExpression predicate must have a Boolean result type");

            this._input = input;
            this._predicate = predicate;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input { get { return _input; } }

        /// <summary>
        /// Gets the Boolean predicate that should be evaluated for each element in the input set.
        /// </summary>
        public DbExpression Predicate { get { return _predicate; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Specifies a sort key that can be used as part of the sort order in a DbSortExpression.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbSortClause
    {
        private readonly DbExpression _expr;
        private readonly bool _asc;
        private readonly string _coll;

        internal DbSortClause(DbExpression key, bool asc, string collation)
        {
            Debug.Assert(key != null, "DbSortClause key cannot be null");
                        
            _expr = key;            
            _asc = asc;
            _coll = collation;
        }

        /// <summary>
        /// Gets a Boolean value indicating whether or not this sort key is sorted ascending.
        /// </summary>
        public bool Ascending { get { return _asc; } }
        
        /// <summary>
        /// Gets a string value that specifies the collation for this sort key.
        /// </summary>
        public string Collation { get { return _coll; } }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that provides the value for this sort key.
        /// </summary>
        public DbExpression Expression { get { return _expr; } }
    }

    /// <summary>
    /// Represents a skip operation of the specified number of elements of the input set after the ordering described in the given sort keys is applied.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbSkipExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<DbSortClause> _keys;
        private readonly DbExpression _count;

        internal DbSkipExpression(TypeUsage resultType, DbExpressionBinding input, System.Collections.ObjectModel.ReadOnlyCollection<DbSortClause> sortOrder, DbExpression count)
            : base(DbExpressionKind.Skip, resultType)
        {
            Debug.Assert(input != null, "DbSkipExpression input cannot be null");
            Debug.Assert(sortOrder != null, "DbSkipExpression sort order cannot be null");
            Debug.Assert(count != null, "DbSkipExpression count cannot be null");
            Debug.Assert(TypeSemantics.IsCollectionType(resultType), "DbSkipExpression requires a collection result type");

            this._input = input;
            this._keys = sortOrder;
            this._count = count;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input { get { return _input; } }

        /// <summary>
        /// Gets a <see cref="DbSortClause"/> list that defines the sort order.
        /// </summary>
        public IList<DbSortClause> SortOrder { get { return _keys; } }

        /// <summary>
        /// Gets the expression that specifies the number of elements from the input collection to skip.
        /// </summary>
        public DbExpression Count
        {
            get { return _count; } 
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }

    /// <summary>
    /// Represents a sort operation applied to the elements of the specified input set based on the given sort keys.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbSortExpression : DbExpression
    {
        private readonly DbExpressionBinding _input;
        private readonly System.Collections.ObjectModel.ReadOnlyCollection<DbSortClause> _keys;

        internal DbSortExpression(TypeUsage resultType, DbExpressionBinding input, System.Collections.ObjectModel.ReadOnlyCollection<DbSortClause> sortOrder)
            : base(DbExpressionKind.Sort, resultType)
        {
            Debug.Assert(input != null, "DbSortExpression input cannot be null");
            Debug.Assert(sortOrder != null, "DbSortExpression sort order cannot be null");
            Debug.Assert(TypeSemantics.IsCollectionType(resultType), "DbSkipExpression requires a collection result type");

            this._input = input;
            this._keys = sortOrder;
        }

        /// <summary>
        /// Gets the <see cref="DbExpressionBinding"/> that specifies the input set.
        /// </summary>
        public DbExpressionBinding Input { get { return _input; } }

        /// <summary>
        /// Gets a <see cref="DbSortClause"/> list that defines the sort order.
        /// </summary>
        public IList<DbSortClause> SortOrder { get { return _keys; } }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }
             
    /// <summary>
    /// Represents the set union (without duplicate removal) operation between the left and right operands.
    /// </summary>
    /// <remarks>
    /// DbUnionAllExpression requires that its arguments have a common collection result type
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbUnionAllExpression : DbBinaryExpression
    {
        internal DbUnionAllExpression(TypeUsage resultType, DbExpression left, DbExpression right)
            : base(DbExpressionKind.UnionAll, resultType, left, right)
        {
        }

        /// <summary>
        /// The visitor pattern method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public override void Accept(DbExpressionVisitor visitor) { if (visitor != null) { visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }

        /// <summary>
        /// The visitor pattern method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public override TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor) { if (visitor != null) { return visitor.Visit(this); } else { throw EntityUtil.ArgumentNull("visitor"); } }
    }     
}

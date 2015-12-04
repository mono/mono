//---------------------------------------------------------------------
// <copyright file="OperatorExpressions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;

namespace System.Data.Common.CommandTrees
{
    #region Boolean Operators
    /// <summary>
    /// Represents the logical And of two Boolean arguments.
    /// </summary>
    /// <remarks>DbAndExpression requires that both of its arguments have a Boolean result type</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbAndExpression : DbBinaryExpression
    {
        internal DbAndExpression(TypeUsage booleanResultType, DbExpression left, DbExpression right)
            : base(DbExpressionKind.And, booleanResultType, left, right)
        {
            Debug.Assert(TypeSemantics.IsPrimitiveType(booleanResultType, PrimitiveTypeKind.Boolean), "DbAndExpression requires a Boolean result type");
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
    /// Represents the logical Or of two Boolean arguments.
    /// </summary>
    /// <remarks>DbOrExpression requires that both of its arguments have a Boolean result type</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbOrExpression : DbBinaryExpression
    {
        internal DbOrExpression(TypeUsage booleanResultType, DbExpression left, DbExpression right)
            : base(DbExpressionKind.Or, booleanResultType, left, right)
        {
            Debug.Assert(TypeSemantics.IsPrimitiveType(booleanResultType, PrimitiveTypeKind.Boolean), "DbOrExpression requires a Boolean result type");
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
    /// Represents the logical Not of a single Boolean argument.
    /// </summary>
    /// <remarks>DbNotExpression requires that its argument has a Boolean result type</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbNotExpression : DbUnaryExpression
    {
        internal DbNotExpression(TypeUsage booleanResultType, DbExpression argument)
            : base(DbExpressionKind.Not, booleanResultType, argument)
        {
            Debug.Assert(TypeSemantics.IsPrimitiveType(booleanResultType, PrimitiveTypeKind.Boolean), "DbNotExpression requires a Boolean result type");
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
    #endregion

    /// <summary>
    /// Represents an arithmetic operation (addition, subtraction, multiplication, division, modulo or negation) applied to two numeric arguments.
    /// </summary>
    /// <remarks>DbArithmeticExpression requires that its arguments have a common numeric result type</remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbArithmeticExpression : DbExpression
    {
        private readonly DbExpressionList _args;

        internal DbArithmeticExpression(DbExpressionKind kind, TypeUsage numericResultType, DbExpressionList args)
            : base(kind, numericResultType)
        {
            Debug.Assert(TypeSemantics.IsNumericType(numericResultType), "DbArithmeticExpression result type must be numeric");
            
            Debug.Assert(
                DbExpressionKind.Divide == kind ||
                DbExpressionKind.Minus == kind ||
                DbExpressionKind.Modulo == kind ||
                DbExpressionKind.Multiply == kind ||
                DbExpressionKind.Plus == kind ||
                DbExpressionKind.UnaryMinus == kind,
                "Invalid DbExpressionKind used in DbArithmeticExpression: " + Enum.GetName(typeof(DbExpressionKind), kind)
            );

            Debug.Assert(args != null, "DbArithmeticExpression arguments cannot be null");

            Debug.Assert(
                (DbExpressionKind.UnaryMinus == kind && 1 == args.Count) ||
                2 == args.Count,
                "Incorrect number of arguments specified to DbArithmeticExpression"
            );

            this._args = args;            
        }

        /// <summary>
        /// Gets the list of expressions that define the current arguments.
        /// </summary>
        /// <remarks>
        ///     The <code>Arguments</code> property returns a fixed-size list of <see cref="DbExpression"/> elements.
        ///     <see cref="DbArithmeticExpression"/> requires that all elements of it's <code>Arguments</code> list
        ///     have a common numeric result type.
        /// </remarks>
        public IList<DbExpression> Arguments { get { return _args; } }

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
    /// Represents a Case When...Then...Else logical operation.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbCaseExpression : DbExpression
    {
        private readonly DbExpressionList _when;
        private readonly DbExpressionList _then;
        private readonly DbExpression _else;

        internal DbCaseExpression(TypeUsage commonResultType, DbExpressionList whens, DbExpressionList thens, DbExpression elseExpr)
            : base(DbExpressionKind.Case, commonResultType)
        {
            Debug.Assert(whens != null, "DbCaseExpression whens cannot be null");
            Debug.Assert(thens != null, "DbCaseExpression thens cannot be null");
            Debug.Assert(elseExpr != null, "DbCaseExpression else cannot be null");
            Debug.Assert(whens.Count == thens.Count, "DbCaseExpression whens count must match thens count");

            this._when = whens;
            this._then = thens;
            this._else = elseExpr;
        }

        /// <summary>
        /// Gets the When clauses of this DbCaseExpression.
        /// </summary>
        public IList<DbExpression> When { get { return _when; } }

        /// <summary>
        /// Gets the Then clauses of this DbCaseExpression.
        /// </summary>
        public IList<DbExpression> Then { get { return _then; } }

        /// <summary>
        /// Gets the Else clause of this DbCaseExpression.
        /// </summary>
        public DbExpression Else { get { return _else; } }

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
    /// Represents a cast operation applied to a polymorphic argument.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbCastExpression : DbUnaryExpression
    {
        internal DbCastExpression(TypeUsage type, DbExpression argument)
            : base(DbExpressionKind.Cast, type, argument)
        {
            Debug.Assert(TypeSemantics.IsCastAllowed(argument.ResultType, type), "DbCastExpression represents an invalid cast");
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
    /// Represents a comparison operation (equality, greater than, greather than or equal, less than, less than or equal, inequality) applied to two arguments.
    /// </summary>
    /// <remarks>
    ///     DbComparisonExpression requires that its arguments have a common result type
    ///     that is equality comparable (for <see cref="DbExpressionKind"/>.Equals and <see cref="DbExpressionKind"/>.NotEquals),
    ///     order comparable (for <see cref="DbExpressionKind"/>.GreaterThan and <see cref="DbExpressionKind"/>.LessThan),
    ///     or both (for <see cref="DbExpressionKind"/>.GreaterThanOrEquals and <see cref="DbExpressionKind"/>.LessThanOrEquals).
    /// </remarks>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbComparisonExpression : DbBinaryExpression
    {
        internal DbComparisonExpression(DbExpressionKind kind, TypeUsage booleanResultType, DbExpression left, DbExpression right)
            : base(kind, booleanResultType, left, right)
        {
            Debug.Assert(left != null, "DbComparisonExpression left cannot be null");
            Debug.Assert(right != null, "DbComparisonExpression right cannot be null");
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbComparisonExpression result type must be a Boolean type");
            Debug.Assert(
                DbExpressionKind.Equals == kind ||
                DbExpressionKind.LessThan == kind ||
                DbExpressionKind.LessThanOrEquals == kind ||
                DbExpressionKind.GreaterThan == kind ||
                DbExpressionKind.GreaterThanOrEquals == kind ||
                DbExpressionKind.NotEquals == kind,
                "Invalid DbExpressionKind used in DbComparisonExpression: " + Enum.GetName(typeof(DbExpressionKind), kind)
            );
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
    /// Represents empty set determination applied to a single set argument.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbIsEmptyExpression : DbUnaryExpression
    {
        internal DbIsEmptyExpression(TypeUsage booleanResultType, DbExpression argument)
            : base(DbExpressionKind.IsEmpty, booleanResultType, argument)
        {
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbIsEmptyExpression requires a Boolean result type");
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
    /// Represents null determination applied to a single argument.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbIsNullExpression : DbUnaryExpression
    {
        internal DbIsNullExpression(TypeUsage booleanResultType, DbExpression arg, bool isRowTypeArgumentAllowed)
            : base(DbExpressionKind.IsNull, booleanResultType, arg)
        {
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbIsNullExpression requires a Boolean result type");
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
    /// Represents the type comparison of a single argument against the specified type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbIsOfExpression : DbUnaryExpression
    {
        private TypeUsage _ofType;

        internal DbIsOfExpression(DbExpressionKind isOfKind, TypeUsage booleanResultType, DbExpression argument, TypeUsage isOfType)
            : base(isOfKind, booleanResultType, argument)
        {
            Debug.Assert(DbExpressionKind.IsOf == this.ExpressionKind || DbExpressionKind.IsOfOnly == this.ExpressionKind, string.Format(CultureInfo.InvariantCulture, "Invalid DbExpressionKind used in DbIsOfExpression: {0}", Enum.GetName(typeof(DbExpressionKind), this.ExpressionKind)));
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbIsOfExpression requires a Boolean result type");

            this._ofType = isOfType;
        }

        /// <summary>
        /// Gets the type metadata that the type metadata of the argument should be compared to.
        /// </summary>
        public TypeUsage OfType
        {
            get { return _ofType; }
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
    /// Represents the retrieval of elements of the specified type from the given set argument.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbOfTypeExpression : DbUnaryExpression
    {
        private readonly TypeUsage _ofType;

        internal DbOfTypeExpression(DbExpressionKind ofTypeKind, TypeUsage collectionResultType, DbExpression argument, TypeUsage type)
            : base(ofTypeKind, collectionResultType, argument)
        {
            Debug.Assert(DbExpressionKind.OfType == ofTypeKind ||
                         DbExpressionKind.OfTypeOnly == ofTypeKind,
                         "ExpressionKind for DbOfTypeExpression must be OfType or OfTypeOnly");

            //
            // Assign the requested element type to the OfType property.
            //
            this._ofType = type;
        }

        /// <summary>
        /// Gets the metadata of the type of elements that should be retrieved from the set argument.
        /// </summary>
        public TypeUsage OfType
        {
            get { return _ofType; }
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
    /// Represents the type conversion of a single argument to the specified type.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbTreatExpression : DbUnaryExpression
    {
        internal DbTreatExpression(TypeUsage asType, DbExpression argument)
            : base(DbExpressionKind.Treat, asType, argument)
        {
            Debug.Assert(TypeSemantics.IsValidPolymorphicCast(argument.ResultType, asType), "DbTreatExpression represents an invalid treat");
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
    /// Represents a string comparison against the specified pattern with an optional escape string
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbLikeExpression : DbExpression
    {
        private readonly DbExpression _argument;
        private readonly DbExpression _pattern;
        private readonly DbExpression _escape;

        internal DbLikeExpression(TypeUsage booleanResultType, DbExpression input, DbExpression pattern, DbExpression escape)
            : base(DbExpressionKind.Like, booleanResultType)
        {
            Debug.Assert(input != null, "DbLikeExpression argument cannot be null");
            Debug.Assert(pattern != null, "DbLikeExpression pattern cannot be null");
            Debug.Assert(escape != null, "DbLikeExpression escape cannot be null");
            Debug.Assert(TypeSemantics.IsPrimitiveType(input.ResultType, PrimitiveTypeKind.String), "DbLikeExpression argument must have a string result type");
            Debug.Assert(TypeSemantics.IsPrimitiveType(pattern.ResultType, PrimitiveTypeKind.String), "DbLikeExpression pattern must have a string result type");
            Debug.Assert(TypeSemantics.IsPrimitiveType(escape.ResultType, PrimitiveTypeKind.String), "DbLikeExpression escape must have a string result type");
            Debug.Assert(TypeSemantics.IsBooleanType(booleanResultType), "DbLikeExpression must have a Boolean result type");

            this._argument = input;
            this._pattern = pattern;
            this._escape = escape;
        }

        /// <summary>
        /// Gets the expression that specifies the string to compare against the given pattern
        /// </summary>
        public DbExpression Argument { get { return _argument; } }

        /// <summary>
        /// Gets the expression that specifies the pattern against which the given string should be compared
        /// </summary>
        public DbExpression Pattern { get { return _pattern; } }

        /// <summary>
        /// Gets the expression that provides an optional escape string to use for the comparison
        /// </summary>
        public DbExpression Escape { get { return _escape; } }

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
    /// Represents the retrieval of a reference to the specified Entity as a Ref.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbEntityRefExpression : DbUnaryExpression
    {
        internal DbEntityRefExpression(TypeUsage refResultType, DbExpression entity)
            : base(DbExpressionKind.EntityRef, refResultType, entity)
        {
            Debug.Assert(TypeSemantics.IsReferenceType(refResultType), "DbEntityRefExpression requires a reference result type");
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
    /// Represents the retrieval of the key value of the specified Reference as a row.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public sealed class DbRefKeyExpression : DbUnaryExpression
    {
        internal DbRefKeyExpression(TypeUsage rowResultType, DbExpression reference)
            : base(DbExpressionKind.RefKey, rowResultType, reference)
        {
            Debug.Assert(TypeSemantics.IsRowType(rowResultType), "DbRefKeyExpression requires a row result type");
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

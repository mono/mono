//---------------------------------------------------------------------
// <copyright file="AbstractExpressions.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Spatial;
using System.Diagnostics;
using System.Globalization;
using System.Data.Metadata.Edm;
using System.Data.Common.CommandTrees.Internal;

namespace System.Data.Common.CommandTrees
{
    /// <summary>
    /// Describes the different "kinds" (classes) of expressions
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public enum DbExpressionKind
    {
        /// <summary>
        /// True for all.
        /// </summary>
        All = 0,

        /// <summary>
        /// Logical And.
        /// </summary>
        And = 1,

        /// <summary>
        /// True for any.
        /// </summary>
        Any = 2,

        /// <summary>
        /// Conditional case statement.
        /// </summary>
        Case = 3,

        /// <summary>
        /// Polymorphic type cast.
        /// </summary>
        Cast = 4,

        /// <summary>
        /// A constant value.
        /// </summary>
        Constant = 5,

        /// <summary>
        /// Cross apply
        /// </summary>
        CrossApply = 6,

        /// <summary>
        /// Cross join
        /// </summary>
        CrossJoin = 7,

        /// <summary>
        /// Dereference.
        /// </summary>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Deref")]
        Deref = 8,

        /// <summary>
        /// Duplicate removal.
        /// </summary>
        Distinct = 9,

        /// <summary>
        /// Division.
        /// </summary>
        Divide = 10,

        /// <summary>
        /// Set to singleton conversion.
        /// </summary>
        Element = 11,

        /// <summary>
        /// Entity ref value retrieval.
        /// </summary>
        EntityRef = 12,

        /// <summary>
        /// Equality
        /// </summary>
        Equals = 13,
        
        /// <summary>
        /// Set subtraction
        /// </summary>
        Except = 14,

        /// <summary>
        /// Restriction.
        /// </summary>
        Filter = 15,

        /// <summary>
        /// Full outer join
        /// </summary>
        FullOuterJoin = 16,

        /// <summary>
        /// Invocation of a stand-alone function
        /// </summary>
        Function = 17,
                
        /// <summary>
        /// Greater than.
        /// </summary>
        GreaterThan = 18,

        /// <summary>
        /// Greater than or equal.
        /// </summary>
        GreaterThanOrEquals = 19,
        
        /// <summary>
        /// Grouping.
        /// </summary>
        GroupBy = 20,
        
        /// <summary>
        /// Inner join
        /// </summary>
        InnerJoin = 21,

        /// <summary>
        /// Set intersection.
        /// </summary>
        Intersect = 22,
        
        /// <summary>
        /// Empty set determination.
        /// </summary>
        IsEmpty = 23,

        /// <summary>
        /// Null determination.
        /// </summary>
        IsNull = 24,
        
        /// <summary>
        /// Type comparison (specified Type or Subtype).
        /// </summary>
        IsOf = 25,
        
        /// <summary>
        /// Type comparison (specified Type only).
        /// </summary>
        IsOfOnly = 26,

        /// <summary>
        /// Application of a lambda function
        /// </summary>
        Lambda = 57,

        /// <summary>
        /// Left outer join
        /// </summary>
        LeftOuterJoin = 27,

        /// <summary>
        /// Less than.
        /// </summary>
        LessThan = 28,
        
        /// <summary>
        /// Less than or equal.
        /// </summary>
        LessThanOrEquals = 29,

        /// <summary>
        /// String comparison.
        /// </summary>
        Like = 30,

        /// <summary>
        /// Result count restriction (TOP n).
        /// </summary>
        Limit = 31,

#if METHOD_EXPRESSION
        /// <summary>
        /// Invocation of a static or instance method.
        /// </summary>
        Method,
#endif

        /// <summary>
        /// Subtraction.
        /// </summary>
        Minus = 32,

        /// <summary>
        /// Modulo.
        /// </summary>
        Modulo = 33,

        /// <summary>
        /// Multiplication.
        /// </summary>
        Multiply = 34,

        /// <summary>
        /// Instance, row, and set construction.
        /// </summary>
        NewInstance = 35,
        
        /// <summary>
        /// Logical Not.
        /// </summary>
        Not = 36,
        
        /// <summary>
        /// Inequality.
        /// </summary>
        NotEquals = 37,

        /// <summary>
        /// Null.
        /// </summary>
        Null = 38,
        
        /// <summary>
        /// Set members by type (or subtype).
        /// </summary>
        OfType = 39,

        /// <summary>
        /// Set members by (exact) type.
        /// </summary>
        OfTypeOnly = 40,

        /// <summary>
        /// Logical Or.
        /// </summary>
        Or = 41,
        
        /// <summary>
        /// Outer apply.
        /// </summary>
        OuterApply = 42,

        /// <summary>
        /// A reference to a parameter.
        /// </summary>
        ParameterReference = 43,
        
        /// <summary>
        /// Addition.
        /// </summary>
        Plus = 44,
        
        /// <summary>
        /// Projection.
        /// </summary>
        Project = 45,
        
        /// <summary>
        /// Retrieval of a static or instance property.
        /// </summary>
        Property = 46,

        /// <summary>
        /// Reference.
        /// </summary>
        Ref = 47,

        /// <summary>
        /// Ref key value retrieval.
        /// </summary>
        RefKey = 48,

        /// <summary>
        /// Navigation of a (composition or association) relationship.
        /// </summary>
        RelationshipNavigation = 49,

        /// <summary>
        /// Entity or relationship set scan.
        /// </summary>
        Scan = 50,

        /// <summary>
        /// Skip elements of an ordered collection.
        /// </summary>
        Skip = 51,

        /// <summary>
        /// Sorting.
        /// </summary>
        Sort = 52,

        /// <summary>
        /// Type conversion.
        /// </summary>
        Treat = 53,
        
        /// <summary>
        /// Negation.
        /// </summary>
        UnaryMinus = 54,  
        
        /// <summary>
        /// Set union (with duplicates).
        /// </summary>
        UnionAll = 55,

        /// <summary>
        /// A reference to a variable.
        /// </summary>
        VariableReference = 56
    }

    /// <summary>
    /// The base type for all expressions
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbExpression
    {
        private readonly TypeUsage _type;
        private readonly DbExpressionKind _kind;

        internal DbExpression(DbExpressionKind kind, TypeUsage type)
        {
            CheckExpressionKind(kind);
            _kind = kind;

            Debug.Assert(type != null, string.Format(CultureInfo.InvariantCulture, "{0}.Type is null in DbExpression constructor", this.GetType().Name));
            if (!TypeSemantics.IsNullable(type))
            {
                type = type.ShallowCopy(new FacetValues { Nullable = true });
            }            
            Debug.Assert(type.IsReadOnly, "Editable type metadata specified for DbExpression.Type");
            this._type = type;
        }

        /// <summary>
        /// Gets the type metadata for the result type of the expression.
        /// </summary>
        public TypeUsage ResultType { get { return _type; } }
       
        /// <summary>
        /// Gets the kind of the expression, which indicates the operation of this expression.
        /// </summary>
        public DbExpressionKind ExpressionKind { get { return _kind; } }
                
        /// <summary>
        /// The visitor pattern interface method for expression visitors that do not produce a result value.
        /// </summary>
        /// <param name="visitor">An instance of DbExpressionVisitor.</param>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        public abstract void Accept(DbExpressionVisitor visitor);

        /// <summary>
        /// The visitor pattern interface method for expression visitors that produce a result value of a specific type.
        /// </summary>
        /// <param name="visitor">An instance of a typed DbExpressionVisitor that produces a result value of type TResultType.</param>
        /// <typeparam name="TResultType">The type of the result produced by <paramref name="visitor"/></typeparam>
        /// <exception cref="ArgumentNullException"><paramref name="visitor"/> is null</exception>
        /// <returns>An instance of <typeparamref name="TResultType"/>.</returns>
        public abstract TResultType Accept<TResultType>(DbExpressionVisitor<TResultType> visitor);

        #region Equals / GetHashCode

        // Dev10#547254: Easy to confuse DbExpressionBuilder.Equal with object.Equals method
        // The object.Equals method is overriden on DbExpression and marked so that it does
        // not appear in IntelliSense to avoid confusion with the DbExpressionBuilder.Equal
        // expression construction method. Overriding Equals also requires that GetHashCode
        // is overridden, however in both cases we defer to the System.Object implementation.

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        #endregion

        #region Implicit Cast Operators

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified binary value, which may be <c>null</c>
        /// </summary>
        /// <param name="value">The binary value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified binary value</returns>
        public static DbExpression FromBinary(byte[] value)
        {
            if (null == value)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Binary);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(byte[] value)
        {
            return DbExpression.FromBinary(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) Boolean value
        /// </summary>
        /// <param name="value">The Boolean value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified Boolean value</returns>
        public static DbExpression FromBoolean(bool? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Boolean);
            }
            return (value.Value ? ExpressionBuilder.DbExpressionBuilder.True : ExpressionBuilder.DbExpressionBuilder.False);
        }

        public static implicit operator DbExpression(bool? value)
        {
            return DbExpression.FromBoolean(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) byte value
        /// </summary>
        /// <param name="value">The byte value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified byte value</returns>
        public static DbExpression FromByte(byte? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Byte);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(byte? value)
        {
            return DbExpression.FromByte(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) <see cref="DateTime"/> value
        /// </summary>
        /// <param name="value">The DateTime value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified DateTime value</returns>
        public static DbExpression FromDateTime(DateTime? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.DateTime);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(DateTime? value)
        {
            return DbExpression.FromDateTime(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) <see cref="DateTimeOffset"/> value
        /// </summary>
        /// <param name="value">The DateTimeOffset value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified DateTimeOffset value</returns>
        public static DbExpression FromDateTimeOffset(DateTimeOffset? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.DateTimeOffset);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(DateTimeOffset? value)
        {
            return DbExpression.FromDateTimeOffset(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) decimal value
        /// </summary>
        /// <param name="value">The decimal value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified decimal value</returns>
        public static DbExpression FromDecimal(decimal? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Decimal);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(decimal? value)
        {
            return DbExpression.FromDecimal(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) double value
        /// </summary>
        /// <param name="value">The double value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified double value</returns>
        public static DbExpression FromDouble(double? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Double);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(double? value)
        {
            return DbExpression.FromDouble(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified <see cref="DbGeography"/> value, which may be null.
        /// </summary>
        /// <param name="value">The DbGeography value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified DbGeography value</returns>
        public static DbExpression FromGeography(DbGeography value)
        {
            if (value == null)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Geography);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(DbGeography value)
        {
            return DbExpression.FromGeography(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified <see cref="DbGeometry"/> value, which may be null.
        /// </summary>
        /// <param name="value">The DbGeometry value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified DbGeometry value</returns>
        public static DbExpression FromGeometry(DbGeometry value)
        {
            if (value == null)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Geometry);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(DbGeometry value)
        {
            return DbExpression.FromGeometry(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) <see cref="Guid"/> value
        /// </summary>
        /// <param name="value">The Guid value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified Guid value</returns>
        public static DbExpression FromGuid(Guid? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Guid);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(Guid? value)
        {
            return DbExpression.FromGuid(value);
        }
               
        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) Int16 value
        /// </summary>
        /// <param name="value">The Int16 value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified Int16 value</returns>
        public static DbExpression FromInt16(short? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int16);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(short? value)
        {
            return DbExpression.FromInt16(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) Int32 value
        /// </summary>
        /// <param name="value">The Int32 value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified Int32 value</returns>
        public static DbExpression FromInt32(int? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int32);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(int? value)
        {
            return DbExpression.FromInt32(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) Int64 value
        /// </summary>
        /// <param name="value">The Int64 value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified Int64 value</returns>
        public static DbExpression FromInt64(long? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Int64);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(long? value)
        {
            return DbExpression.FromInt64(value);
        }

        //// 









        //public static implicit operator DbExpression(sbyte? value)
        //{
        //    return DbExpression.FromSByte(value);
        //}

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified (nullable) Single value
        /// </summary>
        /// <param name="value">The Single value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified Single value</returns>
        public static DbExpression FromSingle(float? value)
        {
            if (!value.HasValue)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.Single);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value.Value);
        }

        public static implicit operator DbExpression(float? value)
        {
            return DbExpression.FromSingle(value);
        }

        /// <summary>
        /// Creates a <see cref="DbExpression"/> that represents the specified string value
        /// </summary>
        /// <param name="value">The string value on which the returned expression should be based</param>
        /// <returns>A <see cref="DbExpression"/> that represents the specified string value</returns>
        public static DbExpression FromString(string value)
        {
            if (null == value)
            {
                return ExpressionBuilder.DbExpressionBuilder.CreatePrimitiveNullExpression(PrimitiveTypeKind.String);
            }
            return ExpressionBuilder.DbExpressionBuilder.Constant(value);
        }

        public static implicit operator DbExpression(string value)
        {
            return DbExpression.FromString(value);
        }

        // 
                
        #endregion

        #region Internal API

        /// <summary>
        /// Produces a text-based tree representation of the DbExpression tree rooted at this expression
        /// </summary>
        /// <returns>A string containing the text-based tree representation</returns>
        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Performance", "CA1811:AvoidUncalledPrivateCode")]
        internal string Print()
        {
            return new ExpressionPrinter().Print(this);
        }
               
        internal static void CheckExpressionKind(DbExpressionKind kind)
        {
            // Add new valid DbExpressionKind values to this method as well as the enum itself.
            // DbExpressionKind is a contiguous enum from All = 0 through View            
            if ((kind < DbExpressionKind.All) || (DbExpressionKind.Lambda < kind))
            {
                throw EntityUtil.InvalidEnumerationValue(typeof(DbExpressionKind), (int)kind);
            }
        }

        #endregion
    }

    /// <summary>
    /// The abstract base type for expressions that accept two expression operands.
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbBinaryExpression : DbExpression
    {
        private readonly DbExpression _left;
        private readonly DbExpression _right;

        internal DbBinaryExpression(DbExpressionKind kind, TypeUsage type, DbExpression left, DbExpression right)
            : base(kind, type)
        {
            Debug.Assert(left != null, "DbBinaryExpression.Left cannot be null");
            Debug.Assert(right != null, "DbBinaryExpression.Right cannot be null");

            this._left = left;
            this._right = right;
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the left argument.
        /// </summary>
        public DbExpression Left { get { return _left; } }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the right argument.
        /// </summary>
        public DbExpression Right { get { return _right; } }
    }
        
    /// <summary>
    /// The abstract base type for expressions that accept a single expression operand
    /// </summary>
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Naming", "CA1709:IdentifiersShouldBeCasedCorrectly", MessageId = "Db")]
    public abstract class DbUnaryExpression : DbExpression
    {
        private readonly DbExpression _argument;

        internal DbUnaryExpression(DbExpressionKind kind, TypeUsage resultType, DbExpression argument)
            : base(kind, resultType)
        {
            Debug.Assert(argument != null, "DbUnaryExpression.Argument cannot be null");

            this._argument = argument;
        }

        /// <summary>
        /// Gets the <see cref="DbExpression"/> that defines the argument.
        /// </summary>
        public DbExpression Argument { get { return this._argument; } }
    }
}

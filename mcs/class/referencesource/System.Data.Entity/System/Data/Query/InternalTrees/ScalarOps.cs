//---------------------------------------------------------------------
// <copyright file="ScalarOps.cs" company="Microsoft">
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

namespace System.Data.Query.InternalTrees
{
    #region Constants

    /// <summary>
    /// Base class for all constant Ops
    /// </summary>
    internal abstract class ConstantBaseOp : ScalarOp
    {
        #region private state
        private readonly object m_value;
        #endregion

        #region constructors
        protected ConstantBaseOp(OpType opType, TypeUsage type, object value)
            : base(opType, type)
        {
            m_value = value;
        }

        /// <summary>
        /// Constructor overload for rules
        /// </summary>
        /// <param name="opType"></param>
        protected ConstantBaseOp(OpType opType)
            : base(opType)
        {
        }
        #endregion

        #region public properties and methods
        /// <summary>
        /// Get the constant value
        /// </summary>
        internal virtual Object Value { get { return m_value; } }

        /// <summary>
        /// 0 children
        /// </summary>
        internal override int Arity { get { return 0; } }

        /// <summary>
        /// Two CostantBaseOps are equivalent if they are of the same 
        /// derived type and have the same type and value. 
        /// </summary>
        /// <param name="other">the other Op</param>
        /// <returns>true, if these are equivalent (not a strict equality test)</returns>
        internal override bool IsEquivalent(Op other)
        {
            ConstantBaseOp otherConstant = other as ConstantBaseOp;
            return 
                otherConstant != null && 
                this.OpType == other.OpType &&
                otherConstant.Type.EdmEquals(this.Type) && 
                ((otherConstant.Value == null && this.Value == null) || otherConstant.Value.Equals(this.Value));
        }
        #endregion
    }

    /// <summary>
    /// Represents an external constant
    /// </summary>
    internal sealed class ConstantOp : ConstantBaseOp
    {
        #region constructors
        internal ConstantOp(TypeUsage type, object value)
            : base(OpType.Constant, type, value)
        {
            Debug.Assert(value != null, "ConstantOp with a null value?");
        }
        private ConstantOp() : base(OpType.Constant) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly ConstantOp Pattern = new ConstantOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }

        #endregion
    }

    /// <summary>
    /// Represents null constants
    /// </summary>
    internal sealed class NullOp : ConstantBaseOp
    {
        #region constructors
        internal NullOp(TypeUsage type)
            : base(OpType.Null, type, null)
        {
        }
        private NullOp() : base(OpType.Null) { }
        #endregion

        #region public apis

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NullOp Pattern = new NullOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }

        #endregion
    }

    /// <summary>
    /// Represents internally generated constants
    /// </summary>
    internal sealed class InternalConstantOp : ConstantBaseOp
    {
        #region constructors
        internal InternalConstantOp(TypeUsage type, object value)
            : base(OpType.InternalConstant, type, value)
        {
            Debug.Assert(value != null, "InternalConstantOp with a null value?");
        }
        private InternalConstantOp() : base(OpType.InternalConstant) { }
        #endregion

        #region public apis

        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly InternalConstantOp Pattern = new InternalConstantOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }

        #endregion
    }

    /// <summary>
    /// Represents an internally generated constant that is used to serve as a null sentinel, 
    /// i.e. to be checked whether it is null.
    /// </summary>
    internal sealed class NullSentinelOp : ConstantBaseOp
    {
        #region constructors
        internal NullSentinelOp(TypeUsage type, object value)
            : base(OpType.NullSentinel, type, value)
        {
        }
        private NullSentinelOp() : base(OpType.NullSentinel) { }
        #endregion

        #region public apis
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NullSentinelOp Pattern = new NullSentinelOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }

        #endregion
    }

    /// <summary>
    /// Represents a constant predicate (with a value of either true or false)
    /// </summary>
    internal sealed class ConstantPredicateOp : ConstantBaseOp
    {
        #region constructors
        internal ConstantPredicateOp(TypeUsage type, bool value)
            : base(OpType.ConstantPredicate, type, value)
        {
        }
        private ConstantPredicateOp()
            : base(OpType.ConstantPredicate)
        { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly ConstantPredicateOp Pattern = new ConstantPredicateOp();

        /// <summary>
        /// Value of the constant predicate
        /// </summary>
        internal new bool Value { get { return (bool)base.Value; } }

        /// <summary>
        /// Is this the true predicate
        /// </summary>
        internal bool IsTrue { get { return this.Value; } }

        /// <summary>
        /// Is this the 'false' predicate
        /// </summary>
        internal bool IsFalse { get { return this.Value == false; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }

        #endregion
    }

    #endregion

    /// <summary>
    /// A reference to an existing variable
    /// </summary>
    internal sealed class VarRefOp : ScalarOp
    {
        #region private state
        private Var m_var;
        #endregion

        #region constructors
        internal VarRefOp(Var v) : base(OpType.VarRef, v.Type)
        {
            m_var = v;
        }
        private VarRefOp() : base(OpType.VarRef) { }
        #endregion

        #region public methods
        /// <summary>
        /// Singleton used for pattern matching
        /// </summary>
        internal static readonly VarRefOp Pattern = new VarRefOp();

        /// <summary>
        /// 0 children
        /// </summary>
        internal override int Arity { get { return 0; } }

        /// <summary>
        /// Two VarRefOps are equivalent, if they reference the same Var
        /// </summary>
        /// <param name="other">the other Op</param>
        /// <returns>true, if these are equivalent</returns>
        internal override bool IsEquivalent(Op other)
        {
            VarRefOp otherVarRef = other as VarRefOp;
            return (otherVarRef != null && otherVarRef.Var.Equals(this.Var));
        }

        /// <summary>
        /// The Var that this Op is referencing
        /// </summary>
        internal Var Var { get { return m_var; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents an arbitrary function call
    /// </summary>
    internal sealed class FunctionOp : ScalarOp
    {
        #region private state
        private EdmFunction m_function;
        #endregion

        #region constructors
        internal FunctionOp(EdmFunction function)
            : base(OpType.Function, function.ReturnParameter.TypeUsage)
        {
            m_function = function;
        }
        private FunctionOp() : base(OpType.Function) { }
        #endregion

        #region public methods
        /// <summary>
        /// Singleton instance used for patterns in transformation rules
        /// </summary>
        internal static readonly FunctionOp Pattern = new FunctionOp();

        /// <summary>
        /// The function that's being invoked
        /// </summary>
        internal EdmFunction Function { get { return m_function; } }

        /// <summary>
        /// Two FunctionOps are equivalent if they reference the same EdmFunction
        /// </summary>
        /// <param name="other">the other Op</param>
        /// <returns>true, if these are equivalent</returns>
        internal override bool IsEquivalent(Op other)
        {
            FunctionOp otherFunctionOp = other as FunctionOp;
            return (otherFunctionOp != null && otherFunctionOp.Function.EdmEquals(this.Function));
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents a property access
    /// </summary>
    internal sealed class PropertyOp : ScalarOp
    {
        #region private state
        private EdmMember m_property;
        #endregion

        #region constructors
        internal PropertyOp(TypeUsage type, EdmMember property)
            : base(OpType.Property, type)
        {
            Debug.Assert((property is EdmProperty) || (property is RelationshipEndMember) || (property is NavigationProperty), "Unexpected EdmMember type");
            m_property = property;
        }
        private PropertyOp() : base(OpType.Property) { }
        #endregion

        #region public methods
        /// <summary>
        /// Used for patterns in transformation rules
        /// </summary>
        internal static readonly PropertyOp Pattern = new PropertyOp();

        /// <summary>
        /// 1 child - the instance
        /// </summary>
        internal override int Arity { get { return 1; } }

		/// <summary>
        /// The property metadata
        /// </summary>
        internal EdmMember PropertyInfo { get { return m_property; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
	    #endregion
    }

    /// <summary>
    /// Represents a TREAT AS operation
    /// </summary>
    internal sealed class TreatOp : ScalarOp
    {
        #region private state
        private bool m_isFake;
        #endregion

        #region constructors
        internal TreatOp(TypeUsage type, bool isFake)
            : base(OpType.Treat, type)
        {
            m_isFake = isFake;
        }
        private TreatOp() : base(OpType.Treat) { }
        #endregion

        #region public methods
        /// <summary>
        /// Used as patterns in transformation rules
        /// </summary>
        internal static readonly TreatOp Pattern = new TreatOp();

        /// <summary>
        /// 1 child - instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Is this a "fake" treat?
        /// </summary>
        internal bool IsFakeTreat { get { return m_isFake; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// An IS OF operation
    /// </summary>
    internal sealed class IsOfOp : ScalarOp
    {
        #region private state
        private TypeUsage m_isOfType;
        private bool m_isOfOnly;
        #endregion

        #region constructors
        internal IsOfOp(TypeUsage isOfType, bool isOfOnly, TypeUsage type)
            : base(OpType.IsOf, type)
        {
            m_isOfType = isOfType;
            m_isOfOnly = isOfOnly;
        }
        private IsOfOp() : base(OpType.IsOf) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern used for transformation rules
        /// </summary>
        internal static readonly IsOfOp Pattern = new IsOfOp();

        /// <summary>
        /// 1 child - instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// The type being checked for
        /// </summary>
        internal TypeUsage IsOfType { get { return m_isOfType; } }

        internal bool IsOfOnly { get { return m_isOfOnly; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Cast operation. Convert a type instance into an instance of another type
    /// </summary>
    internal sealed class CastOp : ScalarOp
    {
        #region constructors
        internal CastOp(TypeUsage type) : base(OpType.Cast, type) { }
        private CastOp() : base(OpType.Cast) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly CastOp Pattern = new CastOp();

        /// <summary>
        /// 1 child - instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// An internal cast operation. (Softly) Convert a type instance into an instance of another type
    /// 
    /// This Op is intended to capture "promotion" semantics. (ie) int16 promotes to an int32; Customer promotes to Person
    /// etc. This Op is intended to shield the PlanCompiler from having to reason about 
    /// the promotion semantics; and is intended to make the query tree very 
    /// explicit
    /// 
    /// </summary>
    internal sealed class SoftCastOp : ScalarOp
    {
        #region constructors
        internal SoftCastOp(TypeUsage type) : base(OpType.SoftCast, type) { }
        private SoftCastOp() : base(OpType.SoftCast) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly SoftCastOp Pattern = new SoftCastOp();

        /// <summary>
        /// 1 child - input expression
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents a comparision operation (LT, GT etc.)
    /// </summary>
    internal sealed class ComparisonOp : ScalarOp
    {
        #region constructors
        internal ComparisonOp(OpType opType, TypeUsage type)
            : base(opType, type)
        {
        }
        private ComparisonOp(OpType opType) : base(opType) { }
        #endregion

        #region public methods
        /// <summary>
        /// Patterns for use in transformation rules
        /// </summary>
        internal static readonly ComparisonOp PatternEq = new ComparisonOp(OpType.EQ);

        /// <summary>
        /// 2 children - left, right
        /// </summary>
        internal override int Arity { get { return 2; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents a string comparison operation
    /// </summary>
    internal sealed class LikeOp : ScalarOp
    {
        #region constructors
        internal LikeOp(TypeUsage boolType)
            : base(OpType.Like, boolType) { }
        private LikeOp() : base(OpType.Like) { }
        #endregion

        #region public surface
        /// <summary>
        /// Pattern for use in transformation rules
        /// </summary>
        internal static readonly LikeOp Pattern = new LikeOp();

        /// <summary>
        /// 3 children - string, pattern , escape
        /// </summary>
        internal override int Arity { get { return 3; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents a conditional operation - and,or,not, is null
    /// A little hacky - since it represents and/or/not as optypes - could I not
    /// have done the same with the comparison operators?
    /// </summary>
    internal sealed class ConditionalOp : ScalarOp
    {
        #region constructors
        internal ConditionalOp(OpType optype, TypeUsage type) : base(optype, type)
        {
        }
        private ConditionalOp(OpType opType) : base(opType) { }
        #endregion

        #region public methods
        /// <summary>
        /// Patterns for use in transformation rules
        /// </summary>
        internal static readonly ConditionalOp PatternAnd = new ConditionalOp(OpType.And);
        internal static readonly ConditionalOp PatternOr = new ConditionalOp(OpType.Or);
        internal static readonly ConditionalOp PatternNot = new ConditionalOp(OpType.Not);
        internal static readonly ConditionalOp PatternIsNull = new ConditionalOp(OpType.IsNull);

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// ANSI switched Case expression.
    /// </summary>
    internal sealed class CaseOp : ScalarOp
    {
        #region constructors
        internal CaseOp(TypeUsage type) : base(OpType.Case, type) { }
        private CaseOp() : base(OpType.Case) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for use in transformation rules
        /// </summary>
        internal static readonly CaseOp Pattern = new CaseOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Basic Aggregates
    /// </summary>
    internal sealed class AggregateOp : ScalarOp
    {
        #region private state
        private EdmFunction m_aggFunc;
        private bool m_distinctAgg;
        #endregion

        #region constructors
        internal AggregateOp(EdmFunction aggFunc, bool distinctAgg)
            : base(OpType.Aggregate, aggFunc.ReturnParameter.TypeUsage)
        {
            m_aggFunc = aggFunc;
            m_distinctAgg = distinctAgg;
        }
        private AggregateOp() : base(OpType.Aggregate) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly AggregateOp Pattern = new AggregateOp();

        /// <summary>
        /// The Aggregate function's metadata
        /// </summary>
        internal EdmFunction AggFunc { get { return m_aggFunc; } }

        /// <summary>
        /// Is this a "distinct" aggregate
        /// </summary>
        internal bool IsDistinctAggregate { get { return m_distinctAgg; } }

        /// <summary>
        /// Yes; this is an aggregate
        /// </summary>
        internal override bool IsAggregateOp {get{return true;}}

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents an arbitrary nest operation - can be used anywhere
    /// </summary>
    internal sealed class CollectOp : ScalarOp
    {
        #region constructors
        internal CollectOp(TypeUsage type) : base(OpType.Collect, type) { }
        private CollectOp() : base(OpType.Collect) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for use in transformation rules
        /// </summary>
        internal static readonly CollectOp Pattern = new CollectOp();

        /// <summary>
        /// 1 child - instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Almost identical to a PropertyOp - the only difference being that we're dealing with an 
    /// "extended" property (a rel property) this time
    /// </summary>
    internal sealed class RelPropertyOp : ScalarOp
    {
        #region private state
        private readonly RelProperty m_property;
        #endregion

        #region constructors
        private RelPropertyOp() : base(OpType.RelProperty) { }

        internal RelPropertyOp(TypeUsage type, RelProperty property)
            : base(OpType.RelProperty, type)
        {
            m_property = property;
        }
        #endregion

        #region public APIs
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly RelPropertyOp Pattern = new RelPropertyOp();

        /// <summary>
        /// 1 child - the entity instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Get the property metadata
        /// </summary>
        public RelProperty PropertyInfo { get { return m_property; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }

        #endregion
    }

    /// <summary>
    /// Base class for DiscriminatedNewEntityOp and NewEntityOp
    /// </summary>
    internal abstract class NewEntityBaseOp : ScalarOp
    {
        #region private state
        private readonly bool m_scoped;
        private readonly EntitySet m_entitySet;
        private readonly List<RelProperty> m_relProperties; // list of relationship properties for which we have values
        #endregion

        #region constructors
        internal NewEntityBaseOp(OpType opType, TypeUsage type, bool scoped, EntitySet entitySet, List<RelProperty> relProperties)
            : base(opType, type)
        {
            Debug.Assert(scoped || entitySet == null, "entitySet cann't be set of constructor isn't scoped");
            Debug.Assert(relProperties != null, "expected non-null list of rel-properties");
            m_scoped = scoped;
            m_entitySet = entitySet;
            m_relProperties = relProperties;
        }

        protected NewEntityBaseOp(OpType opType) : base(opType) { }
        #endregion

        #region public APIs
        /// <summary>
        /// True if the entity constructor is scoped to a particular entity set or null (scoped as "unscoped").
        /// False if the scope is not yet known. Scope is determined in PreProcessor.
        /// </summary>
        internal bool Scoped { get { return m_scoped; } }

        /// <summary>
        /// Get the entityset (if any) associated with this constructor
        /// </summary>
        internal EntitySet EntitySet { get { return m_entitySet; } }

        /// <summary>
        /// get the list of relationship properties (if any) specified for this constructor
        /// </summary>
        internal List<RelProperty> RelationshipProperties { get { return m_relProperties; } }
        #endregion
    }

    /// <summary>
    /// A new entity instance constructor
    /// </summary>
    internal sealed class NewEntityOp : NewEntityBaseOp
    {
        #region constructors
        private NewEntityOp() : base(OpType.NewEntity) { }

        internal NewEntityOp(TypeUsage type, List<RelProperty> relProperties, bool scoped, EntitySet entitySet)
            : base(OpType.NewEntity, type, scoped, entitySet, relProperties)
        {
        }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NewEntityOp Pattern = new NewEntityOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// A new instance creation
    /// </summary>
    internal sealed class NewInstanceOp : ScalarOp
    {
        #region constructors
        internal NewInstanceOp(TypeUsage type) : base(OpType.NewInstance, type) 
        {
            Debug.Assert(!type.EdmType.Abstract, "cannot create new instance of abstract type");
            Debug.Assert(!TypeSemantics.IsEntityType(type), "cannot use this Op for entity construction");
        }
        private NewInstanceOp() : base(OpType.NewInstance) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NewInstanceOp Pattern = new NewInstanceOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Polymorphic new instance creation (takes all properties of all types in the hierarchy + discriminator)
    /// </summary>
    internal sealed class DiscriminatedNewEntityOp : NewEntityBaseOp 
    {
        #region Private state
        private readonly ExplicitDiscriminatorMap m_discriminatorMap;
        #endregion

        #region Constructors
        internal DiscriminatedNewEntityOp(TypeUsage type, ExplicitDiscriminatorMap discriminatorMap,
            EntitySet entitySet, List<RelProperty> relProperties) 
            : base(OpType.DiscriminatedNewEntity, type, true, entitySet, relProperties)
        {
            Debug.Assert(null != discriminatorMap, "null discriminator map");
            m_discriminatorMap = discriminatorMap;
        }
        private DiscriminatedNewEntityOp() : base(OpType.DiscriminatedNewEntity) { }
        #endregion 

        #region "Public" members
        internal static readonly DiscriminatedNewEntityOp Pattern = new DiscriminatedNewEntityOp();

        /// <summary>
        /// Gets discriminator and type information used in construction of type.
        /// </summary>
        internal ExplicitDiscriminatorMap DiscriminatorMap
        {
            get { return m_discriminatorMap; }
        }

        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents a new record constructor
    /// </summary>
    internal sealed class NewRecordOp : ScalarOp
    {
        #region private state
        private List<EdmProperty> m_fields; // list of fields with specified values
        #endregion

        #region constructors
        /// <summary>
        /// Basic constructor. All fields have a value specified
        /// </summary>
        /// <param name="type"></param>
        internal NewRecordOp(TypeUsage type) : base(OpType.NewRecord, type)
        {
            m_fields = new List<EdmProperty>(TypeHelpers.GetEdmType<RowType>(type).Properties);
        }
        /// <summary>
        /// Alternate form of the constructor. Only some fields have a value specified
        /// The arguments to the corresponding Node are exactly 1-1 with the fields
        /// described here.
        /// The missing fields are considered to be "null"
        /// </summary>
        /// <param name="type"></param>
        /// <param name="fields"></param>
        internal NewRecordOp(TypeUsage type, List<EdmProperty> fields)
            : base(OpType.NewRecord, type)
        {
#if DEBUG
            foreach (EdmProperty p in fields)
            {
                Debug.Assert(Object.ReferenceEquals(p.DeclaringType, this.Type.EdmType));
            }
#endif
            m_fields = fields;
        }
        private NewRecordOp() : base(OpType.NewRecord) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NewRecordOp Pattern = new NewRecordOp();

        /// <summary>
        /// Determine if a value has been provided for the specified field.
        /// Returns the position of this field (ie) the specific argument in the Node's
        /// children. If no value has been provided for this field, then simply
        /// return false
        /// </summary>
        /// <param name="field"></param>
        /// <param name="fieldPosition"></param>
        /// <returns></returns>
        internal bool GetFieldPosition(EdmProperty field, out int fieldPosition)
        {
            Debug.Assert(Object.ReferenceEquals(field.DeclaringType, this.Type.EdmType),
                "attempt to get invalid field from this record type");

            fieldPosition = 0;
            for (int i = 0; i < m_fields.Count; i++)
            {
                if (m_fields[i] == field)
                {
                    fieldPosition = i;
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// List of all properties that have values specified
        /// </summary>
        internal List<EdmProperty> Properties { get { return m_fields; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    internal sealed class NewMultisetOp : ScalarOp
    {
        #region constructors
        internal NewMultisetOp(TypeUsage type) : base(OpType.NewMultiset, type) { }
        private NewMultisetOp() : base(OpType.NewMultiset) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NewMultisetOp Pattern = new NewMultisetOp();

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents arithmetic operators - Plus,Minus,Multiply,Divide,Modulo,UnaryMinus
    /// </summary>
    internal sealed class ArithmeticOp : ScalarOp
    {
        #region constructors
        internal ArithmeticOp(OpType opType, TypeUsage type)
            : base(opType, type) { }
        #endregion

        #region public methods

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    ///
    /// </summary>
    internal sealed class RefOp : ScalarOp
    {
        #region private state
        private EntitySet m_entitySet;
        #endregion

        #region constructors
        internal RefOp(EntitySet entitySet, TypeUsage type)
            : base(OpType.Ref, type)
        {
            m_entitySet = entitySet;
        }
        private RefOp() : base(OpType.Ref) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly RefOp Pattern = new RefOp();

        /// <summary>
        /// 1 child - key
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// The EntitySet to which the reference refers
        /// </summary>
        internal EntitySet EntitySet { get { return m_entitySet; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents an EXISTS subquery?
    /// </summary>
    internal sealed class ExistsOp : ScalarOp
    {
        #region constructors
        internal ExistsOp(TypeUsage type)
            : base(OpType.Exists, type)
        {
        }
        private ExistsOp() : base(OpType.Exists) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly ExistsOp Pattern = new ExistsOp();

        /// <summary>
        /// 1 child - collection input
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Represents an Element() op - extracts the scalar value from a collection
    /// </summary>
    internal sealed class ElementOp : ScalarOp
    {
        #region constructors
        internal ElementOp(TypeUsage type) : base(OpType.Element, type) { }
        private ElementOp() : base(OpType.Element) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly ElementOp Pattern = new ElementOp();

        /// <summary>
        /// 1 child - collection instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// extracts the key from a ref
    /// </summary>
    internal sealed class GetRefKeyOp : ScalarOp
    {
        #region constructors
        internal GetRefKeyOp(TypeUsage type) : base(OpType.GetRefKey, type) { }
        private GetRefKeyOp() : base(OpType.GetRefKey) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly GetRefKeyOp Pattern = new GetRefKeyOp();

        /// <summary>
        /// 1 child - ref instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Extracts the ref from an entity instance
    /// </summary>
    internal sealed class GetEntityRefOp : ScalarOp
    {
        #region constructors
        internal GetEntityRefOp(TypeUsage type) : base(OpType.GetEntityRef, type) { }
        private GetEntityRefOp() : base(OpType.GetEntityRef) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly GetEntityRefOp Pattern = new GetEntityRefOp();

        /// <summary>
        /// 1 child - entity instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Gets the target entity pointed at by a reference
    /// </summary>
    internal sealed class DerefOp : ScalarOp
    {
        #region constructors
        internal DerefOp(TypeUsage type) : base(OpType.Deref, type) { }
        private DerefOp() : base(OpType.Deref) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly DerefOp Pattern = new DerefOp();

        /// <summary>
        /// 1 child - entity instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }

    /// <summary>
    /// Navigate a relationship, and get the reference(s) of the target end
    /// </summary>
    internal sealed class NavigateOp : ScalarOp
    {
        #region private state
        private readonly RelProperty m_property;
        #endregion

        #region constructors
        internal NavigateOp(TypeUsage type, RelProperty relProperty)
            : base(OpType.Navigate, type) 
        {
            m_property = relProperty;
        }
        private NavigateOp() : base(OpType.Navigate) { }
        #endregion

        #region public methods
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly NavigateOp Pattern = new NavigateOp();

        /// <summary>
        /// 1 child - entity instance
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// The rel property that describes this nvaigation
        /// </summary>
        internal RelProperty RelProperty { get { return m_property; } }

        /// <summary>
        /// The relationship we're traversing
        /// </summary>
        internal RelationshipType Relationship { get { return m_property.Relationship; } }
        /// <summary>
        /// The starting point of the traversal
        /// </summary>
        internal RelationshipEndMember FromEnd { get { return m_property.FromEnd; } }
        /// <summary>
        /// The end-point of the traversal
        /// </summary>
        internal RelationshipEndMember ToEnd { get { return m_property.ToEnd; } }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n) { v.Visit(this, n); }

        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) { return v.Visit(this, n); }
        #endregion
    }
}

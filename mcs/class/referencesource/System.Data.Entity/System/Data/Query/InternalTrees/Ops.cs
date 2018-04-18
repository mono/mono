//---------------------------------------------------------------------
// <copyright file="Ops.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

namespace System.Data.Query.InternalTrees
{
    using System.Data.Metadata.Edm;
    using System.Diagnostics;

    /// <summary>
    /// The operator types. Includes both scalar and relational operators, 
    /// and physical and logical operators, and rule operators 
    /// </summary>
    internal enum OpType
    {
        #region ScalarOpType
        /// <summary>
        /// Constants
        /// </summary>
        Constant,

        /// <summary>
        /// An internally generated constant
        /// </summary>
        InternalConstant,

        /// <summary>
        /// An internally generated constant used as a null sentinel
        /// </summary>
        NullSentinel,

        /// <summary>
        /// A null constant
        /// </summary>
        Null,

        /// <summary>
        /// ConstantPredicate
        /// </summary>
        ConstantPredicate,
        
        /// <summary>
        /// A Var reference
        /// </summary>
        VarRef,

        /// <summary>
        /// GreaterThan
        /// </summary>
        GT,
        
        /// <summary>
        /// >=
        /// </summary>
        GE,
        
        /// <summary>
        /// Lessthan or equals
        /// </summary>
        LE,

        /// <summary>
        /// Less than
        /// </summary>
        LT,

        /// <summary>
        /// Equals 
        /// </summary>
        EQ,

        /// <summary>
        /// Not equals
        /// </summary>
        NE,

        /// <summary>
        /// String comparison
        /// </summary>
        Like,

        /// <summary>
        /// Addition
        /// </summary>
        Plus,

        /// <summary>
        /// Subtraction
        /// </summary>
        Minus,

        /// <summary>
        /// Multiplication 
        /// </summary>
        Multiply,

        /// <summary>
        /// Division 
        /// </summary>
        Divide,

        /// <summary>
        /// Modulus 
        /// </summary>
        Modulo,

        /// <summary>
        /// Unary Minus 
        /// </summary>
        UnaryMinus,

        /// <summary>
        /// And 
        /// </summary>
        And,

        /// <summary>
        /// Or
        /// </summary>
        Or,

        /// <summary>
        /// Not
        /// </summary>
        Not,
        
        /// <summary>
        /// is null 
        /// </summary>
        IsNull,

        /// <summary>
        /// switched case expression
        /// </summary>
        Case,

        /// <summary>
        /// treat-as 
        /// </summary>
        Treat,

        /// <summary>
        /// is-of 
        /// </summary>
        IsOf,

        /// <summary>
        /// Cast
        /// </summary>
        Cast,

        /// <summary>
        /// Internal cast
        /// </summary>
        SoftCast,

        /// <summary>
        /// a basic aggregate
        /// </summary>
        Aggregate,
        
        /// <summary>
        /// function call
        /// </summary>
        Function,

        /// <summary>
        /// Reference to a "relationship" property
        /// </summary>
        RelProperty,

        /// <summary>
        /// property reference
        /// </summary>
        Property,

        /// <summary>
        /// entity constructor
        /// </summary>
        NewEntity,

        /// <summary>
        /// new instance constructor for a named type(other than multiset, record)
        /// </summary>
        NewInstance,

        /// <summary>
        /// new instance constructor for a named type and sub-types
        /// </summary>
        DiscriminatedNewEntity,

        /// <summary>
        /// Multiset constructor
        /// </summary>
        NewMultiset,
        
        /// <summary>
        /// record constructor
        /// </summary>
        NewRecord,
        
        /// <summary>
        /// Get the key from a Ref
        /// </summary>
        GetRefKey,
        
       /// <summary>
        /// Get the ref from an entity instance
        /// </summary>
        GetEntityRef,
        
        /// <summary>
        /// create a reference 
        /// </summary>
        Ref,
        
        /// <summary>
        /// exists
        /// </summary>
        Exists,

        /// <summary>
        /// get the singleton element from a collection
        /// </summary>
        Element,

        /// <summary>
        /// Builds up a collection
        /// </summary>
        Collect,

        /// <summary>
        /// gets the target entity pointed at by a reference
        /// </summary>
        Deref,

        /// <summary>
        /// Traverse a relationship and get the references of the other end
        /// </summary>
        Navigate,
        #endregion

        #region RelOpType
        /// <summary>
        /// A table scan
        /// </summary>
        ScanTable,
        /// <summary>
        /// A view scan
        /// </summary>
        ScanView,

        /// <summary>
        /// Filter
        /// </summary>
        Filter,
        
        /// <summary>
        /// Project
        /// </summary>
        Project,

        /// <summary>
        /// InnerJoin
        /// </summary>
        InnerJoin,

        /// <summary>
        /// LeftOuterJoin
        /// </summary>
        LeftOuterJoin,

        /// <summary>
        /// FullOuter join
        /// </summary>
        FullOuterJoin,

        /// <summary>
        /// Cross join
        /// </summary>
        CrossJoin,

        /// <summary>
        /// cross apply
        /// </summary>
        CrossApply,

        /// <summary>
        /// outer apply 
        /// </summary>
        OuterApply,

        /// <summary>
        /// Unnest
        /// </summary>
        Unnest,

        /// <summary>
        /// Sort
        /// </summary>
        Sort,

        /// <summary>
        /// Constrained Sort (physical paging - Limit and Skip)
        /// </summary>
        ConstrainedSort,

        /// <summary>
        /// GroupBy
        /// </summary>
        GroupBy,

        /// <summary>
        /// GroupByInto (projects the group as well)
        /// </summary>
        GroupByInto,

        /// <summary>
        /// UnionAll
        /// </summary>
        UnionAll,
        /// <summary>
        /// Intersect
        /// </summary>
        Intersect,
        /// <summary>
        /// Except
        /// </summary>
        Except,

        /// <summary>
        /// Distinct
        /// </summary>
        Distinct,

        /// <summary>
        /// Select a single row from a subquery
        /// </summary>
        SingleRow,

        /// <summary>
        /// A table with exactly one row
        /// </summary>
        SingleRowTable,

        #endregion

        #region AncillaryOpType
        /// <summary>
        /// Variable definition
        /// </summary>
        VarDef,
        /// <summary>
        /// List of variable definitions
        /// </summary>
        VarDefList,
        #endregion
        
        #region RulePatternOpType
        /// <summary>
        /// Leaf
        /// </summary>
        Leaf,
        #endregion

        #region PhysicalOpType
        /// <summary>
        /// Physical Project
        /// </summary>
        PhysicalProject,

        /// <summary>
        /// single-stream nest aggregation
        /// </summary>
        SingleStreamNest,
        /// <summary>
        /// multi-stream nest aggregation
        /// </summary>
        MultiStreamNest,
        #endregion

        /// <summary>
        /// NotValid
        /// </summary>
        MaxMarker,
        NotValid = MaxMarker
    }

    /// <summary>
    /// Represents an operator 
    /// </summary>
    internal abstract class Op
    {
        #region private state
        private OpType m_opType;
        #endregion

        #region constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        internal Op(OpType opType) 
        { 
            m_opType = opType; 
        }
        #endregion

        #region public methods
        /// <summary>
        /// Represents an unknown arity. Usually for Ops that can have a varying number of Args
        /// </summary>
        internal const int ArityVarying = -1;
 
        /// <summary>
        /// Kind of Op
        /// </summary>
        internal OpType OpType { get { return m_opType; } }

        /// <summary>
        /// The Arity of this Op (ie) how many arguments can it have.
        /// Returns -1 if the arity is not known a priori
        /// </summary>
        internal virtual int Arity { get { return ArityVarying; } }

        /// <summary>
        /// Is this a ScalarOp
        /// </summary>
        internal virtual bool IsScalarOp { get { return false; } }

        /// <summary>
        /// Is this a RulePatternOp
        /// </summary>
        internal virtual bool IsRulePatternOp { get { return false; } }

        /// <summary>
        /// Is this a RelOp
        /// </summary>
        internal virtual bool IsRelOp { get { return false; } }

        /// <summary>
        /// Is this an AncillaryOp
        /// </summary>
        internal virtual bool IsAncillaryOp { get { return false; } }

        /// <summary>
        /// Is this a PhysicalOp
        /// </summary>
        internal virtual bool IsPhysicalOp { get { return false; } }

        /// <summary>
        /// Is the other Op equivalent?
        /// </summary>
        /// <param name="other">the other Op to compare</param>
        /// <returns>true, if the Ops are equivalent</returns>
        internal virtual bool IsEquivalent(Op other)
        {
            return false;
        }

        /// <summary>
        /// Simple mechanism to get the type for an Op. Applies only to scalar and ancillaryOps
        /// </summary>
        internal virtual TypeUsage Type
        {
            get { return null; }
            set { throw System.Data.Entity.Error.NotSupported(); }
        }

        /// <summary>
        /// Visitor pattern method
        /// </summary>
        /// <param name="v">The BasicOpVisitor that is visiting this Op</param>
        /// <param name="n">The Node that references this Op</param>
        [DebuggerNonUserCode]
        internal virtual void Accept(BasicOpVisitor v, Node n) 
        { 
            v.Visit(this, n); 
        }
        
        /// <summary>
        /// Visitor pattern method for visitors with a return value
        /// </summary>
        /// <param name="v">The visitor</param>
        /// <param name="n">The node in question</param>
        /// <returns>An instance of TResultType</returns>
        [DebuggerNonUserCode]
        internal virtual TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n) 
        { 
            return v.Visit(this, n);
        }
        #endregion
    }

    /// <summary>
    /// All scalars fall into this category
    /// </summary>
    internal abstract class ScalarOp : Op
    {
        #region private state
        private TypeUsage m_type;
        #endregion

        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType">kind of Op</param>
        /// <param name="type">type of value produced by this Op</param>
        internal ScalarOp(OpType opType, TypeUsage type)
            : this(opType)
        {
            Debug.Assert(type != null, "No type specified for ScalarOp");
            m_type = type;
        }

        protected ScalarOp(OpType opType) : base(opType) { }
        #endregion

        #region public methods
        /// <summary>
        /// ScalarOp
        /// </summary>
        internal override bool IsScalarOp { get { return true; } }

        /// <summary>
        /// Two scalarOps are equivalent (usually) if their OpTypes and types are the 
        /// same. Obviously, their arguments need to be equivalent as well - but that's
        /// checked elsewhere
        /// </summary>
        /// <param name="other">The other Op to compare against</param>
        /// <returns>true, if the Ops are indeed equivalent</returns>
        internal override bool IsEquivalent(Op other)
        {
            return (other.OpType == this.OpType && TypeSemantics.IsStructurallyEqual(this.Type, other.Type));
        }

        /// <summary>
        /// Datatype of result
        /// </summary>
        internal override TypeUsage Type 
        { 
            get { return m_type; } 
            set { m_type = value; } 
        }
        
        /// <summary>
        /// Is this an Aggregate
        /// </summary>
        internal virtual bool IsAggregateOp 
        {
            get{return false;}
        }
        #endregion
    }

    /// <summary>
    /// All relational operators - filter, project, join etc.
    /// </summary>
    internal abstract class RelOp : Op
    {
        #region constructors
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="opType">kind of Op</param>
        internal RelOp(OpType opType) : base(opType) { }
        #endregion

        #region public methods
        /// <summary>
        /// RelOp
        /// </summary>
        internal override bool IsRelOp { get { return true; } }
        #endregion
    }

    /// <summary>
    /// AncillaryOp
    /// </summary>
    internal abstract class AncillaryOp : Op
    {
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType">kind of Op</param>
        internal AncillaryOp(OpType opType) : base(opType) { }
        #endregion

        #region public methods
        /// <summary>
        /// AncillaryOp
        /// </summary>
        internal override bool IsAncillaryOp { get { return true; } }
        #endregion
    }

    /// <summary>
    /// Represents all physical operators
    /// </summary>
    internal abstract class PhysicalOp : Op
    {
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType">the op type</param>
        internal PhysicalOp(OpType opType) : base(opType) { }
        #endregion

        #region public methods
        /// <summary>
        /// This is a physical Op
        /// </summary>
        internal override bool IsPhysicalOp { get { return true; } }
        #endregion
    }

    /// <summary>
    /// All rule pattern operators - Leaf, Tree
    /// </summary>
    internal abstract class RulePatternOp : Op
    {
        #region constructors
        /// <summary>
        /// Default constructor
        /// </summary>
        /// <param name="opType">kind of Op</param>
        internal RulePatternOp(OpType opType) : base(opType) { }
        #endregion

        #region public methods
        /// <summary>
        /// RulePatternOp
        /// </summary>
        internal override bool IsRulePatternOp { get { return true; } }
        #endregion
    }
}

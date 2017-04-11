//---------------------------------------------------------------------
// <copyright file="RelOps.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Diagnostics;
using System.Globalization;
using System.Text;

namespace System.Data.Query.InternalTrees
{
    internal abstract class ScanTableBaseOp : RelOp
    {
        #region private state
        private Table m_table;
        #endregion

        #region constructors
        protected ScanTableBaseOp(OpType opType, Table table)
            : base(opType)
        {
            m_table = table;
        }
        protected ScanTableBaseOp(OpType opType)
            : base(opType)
        { }
        #endregion

        #region public methods
        /// <summary>
        /// Get the table instance produced by this Op
        /// </summary>
        internal Table Table { get { return m_table; } }
        #endregion
    }

    /// <summary>
    /// Scans a table
    /// </summary>
    internal sealed class ScanTableOp : ScanTableBaseOp
    {
        #region constructors
        /// <summary>
        /// Scan constructor
        /// </summary>
        /// <param name="table"></param>
        internal ScanTableOp(Table table)
            : base(OpType.ScanTable, table)
        {
        }

        private ScanTableOp() : base(OpType.ScanTable) { }
#endregion

        #region public methods
        /// <summary>
        /// Only to be used for pattern matches
        /// </summary>
        internal static readonly ScanTableOp Pattern = new ScanTableOp();

        /// <summary>
        /// No children
        /// </summary>
        internal override int Arity {get {return 0;} }

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
    /// Scans a view - very similar to a ScanTable
    /// </summary>
    internal sealed class ScanViewOp : ScanTableBaseOp
    {
        #region constructors
        /// <summary>
        /// Scan constructor
        /// </summary>
        /// <param name="table"></param>
        internal ScanViewOp(Table table)
            : base(OpType.ScanView, table)
        {
        }
        private ScanViewOp() : base(OpType.ScanView) { }
#endregion

        #region public methods
        /// <summary>
        /// Only to be used for pattern matches
        /// </summary>
        internal static readonly ScanViewOp Pattern = new ScanViewOp();

        /// <summary>
        /// Exactly 1 child
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
    /// Scans a virtual extent (ie) a transient collection
    /// </summary>
    internal sealed class UnnestOp : RelOp
    {
        #region private state
        private Table m_table;
        private Var m_var;
        #endregion

        #region constructors
        internal UnnestOp(Var v, Table t) : this()
        {
            m_var = v;
            m_table = t;
        }
        private UnnestOp()
            : base(OpType.Unnest)
        {
        }
#endregion

        #region publics
        internal static readonly UnnestOp Pattern = new UnnestOp();

        /// <summary>
        /// The (collection-typed) Var that's being unnested
        /// </summary>
        internal Var Var { get { return m_var; } }

        /// <summary>
        /// The table instance produced by this Op
        /// </summary>
        internal Table Table { get { return m_table; } }

        /// <summary>
        /// Exactly 1 child
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
    /// Base class for all Join operations
    /// </summary>
    internal abstract class JoinBaseOp : RelOp
    {
        #region constructors
        internal JoinBaseOp(OpType opType) : base(opType) { }
        #endregion

        #region public surface
        /// <summary>
        /// 3 children - left, right, pred
        /// </summary>
        internal override int Arity { get { return 3; } }
        #endregion
    }

    /// <summary>
    /// A CrossJoin (n-way)
    /// </summary>
    internal sealed class CrossJoinOp : JoinBaseOp
    {
        #region constructors
        private CrossJoinOp() : base(OpType.CrossJoin) { }
        #endregion

        #region public methods
        /// <summary>
        /// Singleton instance
        /// </summary>
        internal static readonly CrossJoinOp Instance = new CrossJoinOp();
        internal static readonly CrossJoinOp Pattern = CrossJoinOp.Instance;

        /// <summary>
        /// varying number of children (but usually greater than 1)
        /// </summary>
        internal override int Arity { get { return ArityVarying; } }

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
    /// An InnerJoin
    /// </summary>
    internal sealed class InnerJoinOp : JoinBaseOp
    {
        #region constructors
        private InnerJoinOp() : base(OpType.InnerJoin) { }
        #endregion

        #region public methods
        internal static readonly InnerJoinOp Instance = new InnerJoinOp();
        internal static readonly InnerJoinOp Pattern = InnerJoinOp.Instance;

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
    /// A LeftOuterJoin
    /// </summary>
    internal sealed class LeftOuterJoinOp : JoinBaseOp
    {
        #region constructors
        private LeftOuterJoinOp() : base(OpType.LeftOuterJoin) { }
        #endregion

        #region public methods
        internal static readonly LeftOuterJoinOp Instance = new LeftOuterJoinOp();
        internal static readonly LeftOuterJoinOp Pattern = LeftOuterJoinOp.Instance;

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
    /// A FullOuterJoin
    /// </summary>
    internal sealed class FullOuterJoinOp : JoinBaseOp
    {
        #region private constructors
        private FullOuterJoinOp() : base(OpType.FullOuterJoin) { }
        #endregion

        #region public methods
        internal static readonly FullOuterJoinOp Instance = new FullOuterJoinOp();
        internal static readonly FullOuterJoinOp Pattern = FullOuterJoinOp.Instance;

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
    /// Base class for all Apply Ops
    /// </summary>
    internal abstract class ApplyBaseOp : RelOp
    {
        #region constructors
        internal ApplyBaseOp(OpType opType) : base(opType) { }
        #endregion

        #region public surface
        /// <summary>
        /// 2 children - left, right
        /// </summary>
        internal override int Arity { get { return 2; } }
        #endregion
    }

    /// <summary>
    /// CrossApply
    /// </summary>
    internal sealed class CrossApplyOp : ApplyBaseOp
    {
        #region constructors
        private CrossApplyOp() : base(OpType.CrossApply) { }
        #endregion

        #region public methods
        internal static readonly CrossApplyOp Instance = new CrossApplyOp();
        internal static readonly CrossApplyOp Pattern = CrossApplyOp.Instance;

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
    /// OuterApply
    /// </summary>
    internal sealed class OuterApplyOp : ApplyBaseOp
    {
        #region constructors
        private OuterApplyOp() : base(OpType.OuterApply) { }
        #endregion

        #region public methods
        internal static readonly OuterApplyOp Instance = new OuterApplyOp();
        internal static readonly OuterApplyOp Pattern = OuterApplyOp.Instance;

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
    /// FilterOp
    /// </summary>
    internal sealed class FilterOp : RelOp
    {
        #region constructors
        private FilterOp() : base(OpType.Filter) { }
        #endregion

        #region public methods
        internal static readonly FilterOp Instance = new FilterOp();
        internal static readonly FilterOp Pattern = FilterOp.Instance;

        /// <summary>
        /// 2 children - input, pred
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
    /// ProjectOp
    /// </summary>
    internal sealed class ProjectOp : RelOp
    {
        #region private state
        private VarVec m_vars;
        #endregion

        #region constructors
        private ProjectOp()
            : base(OpType.Project)
        { }
        internal ProjectOp(VarVec vars) : this()
        {
            Debug.Assert(null != vars, "null vars?");
            Debug.Assert(!vars.IsEmpty, "empty varlist?");
            m_vars = vars;
        }
        #endregion

        #region public methods
        internal static readonly ProjectOp Pattern = new ProjectOp();

        /// <summary>
        /// 2 children - input, projections (VarDefList)
        /// </summary>
        internal override int Arity { get { return 2; } }

        /// <summary>
        /// The Vars projected by this Op
        /// </summary>
        internal VarVec Outputs { get { return m_vars; } }

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
    /// A Sortkey
    /// </summary>
    internal class SortKey
    {
        #region private state
        private Var m_var;
        private bool m_asc;
        private string m_collation;
        #endregion

        #region constructors
        internal SortKey(Var v, bool asc, string collation)
        {
            m_var = v;
            m_asc = asc;
            m_collation = collation;
        }
        #endregion

        #region public methods
        /// <summary>
        /// The Var being sorted
        /// </summary>
        internal Var Var
        {
            get { return m_var; }
            set { m_var = value; }
        }

        /// <summary>
        /// Is this a sort asc, or a sort desc
        /// </summary>
        internal bool AscendingSort { get { return m_asc; } }

        /// <summary>
        /// An optional collation (only for string types)
        /// </summary>
        internal string Collation { get { return m_collation; } }
        #endregion
    }

    /// <summary>
    /// Base type for SortOp and ConstrainedSortOp
    /// </summary>
    internal abstract class SortBaseOp : RelOp
    {
        #region private state
        private List<SortKey> m_keys;
        #endregion

        #region Constructors
        // Pattern constructor
        internal SortBaseOp(OpType opType)
            : base(opType)
        {
            Debug.Assert(opType == OpType.Sort || opType == OpType.ConstrainedSort, "SortBaseOp OpType must be Sort or ConstrainedSort");
        }

        internal SortBaseOp(OpType opType, List<SortKey> sortKeys)
            : this(opType)
        {
            m_keys = sortKeys;
        }

        #endregion

        /// <summary>
        /// Sort keys
        /// </summary>
        internal List<SortKey> Keys { get { return m_keys; } }
    }

    /// <summary>
    /// A SortOp
    /// </summary>
    internal sealed class SortOp : SortBaseOp
    {
        #region constructors
        private SortOp() : base(OpType.Sort) { }

        internal SortOp(List<SortKey> sortKeys) : base(OpType.Sort, sortKeys) {}
        #endregion

        #region public methods
        internal static readonly SortOp Pattern = new SortOp();

        /// <summary>
        /// 1 child - the input, SortOp must not contain local VarDefs
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
    /// A Constrained SortOp. Used to represent physical paging (skip, limit, skip + limit) operations.
    /// </summary>
    internal sealed class ConstrainedSortOp : SortBaseOp
    {
        #region private state
        private bool _withTies;
        #endregion

        #region constructors
        // Pattern constructor
        private ConstrainedSortOp() : base(OpType.ConstrainedSort) { }

        internal ConstrainedSortOp(List<SortKey> sortKeys, bool withTies)
            : base(OpType.ConstrainedSort, sortKeys)
        {
            _withTies = withTies;
        }
        #endregion

        #region public methods
        internal bool WithTies { get { return _withTies; } set { _withTies = value; } }

        internal static readonly ConstrainedSortOp Pattern = new ConstrainedSortOp();

        /// <summary>
        /// 3 children - the input, a possibly NullOp limit and a possibly NullOp skip count.
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
    /// GroupByBaseOp
    /// </summary>
    internal abstract class GroupByBaseOp : RelOp
    {
        #region private state
        private VarVec m_keys;
        private VarVec m_outputs;
        #endregion

        #region constructors
        protected GroupByBaseOp(OpType opType) : base(opType) 
        {
            Debug.Assert(opType == OpType.GroupBy || opType == OpType.GroupByInto, "GroupByBaseOp OpType must be GroupBy or GroupByInto");
        }
        internal GroupByBaseOp(OpType opType, VarVec keys, VarVec outputs)
            : this(opType)
        {
            m_keys = keys;
            m_outputs = outputs;
        }
        #endregion

        #region public methods
        /// <summary>
        /// GroupBy keys
        /// </summary>
        internal VarVec Keys { get { return m_keys; } }

        /// <summary>
        /// All outputs of this Op - includes keys and aggregates
        /// </summary>
        internal VarVec Outputs { get { return m_outputs; } }

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
    /// GroupByOp
    /// </summary>
    internal sealed class GroupByOp : GroupByBaseOp
    {
        #region constructors
        private GroupByOp() : base(OpType.GroupBy) { }
        internal GroupByOp(VarVec keys, VarVec outputs)
            : base(OpType.GroupBy, keys, outputs)
        {
        }
        #endregion

        #region public methods
        internal static readonly GroupByOp Pattern = new GroupByOp();

        /// <summary>
        /// 3 children - input, keys (vardeflist), aggregates (vardeflist)
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
    /// GroupByIntoOp
    /// </summary>
    internal sealed class GroupByIntoOp : GroupByBaseOp
    {
        #region private state
        private readonly VarVec m_inputs;
        #endregion 

        #region constructors
        private GroupByIntoOp() : base(OpType.GroupByInto) { }
        internal GroupByIntoOp(VarVec keys, VarVec inputs, VarVec outputs)
            : base(OpType.GroupByInto, keys, outputs)
        {
            this.m_inputs = inputs;
        }
        #endregion

        #region public methods
        /// <summary>
        /// GroupBy keys
        /// </summary>
        internal VarVec Inputs { get { return m_inputs; } }

        internal static readonly GroupByIntoOp Pattern = new GroupByIntoOp();

        /// <summary>
        /// 4 children - input, keys (vardeflist), aggregates (vardeflist), groupaggregates (vardeflist)
        /// </summary>
        internal override int Arity { get { return 4; } }

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
    /// Base class for set operations - union, intersect, except
    /// </summary>
    internal abstract class SetOp : RelOp
    {
        #region private state
        private VarMap[] m_varMap;
        private VarVec m_outputVars;
        #endregion

        #region constructors
        internal SetOp(OpType opType, VarVec outputs, VarMap left, VarMap right)
            : this(opType)
        {
            m_varMap = new VarMap[2];
            m_varMap[0] = left;
            m_varMap[1] = right;
            m_outputVars = outputs;
        }
        protected SetOp(OpType opType) : base(opType)
        {
        }
        #endregion

        #region public methods

        /// <summary>
        /// 2 children - left, right
        /// </summary>
        internal override int Arity { get { return 2; } }

        /// <summary>
        /// Map of result vars to the vars of each branch of the setOp
        /// </summary>
        internal VarMap[] VarMap { get { return m_varMap; } }

        /// <summary>
        /// Get the set of output vars produced
        /// </summary>
        internal VarVec Outputs { get { return m_outputVars; } }
        #endregion
    }

    /// <summary>
    /// UnionAll (ie) no duplicate elimination
    /// </summary>
    internal sealed class UnionAllOp : SetOp
    {
        #region private state
        private Var m_branchDiscriminator;
        #endregion 

        #region constructors
        private UnionAllOp() : base(OpType.UnionAll) { }

        internal UnionAllOp(VarVec outputs, VarMap left, VarMap right, Var branchDiscriminator) : base(OpType.UnionAll, outputs, left, right) 
        { 
            m_branchDiscriminator = branchDiscriminator; 
        }
        #endregion

        #region public methods
        internal static readonly UnionAllOp Pattern = new UnionAllOp();

        /// <summary>
        /// Returns the branch discriminator var for this op.  It may be null, if
        /// we haven't been through key pullup yet.
        /// </summary>
        internal Var BranchDiscriminator { get { return m_branchDiscriminator; } }

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
    /// An IntersectOp
    /// </summary>
    internal sealed class IntersectOp : SetOp
    {
        #region constructors
        private IntersectOp() : base(OpType.Intersect) { }
        internal IntersectOp(VarVec outputs, VarMap left, VarMap right) : base(OpType.Intersect, outputs, left,right) { }
#endregion

        #region public methods
        internal static readonly IntersectOp Pattern = new IntersectOp();

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
    /// ExceptOp (Minus)
    /// </summary>
    internal sealed class ExceptOp : SetOp
    {
        #region constructors
        private ExceptOp() : base(OpType.Except) { }
        internal ExceptOp(VarVec outputs, VarMap left, VarMap right) : base(OpType.Except, outputs, left, right) { }
        #endregion

        #region public methods
        internal static readonly ExceptOp Pattern = new ExceptOp();

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
    /// DistinctOp
    /// </summary>
    internal sealed class DistinctOp : RelOp
    {
        #region private state
        private VarVec m_keys;
        #endregion

        #region constructors
        private DistinctOp() : base(OpType.Distinct)
        {
        }
        internal DistinctOp(VarVec keyVars) : this()
        {
            Debug.Assert(keyVars != null);
            Debug.Assert(!keyVars.IsEmpty);
            m_keys = keyVars;
        }
        #endregion

        #region public methods
        internal static readonly DistinctOp Pattern = new DistinctOp();

        /// <summary>
        /// 1 child - input
        /// </summary>
        internal override int Arity { get { return 1; } }

        /// <summary>
        /// Get "key" vars for the distinct
        /// </summary>
        internal VarVec Keys { get { return m_keys; } }

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
    /// Selects out a single row from a underlying subquery. Two flavors of this Op exist.
    /// The first flavor enforces the single-row-ness (ie) an error is raised if the
    /// underlying subquery produces more than one row.
    /// The other flavor simply choses any row from the input
    /// </summary>
    internal sealed class SingleRowOp : RelOp
    {
        #region constructors
        private SingleRowOp() : base(OpType.SingleRow) { }
        #endregion

        #region public methods
        /// <summary>
        /// Singleton instance
        /// </summary>
        internal static readonly SingleRowOp Instance = new SingleRowOp();
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly SingleRowOp Pattern = Instance;

        /// <summary>
        /// 1 child - input
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
    /// Represents a table with a single row
    /// </summary>
    internal sealed class SingleRowTableOp : RelOp
    {
        #region constructors
        private SingleRowTableOp() : base(OpType.SingleRowTable) { }
        #endregion

        #region public methods
        /// <summary>
        /// Singleton instance
        /// </summary>
        internal static readonly SingleRowTableOp Instance = new SingleRowTableOp();
        /// <summary>
        /// Pattern for transformation rules
        /// </summary>
        internal static readonly SingleRowTableOp Pattern = Instance;

        /// <summary>
        /// 0 children
        /// </summary>
        internal override int Arity { get { return 0; } }

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

//---------------------------------------------------------------------
// <copyright file="JoinGraph.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
//using System.Diagnostics; // Please use PlanCompiler.Assert instead of Debug.Assert in this class...

// It is fine to use Debug.Assert in cases where you assert an obvious thing that is supposed
// to prevent from simple mistakes during development (e.g. method argument validation 
// in cases where it was you who created the variables or the variables had already been validated or 
// in "else" clauses where due to code changes (e.g. adding a new value to an enum type) the default 
// "else" block is chosen why the new condition should be treated separately). This kind of asserts are 
// (can be) helpful when developing new code to avoid simple mistakes but have no or little value in 
// the shipped product. 
// PlanCompiler.Assert *MUST* be used to verify conditions in the trees. These would be assumptions 
// about how the tree was built etc. - in these cases we probably want to throw an exception (this is
// what PlanCompiler.Assert does when the condition is not met) if either the assumption is not correct 
// or the tree was built/rewritten not the way we thought it was.
// Use your judgment - if you rather remove an assert than ship it use Debug.Assert otherwise use
// PlanCompiler.Assert.

using System.Globalization;
using System.Linq;

using System.Data.Query.InternalTrees;
using md = System.Data.Metadata.Edm;

//
// The JoinGraph module is responsible for performing the following kinds of 
// join elimination.
// This module deals with the following kinds of joins
//    * Self-joins: The join can be eliminated, and either of the table instances can be 
//                  used instead
//    * Implied self-joins: Same as above
//    * PK-FK joins: (More generally, UniqueKey-FK joins): Eliminate the join, and use just the FK table, if no 
//       column of the PK table is used (other than the join condition)
//    * PK-PK joins: Eliminate the right side table, if we have a left-outer join
//
// This module is organized into the following phases.
//   * Building an Augmented Tree: In this phase, the original node tree is annotated
//       with additional information, and a new "augmented" tree is built up
//   * Building up Join Edges: In this phase, the augmented tree is used to populate
//       the join graph with equi-join edges
//   * Generating transitive edges: Generate transitive join edges
//   * Parent-Child (PK-FK) Join Elimination: We walk through the list of join edges, and 
//       eliminate any redundant tables in parent-child joins
//   * Self-join Elimination: We walk through the list of join edges, and eliminate
//       any redundant tables
//   * Rebuilding the node tree: The augmented node tree is now converted back into 
//       a regular node tree.
//
namespace System.Data.Query.PlanCompiler
{
    #region AugmentedNode
    //
    // This region describes a number of classes that are used to build an annotated 
    // (or augmented) node tree. There are 3 main classes defined here
    //    AugmentedNode - this is the base class for all annotations. This class 
    //       wraps a Node, an id for the node (where the "id" is assigned in DFS order),
    //       and a list of children. All Nodes that are neither joins, nor scanTables
    //       are represented by this class
    //    AugmentedTableNode - the augmentedTableNode is a subclass of AugmentedNode,
    //       and represents a ScanTable node. In addition to the information above, this
    //        class keeps track of all join edges that this node participates in, 
    //        whether this table has been eliminated, and finally, how high in the tree
    //        this node is visible
    //    AugmentedJoinNode - represents all joins (cross-joins, leftouter, fullouter
    //        and innerjoins). This class represents a number of column equijoin conditions
    //        via the LeftVars and RightVars properties, and also keeps track of additional
    //        (non-equijoin column) join predicates
    //

    /// <summary>
    /// Additional information for a node. 
    /// </summary>
    internal class AugmentedNode
    {
        #region private state
        private int m_id;
        private Node m_node;
        protected AugmentedNode m_parent;
        private List<AugmentedNode> m_children;
        private readonly List<JoinEdge> m_joinEdges = new List<JoinEdge>();
        #endregion

        #region constructors
        /// <summary>
        /// basic constructor
        /// </summary>
        /// <param name="id">Id for this node</param>
        /// <param name="node">current node</param>
        internal AugmentedNode(int id, Node node)
            : this(id, node, new List<AugmentedNode>())
        {
        }

        /// <summary>
        /// Yet another constructor
        /// </summary>
        /// <param name="id">Id for this node</param>
        /// <param name="node">current node</param>
        /// <param name="children">list of children</param>
        internal AugmentedNode(int id, Node node, List<AugmentedNode> children)
        {
            m_id = id;
            m_node = node;
            m_children = children;
            PlanCompiler.Assert(children != null, "null children (gasp!)");
            foreach (AugmentedNode chi in m_children)
            {
                chi.m_parent = this;
            }
        }
        #endregion

        #region public properties
        /// <summary>
        /// Id of this node
        /// </summary>
        internal int Id { get { return m_id; } }
        /// <summary>
        /// The node
        /// </summary>
        internal Node Node { get { return m_node; } }

        /// <summary>
        /// Parent node
        /// </summary>
        internal AugmentedNode Parent
        {
            get { return m_parent; }
        }

        /// <summary>
        /// List of children
        /// </summary>
        internal List<AugmentedNode> Children
        {
            get { return m_children; }
        }

        /// <summary>
        /// List of directed edges in which:
        ///     - If this is an AugmentedTableNode, it is the "left" table
        ///     - If it is an AugumentedJoinNode, it is the join on which the edge is based
        /// </summary>
        internal List<JoinEdge> JoinEdges
        {
            get { return m_joinEdges; }
        }
        #endregion
    }

    /// <summary>
    /// Additional information for a "Table" node
    /// </summary>
    internal sealed class AugmentedTableNode : AugmentedNode
    {
        #region private state
        private int m_lastVisibleId;
        private Table m_table;

        // The replacement table 
        private AugmentedTableNode m_replacementTable;

        // Is this table being moved
        private int m_newLocationId;

        // List of columns of this table that are nullable (and must have nulls pruned out)
        private VarVec m_nullableColumns;

        #endregion

        #region constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="id">node id</param>
        /// <param name="node">scan table node</param>
        internal AugmentedTableNode(int id, Node node) : base(id, node)
        {
            ScanTableOp scanTableOp = (ScanTableOp)node.Op;
            m_table = scanTableOp.Table;
            m_lastVisibleId = id;
            m_replacementTable = this;
            m_newLocationId = id;
        }
        #endregion

        #region public properties
        /// <summary>
        /// The Table
        /// </summary>
        internal Table Table { get { return m_table; } }

        /// <summary>
        /// The highest node (id) at which this table is visible
        /// </summary>
        internal int LastVisibleId
        {
            get { return m_lastVisibleId; }
            set { m_lastVisibleId = value; }
        }

        /// <summary>
        /// Has this table been eliminated
        /// </summary>
        internal bool IsEliminated
        {
            get { return m_replacementTable != this; }
        }

        /// <summary>
        /// The replacement table (if any) for this table
        /// </summary>
        internal AugmentedTableNode ReplacementTable
        {
            get { return m_replacementTable; }
            set { m_replacementTable = value; }
        }

        /// <summary>
        /// New location for this table
        /// </summary>
        internal int NewLocationId
        {
            get { return m_newLocationId; }
            set { m_newLocationId = value; }
        }

        /// <summary>
        /// Has this table "moved" ?
        /// </summary>
        internal bool IsMoved
        {
            get { return m_newLocationId != this.Id; }
        }

        /// <summary>
        /// Get the list of nullable columns (that require special handling)
        /// </summary>
        internal VarVec NullableColumns
        {
            get { return m_nullableColumns; }
            set { m_nullableColumns = value; }
        }
        #endregion
    }

    /// <summary>
    /// Additional information for a JoinNode 
    /// </summary>
    internal sealed class AugmentedJoinNode : AugmentedNode
    {
        #region private state
        private List<ColumnVar> m_leftVars;
        private List<ColumnVar> m_rightVars;
        private Node m_otherPredicate;
        #endregion

        #region constructors
        /// <summary>
        /// basic constructor
        /// </summary>
        /// <param name="id">current node id</param>
        /// <param name="node">the join node</param>
        /// <param name="leftChild">left side of the join (innerJoin, LOJ and FOJ only)</param>
        /// <param name="rightChild">right side of the join</param>
        /// <param name="leftVars">left-side equijoin vars</param>
        /// <param name="rightVars">right-side equijoin vars</param>
        /// <param name="otherPredicate">any remaining predicate</param>
        internal AugmentedJoinNode(int id, Node node,
            AugmentedNode leftChild, AugmentedNode rightChild,
            List<ColumnVar> leftVars, List<ColumnVar> rightVars,
            Node otherPredicate)
            : this(id, node, new List<AugmentedNode>(new AugmentedNode[] { leftChild, rightChild }))
        {
            m_otherPredicate = otherPredicate;
            m_rightVars = rightVars;
            m_leftVars = leftVars;
        }

        /// <summary>
        /// Yet another constructor - used for crossjoins
        /// </summary>
        /// <param name="id">node id</param>
        /// <param name="node">current node</param>
        /// <param name="children">list of children</param>
        internal AugmentedJoinNode(int id, Node node, List<AugmentedNode> children)
            : base(id, node, children)
        {
            m_leftVars = new List<ColumnVar>();
            m_rightVars = new List<ColumnVar>();
        }

        #endregion

        #region public properties
        /// <summary>
        /// Non-equijoin predicate
        /// </summary>
        internal Node OtherPredicate { get { return m_otherPredicate; } }
        /// <summary>
        /// Equijoin columns of the left side
        /// </summary>
        internal List<ColumnVar> LeftVars { get { return m_leftVars; } }
        /// <summary>
        /// Equijoin columns of the right side
        /// </summary>
        internal List<ColumnVar> RightVars { get { return m_rightVars; } }
        #endregion

        #region private methods

        #endregion
    }
    #endregion

    #region JoinGraph
    /// <summary>
    /// The only join kinds we care about
    /// </summary>
    internal enum JoinKind
    {
        Inner,
        LeftOuter
    }

    /// <summary>
    /// Represents an "edge" in the join graph.
    /// A JoinEdge is a directed equijoin between the left and the right table. The equijoin
    /// columns are represented by the LeftVars and the RightVars properties
    /// </summary>
    internal class JoinEdge
    {
        #region private state
        private AugmentedTableNode m_left;
        private AugmentedTableNode m_right;
        private AugmentedJoinNode m_joinNode;
        private JoinKind m_joinKind;
        private List<ColumnVar> m_leftVars;
        private List<ColumnVar> m_rightVars;
        #endregion

        #region constructors
        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="left">the left table</param>
        /// <param name="right">the right table</param>
        /// <param name="joinNode">the owner join node</param>
        /// <param name="joinKind">the Join Kind</param>
        /// <param name="leftVars">list of equijoin columns of the left table</param>
        /// <param name="rightVars">equijoin columns of the right table</param>
        private JoinEdge(AugmentedTableNode left, AugmentedTableNode right,
            AugmentedJoinNode joinNode, JoinKind joinKind,
            List<ColumnVar> leftVars, List<ColumnVar> rightVars)
        {
            m_left = left;
            m_right = right;
            m_joinKind = joinKind;
            m_joinNode = joinNode;
            m_leftVars = leftVars;
            m_rightVars = rightVars;
            PlanCompiler.Assert(m_leftVars.Count == m_rightVars.Count, "Count mismatch: " + m_leftVars.Count + "," + m_rightVars.Count);
        }
        #endregion

        #region public apis


        /// <summary>
        /// The left table
        /// </summary>
        internal AugmentedTableNode Left { get { return m_left; } }
        /// <summary>
        /// The right table of the join
        /// </summary>
        internal AugmentedTableNode Right { get { return m_right; } }
        /// <summary>
        /// The underlying join node, may be null
        /// </summary>     
        internal AugmentedJoinNode JoinNode { get { return m_joinNode; } }

        /// <summary>
        /// The join kind
        /// </summary>
        internal JoinKind JoinKind { get { return m_joinKind; } set { m_joinKind = value; } }

        /// <summary>
        /// Equijoin columns of the left table
        /// </summary>
        internal List<ColumnVar> LeftVars { get { return m_leftVars; } }
        /// <summary>
        /// Equijoin columns of the right table
        /// </summary>
        internal List<ColumnVar> RightVars { get { return m_rightVars; } }

        /// <summary>
        /// Is this join edge useless?
        /// </summary>
        internal bool IsEliminated
        {
            get { return this.Left.IsEliminated || this.Right.IsEliminated; }
        }

        /// <summary>
        /// Factory method
        /// </summary>
        /// <param name="left">left table</param>
        /// <param name="right">right table</param>
        /// <param name="joinNode">the owner join node</param>
        /// <param name="leftVar">equijoin column of the left table</param>
        /// <param name="rightVar">equijoin column of the right table</param>
        /// <returns>the new join edge</returns>
        internal static JoinEdge CreateJoinEdge(AugmentedTableNode left, AugmentedTableNode right,
            AugmentedJoinNode joinNode,
            ColumnVar leftVar, ColumnVar rightVar)
        {
            List<ColumnVar> leftVars = new List<ColumnVar>();
            List<ColumnVar> rightVars = new List<ColumnVar>();
            leftVars.Add(leftVar);
            rightVars.Add(rightVar);

            OpType joinOpType = joinNode.Node.Op.OpType;
            PlanCompiler.Assert((joinOpType == OpType.LeftOuterJoin || joinOpType == OpType.InnerJoin),
                "Unexpected join type for join edge: " + joinOpType);

            JoinKind joinKind = joinOpType == OpType.LeftOuterJoin ? JoinKind.LeftOuter : JoinKind.Inner;

            JoinEdge joinEdge = new JoinEdge(left, right, joinNode, joinKind, leftVars, rightVars);
            return joinEdge;
        }

        /// <summary>
        /// Creates a transitively generated join edge
        /// </summary>
        /// <param name="left">the left table</param>
        /// <param name="right">the right table</param>
        /// <param name="joinKind">the join kind</param>
        /// <param name="leftVars">left equijoin vars</param>
        /// <param name="rightVars">right equijoin vars</param>
        /// <returns>the join edge</returns>
        internal static JoinEdge CreateTransitiveJoinEdge(AugmentedTableNode left, AugmentedTableNode right, JoinKind joinKind,
            List<ColumnVar> leftVars, List<ColumnVar> rightVars)
        {
            JoinEdge joinEdge = new JoinEdge(left, right, null, joinKind, leftVars, rightVars);
            return joinEdge;
        }

        /// <summary>
        /// Add a new "equi-join" condition to this edge
        /// </summary>
        /// <param name="joinNode">join node producing this condition</param>
        /// <param name="leftVar">the left-side column</param>
        /// <param name="rightVar">the right-side column</param>
        /// <returns>true, if this condition can be added</returns>
        internal bool AddCondition(AugmentedJoinNode joinNode, ColumnVar leftVar, ColumnVar rightVar)
        {
            if (joinNode != m_joinNode)
            {
                return false;
            }
            m_leftVars.Add(leftVar);
            m_rightVars.Add(rightVar);
            return true;
        }
        #endregion
    }

    /// <summary>
    /// Represents a join graph. The uber-class for join elimination
    /// </summary>
    internal class JoinGraph
    {
        #region private state
        private Command m_command;
        private AugmentedJoinNode m_root;
        private List<AugmentedNode> m_vertexes;
        private List<AugmentedTableNode> m_tableVertexes;
        private Dictionary<Table, AugmentedTableNode> m_tableVertexMap;
        private VarMap m_varMap;
        private Dictionary<Var, VarVec> m_reverseVarMap;
        private Dictionary<Var, AugmentedTableNode> m_varToDefiningNodeMap; //Includes all replacing vars and referenced vars from replacing tables
        private Dictionary<Node, Node> m_processedNodes;
        private bool m_modifiedGraph;
        private ConstraintManager m_constraintManager;
        private VarRefManager m_varRefManager;
        private bool m_isSqlCe;
        #endregion

        #region constructors
        /// <summary>
        /// The basic constructor. Builds up the annotated node tree, and the set of
        /// join edges
        /// </summary>
        /// <param name="command">Current IQT command</param>
        /// <param name="constraintManager">current constraint manager</param>
        /// <param name="varRefManager">the var ref manager for the tree</param>
        /// <param name="joinNode">current join node</param>
        internal JoinGraph(Command command, ConstraintManager constraintManager, VarRefManager varRefManager, Node joinNode, bool isSqlCe)
        {
            m_command = command;
            m_constraintManager = constraintManager;
            m_varRefManager = varRefManager;
            m_isSqlCe = isSqlCe;

            m_vertexes = new List<AugmentedNode>();
            m_tableVertexes = new List<AugmentedTableNode>();
            m_tableVertexMap = new Dictionary<Table, AugmentedTableNode>();
            m_varMap = new VarMap();
            m_reverseVarMap = new Dictionary<Var, VarVec>();
            m_varToDefiningNodeMap = new Dictionary<Var, AugmentedTableNode>();
            m_processedNodes = new Dictionary<Node, Node>();

            // Build the augmented node tree
            m_root = BuildAugmentedNodeTree(joinNode) as AugmentedJoinNode;
            PlanCompiler.Assert(m_root != null, "The root isn't a join?");

            // Build the join edges
            BuildJoinEdges(m_root, m_root.Id);
        }
        #endregion

        #region public methods
        /// <summary>
        /// Perform all kinds of join elimination. The output is the transformed join tree.
        /// The varMap output is a dictionary that maintains var renames - this will be used
        /// by the consumer of this module to fix up references to columns of tables
        /// that have been eliminated
        /// 
        /// The processedNodes dictionary is simply a set of all nodes that have been processed
        /// in this module - and need no further "join graph" processing
        /// </summary>
        /// <param name="varMap">remapped vars</param>
        /// <param name="processedNodes">list of nodes that need no further processing</param>
        internal Node DoJoinElimination(out VarMap varMap,
            out Dictionary<Node, Node> processedNodes)
        {
            //Turn left outer joins into inner joins when possible
            TryTurnLeftOuterJoinsIntoInnerJoins();

            // Generate transitive edges
            GenerateTransitiveEdges();

            // Do real join elimination
            EliminateSelfJoins();
            EliminateParentChildJoins();

            // Build the result tree
            Node result = BuildNodeTree();

            // Get other output properties
            varMap = m_varMap;
            processedNodes = m_processedNodes;

            return result;
        }

        #endregion

        #region private methods

        #region Building the annotated node tree

        //
        // The goal of this submodule is to build up an annotated node tree for a 
        // node tree. As described earlier, we attempt to represent all nodes by 
        // one of the following classes - AugmentedTableNode (for ScanTableOp), 
        // AugmentedJoinNode (for all joins), and AugmentedNode for anything else.
        // We use this information to help enable later stages of this module
        //
        // We employ a "greedy" strategy to handle as much of the node tree as possible.
        // We follow all children of joins - and stop when we see a non-join, non-scan node
        // 

        /// <summary>
        /// Get the subset of vars that are Columns
        /// </summary>
        /// <param name="varVec">a varVec</param>
        /// <returns>a subsetted VarVec that only contains the columnVars from the input vec</returns>
        private VarVec GetColumnVars(VarVec varVec)
        {
            VarVec columnVars = m_command.CreateVarVec();

            foreach (Var v in varVec)
            {
                if (v.VarType == VarType.Column)
                {
                    columnVars.Set(v);
                }
            }
            return columnVars;
        }

        /// <summary>
        /// Generate a list of column Vars from the input vec
        /// </summary>
        /// <param name="columnVars">the list of vars to fill in</param>
        /// <param name="vec">the var set</param>
        private static void GetColumnVars(List<ColumnVar> columnVars, IEnumerable<Var> vec)
        {
            foreach (Var v in vec)
            {
                PlanCompiler.Assert(v.VarType == VarType.Column, "Expected a columnVar. Found " + v.VarType);
                columnVars.Add((ColumnVar)v);
            }
        }

        /// <summary>
        /// Split up the join predicate into equijoin columns and other predicates.
        /// 
        /// For example, if I have a predicate of the form T1.C1 = T2.D1 and T1.C2 > T2.D2
        /// we would generate 
        ///     LeftVars = T1.C1
        ///     RightVars = T2.C1
        ///     OtherPredicate = T1.C2 > T2.D2
        /// 
        /// Special Cases:
        ///   For fullouter joins, we don't do any splitting - the "OtherPredicate" captures the
        ///     entire join condition.
        /// </summary>
        /// <param name="joinNode">the current join node</param>
        /// <param name="leftVars">equijoin columns of the left side</param>
        /// <param name="rightVars">equijoin columns of the right side</param>
        /// <param name="otherPredicateNode">any other predicates</param>
        private void SplitPredicate(Node joinNode,
            out List<ColumnVar> leftVars, out List<ColumnVar> rightVars,
            out Node otherPredicateNode)
        {
            leftVars = new List<ColumnVar>();
            rightVars = new List<ColumnVar>();
            otherPredicateNode = joinNode.Child2;

            //
            // If this is a full-outer join, then don't do any splitting
            //
            if (joinNode.Op.OpType == OpType.FullOuterJoin)
            {
                return;
            }

            Predicate predicate = new Predicate(m_command, joinNode.Child2);

            // 
            // Split the predicate
            //
            ExtendedNodeInfo leftInputNodeInfo = m_command.GetExtendedNodeInfo(joinNode.Child0);
            ExtendedNodeInfo rightInputNodeInfo = m_command.GetExtendedNodeInfo(joinNode.Child1);
            VarVec leftDefinitions = GetColumnVars(leftInputNodeInfo.Definitions);
            VarVec rightDefinitions = GetColumnVars(rightInputNodeInfo.Definitions);
            Predicate otherPredicate;
            List<Var> tempLeftVars;
            List<Var> tempRightVars;
            predicate.GetEquiJoinPredicates(leftDefinitions, rightDefinitions, out tempLeftVars, out tempRightVars, out otherPredicate);

            // Get the non-equijoin conditions
            otherPredicateNode = otherPredicate.BuildAndTree();

            GetColumnVars(leftVars, tempLeftVars);
            GetColumnVars(rightVars, tempRightVars);
        }

        /// <summary>
        /// Build up the annotated node tree for the input subtree. 
        /// If the current node is 
        ///    a ScanTableOp - we build an AugmentedTableNode
        ///    a join (Inner, LOJ, FOJ, CrossJoin) - we build an AugmentedJoinNode,
        ///         after first building annotated node trees for the inputs.
        ///    anything else - we build an AugmentedNode
        /// 
        /// We also mark the node as "processed" - so that the caller will not need
        /// to build join graphs for this again
        /// </summary>
        /// <param name="node">input node tree</param>
        /// <returns>the annotated node tree</returns>
        private AugmentedNode BuildAugmentedNodeTree(Node node)
        {
            AugmentedNode augmentedNode;

            switch (node.Op.OpType)
            {
                case OpType.ScanTable:
                    m_processedNodes[node] = node;
                    ScanTableOp scanTableOp = (ScanTableOp)node.Op;
                    augmentedNode = new AugmentedTableNode(m_vertexes.Count, node);
                    m_tableVertexMap[scanTableOp.Table] = (AugmentedTableNode)augmentedNode;
                    break;

                case OpType.InnerJoin:
                case OpType.LeftOuterJoin:
                case OpType.FullOuterJoin:
                    m_processedNodes[node] = node;
                    AugmentedNode left = BuildAugmentedNodeTree(node.Child0);
                    AugmentedNode right = BuildAugmentedNodeTree(node.Child1);
                    List<ColumnVar> leftVars;
                    List<ColumnVar> rightVars;
                    Node otherPredicate;
                    SplitPredicate(node, out leftVars, out rightVars, out otherPredicate);
                    m_varRefManager.AddChildren(node);
                    augmentedNode = new AugmentedJoinNode(m_vertexes.Count, node, left, right, leftVars, rightVars, otherPredicate);
                    break;

                case OpType.CrossJoin:
                    m_processedNodes[node] = node;
                    List<AugmentedNode> children = new List<AugmentedNode>();
                    foreach (Node chi in node.Children)
                    {
                        children.Add(BuildAugmentedNodeTree(chi));
                    }
                    augmentedNode = new AugmentedJoinNode(m_vertexes.Count, node, children);
                    m_varRefManager.AddChildren(node);
                    break;

                default:
                    augmentedNode = new AugmentedNode(m_vertexes.Count, node);
                    break;
            }

            m_vertexes.Add(augmentedNode);
            return augmentedNode;
        }
        #endregion

        #region Building JoinEdges

        //
        // The goal of this module is to take the annotated node tree, and build up a
        // a set of JoinEdges - this is arguably, the guts of the joingraph.
        //
        // Each join edge represents a directed, equijoin (inner, or leftouter) between
        // two tables. 
        //
        // We impose various constraints on the input node tree
        //

        /// <summary>
        /// Add a new join edge if possible. 
        /// 
        /// - Check to see whether the input columns are columns of a table that we're tracking.
        /// - Make sure that both the tables are "visible" to the current join node
        /// - If there is already a link between the two tables, make sure that the link's
        ///   join kind is compatible with what we have
        /// </summary>
        /// <param name="joinNode">current join Node</param>
        /// <param name="leftVar">left-side column</param>
        /// <param name="rightVar">right-side column</param>
        /// <returns></returns>
        private bool AddJoinEdge(AugmentedJoinNode joinNode, ColumnVar leftVar, ColumnVar rightVar)
        {
            AugmentedTableNode leftTableNode;
            AugmentedTableNode rightTableNode;

            // Are these tables even visible to me?
            if (!m_tableVertexMap.TryGetValue(leftVar.Table, out leftTableNode))
            {
                return false;
            }
            if (!m_tableVertexMap.TryGetValue(rightVar.Table, out rightTableNode))
            {
                return false;
            }

            //
            // If the tables participating in the join are not visible at this node,
            // then simply return. We will not add the join edge
            //
            if (leftTableNode.LastVisibleId < joinNode.Id ||
                rightTableNode.LastVisibleId < joinNode.Id)
            {
                return false;
            }

            // 
            // Check to see if there is already an "edge" between the 2 tables. 
            // If there is, then simply add a predicate to that edge. Otherwise, create
            // an edge
            // 
            foreach (JoinEdge joinEdge in leftTableNode.JoinEdges)
            {
                if (joinEdge.Right.Table.Equals(rightVar.Table))
                {
                    // Try and add this new condition to the existing edge
                    return joinEdge.AddCondition(joinNode, leftVar, rightVar);
                }
            }

            // Create a new join edge
            JoinEdge newJoinEdge = JoinEdge.CreateJoinEdge(leftTableNode, rightTableNode, joinNode, leftVar, rightVar);
            leftTableNode.JoinEdges.Add(newJoinEdge);
            joinNode.JoinEdges.Add(newJoinEdge);
            return true;
        }

        /// <summary>
        /// Check to see if all columns in the input varList are from the same table
        /// Degenerate case: if the list is empty, we still return true
        /// </summary>
        /// <param name="varList">list of columns</param>
        /// <returns>true, if every column is from the same table</returns>
        private static bool SingleTableVars(IEnumerable<ColumnVar> varList)
        {
            Table table = null;
            foreach (ColumnVar v in varList)
            {
                if (table == null)
                {
                    table = v.Table;
                }
                else if (v.Table != table)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Build a set of JoinEdges for this join. 
        /// For cross joins, we simply invoke this function recursively on the children, and return
        /// 
        /// For other joins,
        ///   - We first compute the "visibility" for the left and right branches
        ///     - For full outer joins, the "visibility" is the current join node's id. (ie)
        ///       the tables below are not to be considered as candidates for JoinEdges anywhere
        ///       above this FOJ node
        ///     - For left outer joins, the "visibility" of the left child is the input "maxVisibility"
        ///       parameter. For the right child, the "visibility" is the current join node's id
        ///     - For inner joins, the visibility for both children is the "maxVisibility" parameter
        ///   - We then check to see if the join condition is "ok". If the current join node
        ///     is a full-outer join, OR if the joinNode has an OtherPredicate (ie) stuff
        ///     other than equijoin column conditions, then we don't build any joinedges.
        ///   - Otherwise, we build join edges for each equijoin column
        /// 
        /// </summary>
        /// <param name="joinNode">current join node</param>
        /// <param name="maxVisibility">the highest node where any of the tables below is visible</param>
        private void BuildJoinEdges(AugmentedJoinNode joinNode, int maxVisibility)
        {
            OpType opType = joinNode.Node.Op.OpType;

            // 
            // Simply visit the children for cross-joins
            //
            if (opType == OpType.CrossJoin)
            {
                foreach (AugmentedNode chi in joinNode.Children)
                {
                    BuildJoinEdges(chi, maxVisibility);
                }
                return;
            }

            // 
            // If the current node is a leftouterjoin, or a full outer join, then 
            // none of the tables below should be visible anymore
            //
            int leftMaxVisibility;
            int rightMaxVisibility;
            if (opType == OpType.FullOuterJoin)
            {
                leftMaxVisibility = joinNode.Id;
                rightMaxVisibility = joinNode.Id;
            }
            else if (opType == OpType.LeftOuterJoin)
            {
                leftMaxVisibility = maxVisibility;
                rightMaxVisibility = joinNode.Id;
            }
            else
            {
                leftMaxVisibility = maxVisibility;
                rightMaxVisibility = maxVisibility;
            }

            BuildJoinEdges(joinNode.Children[0], leftMaxVisibility);
            BuildJoinEdges(joinNode.Children[1], rightMaxVisibility);

            // Now handle the predicate

            // Special cases. Nothing further if there exists anything other than 
            // a set of equi-join predicates
            if (joinNode.Node.Op.OpType == OpType.FullOuterJoin ||
                joinNode.OtherPredicate != null ||
                joinNode.LeftVars.Count == 0)
            {
                return;
            }

            //
            // If we have a left-outer join, and the join predicate involves more than one table on the 
            // right side, then quit
            //
            if ((opType == OpType.LeftOuterJoin) &&
                (!SingleTableVars(joinNode.RightVars) || !SingleTableVars(joinNode.LeftVars)))
            {
                return;
            }

            JoinKind joinKind = (opType == OpType.LeftOuterJoin) ? JoinKind.LeftOuter : JoinKind.Inner;
            for (int i = 0; i < joinNode.LeftVars.Count; i++)
            {
                // Add a join edge. 
                if (AddJoinEdge(joinNode, joinNode.LeftVars[i], joinNode.RightVars[i]))
                {
                    // If we have an inner join, then add a "reverse" edge, but only
                    // if the previous AddEdge was successful
                    if (joinKind == JoinKind.Inner)
                    {
                        AddJoinEdge(joinNode, joinNode.RightVars[i], joinNode.LeftVars[i]);
                    }
                }
            }
        }

        /// <summary>
        /// Builds up the list of join edges. If the current node is
        ///   a ScanTable - we simply set the "LastVisibleId" property to the maxVisibility
        ///      parameter
        ///   a join - we invoke the BuildJoinEdges() function on the join node
        ///   anything else - do nothing
        /// </summary>
        /// <param name="node"></param>
        /// <param name="maxVisibility">highest node that this node is visible at</param>
        private void BuildJoinEdges(AugmentedNode node, int maxVisibility)
        {
            switch (node.Node.Op.OpType)
            {
                case OpType.FullOuterJoin:
                case OpType.LeftOuterJoin:
                case OpType.InnerJoin:
                case OpType.CrossJoin:
                    BuildJoinEdges(node as AugmentedJoinNode, maxVisibility);
                    // Now visit the predicate
                    break;

                case OpType.ScanTable:
                    AugmentedTableNode tableNode = (AugmentedTableNode)node;
                    tableNode.LastVisibleId = maxVisibility;
                    break;

                default:
                    break;
            }

            return;
        }
        #endregion

        #region Transitive Edge generation
        //
        // The goal of this module is to generate transitive join edges. 
        // In general, if A is joined to B, and B is joined to C, then A can be joined to
        // C as well. 
        // We apply the rules below to determine if we can indeed generate transitive 
        // join edges
        //   Assume that J1 = (A, B), and J2=(B,C)
        // - J1.Kind must be the same as J2.Kind (both must be Inner, or both must be LeftOuterJoins)
        // - If J1 is a left-outer join, then A,B and C must all be instances of the same table
        // - The same columns of B must participate in the joins with A and C
        // If all of these conditions are satisfied, we generate a new edge between A and C
        // If we're dealing with an inner join, we also generate a C-A edge
        //
        // Note: We never produce any duplicate edges (ie) if an edge already exists between
        // A and C in the example above, we don't try to generate a new edge, or modify the existing
        // edge
        //

        /// <summary>
        /// If edge1 represents (T1, T2), and edge2 represents (T2, T3), try and 
        /// create a (T1,T3) edge.
        /// 
        /// The transitive edge is created if all of the following conditions hold:
        /// 1. edge1 and edge2 are of the same join kind
        /// 2. If edge1 and edge2 are Left Outer Joins, then 
        ///     a. both edges represent joins on the same columns, and
        ///     b. at least one of the edges represents a self join
        /// 3. For inner joins:
        ///     The intersection of the columns on which are the joins represented
        ///     by edge1 and edge2 is non-empty, the transitive edge is created to represent 
        ///     a join on that intersection.
        /// If an edge already exists between these tables, then don't add a new edge
        /// </summary>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        private bool GenerateTransitiveEdge(JoinEdge edge1, JoinEdge edge2)
        {
            PlanCompiler.Assert(edge1.Right == edge2.Left, "need a common table for transitive predicate generation");

            // Ignore the "mirror" image.
            if (edge2.Right == edge1.Left)
            {
                return false;
            }

            // Check to see if the joins are of the same type. 
            if (edge1.JoinKind != edge2.JoinKind)
            {
                return false;
            }

            // Allow left-outer-joins only for self-joins
            if (edge1.JoinKind == JoinKind.LeftOuter &&
                (edge1.Left != edge1.Right || edge2.Left != edge2.Right))
            {
                return false;
            }

            // For LeftOuterJoin, the joins must be on the same columns.
            // Prerequisite for that is they have the same number of vars.
            if (edge1.JoinKind == JoinKind.LeftOuter && edge1.RightVars.Count != edge2.LeftVars.Count)
            {
                return false;
            }

            // check to see whether there already exists an edge for the combination
            // of these tables
            foreach (JoinEdge edge3 in edge1.Left.JoinEdges)
            {
                if (edge3.Right == edge2.Right)
                {
                    return false;
                }
            }

            //
            // Find the subset of columns that are common between the edges
            // For Left Outer Join, that should be all columns from the edges.
            // The algorithm for finding the common columns is based on 
            // sort - merge join. In particular, for each edge we create an ordered key-value pair list
            // where the key value pair has the var coming from the inner (shared table)as a key 
            // and the corresponding var from the other table 

            IEnumerable<KeyValuePair<ColumnVar, ColumnVar>> orderedEdge1Vars = CreateOrderedKeyValueList(edge1.RightVars, edge1.LeftVars);
            IEnumerable<KeyValuePair<ColumnVar, ColumnVar>> orderedEdge2Vars = CreateOrderedKeyValueList(edge2.LeftVars, edge2.RightVars);

            IEnumerator<KeyValuePair<ColumnVar, ColumnVar>> orderedEdge1VarsEnumerator = orderedEdge1Vars.GetEnumerator();
            IEnumerator<KeyValuePair<ColumnVar, ColumnVar>> orderedEdge2VarsEnumerator = orderedEdge2Vars.GetEnumerator();

            List<ColumnVar> leftVars = new List<ColumnVar>();
            List<ColumnVar> rightVars = new List<ColumnVar>();

            bool hasMore = orderedEdge1VarsEnumerator.MoveNext() && orderedEdge2VarsEnumerator.MoveNext();
            while (hasMore)
            {
                if (orderedEdge1VarsEnumerator.Current.Key == orderedEdge2VarsEnumerator.Current.Key)
                {
                    leftVars.Add(orderedEdge1VarsEnumerator.Current.Value);
                    rightVars.Add(orderedEdge2VarsEnumerator.Current.Value);
                    hasMore = orderedEdge1VarsEnumerator.MoveNext() &&
                    orderedEdge2VarsEnumerator.MoveNext();
                }
                else if (edge1.JoinKind == JoinKind.LeftOuter)
                {
                    return false;
                }
                else if (orderedEdge1VarsEnumerator.Current.Key.Id > orderedEdge2VarsEnumerator.Current.Key.Id)
                {
                    hasMore = orderedEdge2VarsEnumerator.MoveNext();
                }
                else
                {
                    hasMore = orderedEdge1VarsEnumerator.MoveNext();
                }
            }


            // Ok, we're now ready to finally create a new edge
            JoinEdge newEdge = JoinEdge.CreateTransitiveJoinEdge(edge1.Left, edge2.Right, edge1.JoinKind,
                leftVars, rightVars);
            edge1.Left.JoinEdges.Add(newEdge);
            if (edge1.JoinKind == JoinKind.Inner)
            {
                JoinEdge reverseEdge = JoinEdge.CreateTransitiveJoinEdge(edge2.Right, edge1.Left, edge1.JoinKind,
                    rightVars, leftVars);
                edge2.Right.JoinEdges.Add(reverseEdge);
            }

            return true;
        }

        /// <summary>
        /// Given a list of key vars a list of corresponding value vars, creates a list 
        /// of key-value pairs that is ordered based on the keys
        /// </summary>
        /// <param name="keyVars"></param>
        /// <param name="valueVars"></param>
        /// <returns></returns>
        private static IEnumerable<KeyValuePair<ColumnVar, ColumnVar>> CreateOrderedKeyValueList(List<ColumnVar> keyVars, List<ColumnVar> valueVars)
        {
            List<KeyValuePair<ColumnVar, ColumnVar>> edgeVars = new List<KeyValuePair<ColumnVar, ColumnVar>>(keyVars.Count);
            for (int i = 0; i < keyVars.Count; i++)
            {
                edgeVars.Add(new KeyValuePair<ColumnVar, ColumnVar>(keyVars[i], valueVars[i]));
            }
            return edgeVars.OrderBy(kv => kv.Key.Id);
        }

        /// <summary>
        /// Try to turn left outer joins into inner joins
        /// 
        /// Turn an augmented join node that represents a Left Outer Join into an Inner join 
        /// if all its edges are candidates to be turned into an Inner Join
        /// 
        /// An edge representing A LOJ B is a candidate to be turned into an inner join (A INNER JOIN B)
        /// if the following conditions hold:
        /// 
        /// 1. a) There is a foreign key constraint (parent-child relationship) between B and A, 
        /// the join is on the constraint, and the joined columns in B are non-nullable, or 
        /// 
        ///    b) There is a foreign key constraint between A and B, the join is on the constraint, 
        /// and the child multiplicity is One. However, this scenario cannot be specified in the ssdl, 
        /// thus this case has not be implemented, and
        /// 
        /// 2. All the rows from the right table B are preserved (i.e. not filtered out) at the level of the join.
        /// This means that if B is participating in any joins prior to being joined with A, these have to be 
        /// left outer joins and B has to be a driver (on the left spine).
        /// 
        /// This second condition does not apply for SQL CE becase it has optimizations that help execute
        /// queries faster when at least one OUTER JOIN statement is still present in the SQL query. If we
        /// convert all OUTER JOIN statements into INNER JOINS then these optimizations don't kick in. In
        /// order to maintain compatibility to .NET 4.0 we had to create a special case for SQL CE. 
        /// See DevDiv bug #462067 for more details.  Also see bug DevDev2 bug#488375 for the UseFx40CompatMode check.
        /// </summary>
        private void TryTurnLeftOuterJoinsIntoInnerJoins()
        {
            foreach (AugmentedJoinNode augmentedJoinNode in m_vertexes.OfType<AugmentedJoinNode>().Where(j => j.Node.Op.OpType == OpType.LeftOuterJoin && j.JoinEdges.Count > 0))
            {
                bool useCompatMode = m_isSqlCe || EntityUtil.UseFx40CompatMode;

                if (useCompatMode ? (augmentedJoinNode.Children.All(c => c is AugmentedTableNode) && augmentedJoinNode.JoinEdges.All(joinEdge => IsConstraintPresentForTurningIntoInnerJoin(joinEdge)))
                  : (CanAllJoinEdgesBeTurnedIntoInnerJoins(augmentedJoinNode.Children[1], augmentedJoinNode.JoinEdges)))
                {
                    augmentedJoinNode.Node.Op = m_command.CreateInnerJoinOp();
                    m_modifiedGraph = true;
                    List<JoinEdge> newJoinEdges = new List<JoinEdge>(augmentedJoinNode.JoinEdges.Count);
                    foreach (JoinEdge joinEdge in augmentedJoinNode.JoinEdges)
                    {
                        joinEdge.JoinKind = JoinKind.Inner;
                        if (!ContainsJoinEdgeForTable(joinEdge.Right.JoinEdges, joinEdge.Left.Table))
                        {
                            //create the mirroring join edge
                            JoinEdge newJoinEdge = JoinEdge.CreateJoinEdge(joinEdge.Right, joinEdge.Left, augmentedJoinNode, joinEdge.RightVars[0], joinEdge.LeftVars[0]);
                            joinEdge.Right.JoinEdges.Add(newJoinEdge);
                            newJoinEdges.Add(newJoinEdge);
                            for (int i = 1; i < joinEdge.LeftVars.Count; i++)
                            {
                                newJoinEdge.AddCondition(augmentedJoinNode, joinEdge.RightVars[i], joinEdge.LeftVars[i]);
                            }
                        }
                    }
                    augmentedJoinNode.JoinEdges.AddRange(newJoinEdges);
                }
            }
        }

        /// <summary>
        /// Are all the rows from the given table that is part of the subtree rooted 
        /// at the given root preserved on the root.
        /// This is true if:
        /// - The root represents the table
        /// - The table is a on the left spine of a left outer join tree
        /// </summary>
        /// <param name="root"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private bool AreAllTableRowsPreserved(AugmentedNode root, AugmentedTableNode table)
        {
            if (root is AugmentedTableNode)
            {
                return true;
            }

            AugmentedJoinNode parent;
            AugmentedNode currentNode = table;
            do
            {
                parent = (AugmentedJoinNode)currentNode.Parent;
                if (parent.Node.Op.OpType != OpType.LeftOuterJoin || parent.Children[0] != currentNode)
                {
                    return false;
                }
                currentNode = parent;
            } while (currentNode != root);

            return true;
        }

        /// <summary>
        /// Does the set of given joinEdges contain a join edge to a given table
        /// </summary>
        /// <param name="joinEdges"></param>
        /// <param name="table"></param>
        /// <returns></returns>
        private bool ContainsJoinEdgeForTable(IEnumerable<JoinEdge> joinEdges, Table table)
        {
            foreach (JoinEdge joinEdge in joinEdges)
            {
                if (joinEdge.Right.Table.Equals(table))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Determines whether each of the given joinEdges can be turned into an inner join
        /// NOTE: Due to how we create join edges, currenlty there can only be one join edge in this group
        /// See <cref="CanJoinEdgeBeTurnedIntoInnerJoin"/> for details.      
        /// </summary>
        /// <param name="rightNode"></param>
        /// <param name="joinEdges"></param>
        /// <returns></returns>
        private bool CanAllJoinEdgesBeTurnedIntoInnerJoins(AugmentedNode rightNode, IEnumerable<JoinEdge> joinEdges)
        {
            foreach (JoinEdge joinEdge in joinEdges)
            {
                if (!CanJoinEdgeBeTurnedIntoInnerJoin(rightNode, joinEdge))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// A LOJ B edge can be turned into an inner join if:
        /// 
        /// 1. There is a foreign key constraint based on which such transformation is possible
        /// 
        /// 2. All the rows from the right table B are preserved (i.e. not filtered out) at the level of the join.
        /// This means that if B is participating in any joins prior to being joined with A, these have to be 
        /// left outer joins and B has to be a driver (on the left spine).
        /// </summary>
        /// <param name="rightNode"></param>
        /// <param name="joinEdge"></param>
        /// <returns></returns>
        private bool CanJoinEdgeBeTurnedIntoInnerJoin(AugmentedNode rightNode, JoinEdge joinEdge)
        {
            return AreAllTableRowsPreserved(rightNode, joinEdge.Right) && IsConstraintPresentForTurningIntoInnerJoin(joinEdge);
        }

        /// <summary>
        ///  A necessary condition for an  A LOJ B edge to be turned into an inner join is 
        ///  the existence of one of the following constraints:
        /// 
        ///  a) There is a foreign key constraint (parent-child relationship) between B and A, 
        /// the join is on the constraint, and the joined columns in B are non-nullable, or 
        /// 
        ///  b) There is a foreign key constraint between A and B, the join is on the constraint, 
        /// and the child multiplicity is One. However, this scenario cannot be specified in the ssdl, 
        /// thus this case has not be implemented
        /// </summary>
        /// <param name="joinEdge"></param>
        /// <returns></returns>
        private bool IsConstraintPresentForTurningIntoInnerJoin(JoinEdge joinEdge)
        {
            List<ForeignKeyConstraint> fkConstraints;

            if (m_constraintManager.IsParentChildRelationship(joinEdge.Right.Table.TableMetadata.Extent, joinEdge.Left.Table.TableMetadata.Extent, out fkConstraints))
            {
                PlanCompiler.Assert(fkConstraints != null && fkConstraints.Count > 0, "invalid fk constraints?");
                foreach (ForeignKeyConstraint fkConstraint in fkConstraints)
                {
                    IList<ColumnVar> columnVars;
                    if (IsJoinOnFkConstraint(fkConstraint, joinEdge.RightVars, joinEdge.LeftVars, out columnVars))
                    {
                        if (fkConstraint.ParentKeys.Count == joinEdge.RightVars.Count &&
                            columnVars.Where(v => v.ColumnMetadata.IsNullable).Count() == 0)
                        {
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Generate a set of transitive edges
        /// </summary>
        private void GenerateTransitiveEdges()
        {
            foreach (AugmentedNode augmentedNode in m_vertexes)
            {
                AugmentedTableNode tableNode = augmentedNode as AugmentedTableNode;
                if (tableNode == null)
                {
                    continue;
                }

                //
                // The reason we use absolute indexing rather than 'foreach'ing is because
                // the inner calls may add new entries to the collections, and cause the 
                // enumeration to throw
                //
                int i = 0;
                while (i < tableNode.JoinEdges.Count)
                {
                    JoinEdge e1 = tableNode.JoinEdges[i];
                    int j = 0;
                    AugmentedTableNode rightTable = e1.Right;
                    while (j < rightTable.JoinEdges.Count)
                    {
                        JoinEdge e2 = rightTable.JoinEdges[j];
                        GenerateTransitiveEdge(e1, e2);
                        j++;
                    }
                    i++;
                }
            }
        }
        #endregion

        #region Join Elimination Helpers
        //
        // Utility routines used both by selfjoin elimination and parent-child join
        // elimination
        //

        /// <summary>
        /// Checks whether a given table can be eliminated to be replaced by the given replacingTable
        /// with regards to possible participation in the driving (left) subtree of Left Outer Joins.
        /// 
        /// In order for elimination to happen, one of the two tables has to logically move, 
        /// either the replacement table to the original table's location, or the table to the 
        /// replacing table's location.
        /// 
        /// For the table that would have to move, it checks whether such move would be valid 
        /// with regards to its participation as driver in Left Outer Joins (<see cref=CanBeMoved/>)
        /// </summary>
        /// <param name="table"></param>
        /// <param name="replacingTable"></param>
        /// <returns></returns>
        private static bool CanBeEliminatedBasedOnLojParticipation(AugmentedTableNode table, AugmentedTableNode replacingTable)
        {
            //The table with lower id, would have to be logically located at the other table's location
            //Check whether it can be moved there
            if (replacingTable.Id < table.NewLocationId)
            {
                return CanBeMovedBasedOnLojParticipation(table, replacingTable);
            }
            else
            {
                return CanBeMovedBasedOnLojParticipation(replacingTable, table);
            }
        }

        /// <summary>
        /// Can the right table of the given tableJoinEdge be eliminated and replaced by the right table of the replacingTableJoinEdge
        /// based on both tables participation in other joins.
        /// It can be if:
        ///     - The table coming from tableJoinEdge does not participate in any other join on the way up to the least common ancestor
        ///     - The table coming from replacingTableJoinEdge does not get filtered on the way up to the least common ancestor   
        /// </summary>
        /// <param name="tableJoinEdge"></param>
        /// <param name="replacingTableJoinEdge"></param>
        /// <returns></returns>
        private static bool CanBeEliminatedViaStarJoinBasedOnOtherJoinParticipation(JoinEdge tableJoinEdge, JoinEdge replacingTableJoinEdge)
        {
            if (tableJoinEdge.JoinNode == null || replacingTableJoinEdge.JoinNode == null)
            {
                return false;
            }

            AugmentedNode leastCommonAncestor = GetLeastCommonAncestor(tableJoinEdge.Right, replacingTableJoinEdge.Right);
            return
                !CanGetFileredByJoins(tableJoinEdge, leastCommonAncestor, true) &&
                !CanGetFileredByJoins(replacingTableJoinEdge, leastCommonAncestor, false);
        }

        /// <summary>
        /// Can the right table of the joinEdge be filtered by joins on the the way up the the given leastCommonAncestor.
        /// It can, if 
        ///     - dissallowAnyJoin is specified, or 
        ///     - if it is on the right side of a left outer join or participates in any inner join, thus it is only 
        ///     allowed to be on the left side of a left outer join
        /// </summary>
        /// <param name="joinEdge"></param>
        /// <param name="leastCommonAncestor"></param>
        /// <param name="disallowAnyJoin"></param>
        /// <returns></returns>
        private static bool CanGetFileredByJoins(JoinEdge joinEdge, AugmentedNode leastCommonAncestor, bool disallowAnyJoin)
        {
            AugmentedNode currentNode = joinEdge.Right;
            AugmentedNode currentParent = currentNode.Parent;

            while (currentParent != null && currentNode != leastCommonAncestor)
            {
                //If the current node is a rigth child of a left outer join return or participates in a inner join
                if (currentParent.Node != joinEdge.JoinNode.Node &&
                        (disallowAnyJoin || currentParent.Node.Op.OpType != OpType.LeftOuterJoin || currentParent.Children[0] != currentNode)
                    )
                {
                    return true;
                }
                currentNode = currentNode.Parent;
                currentParent = currentNode.Parent;
            }
            return false;
        }

        /// <summary>
        /// Determines whether the given table can be moved to the replacing table's location 
        /// with regards to participation in the driving (left) subtree of Left Outer Joins.
        /// If the table to be moved is part of the driving (left) subtree of a Left Outer Join
        /// and the replacing table is not part of that subtree then the table cannot be moved,
        /// otherwise it can.
        /// </summary>
        /// <param name="table"></param>
        /// <param name="replacingTable"></param>
        /// <returns></returns>
        private static bool CanBeMovedBasedOnLojParticipation(AugmentedTableNode table, AugmentedTableNode replacingTable)
        {
            AugmentedNode leastCommonAncestor = GetLeastCommonAncestor(table, replacingTable);
            AugmentedNode currentNode = table;
            while (currentNode.Parent != null && currentNode != leastCommonAncestor)
            {
                //If the current node is a left child of an left outer join return
                if (currentNode.Parent.Node.Op.OpType == OpType.LeftOuterJoin &&
                     currentNode.Parent.Children[0] == currentNode)
                {
                    return false;
                }
                currentNode = currentNode.Parent;
            }
            return true;
        }

        /// <summary>
        /// Gets the least common ancestor for two given nodes in the tree
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        private static AugmentedNode GetLeastCommonAncestor(AugmentedNode node1, AugmentedNode node2)
        {
            if (node1.Id == node2.Id)
            {
                return node1;
            }

            AugmentedNode currentParent;
            AugmentedNode rigthNode;

            if (node1.Id < node2.Id)
            {
                currentParent = node1;
                rigthNode = node2;
            }
            else
            {
                currentParent = node2;
                rigthNode = node1;
            }

            while (currentParent.Id < rigthNode.Id)
            {
                currentParent = currentParent.Parent;
            }

            return currentParent;
        }

        /// <summary>
        /// This function marks a table as eliminated. The replacement varmap
        /// is updated with columns of the table being mapped to the corresponding columns
        /// of the replacement table
        /// </summary>
        /// <param name="tableNode">table being replaced</param>
        /// <param name="replacementNode">the table being used in its place</param>
        /// <param name="tableVars">list of vars to replace</param>
        /// <param name="replacementVars">list of vars to replace with</param>
        /// <typeparam name="T">Var or one of its subtypes</typeparam>
        private void MarkTableAsEliminated<T>(AugmentedTableNode tableNode, AugmentedTableNode replacementNode,
            List<T> tableVars, List<T> replacementVars) where T : Var
        {
            PlanCompiler.Assert(tableVars != null && replacementVars != null, "null vars");
            PlanCompiler.Assert(tableVars.Count == replacementVars.Count, "var count mismatch");
            PlanCompiler.Assert(tableVars.Count > 0, "no vars in the table ?");

            m_modifiedGraph = true;

            // Set up the replacement table (if necessary)
            if (tableNode.Id < replacementNode.NewLocationId)
            {
                tableNode.ReplacementTable = replacementNode;
                replacementNode.NewLocationId = tableNode.Id;
            }
            else
            {
                tableNode.ReplacementTable = null;
            }

            // Add mappings for each var of the table
            for (int i = 0; i < tableVars.Count; i++)
            {
                //
                // Bug 446708: Make sure that the "replacement" column is 
                //   referenced, if the the current column is referenced
                //
                if (tableNode.Table.ReferencedColumns.IsSet(tableVars[i]))
                {
                    m_varMap[tableVars[i]] = replacementVars[i];
                    AddReverseMapping(replacementVars[i], tableVars[i]);
                    replacementNode.Table.ReferencedColumns.Set(replacementVars[i]);
                }
            }

            //
            // It should be possible to retrieve the location of each replacing var
            // It should also be possible to retrieve the location of each referenced var 
            // defined on a replacing table, because replacing tables may get moved.
            //
            foreach (Var var in replacementNode.Table.ReferencedColumns)
            {
                m_varToDefiningNodeMap[var] = replacementNode;
            }
        }

        /// <summary>
        /// Record that replacingVar is replacing replacedVar.
        /// Also, replacedVar was previously replacing any other vars, 
        /// add these to the list of replaced vars for the replacingVar too.
        /// The info about the replacedVar no longer needs to be maintained.
        /// </summary>
        /// <param name="replacingVar"></param>
        /// <param name="replacedVar"></param>
        private void AddReverseMapping(Var replacingVar, Var replacedVar)
        {
            VarVec oldReplacedVars;
            if (m_reverseVarMap.TryGetValue(replacedVar, out oldReplacedVars))
            {
                m_reverseVarMap.Remove(replacedVar);
            }

            VarVec replacedVars;
            if (!m_reverseVarMap.TryGetValue(replacingVar, out replacedVars))
            {
                // Try to reuse oldReplacedVars
                if (oldReplacedVars != null)
                {
                    replacedVars = oldReplacedVars;
                }
                else
                {
                    replacedVars = this.m_command.CreateVarVec();
                }
                m_reverseVarMap[replacingVar] = replacedVars;
            }
            else if (oldReplacedVars != null)
            {
                replacedVars.Or(oldReplacedVars);
            }
            replacedVars.Set(replacedVar);
        }

        #endregion

        #region SelfJoin Elimination
        //
        // The goal of this submodule is to eliminate selfjoins. We consider two kinds
        // of selfjoins here - explicit, and implicit. 
        //
        // An explicit selfjoin J is a join between tables T1 and T2, where T1 and T2
        // are instances of the same table. Furthemore, T1 and T2 must be joined on their
        // key columns (and no more).
        //
        // An implicit self-join is of the form (X, A1, A2, ...) where A1, A2 etc. 
        // are all instances of the same table, and X is joined to A1, A2 etc. on the same
        // columns. We also call this a "star" selfjoin, since "X" is logically the 
        // being star-joined to all the other tables here
        //

        /// <summary>
        /// This function marks a table (part of a selfjoin) as eliminated. The replacement varmap
        /// is updated with columns of the table being mapped to the corresponding columns
        /// of the replacement table
        /// </summary>
        /// <param name="tableNode">table being replaced</param>
        /// <param name="replacementNode">the table being used in its place</param>
        private void EliminateSelfJoinedTable(AugmentedTableNode tableNode, AugmentedTableNode replacementNode)
        {
            MarkTableAsEliminated<Var>(tableNode, replacementNode, tableNode.Table.Columns, replacementNode.Table.Columns);
        }

        /// <summary>
        /// This function is a helper function for star selfjoin elimination. All the 
        /// "right" tables of the join edges in the input list are instances of the same table.
        /// 
        /// Precondition: Each joinedge is of the form (X, Ai),
        ///    where X is the star-joined table, and A1...An are all instances of the same
        /// table A
        /// 
        /// This function first creates groups of join edges such that all tables
        /// in a group:
        ///     1. are joined to the center (X) on the same columns
        ///     2. are of the same join kind
        ///     3. are joined on all key columns of table A
        ///     4. if the join type is Left Outer Join, they are not joined on any other columns
        /// 
        /// For each group, we then identify the table with the 
        /// smallest "Id", and choose that to replace all the other tables from that group
        /// 
        /// </summary>
        /// <param name="joinEdges">list of join edges</param>
        private void EliminateStarSelfJoin(List<JoinEdge> joinEdges)
        {
            List<List<JoinEdge>> compatibleGroups = new List<List<JoinEdge>>();

            foreach (JoinEdge joinEdge in joinEdges)
            {
                // Try to put the join edge in some of the existing groups
                bool matched = false;
                foreach (List<JoinEdge> joinEdgeList in compatibleGroups)
                {
                    if (AreMatchingForStarSelfJoinElimination(joinEdgeList[0], joinEdge))
                    {
                        joinEdgeList.Add(joinEdge);
                        matched = true;
                        break;
                    }
                }

                // If the join edge could not be part of any of the existing groups,
                // see whether it quailifes for leading a new group
                if (!matched && QualifiesForStarSelfJoinGroup(joinEdge))
                {
                    List<JoinEdge> newList = new List<JoinEdge>();
                    newList.Add(joinEdge);
                    compatibleGroups.Add(newList);
                }
            }

            foreach (List<JoinEdge> joinList in compatibleGroups.Where(l => l.Count > 1))
            {
                // Identify the table with the smallest id, and use that as the candidate
                JoinEdge smallestEdge = joinList[0];
                foreach (JoinEdge joinEdge in joinList)
                {
                    if (smallestEdge.Right.Id > joinEdge.Right.Id)
                    {
                        smallestEdge = joinEdge;
                    }
                }

                // Now walk through all the edges in the group, and mark all the tables as eliminated
                foreach (JoinEdge joinEdge in joinList)
                {
                    if (joinEdge == smallestEdge)
                    {
                        continue;
                    }
                    if (CanBeEliminatedViaStarJoinBasedOnOtherJoinParticipation(joinEdge, smallestEdge))
                    {
                        EliminateSelfJoinedTable(joinEdge.Right, smallestEdge.Right);
                    }
                }
            }
        }

        /// <summary>
        /// Two edges match for star self join elimination if:
        ///     1. are joined to the center (X) on the same columns
        ///     2. are of the same join kind
        /// </summary>
        /// <param name="edge1"></param>
        /// <param name="edge2"></param>
        /// <returns></returns>
        private bool AreMatchingForStarSelfJoinElimination(JoinEdge edge1, JoinEdge edge2)
        {
            // In order for the join edges to be compatible thay have to  
            // represent joins on the same number of columns and of the same join kinds.
            if (edge2.LeftVars.Count != edge1.LeftVars.Count ||
                edge2.JoinKind != edge1.JoinKind)
            {
                return false;
            }

            // Now make sure that we're joining on the same columns
            for (int j = 0; j < edge2.LeftVars.Count; j++)
            {
                // Check for reference equality on the left-table Vars. Check for
                // name equality on the right table vars
                if (!edge2.LeftVars[j].Equals(edge1.LeftVars[j]) ||
                    !edge2.RightVars[j].ColumnMetadata.Name.Equals(edge1.RightVars[j].ColumnMetadata.Name))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// A join edge qualifies for starting a group for star self join elimination if:
        ///     1. the join is on all key columns of the right table,
        ///     2. if the join type is Left Outer Join, the join is on no columns 
        ///     other than the keys of the right table. 
        ///     NOTE:  The second limitation is really arbitrary, to should be possible 
        ///     to also allow other conditions
        /// </summary>
        /// <param name="joinEdge"></param>
        /// <returns></returns>
        private bool QualifiesForStarSelfJoinGroup(JoinEdge joinEdge)
        {
            //
            // Now make sure that all key columns of the right table are used
            //
            VarVec keyVars = m_command.CreateVarVec(joinEdge.Right.Table.Keys);
            foreach (Var v in joinEdge.RightVars)
            {
                // Make sure that no other column is referenced in case of an outer join
                if (joinEdge.JoinKind == JoinKind.LeftOuter && !keyVars.IsSet(v))
                {
                    return false;
                }
                keyVars.Clear(v);
            }
            if (!keyVars.IsEmpty)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Eliminates any star self joins. This function looks at all the tables that
        /// this table is joined to, groups the tables based on the table name (metadata),
        /// and then tries selfjoin elimination on each group (see function above)
        /// </summary>
        /// <param name="tableNode">the star-joined table?</param>
        private void EliminateStarSelfJoins(AugmentedTableNode tableNode)
        {
            // First build up a number of equivalence classes. Each equivalence class
            // contains instances of the same table
            Dictionary<md.EntitySetBase, List<JoinEdge>> groupedEdges = new Dictionary<md.EntitySetBase, List<JoinEdge>>();
            foreach (JoinEdge joinEdge in tableNode.JoinEdges)
            {
                // Ignore useless edges
                if (joinEdge.IsEliminated)
                {
                    continue;
                }

                List<JoinEdge> edges;
                if (!groupedEdges.TryGetValue(joinEdge.Right.Table.TableMetadata.Extent, out edges))
                {
                    edges = new List<JoinEdge>();
                    groupedEdges[joinEdge.Right.Table.TableMetadata.Extent] = edges;
                }
                edges.Add(joinEdge);
            }

            // Now walk through each equivalence class, and identify if we can eliminate some of
            // the self-joins
            foreach (KeyValuePair<md.EntitySetBase, List<JoinEdge>> kv in groupedEdges)
            {
                // If there's only one table in the class, skip this and move on
                if (kv.Value.Count <= 1)
                {
                    continue;
                }
                // Try and do the real dirty work
                EliminateStarSelfJoin(kv.Value);
            }
        }

        /// <summary>
        /// Eliminate a self-join edge.
        /// </summary>
        /// <param name="joinEdge">the join edge</param>
        /// <returns>tur, if we did eliminate the self-join</returns>
        private bool EliminateSelfJoin(JoinEdge joinEdge)
        {
            // Nothing further to do, if the right-side has already been eliminated
            if (joinEdge.IsEliminated)
            {
                return false;
            }

            // Am I a self-join?
            if (!joinEdge.Left.Table.TableMetadata.Extent.Equals(joinEdge.Right.Table.TableMetadata.Extent))
            {
                return false;
            }

            // Check to see that only the corresponding columns are being compared
            for (int i = 0; i < joinEdge.LeftVars.Count; i++)
            {
                if (!joinEdge.LeftVars[i].ColumnMetadata.Name.Equals(joinEdge.RightVars[i].ColumnMetadata.Name))
                {
                    return false;
                }
            }

            //
            // Now make sure that the join edge includes every single key column
            // For left-outer joins, we must have no columns other than the key columns
            //
            VarVec keyVars = m_command.CreateVarVec(joinEdge.Left.Table.Keys);
            foreach (Var v in joinEdge.LeftVars)
            {
                if (joinEdge.JoinKind == JoinKind.LeftOuter && !keyVars.IsSet(v))
                {
                    return false;
                }

                keyVars.Clear(v);
            }

            // Are some keys left over?
            if (!keyVars.IsEmpty)
            {
                return false;
            }

            if (!CanBeEliminatedBasedOnLojParticipation(joinEdge.Right, joinEdge.Left))
            {
                return false;
            }

            // Mark the right-table as eliminated
            // Get the parent node for the right node, and replace the parent by the corresponding 
            // left node
            EliminateSelfJoinedTable(joinEdge.Right, joinEdge.Left);
            return true;
        }

        /// <summary>
        /// Eliminate self-joins for this table (if any)
        /// </summary>
        /// <param name="tableNode">current table</param>
        private void EliminateSelfJoins(AugmentedTableNode tableNode)
        {
            // Is this node already eliminated?
            if (tableNode.IsEliminated)
            {
                return;
            }

            // First try and eliminate all explicit self-joins
            foreach (JoinEdge joinEdge in tableNode.JoinEdges)
            {
                EliminateSelfJoin(joinEdge);
            }
        }

        /// <summary>
        /// Eliminate all selfjoins
        /// </summary>
        private void EliminateSelfJoins()
        {
            foreach (AugmentedNode augmentedNode in m_vertexes)
            {
                AugmentedTableNode tableNode = augmentedNode as AugmentedTableNode;
                if (tableNode != null)
                {
                    EliminateSelfJoins(tableNode);
                    EliminateStarSelfJoins(tableNode);
                }
            }
        }
        #endregion

        #region Parent-Child join elimination

        //
        // The goal of this submodule is to eliminate parent-child joins. We consider two kinds
        // of parent-child joins here.
        // 
        // The first category of joins involves a 1-1 or 1-n relationship between a parent
        // and child table, where the tables are (inner) joined on the key columns (pk, fk), and no
        // other columns of the parent table are referenced. In this case, the parent table
        // can be eliminated, and the child table used in place. There are two special considerations
        // here. 
        //   First, the foreign key columns may be nullable - in this case, we need to prune
        //   out rows where these null values might occur (since they would have been pruned
        //   out by the join). In effect, we add a filter node above the table node, if there
        //   are any nullable foreign keys.
        //   The second case is where the parent table appears "lexically" before the child
        //   table in the query. In this case, the child table will need to "move" to the 
        //   parent table's location - this is needed for scenarios where there may be other
        //   intervening tables where the parent table's key columns are referenced - and these 
        //   cannot see the equivalent columns of the child table, unless the child table is
        //   moved to that location.
        //
        // The second category of joins involves a 1-1 relationship between the parent and
        // child table, where the parent table is left outer joined to the child table 
        // on the key columns. If no other columns of the child table are referenced in the
        // query, then the child table can be eliminated.
        //

        /// <summary>
        /// Eliminate the left table 
        /// </summary>
        /// <param name="joinEdge"></param>
        private void EliminateLeftTable(JoinEdge joinEdge)
        {
            PlanCompiler.Assert(joinEdge.JoinKind == JoinKind.Inner, "Expected inner join");
            MarkTableAsEliminated<ColumnVar>(joinEdge.Left, joinEdge.Right, joinEdge.LeftVars, joinEdge.RightVars);

            //
            // Find the list of non-nullable columns
            //
            if (joinEdge.Right.NullableColumns == null)
            {
                joinEdge.Right.NullableColumns = m_command.CreateVarVec();
            }
            foreach (ColumnVar v in joinEdge.RightVars)
            {
                //
                // if the column is known to be non-nullable, then we don't need to 
                // add a filter condition to prune out nulls later.
                //
                if (v.ColumnMetadata.IsNullable)
                {
                    joinEdge.Right.NullableColumns.Set(v);
                }
            }
        }

        /// <summary>
        /// Eliminate the right table
        /// </summary>
        /// <param name="joinEdge"></param>
        private void EliminateRightTable(JoinEdge joinEdge)
        {
            PlanCompiler.Assert(joinEdge.JoinKind == JoinKind.LeftOuter, "Expected left-outer-join");
            PlanCompiler.Assert(joinEdge.Left.Id < joinEdge.Right.Id,
                "(left-id, right-id) = (" + joinEdge.Left.Id + "," + joinEdge.Right.Id + ")");
            MarkTableAsEliminated<ColumnVar>(joinEdge.Right, joinEdge.Left, joinEdge.RightVars, joinEdge.LeftVars);
        }

        /// <summary>
        /// Do we reference any nonkey columns from this table
        /// </summary>
        /// <param name="table">the table instance</param>
        /// <returns>true, if there are any nonkey references</returns>
        private static bool HasNonKeyReferences(Table table)
        {
            return !table.Keys.Subsumes(table.ReferencedColumns);
        }

        /// <summary>
        /// Are any of the key columns from the right table of the given join edge referenced 
        /// elsewhere (outside the join condition)
        /// </summary>
        /// <param name="joinEdge"></param>
        /// <returns></returns>
        private bool RightTableHasKeyReferences(JoinEdge joinEdge)
        {
            //For transitive edges we don't have a joinNode.
            if (joinEdge.JoinNode == null)
            {
                // Note: We have not been able to hit this yet. If we find many cases in which we hit this,
                // we can see if we can do more tracking. This way we may be missing cases that could be optimized.
                return true;
            }

            // In addition to all the keys of the right table we need to also check for all
            // the vars they may be replacing.
            VarVec keys = null;
            foreach (var key in joinEdge.Right.Table.Keys)
            {
                VarVec replacedVars;
                if (m_reverseVarMap.TryGetValue(key, out replacedVars))
                {
                    if (keys == null)
                    {
                        keys = joinEdge.Right.Table.Keys.Clone();
                    }
                    keys.Or(replacedVars);
                }
            }

            //If the keys were not replacing any vars, no need to clone
            if (keys == null)
            {
                keys = joinEdge.Right.Table.Keys;
            }

            return m_varRefManager.HasKeyReferences(keys, joinEdge.Right.Node, joinEdge.JoinNode.Node);
        }

        /// <summary>
        /// Eliminate a parent-child join, given a fk constraint
        /// </summary>
        /// <param name="joinEdge">the current join edge</param>
        /// <param name="fkConstraint">the referential integrity constraint</param>
        /// <returns></returns>
        private bool TryEliminateParentChildJoin(JoinEdge joinEdge, ForeignKeyConstraint fkConstraint)
        {
            //
            // Consider join elimination for left-outer-joins only if we have a 1 - 1 or 1 - 0..1 relationship
            //
            if (joinEdge.JoinKind == JoinKind.LeftOuter && fkConstraint.ChildMultiplicity == md.RelationshipMultiplicity.Many)
            {
                return false;
            }

            IList<ColumnVar> childColumnVars;
            if (!IsJoinOnFkConstraint(fkConstraint, joinEdge.LeftVars, joinEdge.RightVars, out childColumnVars))
            {
                return false;
            }

            //
            // For inner joins, try and eliminate the parent table
            //
            if (joinEdge.JoinKind == JoinKind.Inner)
            {
                if (HasNonKeyReferences(joinEdge.Left.Table))
                {
                    return false;
                }

                if (!CanBeEliminatedBasedOnLojParticipation(joinEdge.Right, joinEdge.Left))
                {
                    return false;
                }

                // Mark the parent (left-side) table as "eliminated"
                EliminateLeftTable(joinEdge);
                return true;
            }
            //
            // For left outer joins, try and eliminate the child table
            //
            else
            {
                // SQLBUDT #512375: For the 1 - 0..1 we also verify that the child's columns are not 
                // referenced outside the join condition, thus passing true for allowRefsForJoinedOnFkOnly only
                // if the multiplicity is 1 - 1
                return TryEliminateRightTable(joinEdge, fkConstraint.ChildKeys.Count, fkConstraint.ChildMultiplicity == md.RelationshipMultiplicity.One);
            }
        }

        /// <summary>
        /// Given a ForeignKeyConstraint and lists of vars on which the tables are joined, 
        /// it checks whether the join condition includes (but is not necessarily joined only on)
        /// the foreign key constraint.
        /// </summary>
        /// <param name="fkConstraint"></param>
        /// <param name="parentVars"></param>
        /// <param name="childVars"></param>
        /// <param name="childForeignKeyVars"></param>
        /// <returns></returns>
        private static bool IsJoinOnFkConstraint(ForeignKeyConstraint fkConstraint, IList<ColumnVar> parentVars, IList<ColumnVar> childVars, out IList<ColumnVar> childForeignKeyVars)
        {
            childForeignKeyVars = new List<ColumnVar>(fkConstraint.ChildKeys.Count);
            //
            // Make sure that every one of the parent key properties is referenced
            //
            foreach (string keyProp in fkConstraint.ParentKeys)
            {
                bool foundKey = false;
                foreach (ColumnVar cv in parentVars)
                {
                    if (cv.ColumnMetadata.Name.Equals(keyProp))
                    {
                        foundKey = true;
                        break;
                    }
                }
                if (!foundKey)
                {
                    return false;
                }
            }

            //
            // Make sure that every one of the child key properties is referenced
            // and furthermore equi-joined to the corresponding parent key properties
            //
            foreach (string keyProp in fkConstraint.ChildKeys)
            {
                bool foundKey = false;
                for (int pos = 0; pos < parentVars.Count; pos++)
                {
                    ColumnVar rightVar = childVars[pos];
                    if (rightVar.ColumnMetadata.Name.Equals(keyProp))
                    {
                        childForeignKeyVars.Add(rightVar);
                        foundKey = true;
                        string parentPropertyName;
                        ColumnVar leftVar = parentVars[pos];
                        if (!fkConstraint.GetParentProperty(rightVar.ColumnMetadata.Name, out parentPropertyName) ||
                            !parentPropertyName.Equals(leftVar.ColumnMetadata.Name))
                        {
                            return false;
                        }
                        break;
                    }
                }
                if (!foundKey)
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Try to eliminate the parent table from a 
        ///         child Left Outer Join parent
        /// join, given a fk constraint
        /// 
        /// More specific:
        /// 
        /// P(p1, p2, p3,…) is the parent table, and C(c1, c2, c3, …) is the child table. 
        /// Say p1,p2 is the PK of P, and c1,c2 is the FK from C to P
        /// 
        /// SELECT …
        /// From C LOJ P ON (p1 = c1 and p2 = c2)
        /// WHERE …
        /// 
        /// If only the keys are used from P, we should but should be carefull about composite keys with nullable foreign key columns.
        /// If a composite foreign key has been defined on columns that allow nulls, 
        /// and at least one of the columns, upon the insert or update of a row, is set to null, then the foreign key constraint will be satisfied
        /// on SqlServer. 
        /// 
        /// Thus we should do the elimination only if
        /// 1.	The key is not composite
        /// 2.	All columns on the child side are non nullable
        /// </summary>
        /// <param name="joinEdge">the current join edge</param>
        /// <param name="fkConstraint">the referential integrity constraint</param>
        /// <returns></returns>
        private bool TryEliminateChildParentJoin(JoinEdge joinEdge, ForeignKeyConstraint fkConstraint)
        {
            IList<ColumnVar> childColumnVars;
            if (!IsJoinOnFkConstraint(fkConstraint, joinEdge.RightVars, joinEdge.LeftVars, out childColumnVars))
            {
                return false;
            }

            //Verify that either the foreign key is:
            //  1. Non composite, or 
            //  2. All columns on the child side are non nullable
            // NOTE: Technically we could also allow the case when only one column on the child side is nullable
            // and its corresponding column on the parent side is the only column referenced from the parent table. 
            if (childColumnVars.Count > 1 && childColumnVars.Where(v => v.ColumnMetadata.IsNullable).Count() > 0)
            {
                return false;
            }

            return TryEliminateRightTable(joinEdge, fkConstraint.ParentKeys.Count, true);
        }

        /// <summary>
        /// Helper method to try to eliminate the right table given a join edge.
        /// The right table should be eliminated if:
        /// 1. It does not have non key references, and
        /// 2. Either its columns are not referenced anywhere outside the join condition or, 
        /// if allowRefsForJoinedOnFkOnly is true, the join condition is only on the fk constraint 
        /// (which we deduct by only checking the count, since we already checked that the conditions do
        /// include the fk constraint.
        /// 3. It can be eliminated based on possible participation in a left outer join
        /// </summary>
        /// <param name="joinEdge"></param>
        /// <param name="fkConstraintKeyCount"></param>
        /// <param name="allowRefsForJoinedOnFkOnly"
        /// <returns></returns>
        private bool TryEliminateRightTable(JoinEdge joinEdge, int fkConstraintKeyCount, bool allowRefsForJoinedOnFkOnly)
        {
            if (HasNonKeyReferences(joinEdge.Right.Table))
            {
                return false;
            }

            if ((!allowRefsForJoinedOnFkOnly || joinEdge.RightVars.Count != fkConstraintKeyCount) && RightTableHasKeyReferences(joinEdge))
            {
                return false;
            }

            if (!CanBeEliminatedBasedOnLojParticipation(joinEdge.Right, joinEdge.Left))
            {
                return false;
            }

            // Eliminate the child table
            EliminateRightTable(joinEdge);

            return true;
        }

        /// <summary>
        /// Eliminate the join if possible, for this edge
        /// </summary>
        /// <param name="joinEdge">the current join edge</param>
        private void EliminateParentChildJoin(JoinEdge joinEdge)
        {
            List<ForeignKeyConstraint> fkConstraints;

            // Is there a foreign key constraint between these 2 tables?
            if (m_constraintManager.IsParentChildRelationship(joinEdge.Left.Table.TableMetadata.Extent, joinEdge.Right.Table.TableMetadata.Extent,
                out fkConstraints))
            {
                PlanCompiler.Assert(fkConstraints != null && fkConstraints.Count > 0, "invalid fk constraints?");
                // Now walk through the list of foreign key constraints and attempt join 
                // elimination
                foreach (ForeignKeyConstraint fkConstraint in fkConstraints)
                {
                    if (TryEliminateParentChildJoin(joinEdge, fkConstraint))
                    {
                        return;
                    }
                }
            }

            // For LeftOuterJoin we should check for the opportunity to eliminate based on a parent-child
            // relationship in the opposite direction too. For inner joins that should not be an issue
            // as the opposite join edge would have been generated too.
            if (joinEdge.JoinKind == JoinKind.LeftOuter)
            {
                if (m_constraintManager.IsParentChildRelationship(joinEdge.Right.Table.TableMetadata.Extent, joinEdge.Left.Table.TableMetadata.Extent,
                out fkConstraints))
                {
                    PlanCompiler.Assert(fkConstraints != null && fkConstraints.Count > 0, "invalid fk constraints?");
                    // Now walk through the list of foreign key constraints and attempt join 
                    // elimination
                    foreach (ForeignKeyConstraint fkConstraint in fkConstraints)
                    {
                        if (TryEliminateChildParentJoin(joinEdge, fkConstraint))
                        {
                            return;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Eliminate parent child nodes that this node participates in
        /// </summary>
        /// <param name="tableNode">the "left" table in a join</param>
        private void EliminateParentChildJoins(AugmentedTableNode tableNode)
        {
            foreach (JoinEdge joinEdge in tableNode.JoinEdges)
            {
                EliminateParentChildJoin(joinEdge);
                if (tableNode.IsEliminated)
                {
                    return;
                }
            }
        }

        /// <summary>
        /// Eliminate all parent-child joins in the join graph
        /// </summary>
        private void EliminateParentChildJoins()
        {
            foreach (AugmentedNode node in m_vertexes)
            {
                AugmentedTableNode tableNode = node as AugmentedTableNode;
                if (tableNode != null && !tableNode.IsEliminated)
                {
                    EliminateParentChildJoins(tableNode);
                }
            }
        }
        #endregion

        #region Rebuilding the Node Tree
        //
        // The goal of this submodule is to rebuild the node tree from the annotated node tree, 
        // and getting rid of eliminated tables along the way
        // 

        #region Main Rebuilding Methods

        /// <summary>
        /// Return the result of join elimination
        /// </summary>
        /// <returns>the transformed node tree</returns>
        private Node BuildNodeTree()
        {
            // Has anything changed? If not, then simply return the original tree.
            if (!m_modifiedGraph)
            {
                return m_root.Node;
            }

            // Generate transitive closure for all Vars in the varMap
            VarMap newVarMap = new VarMap();
            foreach (KeyValuePair<Var, Var> kv in m_varMap)
            {
                Var newVar1 = kv.Value;
                Var newVar2;
                while (m_varMap.TryGetValue(newVar1, out newVar2))
                {
                    PlanCompiler.Assert(newVar2 != null, "null var mapping?");
                    newVar1 = newVar2;
                }
                newVarMap[kv.Key] = newVar1;
            }
            m_varMap = newVarMap;

            // Otherwise build the tree
            Dictionary<Node, int> predicates;
            Node newNode = RebuildNodeTree(m_root, out predicates);
            PlanCompiler.Assert(newNode != null, "Resulting node tree is null");
            PlanCompiler.Assert(predicates == null || predicates.Count == 0, "Leaking predicates?");
            return newNode;
        }

        /// <summary>
        /// Build a filter node (if necessary) to prune out null values for the specified
        /// columns
        /// </summary>
        /// <param name="inputNode"></param>
        /// <param name="nonNullableColumns"></param>
        /// <returns></returns>
        private Node BuildFilterForNullableColumns(Node inputNode, VarVec nonNullableColumns)
        {
            if (nonNullableColumns == null)
            {
                return inputNode;
            }

            VarVec remappedVarVec = nonNullableColumns.Remap(m_varMap);
            if (remappedVarVec.IsEmpty)
            {
                return inputNode;
            }

            Node predNode = null;
            foreach (Var v in remappedVarVec)
            {
                Node varRefNode = m_command.CreateNode(m_command.CreateVarRefOp(v));
                Node isNotNullNode = m_command.CreateNode(m_command.CreateConditionalOp(OpType.IsNull), varRefNode);
                isNotNullNode = m_command.CreateNode(m_command.CreateConditionalOp(OpType.Not), isNotNullNode);
                if (predNode == null)
                {
                    predNode = isNotNullNode;
                }
                else
                {
                    predNode = m_command.CreateNode(m_command.CreateConditionalOp(OpType.And),
                        predNode, isNotNullNode);
                }
            }

            PlanCompiler.Assert(predNode != null, "Null predicate?");
            Node filterNode = m_command.CreateNode(m_command.CreateFilterOp(), inputNode, predNode);
            return filterNode;
        }

        /// <summary>
        /// Adds a filter node (if necessary) on top of the input node.
        /// Returns the input node, if the filter predicate is null - otherwise, adds a
        /// a new filter node above the input
        /// </summary>
        /// <param name="inputNode">the input node</param>
        /// <param name="predicateNode">the filter predicate</param>
        /// <returns></returns>
        private Node BuildFilterNode(Node inputNode, Node predicateNode)
        {
            if (predicateNode == null)
            {
                return inputNode;
            }
            else
            {
                return m_command.CreateNode(m_command.CreateFilterOp(), inputNode, predicateNode);
            }
        }

        /// <summary>
        /// Rebuilds the predicate for a join node and caculates the minimum location id at which it can be specified. 
        /// The predicate is an AND of the equijoin conditions and the "otherPredicate".
        /// 
        /// We first remap all columns in the equijoin predicates - if a column pair
        /// resolves to the same column, then we skip that pair.
        /// 
        /// The minimum location id at which a predicate can be specified is the minimum location id that is
        /// still at or above the minimum location id of all participating vars.  By default, it is the location id 
        /// of the input join node. However, because a table producing a participating var may be moved or 
        /// replaced by another table, the rebuilt predicate may need to be specified at higher location id.
        /// </summary>
        /// <param name="joinNode">the current join node</param>
        /// <param name="minLocationId">the minimum location id (AugumentedNode.Id) at which this predicate can be specified</param>
        /// <returns>the rebuilt predicate</returns>
        private Node RebuildPredicate(AugmentedJoinNode joinNode, out int minLocationId)
        {
            //
            // It is safe to initilaze the output location id to the location id of the joinNode. The nodes at lower 
            // location ids have already been processed, thus even if the least common ancestor of all participating 
            // vars is lower than the location id of the joinNode, the rebuilt predicate would not be propagated 
            // to nodes at lower location ids.
            //
            minLocationId = joinNode.Id;

            //Get the minimum location Id at which the other predicate can be specified.
            if (joinNode.OtherPredicate != null)
            {
                foreach (Var var in joinNode.OtherPredicate.GetNodeInfo(this.m_command).ExternalReferences)
                {
                    Var newVar;
                    if (!m_varMap.TryGetValue(var, out newVar))
                    {
                        newVar = var;
                    }
                    minLocationId = GetLeastCommonAncestor(minLocationId, GetLocationId(newVar, minLocationId));
                }
            }

            Node predicateNode = joinNode.OtherPredicate;
            for (int i = 0; i < joinNode.LeftVars.Count; i++)
            {
                Var newLeftVar;
                Var newRightVar;
                if (!m_varMap.TryGetValue(joinNode.LeftVars[i], out newLeftVar))
                {
                    newLeftVar = joinNode.LeftVars[i];
                }
                if (!m_varMap.TryGetValue(joinNode.RightVars[i], out newRightVar))
                {
                    newRightVar = joinNode.RightVars[i];
                }
                if (newLeftVar.Equals(newRightVar))
                {
                    continue;
                }

                minLocationId = GetLeastCommonAncestor(minLocationId, GetLocationId(newLeftVar, minLocationId));
                minLocationId = GetLeastCommonAncestor(minLocationId, GetLocationId(newRightVar, minLocationId));

                Node leftVarNode = m_command.CreateNode(m_command.CreateVarRefOp(newLeftVar));
                Node rightVarNode = m_command.CreateNode(m_command.CreateVarRefOp(newRightVar));

                Node equalsNode = m_command.CreateNode(m_command.CreateComparisonOp(OpType.EQ),
                    leftVarNode, rightVarNode);
                if (predicateNode != null)
                {
                    predicateNode = PlanCompilerUtil.CombinePredicates(equalsNode, predicateNode, m_command);
                }
                else
                {
                    predicateNode = equalsNode;
                }
            }

            return predicateNode;
        }

        /// <summary>
        /// Rebuilds a crossjoin node tree. We visit each child of the cross join, and get
        /// back a list of nodes. If the list of nodes has 
        ///   0 children - we return null
        ///   1 child - we return the single child
        ///   otherwise - we build a new crossjoin op with all the children
        /// </summary>
        /// <param name="joinNode">the crossjoin node</param>
        /// <returns>new node tree</returns>
        private Node RebuildNodeTreeForCrossJoins(AugmentedJoinNode joinNode)
        {
            List<Node> newChildren = new List<Node>();
            foreach (AugmentedNode chi in joinNode.Children)
            {
                Dictionary<Node, int> predicates;
                newChildren.Add(RebuildNodeTree(chi, out predicates));
                PlanCompiler.Assert(predicates == null || predicates.Count == 0, "Leaking predicates");
            }

            if (newChildren.Count == 0)
            {
                return null;
            }
            else if (newChildren.Count == 1)
            {
                return newChildren[0];
            }
            else
            {
                Node newJoinNode = m_command.CreateNode(m_command.CreateCrossJoinOp(), newChildren);
                m_processedNodes[newJoinNode] = newJoinNode;
                return newJoinNode;
            }
        }

        /// <summary>
        /// Rebuilds the node tree for a join. 
        /// For crossjoins, we delegate to the function above. For other cases, we first
        /// invoke this function recursively on the left and the right inputs. 
        /// </summary>
        /// <param name="joinNode">the annotated join node tree</param>
        /// <param name="predicates">A dictionary of output predicates that should be included in ancestor joins
        /// along with the minimum location id at which they can be specified</param>
        /// <returns>rebuilt tree</returns>
        private Node RebuildNodeTree(AugmentedJoinNode joinNode, out Dictionary<Node, int> predicates)
        {
            //
            // Handle the simple cases first - cross joins
            //
            if (joinNode.Node.Op.OpType == OpType.CrossJoin)
            {
                predicates = null;
                return RebuildNodeTreeForCrossJoins(joinNode);
            }

            Dictionary<Node, int> leftPredicates;
            Dictionary<Node, int> rightPredicates;

            Node leftNode = RebuildNodeTree(joinNode.Children[0], out leftPredicates);
            Node rightNode = RebuildNodeTree(joinNode.Children[1], out rightPredicates);

            int localPredicateMinLocationId;
            Node localPredicateNode;

            // The special case first, when we may 'eat' the local predicate
            if (leftNode != null && rightNode == null && joinNode.Node.Op.OpType == OpType.LeftOuterJoin)
            {
                // Ignore the local predicate
                // Is this correct always? What kind of assertions can we make here?
                localPredicateMinLocationId = joinNode.Id;
                localPredicateNode = null;
            }
            else
            {
                localPredicateNode = RebuildPredicate(joinNode, out localPredicateMinLocationId);
            }

            localPredicateNode = CombinePredicateNodes(joinNode.Id, localPredicateNode, localPredicateMinLocationId, leftPredicates, rightPredicates, out predicates);

            if (leftNode == null && rightNode == null)
            {
                if (localPredicateNode == null)
                {
                    return null;
                }
                else
                {
                    Node singleRowTableNode = m_command.CreateNode(m_command.CreateSingleRowTableOp());
                    return BuildFilterNode(singleRowTableNode, localPredicateNode);
                }
            }
            else if (leftNode == null)
            {
                return BuildFilterNode(rightNode, localPredicateNode);
            }
            else if (rightNode == null)
            {
                return BuildFilterNode(leftNode, localPredicateNode);
            }
            else
            {
                if (localPredicateNode == null)
                {
                    localPredicateNode = m_command.CreateNode(m_command.CreateTrueOp());
                }

                Node newJoinNode = m_command.CreateNode(joinNode.Node.Op,
                        leftNode, rightNode, localPredicateNode);
                m_processedNodes[newJoinNode] = newJoinNode;
                return newJoinNode;
            }
        }

        /// <summary>
        /// Rebuild the node tree for a TableNode. 
        /// 
        /// - Keep following the ReplacementTable links until we get to a node that
        ///   is either null, or has a "false" value for the IsEliminated property  
        /// - If the result is null, then simply return null
        /// - If the tableNode we ended up with has already been "placed" in the resulting
        ///   node tree, then return null again
        /// - If the tableNode has a set of non-nullable columns, then build a filterNode
        ///   above the ScanTable node (pruning out null values); otherwise, simply return 
        ///   the ScanTable node
        /// </summary>
        /// <param name="tableNode">the "augmented" tableNode</param>
        /// <returns>rebuilt node tree for this node</returns>
        private Node RebuildNodeTree(AugmentedTableNode tableNode)
        {
            AugmentedTableNode replacementNode = tableNode;

            //
            // If this table has already been moved - nothing further to do.
            //
            if (tableNode.IsMoved)
            {
                return null;
            }

            //
            // Identify the replacement table for this node
            //
            while (replacementNode.IsEliminated)
            {
                replacementNode = replacementNode.ReplacementTable;
                if (replacementNode == null)
                {
                    return null;
                }
            }

            //
            // Check to see if the replacement node has already been put
            // in place in the node tree (possibly as part of eliminating some other join). 
            // In that case, we don't need to do anything further - simply return null
            //
            if (replacementNode.NewLocationId < tableNode.Id)
            {
                return null;
            }

            //
            // ok: so we now have a replacement node that must be used in place
            // of the current table. Check to see if the replacement node has any
            // columns that would require nulls to be pruned out
            //
            Node filterNode = BuildFilterForNullableColumns(replacementNode.Node, replacementNode.NullableColumns);
            return filterNode;
        }

        /// <summary>
        /// Rebuilds the node tree from the annotated node tree. This function is 
        /// simply a dispatcher
        ///    ScanTable - call RebuildNodeTree for ScanTable
        ///    Join - call RebuildNodeTree for joinOp
        ///    Anything else - return the underlying node
        /// </summary>
        /// <param name="augmentedNode">annotated node tree</param>
        /// <param name="predicates">the output predicate that should be included in the parent join</param>
        /// <returns>the rebuilt node tree</returns>
        private Node RebuildNodeTree(AugmentedNode augmentedNode, out Dictionary<Node, int> predicates)
        {
            switch (augmentedNode.Node.Op.OpType)
            {
                case OpType.ScanTable:
                    predicates = null;
                    return RebuildNodeTree((AugmentedTableNode)augmentedNode);

                case OpType.CrossJoin:
                case OpType.LeftOuterJoin:
                case OpType.InnerJoin:
                case OpType.FullOuterJoin:
                    return RebuildNodeTree((AugmentedJoinNode)augmentedNode, out predicates);

                default:
                    predicates = null;
                    return augmentedNode.Node;
            }
        }

        #endregion

        #region Helper Methods for Rebuilding the Node Tree

        /// <summary>
        /// Helper method for RebuildNodeTree.
        /// Given predicate nodes and the minimum location ids at which they can be specified, it creates:
        /// 1. A single predicate AND-ing all input predicates with a minimum location id that is less or equal to the given targetNodeId.
        /// 2. A dictionary of all other input predicates and their target minimum location ids.
        /// </summary>
        /// <param name="targetNodeId">The location id of the resulting predicate </param>
        /// <param name="localPredicateNode">A predicate</param>
        /// <param name="localPredicateMinLocationId">The location id for the localPredicateNode</param>
        /// <param name="leftPredicates">A dictionary of predicates and the minimum location id at which they can be specified</param>
        /// <param name="rightPredicates">A dictionary of predicates and the minimum location id at which they can be specified</param>
        /// <param name="outPredicates">An output dictionary of predicates and the minimum location id at which they can be specified 
        /// that includes all input predicates with minimum location id greater then targetNodeId</param>
        /// <returns>A single predicate "AND"-ing all input predicates with a minimum location id that is less or equal to the tiven targetNodeId.</returns>
        private Node CombinePredicateNodes(int targetNodeId, Node localPredicateNode, int localPredicateMinLocationId, Dictionary<Node, int> leftPredicates, Dictionary<Node, int> rightPredicates, out Dictionary<Node, int> outPredicates)
        {
            Node result = null;
            outPredicates = new Dictionary<Node, int>();

            if (localPredicateNode != null)
            {
                result = ClassifyPredicate(targetNodeId, localPredicateNode, localPredicateMinLocationId, result, outPredicates);
            }

            if (leftPredicates != null)
            {
                foreach (KeyValuePair<Node, int> predicatePair in leftPredicates)
                {
                    result = ClassifyPredicate(targetNodeId, predicatePair.Key, predicatePair.Value, result, outPredicates);
                }
            }

            if (rightPredicates != null)
            {
                foreach (KeyValuePair<Node, int> predicatePair in rightPredicates)
                {
                    result = ClassifyPredicate(targetNodeId, predicatePair.Key, predicatePair.Value, result, outPredicates);
                }
            }

            return result;
        }

        /// <summary>
        /// Helper method for <see cref="CombinePredicateNodes"/>
        /// If the predicateMinimuLocationId is less or equal to the target location id of the current result, it is AND-ed with the 
        /// current result, otherwise it is included in the list of predicates that need to be propagated up (outPredicates)
        /// </summary>
        /// <param name="targetNodeId"></param>
        /// <param name="predicateNode"></param>
        /// <param name="predicateMinLocationId"></param>
        /// <param name="result"></param>
        /// <param name="outPredicates"></param>
        /// <returns></returns>
        private Node ClassifyPredicate(int targetNodeId, Node predicateNode, int predicateMinLocationId, Node result, Dictionary<Node, int> outPredicates)
        {
            if (targetNodeId >= predicateMinLocationId)
            {
                result = CombinePredicates(result, predicateNode);
            }
            else
            {
                outPredicates.Add(predicateNode, predicateMinLocationId);
            }
            return result;
        }

        /// <summary>
        /// Combines two predicates into one by AND-ing them.
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <returns></returns>
        private Node CombinePredicates(Node node1, Node node2)
        {
            if (node1 == null)
            {
                return node2;
            }

            if (node2 == null)
            {
                return node1;
            }

            return PlanCompilerUtil.CombinePredicates(node1, node2, m_command);
        }

        /// <summary>
        /// Get the location id of the AugumentedTableNode at which the given var is defined. 
        /// If the var is not in th m_varToDefiningNodeMap, then it return the input defaultLocationId
        /// </summary>
        /// <param name="var"></param>
        /// <param name="defaultLocationId"></param>
        /// <returns></returns>
        private int GetLocationId(Var var, int defaultLocationId)
        {
            AugmentedTableNode node;
            if (m_varToDefiningNodeMap.TryGetValue(var, out node))
            {
                if (node.IsMoved)
                {
                    return node.NewLocationId;
                }
                return node.Id;
            }
            return defaultLocationId;
        }

        /// <summary>
        /// Gets the location id of least common ancestor for two nodes in the tree given their location ids
        /// </summary>
        /// <param name="nodeId1"></param>
        /// <param name="nodeId2"></param>
        /// <returns></returns>
        private int GetLeastCommonAncestor(int nodeId1, int nodeId2)
        {
            if (nodeId1 == nodeId2)
            {
                return nodeId1;
            }

            AugmentedNode currentNode = m_root;
            AugmentedNode child1Parent = currentNode;
            AugmentedNode child2Parent = currentNode;

            while (child1Parent == child2Parent)
            {
                currentNode = child1Parent;
                if (currentNode.Id == nodeId1 || currentNode.Id == nodeId2)
                {
                    return currentNode.Id;
                }
                child1Parent = PickSubtree(nodeId1, currentNode);
                child2Parent = PickSubtree(nodeId2, currentNode);
            }
            return currentNode.Id;
        }

        /// <summary>
        /// Helper method for <see cref="GetLeastCommonAncestor(int, int)"/>
        /// Given a root node pick its immediate child to which the node identifed with the given nodeId bellongs.
        /// </summary>
        /// <param name="nodeId"></param>
        /// <param name="root"></param>
        /// <returns>
        /// The immediate child of the given root that is root of the subree that 
        /// contains the node with the given nodeId.
        /// </returns>
        private static AugmentedNode PickSubtree(int nodeId, AugmentedNode root)
        {
            AugmentedNode subree = root.Children[0];
            int i = 1;
            while ((subree.Id < nodeId) && (i < root.Children.Count))
            {
                subree = root.Children[i];
                i++;
            }
            return subree;
        }

        #endregion

        #endregion

        #endregion
    }
    #endregion
}

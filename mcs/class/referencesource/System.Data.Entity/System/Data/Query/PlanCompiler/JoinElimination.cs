//---------------------------------------------------------------------
// <copyright file="JoinElimination.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
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

using System.Data.Query.InternalTrees;
using System.Data.Metadata.Edm;

namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// The JoinElimination module is intended to do just that - eliminate unnecessary joins. 
    /// This module deals with the following kinds of joins
    ///    * Self-joins: The join can be eliminated, and either of the table instances can be 
    ///                  used instead
    ///    * Implied self-joins: Same as above
    ///    * PK-FK joins: (More generally, UniqueKey-FK joins): Eliminate the join, and use just the FK table, if no 
    ///       column of the PK table is used (other than the join condition)
    ///    * PK-PK joins: Eliminate the right side table, if we have a left-outer join
    /// </summary>
    internal class JoinElimination : BasicOpVisitorOfNode
    {
        #region private constants
        private const string SqlServerCeNamespaceName = "SqlServerCe";
        #endregion

        #region private state
        private PlanCompiler m_compilerState;
        private Command Command { get { return m_compilerState.Command; } }
        private ConstraintManager ConstraintManager { get { return m_compilerState.ConstraintManager;  } }
        private Dictionary<Node, Node> m_joinGraphUnnecessaryMap = new Dictionary<Node,Node>();
        private VarRemapper m_varRemapper;
        private bool m_treeModified = false;
        private VarRefManager m_varRefManager;
        private Nullable<bool> m_isSqlCe = null;
        #endregion

        #region constructors
        private JoinElimination(PlanCompiler compilerState)
        {
            m_compilerState = compilerState;
            m_varRemapper = new VarRemapper(m_compilerState.Command);
            m_varRefManager = new VarRefManager(m_compilerState.Command); 
        }
        #endregion

        #region public surface
        internal static bool Process(PlanCompiler compilerState)
        {
            JoinElimination je = new JoinElimination(compilerState);
            je.Process();
            return je.m_treeModified;
        }
        #endregion

        #region private methods

        /// <summary>
        /// Invokes the visitor
        /// </summary>
        private void Process()
        {
            this.Command.Root = VisitNode(this.Command.Root);
        }

        #region JoinHelpers

        #region Building JoinGraphs
        /// <summary>
        /// Do we need to build a join graph for this node - returns false, if we've already
        /// processed this
        /// </summary>
        /// <param name="joinNode"></param>
        /// <returns></returns>
        private bool NeedsJoinGraph(Node joinNode)
        {
            return !m_joinGraphUnnecessaryMap.ContainsKey(joinNode);
        }

        /// <summary>
        /// Do the real processing of the join graph. 
        /// </summary>
        /// <param name="joinNode">current join node</param>
        /// <returns>modified join node</returns>
        private Node ProcessJoinGraph(Node joinNode)
        {
            // Build the join graph
            JoinGraph joinGraph = new JoinGraph(this.Command, this.ConstraintManager, this.m_varRefManager, joinNode, this.IsSqlCeProvider);

            // Get the transformed node tree
            VarMap remappedVars;
            Dictionary<Node, Node> processedNodes;
            Node newNode = joinGraph.DoJoinElimination(out remappedVars, out processedNodes);

            // Get the set of vars that need to be renamed
            foreach (KeyValuePair<Var, Var> kv in remappedVars)
            {
                m_varRemapper.AddMapping(kv.Key, kv.Value);
            }
            // get the set of nodes that have already been processed
            foreach (Node n in processedNodes.Keys)
            {
                m_joinGraphUnnecessaryMap[n] = n;
            }

            return newNode;
        }

        /// <summary>
        /// Indicates whether we are running against a SQL CE provider or not.
        /// </summary>
        private bool IsSqlCeProvider
        {
            get
            {
                if (!m_isSqlCe.HasValue)
                {
                    // Figure out if we are using SQL CE by asking the store provider manifest for its namespace name.
                    PlanCompiler.Assert(m_compilerState != null, "Plan compiler cannot be null");
                    var sspace = (StoreItemCollection)m_compilerState.MetadataWorkspace.GetItemCollection(Metadata.Edm.DataSpace.SSpace);
                    if (sspace != null)
                    {
                        m_isSqlCe = sspace.StoreProviderManifest.NamespaceName == JoinElimination.SqlServerCeNamespaceName;
                    }
                }
                // If the sspace was null then m_isSqlCe still doesn't have a value. Use 'false' as default.
                return m_isSqlCe.HasValue ? m_isSqlCe.Value : false;
            }
        }
        
        /// <summary>
        /// Default handler for a node. Simply visits the children, then handles any var
        /// remapping, and then recomputes the node info
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private Node VisitDefaultForAllNodes(Node n)
        {
            VisitChildren(n);
            m_varRemapper.RemapNode(n);
            this.Command.RecomputeNodeInfo(n);
            return n;
        }

        #endregion

        #endregion

        #region Visitor overrides

        /// <summary>
        /// Invokes default handling for a node and adds the child-parent tracking info to the VarRefManager.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitDefault(Node n)
        {
            m_varRefManager.AddChildren(n);
            return VisitDefaultForAllNodes(n);
        }

        #region RelOps
        #region JoinOps

        /// <summary>
        /// Build a join graph for this node for this node if necessary, and process it
        /// </summary>
        /// <param name="op">current join op</param>
        /// <param name="joinNode">current join node</param>
        /// <returns></returns>
        protected override Node VisitJoinOp(JoinBaseOp op, Node joinNode)
        {
            Node newNode;

            // Build and process a join graph if necessary
            if (NeedsJoinGraph(joinNode))
            {
                newNode = ProcessJoinGraph(joinNode);
                if (newNode != joinNode)
                {
                    m_treeModified = true;
                }
            }
            else
            {
                newNode = joinNode;
            }

            // Now do the default processing (ie) visit the children, compute the nodeinfo etc.
            return VisitDefaultForAllNodes(newNode);
        }

        #endregion

        #endregion
        #endregion

        #endregion

    }
}

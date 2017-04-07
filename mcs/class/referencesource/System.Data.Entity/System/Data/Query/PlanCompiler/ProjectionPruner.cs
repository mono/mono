//---------------------------------------------------------------------
// <copyright file="ProjectionPruner.cs" company="Microsoft">
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
using System.Text;
using System.Linq;

using md = System.Data.Metadata.Edm;
using cqt = System.Data.Common.CommandTrees;
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{

    /// <summary>
    /// The ProjectionPruner module is responsible for eliminating unnecessary column
    /// references (and other expressions) from the query.
    ///
    /// Projection pruning logically operates in two passes - the first pass is a top-down
    /// pass where information about all referenced columns and expressions is collected
    /// (pushed down from a node to its children).
    /// 
    /// The second phase is a bottom-up phase, where each node (in response to the 
    /// information collected above) attempts to rid itself of unwanted columns and 
    /// expressions.
    /// 
    /// The two phases can be combined into a single tree walk, where for each node, the 
    /// processing is on the lines of:
    /// 
    /// - compute and push information to children (top-down)
    /// - process children
    /// - eliminate unnecessary references from myself (bottom-up)
    /// 
    /// </summary>
    internal class ProjectionPruner : BasicOpVisitorOfNode
    {
        #region Nested Classes
        /// <summary>
        /// This class tracks down the vars that are referenced in the column map
        /// </summary>
        private class ColumnMapVarTracker : ColumnMapVisitor<VarVec>
        {
            #region public methods
            /// <summary>
            /// Find all vars that were referenced in the column map. Looks for VarRefColumnMap
            /// in the ColumnMap tree, and tracks those vars
            /// 
            /// NOTE: The "vec" parameter must be supplied by the caller. The caller is responsible for
            /// clearing out this parameter (if necessary) before calling into this function
            /// </summary>
            /// <param name="columnMap">the column map to traverse</param>
            /// <param name="vec">the set of referenced columns</param>
            internal static void FindVars(ColumnMap columnMap, VarVec vec)
            {
                ColumnMapVarTracker tracker = new ColumnMapVarTracker();
                columnMap.Accept<VarVec>(tracker, vec);
                return;
            }
            #endregion

            #region constructors
            /// <summary>
            /// Trivial constructor
            /// </summary>
            private ColumnMapVarTracker() : base() { }
            #endregion

            #region overrides
            /// <summary>
            /// Handler for VarRefColumnMap. Simply adds the "var" to the set of referenced vars
            /// </summary>
            /// <param name="columnMap">the current varRefColumnMap</param>
            /// <param name="arg">the set of referenced vars so far</param>
            internal override void Visit(VarRefColumnMap columnMap, VarVec arg)
            {
                arg.Set(columnMap.Var);
                base.Visit(columnMap, arg);
            }
            #endregion
        }
        #endregion

        #region private state

        private PlanCompiler m_compilerState;
        private Command m_command { get { return m_compilerState.Command; } }
        private VarVec m_referencedVars; // the list of referenced vars in the query

        #endregion

        #region constructor

        /// <summary>
        /// Trivial private constructor
        /// </summary>
        /// <param name="compilerState">current compiler state</param>
        private ProjectionPruner(PlanCompiler compilerState)
        {
            m_compilerState = compilerState;
            m_referencedVars = compilerState.Command.CreateVarVec();
        }

        #endregion

        #region Process Driver

        /// <summary>
        /// Runs through the root node of the tree, and eliminates all
        /// unreferenced expressions
        /// </summary>
        /// <param name="compilerState">current compiler state</param>
        internal static void Process(PlanCompiler compilerState)
        {
            compilerState.Command.Root = Process(compilerState, compilerState.Command.Root);
        }

        /// <summary>
        /// Runs through the given subtree, and eliminates all
        /// unreferenced expressions
        /// </summary>
        /// <param name="compilerState">current compiler state</param>
        /// <param name="node">The node to be processed</param>
        /// <returns>The processed, i.e. transformed node</returns>
        internal static Node Process(PlanCompiler compilerState, Node node)
        {
            ProjectionPruner pruner = new ProjectionPruner(compilerState);
            return pruner.Process(node);
        }

        /// <summary>
        /// The real driver of the pruning process. Simply invokes the visitor over the input node
        /// </summary>
        /// <param name="node">The node to be processed</param>
        /// <returns>The processed node</returns>
        private Node Process(Node node)
        {
            return VisitNode(node);
        }

        #endregion

        #region misc helpers

        /// <summary>
        /// Adds a reference to this Var
        /// </summary>
        /// <param name="v"></param>
        private void AddReference(Var v)
        {
            m_referencedVars.Set(v);
        }

        /// <summary>
        /// Adds a reference to each var in a set of Vars
        /// </summary>
        /// <param name="varSet"></param>
        private void AddReference(IEnumerable<Var> varSet)
        {
            foreach (Var v in varSet)
            {
                AddReference(v);
            }
        }

        /// <summary>
        /// Is this Var referenced?
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private bool IsReferenced(Var v)
        {
            return m_referencedVars.IsSet(v);
        }
        /// <summary>
        /// Is this var unreferenced? Duh
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private bool IsUnreferenced(Var v)
        {
            return !IsReferenced(v);
        }

        /// <summary>
        /// Prunes a VarMap - gets rid of unreferenced vars from the VarMap inplace
        /// Additionally, propagates var references to the inner vars
        /// </summary>
        /// <param name="varMap"></param>
        private void PruneVarMap(VarMap varMap)
        {
            List<Var> unreferencedVars = new List<Var>();
            // build up a list of unreferenced vars
            foreach (Var v in varMap.Keys)
            {
                if (!IsReferenced(v))
                {
                    unreferencedVars.Add(v);
                }
                else
                {
                    AddReference(varMap[v]);
                }
            }
            // remove each of the corresponding entries from the varmap
            foreach (Var v in unreferencedVars)
            {
                varMap.Remove(v);
            }
        }

        /// <summary>
        /// Prunes a varset - gets rid of unreferenced vars from the Varset in place
        /// </summary>
        /// <param name="varSet">the varset to prune</param>
        private void PruneVarSet(VarVec varSet)
        {
            varSet.And(m_referencedVars);
        }

        #endregion

        #region Visitor Helpers

        /// <summary>
        /// Visits the children and recomputes the node info
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildren(Node n)
        {
            base.VisitChildren(n);
            m_command.RecomputeNodeInfo(n);
        }

        /// <summary>
        /// Visits the children in reverse order and recomputes the node info
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildrenReverse(Node n)
        {
            base.VisitChildrenReverse(n);
            m_command.RecomputeNodeInfo(n);
        }
        #endregion

        #region Visitor methods

        #region AncillaryOp Visitors

        /// <summary>
        /// VarDefListOp
        /// 
        /// Walks the children (VarDefOp), and looks for those whose Vars 
        /// have been referenced. Only those VarDefOps are visited - the 
        /// others are ignored.
        /// 
        /// At the end, a new list of children is created - with only those 
        /// VarDefOps that have been referenced
        /// </summary>
        /// <param name="op">the varDefListOp</param>
        /// <param name="n">corresponding node</param>
        /// <returns>modified node</returns>
        public override Node Visit(VarDefListOp op, Node n)
        {

            // NOTE: It would be nice to optimize this to only create a new node 
            //       and new list, if we needed to eliminate some arguments, but
            //       I'm not sure that the effort to eliminate the allocations 
            //       wouldn't be more expensive than the allocations themselves.
            //       It's something that we can consider if it shows up on the
            //       perf radar.

            // Get rid of all the children that we don't care about (ie)
            // those VarDefOp's that haven't been referenced
            List<Node> newChildren = new List<Node>();
            foreach (Node chi in n.Children)
            {
                VarDefOp varDefOp = chi.Op as VarDefOp;
                if (IsReferenced(varDefOp.Var))
                {
                    newChildren.Add(VisitNode(chi));
                }
            }
            return m_command.CreateNode(op, newChildren);
        }

        #endregion

        #region PhysicalOps

        /// <summary>
        /// PhysicalProjectOp
        /// 
        /// Insist that all Vars in this are required
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(PhysicalProjectOp op, Node n)
        {
            if (n == m_command.Root)
            {
                //
                // Walk the column map to find all the referenced vars
                //
                ColumnMapVarTracker.FindVars(op.ColumnMap, m_referencedVars);
                op.Outputs.RemoveAll(IsUnreferenced);
            }
            else
            {
                AddReference(op.Outputs);
            }
            // then visit the children
            VisitChildren(n);

            return n;
        }

        /// <summary>
        /// NestOps
        /// 
        /// Common handling for all NestOps. 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitNestOp(NestBaseOp op, Node n)
        {
            // Mark all vars as needed
            AddReference(op.Outputs);

            // visit children. Need to do some more actually - to indicate that all 
            // vars from the children are really required. 
            VisitChildren(n);
            return n;
        }

        /// <summary>
        /// SingleStreamNestOp 
        /// 
        /// Insist (for now) that all Vars are required
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(SingleStreamNestOp op, Node n)
        {
            AddReference(op.Discriminator);
            return VisitNestOp(op, n);
        }

        /// <summary>
        /// MultiStreamNestOp
        /// 
        /// Insist (for now) that all Vars are required
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(MultiStreamNestOp op, Node n)
        {
            return VisitNestOp(op, n);
        }

        #endregion

        #region RelOp Visitors

        /// <summary>
        /// ApplyOps
        /// 
        /// Common handling for all ApplyOps. Visit the right child first to capture
        /// any references to the left, and then visit the left child.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n">the apply op</param>
        /// <returns>modified subtree</returns>
        protected override Node VisitApplyOp(ApplyBaseOp op, Node n)
        {
            // visit the right child first, then the left
            VisitChildrenReverse(n);
            return n;
        }

        /// <summary>
        /// DistinctOp
        /// 
        /// We remove all null and constant keys that are not referenced as long as 
        /// there is one key left. We add all remaining keys to the referenced list
        /// and proceed to the inputs
        /// </summary>
        /// <param name="op">the DistinctOp</param>
        /// <param name="n">Current subtree</param>
        /// <returns></returns>
        public override Node Visit(DistinctOp op, Node n)
        {
            if (op.Keys.Count > 1 && n.Child0.Op.OpType == OpType.Project)
            {
                RemoveRedundantConstantKeys(op.Keys, ((ProjectOp)n.Child0.Op).Outputs, n.Child0.Child1);
            }
            AddReference(op.Keys); // mark all keys as referenced - nothing more to do
            VisitChildren(n); // visit the children
            return n;
        }

        /// <summary>
        /// ElementOp
        /// 
        /// An ElementOp that is still present when Projection Prunning is invoked can only get introduced 
        /// in the TransformationRules phase by transforming an apply operation into a scalar subquery. 
        /// Such ElementOp serves as root of a defining expression of a VarDefinitionOp node and 
        /// thus what it produces is useful.
        /// </summary>
        /// <param name="op">the ElementOp</param>
        /// <param name="n">Current subtree</param>
        /// <returns></returns>
        public override Node Visit(ElementOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = m_command.GetExtendedNodeInfo(n.Child0);
            AddReference(nodeInfo.Definitions);

            n.Child0 = VisitNode(n.Child0); // visit the child
            m_command.RecomputeNodeInfo(n);
            return n;
        }

        /// <summary>
        /// FilterOp
        /// 
        /// First visit the predicate (because that may contain references to 
        /// the relop input), and then visit the relop input. No additional 
        /// processing is required
        /// </summary>
        /// <param name="op">the filterOp</param>
        /// <param name="n">current node</param>
        /// <returns></returns>
        public override Node Visit(FilterOp op, Node n)
        {
            // visit the predicate first, and then teh relop input
            VisitChildrenReverse(n);
            return n;
        }

        /// <summary>
        /// GroupByBase
        /// 
        /// First, we visit the vardeflist for aggregates and potentially group aggregates
        /// as they may reference keys (including constant keys). 
        /// Then we remove all null and constant keys that are not referenced as long as 
        /// there is one key left. We add all remaining key columns to the referenced list.
        /// Then we walk through the vardeflist for the keys; and finally process the relop input
        /// Once we're done, we update the "Outputs" varset - to account for any 
        /// pruned vars. The "Keys" varset will not change
        /// </summary>
        /// <param name="op">the groupbyOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>modified subtree</returns>
        protected override Node VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            // DevDiv#322980: Visit the vardeflist for aggregates and potentially group aggregates before removing 
            // redundant constant keys. This is because they may depend on (reference) the keys
            for (int i = n.Children.Count - 1; i >= 2; i--)
            {
                n.Children[i] = VisitNode(n.Children[i]);
            }

            //All constant and null keys that are not referenced can be removed
            //as long as there is at least one key left.           
            if (op.Keys.Count > 1)
            {
                RemoveRedundantConstantKeys(op.Keys, op.Outputs, n.Child1);
            }

            AddReference(op.Keys); // all keys are referenced

            //Visit the keys
            n.Children[1] = VisitNode(n.Children[1]);

            //Visit the input
            n.Children[0] = VisitNode(n.Children[0]);

            PruneVarSet(op.Outputs); // remove unnecessary vars from the outputs

            //SQLBUDT #543064: If there are no keys to start with
            // and none of the aggregates is referenced, the GroupBy
            // is equivalent to a SingleRowTableOp
            if (op.Keys.Count == 0 && op.Outputs.Count == 0)
            {
                return m_command.CreateNode(m_command.CreateSingleRowTableOp());
            }

            m_command.RecomputeNodeInfo(n);
            return n;
        }

        /// <summary>
        /// Helper method for removing redundant constant keys from GroupByOp and DistictOp.
        /// It only examines the keys defined in the given varDefListNode.
        /// It removes all constant and null keys that are not referenced elsewhere, 
        /// but ensuring that at least one key is left. 
        /// It should not be called with empty keyVec. 
        /// </summary>
        /// <param name="keyVec">The keys</param>
        /// <param name="outputVec">The var vec that needs to be updated along with the keys</param>
        /// <param name="varDefListNode">Var def list node for the keys </param>
        private void RemoveRedundantConstantKeys(VarVec keyVec, VarVec outputVec, Node varDefListNode)
        {
            //Find all the keys that are nulls and constants
            List<Node> constantKeys = varDefListNode.Children.Where(d => d.Op.OpType == OpType.VarDef 
                && PlanCompilerUtil.IsConstantBaseOp(d.Child0.Op.OpType)).ToList();

            VarVec constantKeyVars = this.m_command.CreateVarVec(constantKeys.Select(d => ((VarDefOp)d.Op).Var));

            //Get the list of unreferenced  constant keys
            constantKeyVars.Minus(m_referencedVars);

            //Remove the unreferenced constant keys 
            keyVec.Minus(constantKeyVars);
            outputVec.Minus(constantKeyVars);

            varDefListNode.Children.RemoveAll(c => constantKeys.Contains(c) && constantKeyVars.IsSet(((VarDefOp)c.Op).Var));

            //If no keys are left add one. 
            if (keyVec.Count == 0)
            {
                Node keyNode = constantKeys.First();
                Var keyVar = ((VarDefOp)keyNode.Op).Var;
                keyVec.Set(keyVar);
                outputVec.Set(keyVar);
                varDefListNode.Children.Add(keyNode);
            }
        }

        /// <summary>
        /// First defer to default handling for groupby nodes
        /// If all group aggregate vars are prunned out turn it into a GroupBy.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(GroupByIntoOp op, Node n)
        {
            Node result = VisitGroupByOp(op, n);

            //Transform the GroupByInto into a GroupBy if all group aggregate vars were prunned out
            if (result.Op.OpType == OpType.GroupByInto && n.Child3.Children.Count == 0)
            {
                GroupByIntoOp newOp = (GroupByIntoOp)result.Op;

                result = m_command.CreateNode(m_command.CreateGroupByOp(newOp.Keys, newOp.Outputs),
                    result.Child0, result.Child1, result.Child2);
            }
            return result;
        }

        /// <summary>
        /// JoinOps
        /// 
        /// Common handling for all join ops. For all joins (other than crossjoin),
        /// we must first visit the predicate (to capture any references from it), and
        /// then visit the relop inputs. The relop inputs can be visited in any order
        /// because there can be no correlations between them
        /// For crossjoins, we simply use the default processing - visit all children
        /// ; there can be no correlations between the nodes anyway
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n">Node for the join subtree</param>
        /// <returns>modified subtree</returns>
        protected override Node VisitJoinOp(JoinBaseOp op, Node n)
        {
            // Simply visit all children for a CrossJoin
            if (n.Op.OpType == OpType.CrossJoin)
            {
                VisitChildren(n);
                return n;
            }

            // For other joins, we first need to visit the predicate, and then the
            // other inputs
            // first visit the predicate
            n.Child2 = VisitNode(n.Child2);
            // then visit the 2 join inputs
            n.Child0 = VisitNode(n.Child0);
            n.Child1 = VisitNode(n.Child1);
            m_command.RecomputeNodeInfo(n);

            return n;
        }

        /// <summary>
        /// ProjectOp
        /// 
        /// We visit the projections first (the VarDefListOp child), and then 
        /// the input (the RelOp child) - this reverse order is necessary, since
        /// the projections need to be visited to determine if anything from
        /// the input is really needed.
        /// 
        /// The VarDefListOp child will handle the removal of unnecessary VarDefOps.
        /// On the way out, we then update our "Vars" property to reflect the Vars
        /// that have been eliminated
        /// </summary>
        /// <param name="op">the ProjectOp</param>
        /// <param name="n">the current node</param>
        /// <returns>modified subtree</returns>
        public override Node Visit(ProjectOp op, Node n)
        {

            // Update my Vars - to remove "unreferenced" vars. Do this before visiting 
            // the children - the outputs of the ProjectOp are only consumed by upstream
            // consumers, and if a Var has not yet been referenced, its not needed upstream
            PruneVarSet(op.Outputs);

            // first visit the computed expressions, then visit the input relop
            VisitChildrenReverse(n);

            // If there are no Vars left, then simply return my child - otherwise, 
            // return the current node
            return op.Outputs.IsEmpty ? n.Child0 : n;
        }

        /// <summary>
        /// ScanTableOp 
        /// 
        /// Update the list of referenced columns
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ScanTableOp op, Node n)
        {
            PlanCompiler.Assert(!n.HasChild0, "scanTable with an input?"); // no more views
            // update the list of referenced columns in the table
            op.Table.ReferencedColumns.And(m_referencedVars);
            m_command.RecomputeNodeInfo(n);
            return n;
        }

        /// <summary>
        /// SetOps
        /// 
        /// Common handling for all SetOps. We first identify the "output" vars 
        /// that are referenced, and mark the corresponding "input" vars as referenced
        /// We then remove all unreferenced output Vars from the "Outputs" varset
        /// as well as from the Varmaps.
        /// Finally, we visit the children
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n">current node</param>
        /// <returns></returns>
        protected override Node VisitSetOp(SetOp op, Node n)
        {
            // Prune the outputs varset, except for Intersect and Except, which require 
            // all their outputs to compare, so don't bother pruning them.
            if (OpType.Intersect == op.OpType || OpType.Except == op.OpType)
            {
                AddReference(op.Outputs);
            }

            PruneVarSet(op.Outputs);

            // Prune the varmaps. Identify which of the setOp vars have been
            // referenced, and eliminate those entries that don't show up. Additionally
            // mark all the other Vars as referenced
            foreach (VarMap varMap in op.VarMap)
            {
                PruneVarMap(varMap);
            }

            // Now visit the children
            VisitChildren(n);
            return n;
        }

        /// <summary>
        /// SortOp 
        /// 
        /// First visit the sort keys - no sort key can be eliminated. 
        /// Then process the vardeflist child (if there is one) that contains computed
        /// vars, and finally process the relop input. As before, the computedvars 
        /// and sortkeys need to be processed before the relop input
        /// </summary>
        /// <param name="op">the sortop</param>
        /// <param name="n">the current subtree</param>
        /// <returns>modified subtree</returns>
        protected override Node VisitSortOp(SortBaseOp op, Node n)
        {
            // first visit the sort keys
            foreach (InternalTrees.SortKey sk in op.Keys)
            {
                AddReference(sk.Var);
            }
            // next walk through all the computed expressions
            if (n.HasChild1)
            {
                n.Child1 = VisitNode(n.Child1);
            }
            // finally process the input
            n.Child0 = VisitNode(n.Child0);

            m_command.RecomputeNodeInfo(n);
            return n;
        }

        /// <summary>
        /// UnnestOp
        /// 
        /// Marks the unnestVar as referenced, and if there
        /// is a child, visits the child.
        /// </summary>
        /// <param name="op">the unnestOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>modified subtree</returns>
        public override Node Visit(UnnestOp op, Node n)
        {
            AddReference(op.Var);
            VisitChildren(n); // visit my vardefop - defining the unnest var(if any)
            return n;
        }

        #endregion

        #region ScalarOps Visitors

        //
        // The only ScalarOps that need special processing are 
        //  * VarRefOp: we mark the corresponding Var as referenced
        //  * ExistsOp: We mark the (only) Var of the child ProjectOp as referenced
        //

        #region ScalarOps with special treatment

        /// <summary>
        /// VarRefOp
        /// 
        /// Mark the corresponding Var as "referenced"
        /// </summary>
        /// <param name="op">the VarRefOp</param>
        /// <param name="n">current node</param>
        /// <returns></returns>
        public override Node Visit(VarRefOp op, Node n)
        {
            AddReference(op.Var);
            return n;
        }

        /// <summary>
        /// ExistsOp
        /// 
        /// The child must be a ProjectOp - with exactly 1 var. Mark it as referenced
        /// </summary>
        /// <param name="op">the ExistsOp</param>
        /// <param name="n">the input node</param>
        /// <returns></returns>
        public override Node Visit(ExistsOp op, Node n)
        {
            // Ensure that the child is a projectOp, and has exactly one var. Mark 
            // that var as referenced always
            ProjectOp projectOp = (ProjectOp)n.Child0.Op;

            //It is enougth to reference the first output, this usually is a simple constant
            AddReference(projectOp.Outputs.First);

            VisitChildren(n);
            return n;
        }
        #endregion

        #endregion

        #endregion
    }
}

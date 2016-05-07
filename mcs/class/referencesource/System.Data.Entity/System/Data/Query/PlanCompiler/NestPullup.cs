//---------------------------------------------------------------------
// <copyright file="NestPullup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
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

using System.Collections.Generic;
using System.Globalization;

using System.Data.Common;
using md = System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;
using System.Data.Query.PlanCompiler;
using System.Linq;

namespace System.Data.Query.PlanCompiler
{

    /// <summary>
    /// This class "pulls" up nest operations to the root of the tree
    /// </summary>
    /// <remarks>
    /// The goal of this module is to eliminate nest operations from the query - more
    /// specifically, the nest operations are pulled up to the root of the query instead.
    ///</remarks>
    internal class NestPullup : BasicOpVisitorOfNode
    {

        #region private state

        private PlanCompiler m_compilerState;

        /// <summary>
        /// map from a collection var to the node where it's defined; the node should be
        /// the node that should be used as the replacement for the var if it is referred
        /// to in an UnnestOp (through a VarRef)  Note that we expect this to contain the
        /// PhysicalProjectOp of the node, so we can use the VarList when mapping vars to
        /// the copy; (We'll remove the PhysicalProjectOp when we copy it...)
        /// </summary>
        private Dictionary<Var, Node> m_definingNodeMap = new Dictionary<Var, Node>();

        /// <summary>
        /// map from var to the var we're supposed to replace it with
        /// </summary>
        private VarRemapper m_varRemapper;

        /// <summary>
        /// Map from VarRef vars to what they're referencing; used to enable the defining
        /// node map to contain only the definitions, not all the references to it.
        /// </summary>
        private Dictionary<Var, Var> m_varRefMap = new Dictionary<Var, Var>();

        /// <summary>
        /// Whether a sort was encountered under an UnnestOp.
        /// If so, sort removal needs to be performed.  
        /// </summary>
        private bool m_foundSortUnderUnnest = false;
        #endregion

        #region constructor

        private NestPullup(PlanCompiler compilerState)
        {
            m_compilerState = compilerState;
            m_varRemapper = new VarRemapper(compilerState.Command);
        }

        #endregion

        #region Process Driver

        internal static void Process(PlanCompiler compilerState)
        {
            NestPullup np = new NestPullup(compilerState);
            np.Process();
        }

        /// <summary>
        /// The driver routine. Does all the hard work of processing
        /// </summary>
        private void Process()
        {
            PlanCompiler.Assert(Command.Root.Op.OpType == OpType.PhysicalProject, "root node is not physicalProject?");
            Command.Root = VisitNode(Command.Root);

            if (m_foundSortUnderUnnest)
            {
                SortRemover.Process(Command);
            }
        }

        #endregion

        #region private methods

        #region VisitorHelpers

        /// <summary>
        /// the iqt we're processing
        /// </summary>
        private Command Command { get { return m_compilerState.Command; } }

        /// <summary>
        /// is the node a NestOp node?
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private static bool IsNestOpNode(Node n)
        {
            PlanCompiler.Assert(n.Op.OpType != OpType.SingleStreamNest, "illegal singleStreamNest?");
            return (n.Op.OpType == OpType.SingleStreamNest || n.Op.OpType == OpType.MultiStreamNest);
        }

        /// <summary>
        /// Not Supported common processing
        ///
        /// For all those cases where we don't intend to support
        /// a nest operation as a child, we have this routine to
        /// do the work.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private Node NestingNotSupported(Op op, Node n)
        {
            // First, visit my children
            VisitChildren(n);
            m_varRemapper.RemapNode(n);

            // Make sure we don't have a child that is a nest op.
            foreach (Node chi in n.Children)
            {
                if (IsNestOpNode(chi))
                {
                    throw EntityUtil.NestingNotSupported(op, chi.Op);
                }
            }
            return n;
        }

        /// <summary>
        /// Follow the VarRef chain to the defining var
        /// </summary>
        /// <param name="refVar"></param>
        /// <returns></returns>
        private Var ResolveVarReference(Var refVar)
        {
            Var x = refVar;
            while (m_varRefMap.TryGetValue(x, out x))
            {
                refVar = x;
            }
            return refVar;
        }

        /// <summary>
        /// Update the replacement Var map with the vars from the pulled-up
        /// operation; the shape is supposed to be identical, so we should not
        /// have more vars on either side, and the order is guaranteed to be
        /// the same.
        /// </summary>
        /// <param name="fromVars"></param>
        /// <param name="toVars"></param>
        private void UpdateReplacementVarMap(IEnumerable<Var> fromVars, IEnumerable<Var> toVars)
        {
            IEnumerator<Var> toVarEnumerator = toVars.GetEnumerator();

            foreach (Var v in fromVars)
            {
                if (!toVarEnumerator.MoveNext())
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 2);
                }
                m_varRemapper.AddMapping(v, toVarEnumerator.Current);
            }

            if (toVarEnumerator.MoveNext())
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 3);
            }
        }

        #region remapping helpers

        /// <summary>
        /// Replace a list of sortkeys *IN-PLACE* with the corresponding "mapped" Vars
        /// </summary>
        /// <param name="sortKeys">sortkeys</param>
        /// <param name="varMap">the mapping info for Vars</param>
        private static void RemapSortKeys(List<InternalTrees.SortKey> sortKeys, Dictionary<Var, Var> varMap)
        {
            if (sortKeys != null)
            {
                foreach (InternalTrees.SortKey sortKey in sortKeys)
                {
                    Var replacementVar;
                    if (varMap.TryGetValue(sortKey.Var, out replacementVar))
                    {
                        sortKey.Var = replacementVar;
                    }
                }
            }
        }

        /// <summary>
        /// Produce a "mapped" sequence of the input Var sequence - based on the supplied
        /// map
        /// </summary>
        /// <param name="vars">input var sequence</param>
        /// <param name="varMap">var->var map</param>
        /// <returns>the mapped var sequence</returns>
        private IEnumerable<Var> RemapVars(IEnumerable<Var> vars, Dictionary<Var, Var> varMap)
        {
            foreach (Var v in vars)
            {
                Var mappedVar;
                if (varMap.TryGetValue(v, out mappedVar))
                {
                    yield return mappedVar;
                }
                else
                {
                    yield return v;
                }
            }
        }

        /// <summary>
        /// Produce a "mapped" varList
        /// </summary>
        /// <param name="varList"></param>
        /// <param name="varMap"></param>
        /// <returns></returns>
        private VarList RemapVarList(VarList varList, Dictionary<Var, Var> varMap)
        {
            VarList newVarList = Command.CreateVarList(RemapVars(varList, varMap));
            return newVarList;
        }

        /// <summary>
        /// Produce a "mapped" varVec
        /// </summary>
        /// <param name="varVec"></param>
        /// <param name="varMap"></param>
        /// <returns></returns>
        private VarVec RemapVarVec(VarVec varVec, Dictionary<Var, Var> varMap)
        {
            VarVec newVarVec = Command.CreateVarVec(RemapVars(varVec, varMap));
            return newVarVec;
        }

        #endregion

        #endregion

        #region AncillaryOp Visitors

        /// <summary>
        /// VarDefOp
        /// 
        /// Essentially, maintains m_varRefMap, adding an entry for each VarDef that has a 
        /// VarRef on it.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(VarDefOp op, Node n)
        {
            VisitChildren(n);

            // perform any "remapping"
            m_varRemapper.RemapNode(n);

            if (n.Child0.Op.OpType == OpType.VarRef)
            {
                m_varRefMap.Add(op.Var, ((VarRefOp)n.Child0.Op).Var);
            }
            return n;
        }

        /// <summary>
        /// VarRefOp
        /// </summary>
        /// <remarks>
        /// When we remove the UnnestOp, we are left with references to it's column vars that
        /// need to be fixed up; we do this by creating a var replacement map when we remove the
        /// UnnestOp and whenever we find a reference to a var in the map, we replace it with a
        /// reference to the replacement var instead;
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(VarRefOp op, Node n)
        {
            // First, visit my children (do I have children?)
            VisitChildren(n);
            // perform any "remapping"
            m_varRemapper.RemapNode(n);
            return n;
        }

        #endregion

        #region ScalarOp Visitors
        /// <summary>
        /// We don't yet support nest pullups over Case
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(CaseOp op, Node n)
        {
            // Make sure we don't have a child that is a nest op.
            foreach (Node chi in n.Children)
            {
                if (chi.Op.OpType == OpType.Collect)
                {
                    throw EntityUtil.NestingNotSupported(op, chi.Op);
                }
                else if (chi.Op.OpType == OpType.VarRef)
                {
                    Var refVar = ((VarRefOp)chi.Op).Var;
                    if (m_definingNodeMap.ContainsKey(refVar))
                    {
                        throw EntityUtil.NestingNotSupported(op, chi.Op);
                    }
                }
            }

            return VisitDefault(n);
        }

        /// <summary>
        /// The input to Exists is always a ProjectOp with a single constant var projected.
        /// If the input to that ProjectOp contains nesting, it may end up with additional outputs after being
        /// processed. If so, we clear out those additional outputs.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ExistsOp op, Node n)
        {
            Var inputVar = ((ProjectOp)n.Child0.Op).Outputs.First;
            VisitChildren(n);

            VarVec newOutputs = ((ProjectOp)n.Child0.Op).Outputs;
            if (newOutputs.Count > 1)
            {
                PlanCompiler.Assert(newOutputs.IsSet(inputVar), "The constant var is not present after NestPull up over the input of ExistsOp.");
                newOutputs.Clear();
                newOutputs.Set(inputVar);
            }
            return n;
        }
        #endregion

        #region RelOp Visitors

        /// <summary>
        /// Default RelOp processing: 
        /// 
        /// We really don't want to allow any NestOps through; just fail if we don't have
        /// something coded.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitRelOpDefault(RelOp op, Node n)
        {
            return NestingNotSupported(op, n);
        }

        /// <summary>
        /// ApplyOp/JoinOp common processing
        /// </summary>
        /// <remarks>
        /// If one of the inputs to any JoinOp/ApplyOp is a NestOp, then the NestOp
        /// can be pulled above the join/apply if every input to the join/apply has
        /// a key(s). The keys of the NestOp are augmented with the keys of the
        /// other join inputs:
        ///
        ///     JoinOp/ApplyOp(NestOp(X, ...), Y) => NestOp(JoinOp/ApplyOp(X, Y), ...)
        ///
        /// In addition, if the NestOp is on a 'nullable' side of a join (i.e. right side of
        /// LeftOuterJoin/OuterApply or either side of FullOuterJoin), the driving node 
        /// of that NestOp (X) is capped with a project with a null sentinel and 
        /// the dependant collection nodes (the rest of the NestOp children)
        /// are filtered based on that sentinel:
        ///
        ///  LOJ/OA/FOJ (X, NestOp(Y, Z1, Z2, ..ZN))  =>  NestOp( LOJ/OA/FOJ (X, PROJECT (Y, v = 1)), FILTER(Z1, v!=null), FILTER(Z2, v!=null), ... FILTER(ZN, v!=null))
        ///         FOJ (NestOp(Y, Z1, Z2, ..ZN), X)  =>  NestOp( LOJ/OA/FOJ (PROJECT (Y, v = 1), X), FILTER(Z1, v!=null), FILTER(Z2, v!=null), ... FILTER(ZN, v!=null))
        ///
        ///  Also, FILTER(Zi, v != null) may be transformed to push the filter below any NestOps. 
        ///  The definitions for collection vars corresponding to the filtered collection nodes (in m_definingNodeMap)
        ///  are also updated to filter based on the sentinel. 
        ///  
        /// Requires: Every input to the join/apply must have a key.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        private Node ApplyOpJoinOp(Op op, Node n)
        {
            // First, visit my children
            VisitChildren(n);

            // Now determine if any of the input nodes are a nestOp.
            int countOfNestInputs = 0;

            foreach (Node chi in n.Children)
            {
                NestBaseOp nestOp = chi.Op as NestBaseOp;
                if (null != nestOp)
                {
                    countOfNestInputs++;

                    if (OpType.SingleStreamNest == chi.Op.OpType)
                    {
                        // There should not be a SingleStreamNest in the tree, because we made a decision
                        // that in essence means the only way to get a SingleStreamNest is to have a
                        // PhysicalProject over something with an underlying NestOp.   Having
                        //
                        //      Project(Collect(PhysicalProject(...)))
                        //
                        // isn’t good enough, because that will get converted to a MultiStreamNest, with
                        // the SingleStreamNest as the input to the MultiStreamNest.
                        throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.JoinOverSingleStreamNest);
                    }
                }
            }

            // If none of the inputs are a nest, then we don't really need to do anything.
            if (0 == countOfNestInputs)
            {
                return n;
            }

            // We can only pull the nest over a Join/Apply if it has keys, so
            // we can order things; if it doesn't have keys, we throw a NotSupported
            // exception.
            foreach (Node chi in n.Children)
            {
                if (op.OpType != OpType.MultiStreamNest && chi.Op.IsRelOp)
                {
                    KeyVec keys = Command.PullupKeys(chi);

                    if (null == keys || keys.NoKeys)
                    {
                        throw EntityUtil.KeysRequiredForJoinOverNest(op);
                    }
                }
            }

            // Alright, we're OK to pull the nestOp over the joinOp/applyOp.
            //
            // That means:
            //
            // (1) build a new list of children for the nestOp and for the joinOp/applyOp
            // (2) build the new list of collectionInfos for the new nestOp.
            List<Node> newNestChildren = new List<Node>();
            List<Node> newJoinApplyChildren = new List<Node>();
            List<CollectionInfo> newCollectionInfoList = new List<CollectionInfo>();

            foreach (Node chi in n.Children)
            {
                if (chi.Op.OpType == OpType.MultiStreamNest)
                {
                    newCollectionInfoList.AddRange(((MultiStreamNestOp)chi.Op).CollectionInfo);

                    // SQLBUDT #615513: If the nest op is on a 'nullable' side of join 
                    // (i.e. right side of LeftOuterJoin/OuterApply or either side of FullOuterJoin)
                    // the driving node of that nest operation needs to be capped with a project with 
                    // a null sentinel and the dependant collection nodes need to be filtered based on that sentinel.
                    //
                    //  LOJ/OA/FOJ (X, MSN(Y, Z1, Z2, ..ZN))  =>  MSN( LOJ/OA/FOJ (X, PROJECT (Y, v = 1)), FILTER(Z1, v!=null), FILTER(Z2, v!=null), ... FILTER(ZN, v!=null))
                    //         FOJ (MSN(Y, Z1, Z2, ..ZN), X)  =>  MSN( LOJ/OA/FOJ (PROJECT (Y, v = 1), X), FILTER(Z1, v!=null), FILTER(Z2, v!=null), ... FILTER(ZN, v!=null))
                    //
                    //  Note: we transform FILTER(Zi, v != null) to push the filter below any MSNs. 
                    if ((op.OpType == OpType.FullOuterJoin)
                        || ((op.OpType == OpType.LeftOuterJoin || op.OpType == OpType.OuterApply) && n.Child1.Op.OpType == OpType.MultiStreamNest))
                    {
                        Var sentinelVar = null;
                        newJoinApplyChildren.Add(AugmentNodeWithConstant(chi.Child0,  () => Command.CreateNullSentinelOp(), out sentinelVar));

                        // Update the definitions corresponding ot the collection vars to be filtered based on the sentinel. 
                        foreach (CollectionInfo collectionInfo in ((MultiStreamNestOp)chi.Op).CollectionInfo)
                        {
                            m_definingNodeMap[collectionInfo.CollectionVar].Child0 = ApplyIsNotNullFilter(m_definingNodeMap[collectionInfo.CollectionVar].Child0, sentinelVar);
                        }
                        
                        for (int i = 1; i < chi.Children.Count; i++)
                        {
                            Node newNestChild = ApplyIsNotNullFilter(chi.Children[i], sentinelVar);
                            newNestChildren.Add(newNestChild);
                        }
                    }
                    else
                    {
                        newJoinApplyChildren.Add(chi.Child0);
                        for (int i = 1; i < chi.Children.Count; i++)
                        {
                            newNestChildren.Add(chi.Children[i]);
                        }
                    }
                }
                else
                {
                    newJoinApplyChildren.Add(chi);
                }
            }

            // (3) create the new Join/Apply node using the existing op and the
            //     new list of children from (1).
            Node newJoinApplyNode = Command.CreateNode(op, newJoinApplyChildren);

            // (4) insert the apply op as the driving node of the nestOp (put it
            //     at the beginning of the new nestOps' children.
            newNestChildren.Insert(0, newJoinApplyNode);

            // (5) build an updated list of output vars based upon the new join/apply
            //     node, and ensure all the collection vars from the nestOp(s) are
            //     included.
            ExtendedNodeInfo xni = newJoinApplyNode.GetExtendedNodeInfo(Command);
            VarVec newOutputVars = Command.CreateVarVec(xni.Definitions);

            foreach (CollectionInfo ci in newCollectionInfoList)
            {
                newOutputVars.Set(ci.CollectionVar);
            }

            // (6) create the new nestop
            NestBaseOp newNestOp = Command.CreateMultiStreamNestOp(new List<InternalTrees.SortKey>(), newOutputVars, newCollectionInfoList);
            Node newNode = Command.CreateNode(newNestOp, newNestChildren);
            return newNode;
        }

        /// <summary>
        /// Applies a IsNotNull(sentinelVar) filter to the given node.
        /// The filter is pushed below all MultiStremNest-s, because this part of the tree has 
        /// already been visited and it is expected that the MultiStreamNests have bubbled up
        /// above the filters. 
        /// </summary>
        /// <param name="node"></param>
        /// <param name="sentinelVar"></param>
        /// <returns></returns>
        private Node ApplyIsNotNullFilter(Node node, Var sentinelVar)
        {
            Node newFilterChild = node;
            Node newFilterParent = null;
            while (newFilterChild.Op.OpType == OpType.MultiStreamNest)
            {
                newFilterParent = newFilterChild;
                newFilterChild = newFilterChild.Child0;
            }

            Node newFilterNode = CapWithIsNotNullFilter(newFilterChild, sentinelVar);
            Node result;

            if (newFilterParent != null)
            {
                newFilterParent.Child0 = newFilterNode;
                result = node;
            }
            else
            {
                result = newFilterNode;
            }
            return result;
        }

        /// <summary>
        /// Input =>  Filter(input, Ref(var) is not null)
        /// </summary>
        /// <param name="input"></param>
        /// <param name="var"></param>
        /// <returns></returns>
        private Node CapWithIsNotNullFilter(Node input, Var var)
        {
            Node varRefNode = Command.CreateNode(Command.CreateVarRefOp(var));
            Node predicateNode = Command.CreateNode(
                                            Command.CreateConditionalOp(OpType.Not),
                                            Command.CreateNode(
                                                    Command.CreateConditionalOp(OpType.IsNull),
                                                    varRefNode));


            Node filterNode = Command.CreateNode(Command.CreateFilterOp(), input, predicateNode);
            return filterNode;
        }

        /// <summary>
        /// ApplyOp common processing
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitApplyOp(ApplyBaseOp op, Node n)
        {
            return ApplyOpJoinOp(op, n);
        }

        /// <summary>
        /// DistinctOp
        /// </summary>
        /// <remarks>
        /// The input to a DistinctOp cannot be a NestOp – that would imply that
        /// we support distinctness over collections - which we don’t.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(DistinctOp op, Node n)
        {
            return NestingNotSupported(op, n);
        }

        /// <summary>
        /// FilterOp
        /// </summary>
        /// <remarks>
        /// If the input to the FilterOp is a NestOp, and if the filter predicate
        /// does not reference any of the collection Vars of the nestOp, then the
        /// FilterOp can be simply pushed below the NestOp:
        ///
        ///     Filter(Nest(X, ...), pred) => Nest(Filter(X, pred), ...)
        ///
        /// Note: even if the filter predicate originally referenced one of the
        /// collection vars, as part of our bottom up traversal, the appropriate
        /// Var was replaced by a copy of the source of the collection. So, this
        /// transformation should always be legal.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(FilterOp op, Node n)
        {
            // First, visit my children
            VisitChildren(n);

            // see if the child is a nestOp
            NestBaseOp nestOp = n.Child0.Op as NestBaseOp;

            if (null != nestOp)
            {
#if DEBUG
                // check to see if the predicate references any of the collection
                // expressions. If it doesn't, then we can push the filter down, but
                // even if it does it's probably OK.
                NodeInfo predicateNodeInfo = Command.GetNodeInfo(n.Child1);
                foreach (CollectionInfo ci in nestOp.CollectionInfo)
                {
                    PlanCompiler.Assert(!predicateNodeInfo.ExternalReferences.IsSet(ci.CollectionVar), "predicate references collection?");
                }
#endif //DEBUG

                // simply pull up the nest child above ourself.
                Node nestOpNode = n.Child0;
                Node nestOpInputNode = nestOpNode.Child0;
                n.Child0 = nestOpInputNode;
                nestOpNode.Child0 = n;

                // recompute node info - no need to perform anything for the predicate
                Command.RecomputeNodeInfo(n);
                Command.RecomputeNodeInfo(nestOpNode);
                return nestOpNode;
            }

            return n;
        }

        /// <summary>
        /// GroupByOp
        /// </summary>
        /// <remarks>
        /// At this point in the process, there really isn't a way we should actually
        /// have a NestOp as an input to the GroupByOp, and we currently aren't allowing
        /// you to specify a collection as an aggregation Var or key, so if we find a
        /// NestOp anywhere on the inputs, it's a NotSupported situation.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(GroupByOp op, Node n)
        {
            return NestingNotSupported(op, n);
        }

        /// <summary>
        /// GroupByIntoOp
        /// </summary>
        /// <remarks>
        /// Transform the GroupByInto node into a Project over a GroupBy. The project 
        /// outputs all keys and aggregates produced by the GroupBy and has the definition of the 
        /// group aggregates var in its var def list.
        /// 
        /// GroupByInto({key1, key2, ... , keyn}, {fa1, fa1, ... , fan}, {ga1, ga2, ..., gn}) => 
        ///     Project(GroupBy({key1, key2, ... , keyn}, {fa1, fa1, ... , fan}),   // input
        ///             {ga1, ga2, ..., gn}                                         // vardeflist
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(GroupByIntoOp op, Node n)
        {
            PlanCompiler.Assert(n.HasChild3 && n.Child3.Children.Count > 0, "GroupByIntoOp with no group aggregates?");
            Node varDefListNode = n.Child3;

            VarVec projectOpOutputs = Command.CreateVarVec(op.Outputs);
            VarVec groupByOutputs = op.Outputs;

            // Local definitions
            foreach (Node chi in varDefListNode.Children)
            {
                VarDefOp varDefOp = chi.Op as VarDefOp;
                groupByOutputs.Clear(varDefOp.Var);
            }

            //Create the new groupByOp
            Node groupByNode = Command.CreateNode(
                Command.CreateGroupByOp(op.Keys, groupByOutputs), n.Child0, n.Child1, n.Child2);

            Node projectNode = Command.CreateNode(
                                Command.CreateProjectOp(projectOpOutputs),
                                groupByNode, varDefListNode);

            return VisitNode(projectNode);
        }

        /// <summary>
        /// JoinOp common processing
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitJoinOp(JoinBaseOp op, Node n)
        {
            return ApplyOpJoinOp(op, n);
        }

        /// <summary>
        /// ProjectOp
        /// </summary>
        /// <remarks>
        /// If after visiting the children, the ProjectOp's input is a SortOp, swap the ProjectOp and the SortOp, 
        /// to allow the SortOp to bubble up and be honored. This may only occur if the original input to the  
        /// ProjectOp was an UnnestOp. 
        /// 
        /// There are three cases to handle in ProjectOp:
        ///
        ///  (1) The input is not a NestOp; but the ProjectOp locally defines some Vars
        ///      as collections:
        ///
        ///         ProjectOp(X,{a,CollectOp(PhysicalProjectOp(Y)),b,...}) ==> MsnOp(ProjectOp'(X,{a,b,...}),Y)
        ///         ProjectOp(X,{a,VarRef(ref-to-collect-var-Y),b,...})    ==> MsnOp(ProjectOp'(X,{a,b,...}),copy-of-Y)
        ///
        ///     Where:
        ///         ProjectOp' is ProjectOp less any vars that were collection vars, plus
        ///                    any additional Vars needed by the collection.
        ///
        ///  (2) The input is a NestOp, but the ProjectOp does not local define some Vars
        ///      as collections:
        ///
        ///         ProjectOp(MsnOp(X,Y,...)) => MsnOp'(ProjectOp'(X),Y,...)
        ///
        ///      Where:
        ///         ProjectOp' is ProjectOp plus any additional Vars needed by NestOp
        ///                    (see NestOp.Outputs – except the collection vars)
        ///
        ///         MsnOp'     should be MsnOp. Additionally, its Outputs should be enhanced
        ///                    to include any Vars produced by the ProjectOp
        ///
        ///  (3) The combination of both (1) and (2) -- both the vars define a collection,
        ///      and the input is also a nestOp.  we handle this by first processing Case1,
        ///      then processing Case2.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ProjectOp op, Node n)
        {
#if DEBUG
            string input = Dump.ToXml(Command, n);
#endif //DEBUG

            // First, visit my children
            VisitChildren(n);
            m_varRemapper.RemapNode(n);

            Node newNode;

            // If the ProjectOp's input is a SortOp, swap the ProjectOp and the SortOp, 
            // to allow the SortOp to buble up and be honored. This may only occur if the original input to the  
            // ProjectOp was an UnnestOp (or a Project over a Unnest Op). 
            if (n.Child0.Op.OpType == OpType.Sort)
            {
                Node sortNode = n.Child0;
                foreach (System.Data.Query.InternalTrees.SortKey key in ((SortOp)sortNode.Op).Keys)
                {
                    if (!Command.GetExtendedNodeInfo(sortNode).ExternalReferences.IsSet(key.Var))
                    {
                        op.Outputs.Set(key.Var);
                    }
                }
                n.Child0 = sortNode.Child0;
                this.Command.RecomputeNodeInfo(n);
                sortNode.Child0 = HandleProjectNode(n);
                this.Command.RecomputeNodeInfo(sortNode);

                newNode = sortNode;
            }
            else
            {
                newNode = HandleProjectNode(n);
            }

#if DEBUG
            int size = input.Length;// GC.KeepAlive makes FxCop Grumpy.
            string output = Dump.ToXml(Command, newNode);
#endif //DEBUG
            return newNode;
        }

        /// <summary>
        /// Helper method for <see cref="Visit(ProjectOp, Node)"/>.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private Node HandleProjectNode(Node n)
        {
            // First, convert any nestOp inputs;
            Node newNode = ProjectOpCase1(n);

            // Then, if we have a NestOp as an input (and we didn't
            // produce a NestOp when handling Case1) pull it over our
            // ProjectOp.
            if (newNode.Op.OpType == OpType.Project && IsNestOpNode(newNode.Child0))
            {
                newNode = ProjectOpCase2(newNode);
            }

            // Finally we fold any nested NestOps into one.
            newNode = MergeNestedNestOps(newNode);

            return newNode;
        }

        /// <summary>
        /// Fold nested MultiStreamNestOps into one:
        /// 
        ///     MSN(MSN(X,Y),Z) ==> MSN(X,Y,Z)
        /// 
        /// NOTE: It would be incorrect to merge NestOps from the non-driving node
        ///       into one nest op, because that would change the intent.  Instead, 
        ///       we let those go through the tree and wait until we get to the top
        ///       level PhysicalProject, when we'll use the ConvertToSingleStreamNest
        ///       process to handle them.
        /// 
        /// NOTE: We should never have three levels of nestOps, because we should
        ///       have folded the lower two together when we constructed one of them.
        /// 
        /// We also remove unreferenced collections, that is, if any collection is
        /// not referred to by the top level-NestOp, we can safely remove it from 
        /// the merged NestOp we produce.
        /// </summary>
        /// <param name="nestNode"></param>
        /// <returns></returns>
        private Node MergeNestedNestOps(Node nestNode)
        {

            // First, determine if there is anything we can actually do.  If we 
            // aren't given a NestOp or if it's driving node isn't a NestOp we 
            // can just ignore this.
            if (!IsNestOpNode(nestNode) || !IsNestOpNode(nestNode.Child0))
            {
                return nestNode;
            }

#if DEBUG
            string input = Dump.ToXml(Command, nestNode);
#endif //DEBUG
            NestBaseOp nestOp = (NestBaseOp)nestNode.Op;
            Node nestedNestNode = nestNode.Child0;
            NestBaseOp nestedNestOp = (NestBaseOp)nestedNestNode.Op;

            // Get the collection Vars from the top level NestOp
            VarVec nestOpCollectionOutputs = Command.CreateVarVec();
            foreach (CollectionInfo ci in nestOp.CollectionInfo)
            {
                nestOpCollectionOutputs.Set(ci.CollectionVar);
            }

            // Now construct a new list of inputs, collections; and output vars.
            List<Node> newNestInputs = new List<Node>();
            List<CollectionInfo> newCollectionInfo = new List<CollectionInfo>();
            VarVec newOutputVars = Command.CreateVarVec(nestOp.Outputs);

            // Add the new DrivingNode;
            newNestInputs.Add(nestedNestNode.Child0);

            // Now add each of the nested nodes collections, but only when they're 
            // referenced by the top level nestOp's outputs.
            for (int i = 1; i < nestedNestNode.Children.Count; i++)
            {
                CollectionInfo ci = nestedNestOp.CollectionInfo[i - 1];
                if (nestOpCollectionOutputs.IsSet(ci.CollectionVar) || newOutputVars.IsSet(ci.CollectionVar))
                {
                    newCollectionInfo.Add(ci);
                    newNestInputs.Add(nestedNestNode.Children[i]);
                    PlanCompiler.Assert(newOutputVars.IsSet(ci.CollectionVar), "collectionVar not in output Vars?"); // I must have missed something...
                }
            }

            // Then add in the rest of the inputs to the top level nest node (and
            // they're collection Infos)
            for (int i = 1; i < nestNode.Children.Count; i++)
            {
                CollectionInfo ci = nestOp.CollectionInfo[i - 1];
                newCollectionInfo.Add(ci);
                newNestInputs.Add(nestNode.Children[i]);
                PlanCompiler.Assert(newOutputVars.IsSet(ci.CollectionVar), "collectionVar not in output Vars?"); // I must have missed something...
            }

            //The prefix sort keys for the new nest op should include these of the input nestOp followed by the nestedNestOp
            //(The nestOp-s that are being merged may have prefix sort keys propagated to them by constrainedSortOp-s pushed below them.
            List<InternalTrees.SortKey> sortKeys = ConsolidateSortKeys(nestOp.PrefixSortKeys, nestedNestOp.PrefixSortKeys);

            // Make sure we pullup the sort keys in our output too...
            foreach (InternalTrees.SortKey sk in sortKeys)
            {
                newOutputVars.Set(sk.Var);
            }

            // Ready to go; build the new NestNode, etc.
            MultiStreamNestOp newNestOp = Command.CreateMultiStreamNestOp(sortKeys, newOutputVars, newCollectionInfo);
            Node newNode = Command.CreateNode(newNestOp, newNestInputs);

            // Finally, recompute node info
            Command.RecomputeNodeInfo(newNode);

#if DEBUG
            int size = input.Length;// GC.KeepAlive makes FxCop Grumpy.
            string output = Dump.ToXml(Command, newNode);
#endif //DEBUG
            return newNode;
        }

        /// <summary>
        /// ProjectOp(X,{a,CollectOp(PhysicalProjectOp(Y)),b,...}) ==> MsnOp(ProjectOp'(X,{a,b,...}),Y)
        /// ProjectOp(X,{a,VarRef(ref-to-collect-var-Y),b,...})    ==> MsnOp(ProjectOp'(X,{a,b,...}),copy-of-Y)
        ///
        /// Remove CollectOps from projection, constructing a NestOp
        /// over the ProjectOp.
        /// </summary>
        /// <param name="projectNode"></param>
        /// <returns></returns>
        private Node ProjectOpCase1(Node projectNode)
        {
#if DEBUG
            string input = Dump.ToXml(Command, projectNode);
#endif //DEBUG

            ProjectOp op = (ProjectOp)projectNode.Op;

            // Check to see if any of the computed Vars are in fact NestOps, and
            // construct a collection column map for them.
            List<CollectionInfo> collectionInfoList = new List<CollectionInfo>();
            List<Node> newChildren = new List<Node>();
            List<Node> collectionNodes = new List<Node>();
            VarVec externalReferences = Command.CreateVarVec();
            VarVec collectionReferences = Command.CreateVarVec();
            List<Node> definedVars = new List<Node>();
            List<Node> referencedVars = new List<Node>();

            foreach (Node chi in projectNode.Child1.Children)
            {
                VarDefOp varDefOp = (VarDefOp)chi.Op;
                Node definingExprNode = chi.Child0;

                if (OpType.Collect == definingExprNode.Op.OpType)
                {
                    PlanCompiler.Assert(definingExprNode.HasChild0, "collect without input?");
                    PlanCompiler.Assert(OpType.PhysicalProject == definingExprNode.Child0.Op.OpType, "collect without physicalProject?");
                    Node physicalProjectNode = definingExprNode.Child0;

                    // Update collection var->defining node map;
                    m_definingNodeMap.Add(varDefOp.Var, physicalProjectNode);

                    ConvertToNestOpInput(physicalProjectNode, varDefOp.Var, collectionInfoList, collectionNodes, externalReferences, collectionReferences);
                }
                else if (OpType.VarRef == definingExprNode.Op.OpType)
                {
                    Var refVar = ((VarRefOp)definingExprNode.Op).Var;
                    Node physicalProjectNode;

                    if (m_definingNodeMap.TryGetValue(refVar, out physicalProjectNode))
                    {
                        physicalProjectNode = CopyCollectionVarDefinition(physicalProjectNode);
                        //SQLBUDT #602888: We need to track the copy too, in case we need to reuse it
                        m_definingNodeMap.Add(varDefOp.Var, physicalProjectNode);
                        ConvertToNestOpInput(physicalProjectNode, varDefOp.Var, collectionInfoList, collectionNodes, externalReferences, collectionReferences);
                    }
                    else
                    {
                        referencedVars.Add(chi);
                        newChildren.Add(chi);
                    }
                }
                else
                {
                    definedVars.Add(chi);
                    newChildren.Add(chi);
                }
            }

            // If we haven't identified a set of collection nodes, then we're done.
            if (0 == collectionNodes.Count)
            {
                return projectNode;
            }

            // OK, we found something. We have some heavy lifting to perform.

            // Then we need to build up a MultiStreamNestOp above the ProjectOp and the
            // new collection nodes to get what we really need.
            // pretend that the keys included everything from the new projectOp
            VarVec outputVars = Command.CreateVarVec(op.Outputs);

            // First we need to modify this physicalProjectNode to leave out the collection
            // Vars that we've just seen.
            VarVec newProjectVars = Command.CreateVarVec(op.Outputs);
            newProjectVars.Minus(collectionReferences);

            // If there are any external references from any of the collections, add
            // those to the projectOp explicitly. This must be ok because the projectOp
            // could not have had any left-correlation
            newProjectVars.Or(externalReferences);

            // Create the new projectOp, and hook it into this one.  The new projectOp
            // no longer references the collections in it's children; of course we only
            // construct a new projectOp if it actually projects out some Vars.  
            if (!newProjectVars.IsEmpty)
            {
                if (IsNestOpNode(projectNode.Child0))
                {
                    // If the input is a nest node, we need to figure out what to do with the
                    // rest of the in the VarDefList; we can't just pitch them, but we also 
                    // really want to have the input be a nestop.
                    //
                    // What we do is essentially push any non-collection VarDef’s down under 
                    // the driving node of the MSN:
                    //
                    //      Project[Z,Y,W](Msn(X,Y),VarDef(Z=blah),VarDef(W=Collect(etc)) ==> MSN(MSN(Project[Z](X,VarDef(Z=blah)),Y),W)
                    //
                    // An optimization, of course being to not push anything down when there
                    // aren't any extra vars defined.

                    if (definedVars.Count == 0 && referencedVars.Count == 0)
                    {
                        // We'll just pick the NestNode; we expect MergeNestedNestOps to merge
                        // it into what we're about to generate later.
                        projectNode = projectNode.Child0;
                        EnsureReferencedVarsAreRemoved(referencedVars, outputVars);
                    }
                    else
                    {
                        NestBaseOp nestedNestOp = (NestBaseOp)projectNode.Child0.Op;

                        // Build the new ProjectOp to be used as input to the new nestedNestOp; 
                        // it's input is the input to the current nestedNestOp and a new 
                        // VarDefList with only the vars that were defined on the top level 
                        // ProjectOp.
                        List<Node> newNestedProjectNodeInputs = new List<Node>();
                        newNestedProjectNodeInputs.Add(projectNode.Child0.Child0);
                        referencedVars.AddRange(definedVars);
                        newNestedProjectNodeInputs.Add(Command.CreateNode(Command.CreateVarDefListOp(), referencedVars));

                        VarVec newNestedProjectOutputs = Command.CreateVarVec(nestedNestOp.Outputs);

                        // SQLBUDT #508722:  We need to remove the collection vars, 
                        //  these are not produced by the project
                        foreach (CollectionInfo ci in nestedNestOp.CollectionInfo)
                        {
                            newNestedProjectOutputs.Clear(ci.CollectionVar);
                        }

                        foreach (Node varDefNode in referencedVars)
                        {
                            newNestedProjectOutputs.Set(((VarDefOp)varDefNode.Op).Var);
                        }

                        Node newNestedProjectNode = Command.CreateNode(Command.CreateProjectOp(newNestedProjectOutputs), newNestedProjectNodeInputs);

                        // Now build the new nestedNestedNestOp, with the new nestedProjectOp
                        // as it's input; we have to update the outputs of the NestOp to include
                        // the vars we pushed down.
                        VarVec newNestedNestOutputs = Command.CreateVarVec(newNestedProjectOutputs);
                        newNestedNestOutputs.Or(nestedNestOp.Outputs);

                        MultiStreamNestOp newNestedNestOp = Command.CreateMultiStreamNestOp(
                                                                    nestedNestOp.PrefixSortKeys,
                                                                    newNestedNestOutputs,
                                                                    nestedNestOp.CollectionInfo);

                        List<Node> newNestedNestNodeInputs = new List<Node>();
                        newNestedNestNodeInputs.Add(newNestedProjectNode);
                        for (int j = 1; j < projectNode.Child0.Children.Count; j++)
                        {
                            newNestedNestNodeInputs.Add(projectNode.Child0.Children[j]);
                        }
                        projectNode = Command.CreateNode(newNestedNestOp, newNestedNestNodeInputs);
                        // We don't need to remove or remap referenced vars here because
                        // we're including them on the node we create; they won't become
                        // invalid.
                    }
                }
                else
                {
                    ProjectOp newProjectOp = Command.CreateProjectOp(newProjectVars);
                    projectNode.Child1 = Command.CreateNode(projectNode.Child1.Op, newChildren);
                    projectNode.Op = newProjectOp;
                    EnsureReferencedVarsAreRemapped(referencedVars);
                }
            }
            else
            {
                projectNode = projectNode.Child0;
                EnsureReferencedVarsAreRemoved(referencedVars, outputVars);
            }

            // We need to make sure that we project out any external references to the driving
            // node that the nested collections have, or we're going to end up with unresolvable
            // vars when we pull them up over the current driving node.  Of course, we only 
            // want the references that are actually ON the driving node.
            externalReferences.And(projectNode.GetExtendedNodeInfo(Command).Definitions);
            outputVars.Or(externalReferences);

            // There are currently no prefix sortkeys. The processing for a SortOp may later
            // introduce some prefix sortkeys, but there aren't any now.
            MultiStreamNestOp nestOp = Command.CreateMultiStreamNestOp(new List<InternalTrees.SortKey>(), outputVars, collectionInfoList);

            // Insert the current node at the head of the the list of collections
            collectionNodes.Insert(0, projectNode);
            Node nestNode = Command.CreateNode(nestOp, collectionNodes);

            // Finally, recompute node info
            Command.RecomputeNodeInfo(projectNode);
            Command.RecomputeNodeInfo(nestNode);

#if DEBUG
            int size = input.Length;// GC.KeepAlive makes FxCop Grumpy.
            string output = Dump.ToXml(Command, nestNode);
#endif //DEBUG
            return nestNode;
        }

        /// <summary>
        /// If we're going to eat the ProjectNode, then we at least need to make 
        /// sure we remap any vars it defines as varRefs, and ensure that any
        /// references to them are switched.
        /// </summary>
        /// <param name="referencedVars"></param>
        /// <param name="outputVars"></param>
        private void EnsureReferencedVarsAreRemoved(List<Node> referencedVars, VarVec outputVars)
        {
            foreach (Node chi in referencedVars)
            {
                VarDefOp varDefOp = (VarDefOp)chi.Op;
                Var defVar = varDefOp.Var;
                Var refVar = ResolveVarReference(defVar);
                m_varRemapper.AddMapping(defVar, refVar);
                outputVars.Clear(defVar);
                outputVars.Set(refVar);
            }
        }

        /// <summary>
        /// We need to make sure that we remap the column maps that we're pulling
        /// up to point to the defined var, not it's reference. 
        /// </summary>
        /// <param name="referencedVars"></param>
        private void EnsureReferencedVarsAreRemapped(List<Node> referencedVars)
        {
            foreach (Node chi in referencedVars)
            {
                VarDefOp varDefOp = (VarDefOp)chi.Op;
                Var defVar = varDefOp.Var;
                Var refVar = ResolveVarReference(defVar);
                m_varRemapper.AddMapping(refVar, defVar);
            }
        }

        /// <summary>
        /// Convert a CollectOp subtree (when used as the defining expression for a
        /// VarDefOp) into a reasonable input to a NestOp.
        /// </summary>
        /// <remarks>
        /// There are a couple of cases that we handle here:
        ///
        ///     (a) PhysicalProject(X) ==> X
        ///     (b) PhysicalProject(Sort(X)) ==> Sort(X)
        ///
        /// </remarks>
        /// <param name="physicalProjectNode">the child of the CollectOp</param>
        /// <param name="collectionVar">the collectionVar being defined</param>
        /// <param name="collectionInfoList">where to append the new collectionInfo</param>
        /// <param name="collectionNodes">where to append the collectionNode</param>
        /// <param name="externalReferences">a bit vector of external references of the physicalProject</param>
        /// <param name="collectionReferences">a bit vector of collection vars</param>
        private void ConvertToNestOpInput(Node physicalProjectNode, Var collectionVar, List<CollectionInfo> collectionInfoList, List<Node> collectionNodes, VarVec externalReferences, VarVec collectionReferences)
        {
            // Keep track of any external references the physicalProjectOp has
            externalReferences.Or(Command.GetNodeInfo(physicalProjectNode).ExternalReferences);

            // Case: (a) PhysicalProject(X) ==> X
            Node nestOpInput = physicalProjectNode.Child0;

            // Now build the collectionInfo for this input, including the flattened
            // list of vars, which is essentially the outputs from the physicalProject
            // with the sortKey vars that aren't already in the outputs we already 
            // have.
            PhysicalProjectOp physicalProjectOp = (PhysicalProjectOp)physicalProjectNode.Op;
            VarList flattenedElementVarList = Command.CreateVarList(physicalProjectOp.Outputs);
            VarVec flattenedElementVarVec = Command.CreateVarVec(flattenedElementVarList); // Use a VarVec to make the lookups faster
            List<InternalTrees.SortKey> sortKeys = null;

            if (OpType.Sort == nestOpInput.Op.OpType)
            {
                // Case: (b) PhysicalProject(Sort(X)) ==> Sort(X)
                SortOp sortOp = (SortOp)nestOpInput.Op;
                sortKeys = OpCopier.Copy(Command, sortOp.Keys);

                foreach (InternalTrees.SortKey sk in sortKeys)
                {
                    if (!flattenedElementVarVec.IsSet(sk.Var))
                    {
                        flattenedElementVarList.Add(sk.Var);
                        flattenedElementVarVec.Set(sk.Var);
                    }
                }
            }
            else
            {
                sortKeys = new List<InternalTrees.SortKey>();
            }

            // Get the keys for the collection
            VarVec keyVars = Command.GetExtendedNodeInfo(nestOpInput).Keys.KeyVars;

            //Check whether all key are projected
            VarVec keyVarsClone = keyVars.Clone();
            keyVarsClone.Minus(flattenedElementVarVec);

            VarVec keys = (keyVarsClone.IsEmpty) ?  keyVars.Clone() : Command.CreateVarVec();    

            // Create the collectionInfo
            CollectionInfo collectionInfo = Command.CreateCollectionInfo(collectionVar, physicalProjectOp.ColumnMap.Element, flattenedElementVarList, keys, sortKeys, null/*discriminatorValue*/);

            // Now update the collections we're tracking.
            collectionInfoList.Add(collectionInfo);
            collectionNodes.Add(nestOpInput);
            collectionReferences.Set(collectionVar);
        }

        /// <summary>
        /// Case 2 for ProjectOp: NestOp is the input:
        ///
        ///     ProjectOp(NestOp(X,Y,...)) => NestOp'(ProjectOp'(X),Y,...)
        ///
        /// Remove collection references from the ProjectOp and pull the
        /// NestOp over it, adding any outputs that the projectOp added.
        ///
        /// The outputs are important here; expanding the above:
        ///
        ///     P{a,n}(N{x1,x2,x3,y}(X,Y)) => N{a,x1,x2,x3,y}(P{a,x1,x2,x3}(X),Y)
        ///
        /// Strategy:
        ///
        ///     (1) Determine oldNestOpCollectionOutputs
        ///     (2) oldNestOpNonCollectionOutputs = oldNestOpOutputs - oldNestOpCollectionOutputs;
        ///     (3) oldProjectOpNonCollectionOutputs = oldProjectOpOutputs - oldNestOpCollectionOutputs
        ///     (4) oldProjectOpCollectionOutputs = oldProjectOpOutputs - oldProjectOpNonCollectionOutputs
        ///     (5) build a new list of collectionInfo's for the new NestOp, including
        ///         only oldProjectOpCollectionOutputs.
        ///     (6) leftCorrelationVars = vars that are defined by the left most child of the input nestOpNode 
        ///         and used in the subtrees rooted at the other children of the input nestOpNode
        ///     (7) newProjectOpOutputs = oldProjectOpNonCollectionOutputs + oldNestOpNonCollectionOutputs + leftCorrelationVars
        ///         
        ///     (8) newProjectOpChildren = ....
        /// 
        /// Of course everything needs to be "derefed", that is, expressed in the projectOp Var Ids.
        ///
        ///     (9) Set ProjectOp's input to NestOp's input
        ///     (10) Set NestOp's input to ProjectOp.
        /// </summary>
        /// <param name="projectNode"></param>
        /// <returns></returns>
        private Node ProjectOpCase2(Node projectNode)
        {
#if DEBUG
            string input = Dump.ToXml(Command, projectNode);
#endif //DEBUG
            ProjectOp projectOp = (ProjectOp)projectNode.Op;
            Node nestNode = projectNode.Child0;
            NestBaseOp nestOp = nestNode.Op as NestBaseOp;
#if DEBUG
            // NOTE: I do not believe that we need to remap the nest op in terms of
            //       the project op, but I can't prove it right now; if the assert
            //       below fires, I was wrong.
            //Dictionary<Var, Var> projectToNestVarMap = new Dictionary<Var, Var>();

            Command.RecomputeNodeInfo(projectNode);
            ExtendedNodeInfo projectNodeInfo = Command.GetExtendedNodeInfo(projectNode);
            ExtendedNodeInfo nestNodeInfo = Command.GetExtendedNodeInfo(nestNode);

            foreach (Node chi in projectNode.Child1.Children)
            {
                VarDefOp varDefOp = (VarDefOp)chi.Op;
                Node definingExprNode = chi.Child0;

                if (OpType.VarRef == definingExprNode.Op.OpType)
                {
                    VarRefOp varRefOp = (VarRefOp)definingExprNode.Op;
                    PlanCompiler.Assert(varRefOp.Var == varDefOp.Var || !projectNodeInfo.LocalDefinitions.IsSet(varRefOp.Var), "need to remap vars!");

                    //if (!projectToNestVarMap.ContainsKey(varRefOp.Var)) {
                    //    projectToNestVarMap.Add(varRefOp.Var, varDefOp.Var);
                    //}
                }
            }
#endif //DEBUG

            // (1) Determine oldNestOpCollectionOutputs
            VarVec oldNestOpCollectionOutputs = Command.CreateVarVec();
            foreach (CollectionInfo ci in nestOp.CollectionInfo)
            {
                oldNestOpCollectionOutputs.Set(ci.CollectionVar);
            }

            // (2) oldNestOpNonCollectionOutputs = oldNestOpOutputs - oldNestOpCollectionOutputs;
            VarVec oldNestOpNonCollectionOutputs = Command.CreateVarVec(nestOp.Outputs);
            oldNestOpNonCollectionOutputs.Minus(oldNestOpCollectionOutputs);

            // (3) oldProjectOpNonCollectionOutputs = oldProjectOpOutputs - oldNestOpCollectionOutputs
            VarVec oldProjectOpNonCollectionOutputs = Command.CreateVarVec(projectOp.Outputs);
            oldProjectOpNonCollectionOutputs.Minus(oldNestOpCollectionOutputs);

            // (4) oldProjectOpCollectionOutputs = oldProjectOpOutputs - oldProjectOpNonCollectionOutputs
            VarVec oldProjectOpCollectionOutputs = Command.CreateVarVec(projectOp.Outputs);
            oldProjectOpCollectionOutputs.Minus(oldProjectOpNonCollectionOutputs);

            // (5) build a new list of collectionInfo's for the new NestOp, including
            //     only oldProjectOpCollectionOutputs.
            VarVec collectionsToRemove = Command.CreateVarVec(oldNestOpCollectionOutputs);
            collectionsToRemove.Minus(oldProjectOpCollectionOutputs);
            List<CollectionInfo> newCollectionInfoList;
            List<Node> newNestNodeChildren;

            if (collectionsToRemove.IsEmpty)
            {
                newCollectionInfoList = nestOp.CollectionInfo;
                newNestNodeChildren = new List<Node>(nestNode.Children);
            }
            else
            {
                newCollectionInfoList = new List<CollectionInfo>();
                newNestNodeChildren = new List<Node>();
                newNestNodeChildren.Add(nestNode.Child0);
                int i = 1;
                foreach (CollectionInfo ci in nestOp.CollectionInfo)
                {
                    if (!collectionsToRemove.IsSet(ci.CollectionVar))
                    {
                        newCollectionInfoList.Add(ci);
                        newNestNodeChildren.Add(nestNode.Children[i]);
                    }
                    i++;
                }
            }

            // (6) leftCorrelationVars = vars that are defined by the left most child of the input nestOpNode 
            //   and used in the subtrees rooted at the other children of the input nestOpNode
            //   #479547:  These need to be added to the outputs of the project 
            VarVec leftCorrelationVars = Command.CreateVarVec();
            for (int i = 1; i < nestNode.Children.Count; i++)
            {
                leftCorrelationVars.Or(nestNode.Children[i].GetExtendedNodeInfo(Command).ExternalReferences);
            }
            leftCorrelationVars.And(nestNode.Child0.GetExtendedNodeInfo(this.Command).Definitions);

            // (7) newProjectOpOutputs = oldProjectOpNonCollectionOutputs + oldNestOpNonCollectionOutputs + leftCorrelationVars
            VarVec newProjectOpOutputs = Command.CreateVarVec(oldProjectOpNonCollectionOutputs);
            newProjectOpOutputs.Or(oldNestOpNonCollectionOutputs);
            newProjectOpOutputs.Or(leftCorrelationVars);

            // (8) newProjectOpChildren = ....
            List<Node> newProjectOpChildren = new List<Node>(projectNode.Child1.Children.Count);
            foreach (Node chi in projectNode.Child1.Children)
            {
                VarDefOp varDefOp = (VarDefOp)chi.Op;

                if (newProjectOpOutputs.IsSet(varDefOp.Var))
                {
                    newProjectOpChildren.Add(chi);
                }
            }

            // (9) and (10), do the switch.
            if (0 != newCollectionInfoList.Count)
            {
                // In some cases, the only var in the projection is the collection var; so
                // the new projectOp will have an empty projection list; we can't just pullup
                // the input, so we add a temporary constant op to it, ensuring that we don't
                // have an empty projection list.
                if (newProjectOpOutputs.IsEmpty)
                {
                    PlanCompiler.Assert(newProjectOpChildren.Count == 0, "outputs is empty with non-zero count of children?");

                    NullOp tempOp = Command.CreateNullOp(Command.StringType);
                    Node tempNode = Command.CreateNode(tempOp);
                    Var tempVar;
                    Node varDefNode = Command.CreateVarDefNode(tempNode, out tempVar);
                    newProjectOpChildren.Add(varDefNode);
                    newProjectOpOutputs.Set(tempVar);
                }
            }

            // Update the projectOp node with the new list of vars and
            // the new list of children.
            projectNode.Op = Command.CreateProjectOp(Command.CreateVarVec(newProjectOpOutputs));
            projectNode.Child1 = Command.CreateNode(projectNode.Child1.Op, newProjectOpChildren);

            if (0 == newCollectionInfoList.Count)
            {
                // There are no remaining nested collections (because none of them
                // were actually referenced)  We just pullup the driving node of the
                // nest and eliminate the nestOp entirely.
                projectNode.Child0 = nestNode.Child0;
                nestNode = projectNode;
            }
            else
            {
                // We need to make sure that we project out any external references to the driving
                // node that the nested collections have, or we're going to end up with unresolvable
                // vars when we pull them up over the current driving node.
                VarVec nestOpOutputs = Command.CreateVarVec(projectOp.Outputs);

                for (int i = 1; i < newNestNodeChildren.Count; i++)
                {
                    nestOpOutputs.Or(newNestNodeChildren[i].GetNodeInfo(Command).ExternalReferences);
                }

                // We need to make sure we project out the sort keys too...
                foreach (InternalTrees.SortKey sk in nestOp.PrefixSortKeys)
                {
                    nestOpOutputs.Set(sk.Var);
                }

                nestNode.Op = Command.CreateMultiStreamNestOp(nestOp.PrefixSortKeys, nestOpOutputs, newCollectionInfoList);

                // we need to create a new node because we may have removed some of the collections.
                nestNode = Command.CreateNode(nestNode.Op, newNestNodeChildren);

                // Pull the nestNode up over the projectNode, and adjust
                // their inputs accordingly.
                projectNode.Child0 = nestNode.Child0;
                nestNode.Child0 = projectNode;

                Command.RecomputeNodeInfo(projectNode);
            }

            // Finally, recompute node info
            Command.RecomputeNodeInfo(nestNode);
#if DEBUG
            int size = input.Length; // GC.KeepAlive makes FxCop Grumpy.
            string output = Dump.ToXml(Command, nestNode);
#endif //DEBUG
            return nestNode;
        }

        /// <summary>
        /// SetOp common processing
        /// </summary>
        /// <remarks>
        /// The input to an IntersectOp or an ExceptOp cannot be a NestOp – that
        /// would imply that we support distinctness over collections  - which
        /// we don’t.
        ///
        /// UnionAllOp is somewhat trickier. We would need a way to percolate keys
        /// up the UnionAllOp – and I’m ok with not supporting this case for now.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitSetOp(SetOp op, Node n)
        {
            return NestingNotSupported(op, n);
        }

        /// <summary>
        /// SingleRowOp
        /// 
        /// SingleRowOp(NestOp(x,...)) => NestOp(SingleRowOp(x),...)
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(SingleRowOp op, Node n)
        {
            VisitChildren(n);

            if (IsNestOpNode(n.Child0))
            {
                n = n.Child0;
                Node newSingleRowOpNode = Command.CreateNode(op, n.Child0);
                n.Child0 = newSingleRowOpNode;
                Command.RecomputeNodeInfo(n);
            }
            return n;
        }

        /// <summary>
        /// SortOp
        /// </summary>
        /// <remarks>
        /// If the input to a SortOp is a NestOp, then none of the sort
        /// keys can be collection Vars of the NestOp – we don't support
        /// sorts over collections.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(SortOp op, Node n)
        {
            // Visit the children
            VisitChildren(n);
            m_varRemapper.RemapNode(n);

            // If the child is a NestOp, then simply push the sortkeys into the
            // "prefixKeys" of the nestOp, and return the NestOp itself.
            // The SortOp has now been merged into the NestOp
            NestBaseOp nestOp = n.Child0.Op as NestBaseOp;
            if (nestOp != null)
            {
                n.Child0.Op = GetNestOpWithConsolidatedSortKeys(nestOp, op.Keys);
                return n.Child0;
            }

            return n;
        }

        /// <summary>
        /// ConstrainedSortOp
        /// </summary>
        /// <remarks>
        /// Push the ConstrainedSortOp onto the driving node of the NestOp:
        ///
        ///     ConstrainedSortOp(NestOp(X,Y,...)) ==> NestOp(ConstrainedSortOp(X),Y,...)
        /// 
        /// There should not be any need for var renaming, because the ConstrainedSortOp cannot 
        /// refer to any vars from the NestOp
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ConstrainedSortOp op, Node n)
        {
            // Visit the children
            VisitChildren(n);

            // If the input is a nest op, we push the ConstrainedSort onto
            // the driving node.
            NestBaseOp nestOp = n.Child0.Op as NestBaseOp;
            if (nestOp != null)
            {
                Node nestNode = n.Child0;
                n.Child0 = nestNode.Child0;
                nestNode.Child0 = n;
                nestNode.Op = GetNestOpWithConsolidatedSortKeys(nestOp, op.Keys);
                n = nestNode;
            }
            return n;
        }

        /// <summary>
        /// Helper method used by Visit(ConstrainedSortOp, Node)and Visit(SortOp, Node).
        /// It returns a NestBaseOp equivalent to the inputNestOp, only with the given sortKeys
        /// prepended to the prefix sort keys already on the inputNestOp.
        /// </summary>
        /// <param name="inputNestOp"></param>
        /// <param name="sortKeys"></param>
        /// <returns></returns>
        private NestBaseOp GetNestOpWithConsolidatedSortKeys(NestBaseOp inputNestOp, List<InternalTrees.SortKey> sortKeys)
        {
            NestBaseOp result;

            // Include the  sort keys as the prefix sort keys;
            // Note that we can't actually have a SSNest at this point in
            // the tree; they're only introduced once we've processed the
            // entire tree.

            if (inputNestOp.PrefixSortKeys.Count == 0)
            {
                foreach (InternalTrees.SortKey sk in sortKeys)
                {
                    //SQLBUDT #507170 - We can't just add the sort keys, we need to copy them, 
                    // to avoid changes to one to affect the other
                    inputNestOp.PrefixSortKeys.Add(Command.CreateSortKey(sk.Var, sk.AscendingSort, sk.Collation));
                }
                result = inputNestOp;
            }
            else
            {

                VarVec sortVars = Command.CreateVarVec();

                // First add the sort keys from the SortBaseOp, then the NestOp keys
                List<InternalTrees.SortKey> sortKeyList = ConsolidateSortKeys(sortKeys, inputNestOp.PrefixSortKeys);

                PlanCompiler.Assert(inputNestOp is MultiStreamNestOp, "Unexpected SingleStreamNestOp?");

                // Finally, build a new NestOp with the keys...
                result = Command.CreateMultiStreamNestOp(sortKeyList, inputNestOp.Outputs, inputNestOp.CollectionInfo);
            }
            return result;
        }

        /// <summary>
        /// Helper method that given two lists of sort keys creates a single list of sort keys without duplicates.
        /// First the keys from the first given list are added, then from the second one.
        /// </summary>
        /// <param name="sortKeyList1"></param>
        /// <param name="sortKeyList2"></param>
        /// <returns></returns>
        private List<InternalTrees.SortKey> ConsolidateSortKeys(List<InternalTrees.SortKey> sortKeyList1, List<InternalTrees.SortKey> sortKeyList2)
        {
            VarVec sortVars = Command.CreateVarVec();
            List<InternalTrees.SortKey> sortKeyList = new List<InternalTrees.SortKey>();

            foreach (InternalTrees.SortKey sk in sortKeyList1)
            {
                if (!sortVars.IsSet(sk.Var))
                {
                    sortVars.Set(sk.Var);

                    //SQLBUDT #507170 - We can't just add the sort keys, we need to copy them, 
                    // to avoid changes to one to affect the other
                    sortKeyList.Add(Command.CreateSortKey(sk.Var, sk.AscendingSort, sk.Collation));
                }
            }

            foreach (InternalTrees.SortKey sk in sortKeyList2)
            {
                if (!sortVars.IsSet(sk.Var))
                {
                    sortVars.Set(sk.Var);
                    sortKeyList.Add(Command.CreateSortKey(sk.Var, sk.AscendingSort, sk.Collation));
                }
            }

            return sortKeyList;
        }

        /// <summary>
        /// UnnestOp
        /// </summary>
        /// <remarks>
        /// Logically, the UnnestOp can simply be replaced with the defining expression
        /// corresponding to the Var property of the UnnestOp. The tricky part is that
        /// the UnnestOp produces a set of ColumnVars which may be referenced in other
        /// parts of the query, and these need to be replaced by the corresponding Vars
        /// produced by the defining expression.
        ///
        /// There are essentially four cases:
        /// 
        /// Case 1: The UnnestOps Var is a UDT. Only the store can handle this, so we
        ///         pass it on without changing it.
        /// 
        /// Case 2: The UnnestOp has a Function as its input.  This implies that the
        ///         store has TVFs, which it can Unnest, so we let it handle that and do 
        ///         nothing.
        /// 
        /// Case 3: The UnnestOp Var defines a Nested collection.  We'll just replace 
        ///         the UnnestOp with the Input:
        /// 
        ///             UnnestOp(VarDef(CollectOp(PhysicalProjectOp(input)))) => input
        ///
        /// Case 4: The UnnestOp Var refers to a Nested collection from elsewhere.  As we
        ///         discover NestOps, we maintain a var->PhysicalProject Node map.  When
        ///         we get this case, we just make a copy of the PhysicalProject node, for
        ///         the referenced Var, and we replace the UnnestOp with it.
        /// 
        ///             UnnestOp(VarDef(VarRef(v))) ==> copy-of-defining-node-for-v
        /// 
        /// Then, we need to update all references to the output Vars (ColumnVars) produced
        /// by the Unnest to instead refer to the Vars produced by the copy of the subquery.
        /// We produce a map from the Vars of the subquery to the corresponding vars of the
        /// UnnestOp. We then use this map as we walk up the tree, and replace any references
        /// to the Unnest Vars by the new Vars.
        ///
        /// To simplify this process, as part of the ITreeGenerator, whenever we generate
        /// an UnnestOp, we will generate a ProjectOp above it – which simply selects out
        /// all Vars from the UnnestOp; and has no local definitions. This allows us to
        /// restrict the Var->Var replacement to just ProjectOp.
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(UnnestOp op, Node n)
        {
#if DEBUG
            string input = Dump.ToXml(Command, n);
#endif //DEBUG
            // First, visit my children
            VisitChildren(n);

            // If we're unnesting a UDT, then simply return - we cannot eliminate this unnest
            // It must be handled by the store
            md.CollectionType collType = TypeHelpers.GetEdmType<md.CollectionType>(op.Var.Type);
            if (TypeUtils.IsUdt(collType.TypeUsage))
            {
                return n;
            }

            // Find the VarDef node for the var we're supposed to unnest.
            PlanCompiler.Assert(n.Child0.Op.OpType == OpType.VarDef, "Unnest without VarDef input?");
            PlanCompiler.Assert(((VarDefOp)n.Child0.Op).Var == op.Var, "Unnest var not found?");
            PlanCompiler.Assert(n.Child0.HasChild0, "VarDef without input?");
            Node newNode = n.Child0.Child0;

            if (OpType.Function == newNode.Op.OpType)
            {
                // If we have an unnest over a function, there's nothing more we can do
                // This really means that the underlying store has the ability to
                // support TVFs, and therefore unnests, and we simply leave it as is
                return n;
            }
            else if (OpType.Collect == newNode.Op.OpType)
            {
                // UnnestOp(VarDef(CollectOp(PhysicalProjectOp(x)))) ==> x

                PlanCompiler.Assert(newNode.HasChild0, "collect without input?");
                newNode = newNode.Child0;

                PlanCompiler.Assert(newNode.Op.OpType == OpType.PhysicalProject, "collect without physicalProject?");

                // Ensure others that reference my var will know to use me;
                m_definingNodeMap.Add(op.Var, newNode);
            }
            else if (OpType.VarRef == newNode.Op.OpType)
            {
                // UnnestOp(VarDef(VarRef(v))) ==> copy-of-defining-node-for-v
                //
                // The Unnest's input is a VarRef; we need to replace it with
                // the defining node, and ensure we fixup the vars.

                Var refVar = ((VarRefOp)newNode.Op).Var;
                Node refVarDefiningNode;
                bool found = m_definingNodeMap.TryGetValue(refVar, out refVarDefiningNode);
                PlanCompiler.Assert(found, "Could not find a definition for a referenced collection var");

                newNode = CopyCollectionVarDefinition(refVarDefiningNode);

                PlanCompiler.Assert(newNode.Op.OpType == OpType.PhysicalProject, "driving node is not physicalProject?");
            }
            else
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.InvalidInternalTree, 2, newNode.Op.OpType);
            }

            IEnumerable<Var> inputVars = ((PhysicalProjectOp)newNode.Op).Outputs;

            PlanCompiler.Assert(newNode.HasChild0, "physicalProject without input?");
            newNode = newNode.Child0;

            // Dev10 #530752 : it is not correct to just remove the sort key    
            if (newNode.Op.OpType == OpType.Sort)
            {
                m_foundSortUnderUnnest = true;
            }

            // Update the replacement vars to reflect the pulled up operation
            UpdateReplacementVarMap(op.Table.Columns, inputVars);

#if DEBUG
            int size = input.Length; // GC.KeepAlive makes FxCop Grumpy.
            string output = Dump.ToXml(Command, newNode);
#endif //DEBUG
            return newNode;
        }

        /// <summary>
        /// Copies the given defining node for a collection var, but also makes sure to 'register' all newly
        /// created collection vars (i.e. copied).
        /// 
        ///SQLBUDT #557427: The defining node that is being copied may itself contain definitions to other
        /// collection vars. These defintions would be present in m_definingNodeMap. However, after we make a copy 
        /// of the defining node, we need to make sure to also put 'matching' definitions of these other collection 
        /// vars into m_definingNodeMap.
        /// The dictionary collectionVarDefinitions (below) contains the copied definitions of such collection vars. 
        /// but without the wrapping PhysicalProjectOp.
        ///
        /// Example:     m_definingNodeMap contains (var1, definition1) and (var2, definintion2). 
        ///              var2 is defined inside the definition of var1. 
        ///              Here we copy definition1 -> definintion1'.
        ///              We need to add to m_definitionNodeMap (var2', definition2').
        ///              definition2' should be a copy of definiton2 in the context of to definition1', 
        ///              i.e. definition2' should relate to definition1' in same way that definition2 relates to definition1 
        ///         /// </summary>
        /// <param name="refVarDefiningNode"></param>
        /// <returns></returns>
        private Node CopyCollectionVarDefinition(Node refVarDefiningNode)
        {

            VarMap varMap;
            Dictionary<Var, Node> collectionVarDefinitions;
            Node newNode = OpCopierTrackingCollectionVars.Copy(Command, refVarDefiningNode, out varMap, out collectionVarDefinitions);

            if (collectionVarDefinitions.Count != 0)
            {
                VarMap reverseMap = varMap.GetReverseMap();

                foreach (KeyValuePair<Var, Node> collectionVarDefinitionPair in collectionVarDefinitions)
                {
                    //
                    // Getting the matching definition for a collection map (i.e. definition2' from the example above)
                    //
                    // Definitions of collection vars are rooted at a PhysicalProjectOp, 
                    //      i.e. definition2 = PhysicalProjectOp(output2, columnMap2, definingSubtree2) 
                    //
                    //  The collectionVarDefinitions dictionary gives us the defining nodes rooted at what would a child 
                    //  of such PhysicalProjectOp, i.e.  definingSubtree2'.
                    // 
                    //  definition2' = PhysicalProjectOp(CopyWithRemap(output2), CopyWithRemap(columnMap2), definingSubtree2') 
                    //

                    Node keyDefiningNode;
                    Var keyDefiningVar = reverseMap[collectionVarDefinitionPair.Key];
                    //Note: we should not call ResolveVarReference(keyDefiningNode), we can only use the exact var
                    if (m_definingNodeMap.TryGetValue(keyDefiningVar, out keyDefiningNode))
                    {
                        PhysicalProjectOp originalPhysicalProjectOp = (PhysicalProjectOp)keyDefiningNode.Op;

                        VarList newOutputs = VarRemapper.RemapVarList(Command, varMap, originalPhysicalProjectOp.Outputs);
                        SimpleCollectionColumnMap newColumnMap = (SimpleCollectionColumnMap)ColumnMapCopier.Copy(originalPhysicalProjectOp.ColumnMap, varMap);

                        PhysicalProjectOp newPhysicalProjectOp = Command.CreatePhysicalProjectOp(newOutputs, newColumnMap);
                        Node newDefiningNode = Command.CreateNode(newPhysicalProjectOp, collectionVarDefinitionPair.Value);

                        m_definingNodeMap.Add(collectionVarDefinitionPair.Key, newDefiningNode);
                    }
                }
            }
            return newNode;
        }

        #endregion

        #region PhysicalOp Visitors

        /// <summary>
        /// MultiStreamNestOp/SingleStreamNestOp common processing.
        ///
        /// Pretty much just verifies that we didn't leave a NestOp behind.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        ///
        protected override Node VisitNestOp(NestBaseOp op, Node n)
        {
            // First, visit my children
            VisitChildren(n);

            // If any of the children are a nestOp, then we have a
            // problem; it shouldn't have happened.
            foreach (Node chi in n.Children)
            {
                if (IsNestOpNode(chi))
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.NestOverNest);
                }
            }
            return n;
        }

        /// <summary>
        /// PhysicalProjectOp
        /// </summary>
        /// <remarks>
        /// Tranformation:
        ///
        ///     PhysicalProjectOp(MultiStreamNestOp(...)) => PhysicalProjectOp(SortOp(...))
        ///
        /// Strategy:
        ///
        ///     (1) Convert MultiStreamNestOp(...) => SingleStreamNestOp(...)
        ///     (2) Convert SingleStreamNestOp(...) => SortOp(...)
        ///     (3) Fixup the column maps.
        ///
        /// </remarks>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(PhysicalProjectOp op, Node n)
        {
            // cannot be multi-input (not at this point)
            PlanCompiler.Assert(n.Children.Count == 1, "multiple inputs to physicalProject?");

            // First visit my children
            VisitChildren(n);
            m_varRemapper.RemapNode(n);

            // Wait until we're processing the root physicalProjectNode to convert the nestOp
            // to sort/union all; it's much easier to unnest them if we don't monkey with them
            // until then.
            //
            // Also, even if we're the root physicalProjectNode and the children aren't NestOps, 
            // then there's nothing further to do.
            if (n != Command.Root || !IsNestOpNode(n.Child0))
            {
                return n;
            }

#if DEBUG
            string input = Dump.ToXml(Command, n);
#endif //DEBUG

            Node nestNode = n.Child0;

            // OK, we're now guaranteed to be processing a root physicalProjectNode with at
            // least one MultiStreamNestOp as it's input.  First step is to convert that into
            // a single SingleStreamNestOp.
            //
            // NOTE: if we ever wanted to support MARS, we would probably avoid the conversion
            //       to SingleStreamNest here, and do something to optimize this a bit 
            //       differently for MARS.  But that's a future feature.
            Dictionary<Var, ColumnMap> varRefReplacementMap = new Dictionary<Var, ColumnMap>();

            //Dev10_579146: The parameters that are output should be retained.
            VarList outputVars = Command.CreateVarList(op.Outputs.Where(v => v.VarType == VarType.Parameter));
            SimpleColumnMap[] keyColumnMaps;

            nestNode = ConvertToSingleStreamNest(nestNode, varRefReplacementMap, outputVars, out keyColumnMaps);
            SingleStreamNestOp ssnOp = (SingleStreamNestOp)nestNode.Op;

            // Build up the sort node (if necessary).
            Node sortNode = BuildSortForNestElimination(ssnOp, nestNode);

            // Create a new column map using the columnMapPatcher that was updated by the
            // conversion to SingleStreamNest process.
            SimpleCollectionColumnMap newProjectColumnMap = (SimpleCollectionColumnMap)ColumnMapTranslator.Translate(((PhysicalProjectOp)n.Op).ColumnMap, varRefReplacementMap);
            newProjectColumnMap = new SimpleCollectionColumnMap(newProjectColumnMap.Type, newProjectColumnMap.Name, newProjectColumnMap.Element, keyColumnMaps, null);

            // Ok, build the new PhysicalProjectOp, slap the sortNode as its input
            // and we're all done.
            n.Op = Command.CreatePhysicalProjectOp(outputVars, newProjectColumnMap);
            n.Child0 = sortNode;

#if DEBUG
            int size = input.Length;// GC.KeepAlive makes FxCop Grumpy.
            string output = Dump.ToXml(Command, n);
#endif //DEBUG

            return n;
        }

        /// <summary>
        /// Build up a sort node above the nestOp's input - only if there 
        /// are any sort keys to produce
        /// </summary>
        /// <param name="ssnOp"></param>
        /// <param name="nestNode"></param>
        /// <returns></returns>
        private Node BuildSortForNestElimination(SingleStreamNestOp ssnOp, Node nestNode)
        {
            Node sortNode;

            List<InternalTrees.SortKey> sortKeyList = BuildSortKeyList(ssnOp);

            // Now if, at this point, there aren't any sort keys then remove the
            // sort operation, otherwise, build a new SortNode;
            if (sortKeyList.Count > 0)
            {
                SortOp sortOp = Command.CreateSortOp(sortKeyList);
                sortNode = Command.CreateNode(sortOp, nestNode.Child0);
            }
            else
            {
                // No sort keys => single_row_table => no need to sort
                sortNode = nestNode.Child0;
            }
            return sortNode;
        }

        /// <summary>
        /// Build up the list of sortkeys. This list should comprise (in order):
        ///
        ///  - Any prefix sort keys (these represent sort operations on the
        ///    driving table, that were logically above the nest)
        ///  - The keys of the nest operation
        ///  - The discriminator column for the nest operation
        ///  - the list of postfix sort keys (used to represent nested collections)
        ///
        /// Note that we only add the first occurrance of a var to the list; further
        /// references to the same variable would be trumped by the first one.
        /// </summary>
        /// <param name="ssnOp"></param>
        /// <returns></returns>
        private List<InternalTrees.SortKey> BuildSortKeyList(SingleStreamNestOp ssnOp)
        {
            VarVec sortVars = Command.CreateVarVec();

            // First add the prefix sort keys
            List<InternalTrees.SortKey> sortKeyList = new List<InternalTrees.SortKey>();
            foreach (InternalTrees.SortKey sk in ssnOp.PrefixSortKeys)
            {
                if (!sortVars.IsSet(sk.Var))
                {
                    sortVars.Set(sk.Var);
                    sortKeyList.Add(sk);
                }
            }

            // Then add the nestop keys
            foreach (Var v in ssnOp.Keys)
            {
                if (!sortVars.IsSet(v))
                {
                    sortVars.Set(v);
                    InternalTrees.SortKey sk = Command.CreateSortKey(v);
                    sortKeyList.Add(sk);
                }
            }

            // Then add the discriminator var
            PlanCompiler.Assert(!sortVars.IsSet(ssnOp.Discriminator), "prefix sort on discriminator?");
            sortKeyList.Add(Command.CreateSortKey(ssnOp.Discriminator));

            // Finally, add the postfix keys
            foreach (InternalTrees.SortKey sk in ssnOp.PostfixSortKeys)
            {
                if (!sortVars.IsSet(sk.Var))
                {
                    sortVars.Set(sk.Var);
                    sortKeyList.Add(sk);
                }
            }
            return sortKeyList;
        }


        /// <summary>
        /// convert MultiStreamNestOp to SingleStreamNestOp
        /// </summary>
        /// <remarks>
        /// A MultiStreamNestOp is typically of the form M(D, N1, N2, ..., Nk)
        /// where D is the driver stream, and N1, N2 etc. represent the collections.
        ///
        /// In general, this can be converted into a SingleStreamNestOp over:
        ///
        ///    (D+ outerApply N1) AugmentedUnionAll (D+ outerApply N2) ...
        ///
        /// Where:
        ///
        ///     D+ is D with an extra discriminator column that helps to identify
        ///     the specific collection.
        ///
        ///     AugmentedUnionAll is simply a unionAll where each branch of the
        ///     unionAll is augmented with nulls for the corresponding columns
        ///     of other tables in the branch
        ///
        /// The simple case where there is only a single nested collection is easier
        /// to address, and can be represented by:
        ///
        ///     MultiStreamNest(D, N1) => SingleStreamNest(OuterApply(D, N1))
        ///
        /// The more complex case, where there is more than one nested column, requires
        /// quite a bit more work:
        ///
        ///     MultiStreamNest(D, X, Y,...) => SingleStreamNest(UnionAll(Project{"1", D1...Dn, X1...Xn, nY1...nYn}(OuterApply(D, X)), Project{"2", D1...Dn, nX1...nXn, Y1...Yn}(OuterApply(D, Y)), ...))
        ///
        /// Where:
        ///      D           is the driving collection
        ///      D1...Dn     are the columns from the driving collection
        ///      X           is the first nested collection
        ///      X1...Xn     are the columns from the first nested collection
        ///      nX1...nXn   are null values for all columns from the first nested collection
        ///      Y           is the second nested collection
        ///      Y1...Yn     are the columns from the second nested collection
        ///      nY1...nYn   are null values for all columns from the second nested collection
        /// </remarks>
        /// <param name="nestNode"></param>
        /// <param name="varRefReplacementMap"></param>
        /// <param name="flattenedOutputVarList"></param>
        /// <param name="parentKeyColumnMaps"></param>
        /// <returns></returns>
        private Node ConvertToSingleStreamNest(Node nestNode, Dictionary<Var, ColumnMap> varRefReplacementMap, VarList flattenedOutputVarList, out SimpleColumnMap[] parentKeyColumnMaps)
        {
#if DEBUG
            string input = Dump.ToXml(Command, nestNode);
#endif //DEBUG
            MultiStreamNestOp nestOp = (MultiStreamNestOp)nestNode.Op;

            // We can't convert this node to a SingleStreamNest until all it's MultiStreamNest 
            // inputs are converted, so do that first.
            for (int i = 1; i < nestNode.Children.Count; i++)
            {
                Node chi = nestNode.Children[i];

                if (chi.Op.OpType == OpType.MultiStreamNest)
                {
                    CollectionInfo chiCi = nestOp.CollectionInfo[i - 1];

                    VarList childFlattenedOutputVars = Command.CreateVarList();
                    SimpleColumnMap[] childKeyColumnMaps;

                    nestNode.Children[i] = ConvertToSingleStreamNest(chi, varRefReplacementMap, childFlattenedOutputVars, out childKeyColumnMaps);

                    // Now this may seem odd here, and it may look like we should have done this
                    // inside the recursive ConvertToSingleStreamNest call above, but that call
                    // doesn't have access to the CollectionInfo for it's parent, which is what
                    // we need to manipulate before we enter the loop below where we try and fold
                    // THIS nestOp nodes into a singleStreamNestOp.
                    ColumnMap childColumnMap = ColumnMapTranslator.Translate(chiCi.ColumnMap, varRefReplacementMap);

                    VarVec childKeys = Command.CreateVarVec(((SingleStreamNestOp)nestNode.Children[i].Op).Keys);

                    nestOp.CollectionInfo[i - 1] = Command.CreateCollectionInfo(chiCi.CollectionVar,
                                                                                  childColumnMap,
                                                                                  childFlattenedOutputVars,
                                                                                  childKeys,
                                                                                  chiCi.SortKeys,
                                                                                  null /*discriminatorValue*/
                                                                                  );
                }
            }

            // Make sure that the driving node has keys defined. Otherwise we're in
            // trouble; we must be able to infer keys from the driving node.
            Node drivingNode = nestNode.Child0;
            KeyVec drivingNodeKeys = Command.PullupKeys(drivingNode);
            if (drivingNodeKeys.NoKeys)
            {
                // [....]: In this case we used to wrap drivingNode into a projection that would also project Edm.NewGuid() thus giving us a synthetic key.
                // This solution did not work however due to a bug in SQL Server that allowed pulling non-deterministic functions above joins and applies, thus 
                // producing incorrect results. SQL Server bug was filed in "sqlbuvsts01\Sql Server" database as #725272.
                // The only known path how we can get a keyless drivingNode is if 
                //    - drivingNode is over a TVF call
                //    - TVF is declared as Collection(Row) is SSDL (the only form of TVF definitions at the moment)
                //    - TVF is not mapped to entities
                //      Note that if TVF is mapped to entities via function import mapping, and the user query is actually the call of the 
                //      function import, we infer keys for the TVF from the c-space entity keys and their mappings.
                throw EntityUtil.KeysRequiredForNesting();
            }

            // Get a deterministic ordering of Vars from this node.
            // NOTE: we're using the drivingNode's definitions, which is a VarVec so it
            //       won't match the order of the input's columns, but the key thing is 
            //       that we use the same order for all nested children, so it's OK.
            ExtendedNodeInfo drivingNodeInfo = Command.GetExtendedNodeInfo(drivingNode);
            VarVec drivingNodeVarVec = drivingNodeInfo.Definitions;
            VarList drivingNodeVars = Command.CreateVarList(drivingNodeVarVec);

            // Normalize all collection inputs to the nestOp. Specifically, remove any
            // SortOps (adding the sort keys to the postfix sortkey list). Additionally,
            // add a discriminatorVar to each collection child
            VarList discriminatorVarList;
            List<List<InternalTrees.SortKey>> postfixSortKeyList;
            NormalizeNestOpInputs(nestOp, nestNode, out discriminatorVarList, out postfixSortKeyList);

            // Now build up the union-all subquery
            List<Dictionary<Var, Var>> varMapList;
            Var outputDiscriminatorVar;
            Node unionAllNode = BuildUnionAllSubqueryForNestOp(nestOp, nestNode, drivingNodeVars, discriminatorVarList, out outputDiscriminatorVar, out varMapList);
            Dictionary<Var, Var> drivingNodeVarMap = varMapList[0];

            // OK.  We've finally created the UnionAll over each of the project/outerApply
            // combinations.  We know that the output columns will be:
            //
            //      Discriminator, DrivingColumns, Collection1Columns, Collection2Columns, ...
            //
            // Now, rebuild the columnMaps, since all of the columns in the original column
            // maps are now referencing newer variables.  To do that, we'll walk the list of
            // outputs from the unionAll, and construct new VarRefColumnMaps for each one,
            // and adding it to a ColumnMapPatcher, which we'll use to actually fix everything
            // up.
            //
            // While we're at it, we'll build a new list of top-level output columns, which
            // should include only the Discriminator, the columns from the driving collection,
            // and and one column for each of the nested collections.

            // Start building the flattenedOutputVarList that the top level PhysicalProjectOp
            // is to output.
            flattenedOutputVarList.AddRange(RemapVars(drivingNodeVars, drivingNodeVarMap));

            VarVec flattenedOutputVarVec = Command.CreateVarVec(flattenedOutputVarList);
            VarVec nestOpOutputs = Command.CreateVarVec(flattenedOutputVarVec);

            // Add any adjustments to the driving nodes vars to the column map patcher
            foreach (KeyValuePair<Var, Var> kv in drivingNodeVarMap)
            {
                if (kv.Key != kv.Value)
                {
                    varRefReplacementMap[kv.Key] = new VarRefColumnMap(kv.Value);
                }
            }

            RemapSortKeys(nestOp.PrefixSortKeys, drivingNodeVarMap);

            List<InternalTrees.SortKey> newPostfixSortKeys = new List<InternalTrees.SortKey>();
            List<CollectionInfo> newCollectionInfoList = new List<CollectionInfo>();

            // Build the discriminator column map, and ensure it's in the outputs
            VarRefColumnMap discriminatorColumnMap = new VarRefColumnMap(outputDiscriminatorVar);
            nestOpOutputs.Set(outputDiscriminatorVar);

            if (!flattenedOutputVarVec.IsSet(outputDiscriminatorVar))
            {
                flattenedOutputVarList.Add(outputDiscriminatorVar);
                flattenedOutputVarVec.Set(outputDiscriminatorVar);
            }

            // Build the key column maps, and ensure they're in the outputs as well.
            VarVec parentKeys = RemapVarVec(drivingNodeKeys.KeyVars, drivingNodeVarMap);
            parentKeyColumnMaps = new SimpleColumnMap[parentKeys.Count];

            int index = 0;
            foreach (Var keyVar in parentKeys)
            {
                parentKeyColumnMaps[index] = new VarRefColumnMap(keyVar);
                index++;

                if (!flattenedOutputVarVec.IsSet(keyVar))
                {
                    flattenedOutputVarList.Add(keyVar);
                    flattenedOutputVarVec.Set(keyVar);
                }
            }

            // Now that we've handled the driving node, deal with each of the 
            // nested inputs, in sequence.
            for (int i = 1; i < nestNode.Children.Count; i++)
            {
                CollectionInfo ci = nestOp.CollectionInfo[i - 1];
                List<InternalTrees.SortKey> postfixSortKeys = postfixSortKeyList[i];

                RemapSortKeys(postfixSortKeys, varMapList[i]);
                newPostfixSortKeys.AddRange(postfixSortKeys);

                ColumnMap newColumnMap = ColumnMapTranslator.Translate(ci.ColumnMap, varMapList[i]);
                VarList newFlattenedElementVars = RemapVarList(ci.FlattenedElementVars, varMapList[i]);
                VarVec newCollectionKeys = RemapVarVec(ci.Keys, varMapList[i]);

                RemapSortKeys(ci.SortKeys, varMapList[i]);

                CollectionInfo newCollectionInfo = Command.CreateCollectionInfo(
                                                                                ci.CollectionVar,
                                                                                newColumnMap,
                                                                                newFlattenedElementVars,
                                                                                newCollectionKeys,
                                                                                ci.SortKeys,
                                                                                i);
                newCollectionInfoList.Add(newCollectionInfo);

                // For a collection Var, we add the flattened elementVars for the
                // collection in place of the collection Var itself, and we create
                // a new column map to represent all the stuff we've done.

                foreach (Var v in newFlattenedElementVars)
                {
                    if (!flattenedOutputVarVec.IsSet(v))
                    {
                        flattenedOutputVarList.Add(v);
                        flattenedOutputVarVec.Set(v);
                    }
                }

                nestOpOutputs.Set(ci.CollectionVar);

                int keyColumnMapIndex = 0;
                SimpleColumnMap[] keyColumnMaps = new SimpleColumnMap[newCollectionInfo.Keys.Count];
                foreach (Var keyVar in newCollectionInfo.Keys)
                {
                    keyColumnMaps[keyColumnMapIndex] = new VarRefColumnMap(keyVar);
                    keyColumnMapIndex++;
                }

                DiscriminatedCollectionColumnMap collectionColumnMap = new DiscriminatedCollectionColumnMap(
                                                                            TypeUtils.CreateCollectionType(newCollectionInfo.ColumnMap.Type),
                                                                            newCollectionInfo.ColumnMap.Name,
                                                                            newCollectionInfo.ColumnMap,
                                                                            keyColumnMaps,
                                                                            parentKeyColumnMaps,
                                                                            discriminatorColumnMap,
                                                                            newCollectionInfo.DiscriminatorValue
                                                                            );
                varRefReplacementMap[ci.CollectionVar] = collectionColumnMap;
            }

            // Finally, build up the SingleStreamNest Node
            SingleStreamNestOp newSsnOp = Command.CreateSingleStreamNestOp(
                                                            parentKeys,
                                                            nestOp.PrefixSortKeys,
                                                            newPostfixSortKeys,
                                                            nestOpOutputs,
                                                            newCollectionInfoList,
                                                            outputDiscriminatorVar);
            Node newNestNode = Command.CreateNode(newSsnOp, unionAllNode);

#if DEBUG
            int size = input.Length;// GC.KeepAlive makes FxCop Grumpy.
            string output = Dump.ToXml(Command, newNestNode);
#endif //DEBUG

            return newNestNode;
        }

        /// <summary>
        /// "Normalize" each input to the NestOp.
        /// We're now in the context of a MultiStreamNestOp, and we're trying to convert this
        /// into a SingleStreamNestOp.
        ///
        /// Normalization specifically refers to
        ///   - augmenting each input with a discriminator value (that describes the collection)
        ///   - removing the sort node at the root (and capturing this information as part of the sortkeys)
        /// </summary>
        /// <param name="nestOp">the nestOp</param>
        /// <param name="nestNode">the nestOp subtree</param>
        /// <param name="discriminatorVarList">Discriminator Vars for each Collection input</param>
        /// <param name="sortKeys">SortKeys (postfix) for each Collection input</param>
        /// 
        /// 
        private void NormalizeNestOpInputs(NestBaseOp nestOp, Node nestNode, out VarList discriminatorVarList, out List<List<InternalTrees.SortKey>> sortKeys)
        {
            discriminatorVarList = Command.CreateVarList();

            // We insert a dummy var and value at poistion 0 for the deriving node, which
            // we should never reference;
            discriminatorVarList.Add(null);

            sortKeys = new List<List<InternalTrees.SortKey>>();
            sortKeys.Add(nestOp.PrefixSortKeys);

            for (int i = 1; i < nestNode.Children.Count; i++)
            {
                Node inputNode = nestNode.Children[i];
                // Since we're called from ConvertToSingleStreamNest, it is possible that we have a 
                // SingleStreamNest here, because the input to the MultiStreamNest we're converting 
                // may have been a MultiStreamNest that was converted to a SingleStreamNest.
                SingleStreamNestOp ssnOp = inputNode.Op as SingleStreamNestOp;

                // If this collection is a SingleStreamNest, we pull up the key information
                // in it, and pullup the input;
                if (null != ssnOp)
                {
                    // Note that the sortKeys argument is 1:1 with the nestOp inputs, that is
                    // each input may have exactly one entry in the list, so we have to combine
                    // all of the sort key components (Prefix+Keys+Discriminator+PostFix) into
                    // one list.
                    List<InternalTrees.SortKey> mySortKeys = BuildSortKeyList(ssnOp);
                    sortKeys.Add(mySortKeys);

                    inputNode = inputNode.Child0;
                }
                else
                {
                    // If the current collection has a SortNode specified, then pull that
                    // out, and add the information to the list of postfix SortColumns
                    SortOp sortOp = inputNode.Op as SortOp;
                    if (null != sortOp)
                    {
                        inputNode = inputNode.Child0; // bypass the sort node
                        // Add the sort keys to the list of postfix sort keys
                        sortKeys.Add(sortOp.Keys);
                    }
                    else
                    {
                        // No postfix sort keys for this case
                        sortKeys.Add(new List<InternalTrees.SortKey>());
                    }
                }

                // #447304: Ensure that any SortKey Vars will be projected from the input in addition to showing up in the postfix sort keys
                // by adding them to the FlattenedElementVars for this NestOp input's CollectionInfo.
                VarList flattenedElementVars = nestOp.CollectionInfo[i - 1].FlattenedElementVars;
                foreach (InternalTrees.SortKey sortKey in sortKeys[i])
                {
                    if (!flattenedElementVars.Contains(sortKey.Var))
                    {
                        flattenedElementVars.Add(sortKey.Var);
                    }
                }

                // Add a discriminator column to the collection-side - this must
                // happen before the outer-apply is added on; we need to use the value of
                // the discriminator to distinguish between null and empty collections
                Var discriminatorVar;
                Node augmentedInput = AugmentNodeWithInternalIntegerConstant(inputNode, i, out discriminatorVar);
                nestNode.Children[i] = augmentedInput;
                discriminatorVarList.Add(discriminatorVar);
            }
        }

        /// <summary>
        /// 'Extend' a given input node to also project out an internal integer constant with the given value
        /// </summary>
        /// <param name="input"></param>
        /// <param name="value"></param>
        /// <param name="internalConstantVar"></param>
        /// <returns></returns>
        private Node AugmentNodeWithInternalIntegerConstant(Node input, int value, out Var internalConstantVar)
        {
            return AugmentNodeWithConstant(input, () => Command.CreateInternalConstantOp(Command.IntegerType, value), out internalConstantVar);
        }

        /// <summary>
        /// Add a constant to a node. Specifically:
        ///
        ///     N ==> Project(N,{definitions-from-N, constant})
        /// </summary>
        /// <param name="input">the input node to augment</param>
        /// <param name="createOp">The fucntion to create the constant op </param>
        /// <param name="constantVar">the computed Var for the internal constant</param>
        /// <returns>the augmented node</returns>
        private Node AugmentNodeWithConstant(Node input, Func<ConstantBaseOp> createOp, out Var constantVar)
        {
            // Construct the op for the constant value and 
            // a VarDef node that that defines it.
            ConstantBaseOp constantOp = createOp();
            Node constantNode = Command.CreateNode(constantOp);
            Node varDefListNode = Command.CreateVarDefListNode(constantNode, out constantVar);

            // Now identify the list of definitions from the input, and project out
            // every one of them and include the constantVar
            ExtendedNodeInfo inputNodeInfo = Command.GetExtendedNodeInfo(input);
            VarVec projectOutputs = Command.CreateVarVec(inputNodeInfo.Definitions);
            projectOutputs.Set(constantVar);

            ProjectOp projectOp = Command.CreateProjectOp(projectOutputs);
            Node projectNode = Command.CreateNode(projectOp, input, varDefListNode);

            return projectNode;
        }

        /// <summary>
        /// Convert a SingleStreamNestOp into a massive UnionAllOp
        /// </summary>
        /// <param name="nestOp"></param>
        /// <param name="nestNode"></param>
        /// <param name="drivingNodeVars"></param>
        /// <param name="discriminatorVarList"></param>
        /// <param name="discriminatorVar"></param>
        /// <param name="varMapList"></param>
        /// <returns></returns>
        private Node BuildUnionAllSubqueryForNestOp(NestBaseOp nestOp, Node nestNode, VarList drivingNodeVars, VarList discriminatorVarList, out Var discriminatorVar, out List<Dictionary<Var, Var>> varMapList)
        {
            Node drivingNode = nestNode.Child0;

            // For each of the NESTED collections...
            Node unionAllNode = null;
            VarList unionAllOutputs = null;
            for (int i = 1; i < nestNode.Children.Count; i++)
            {
                // Ensure we only use the driving collection tree once, so other
                // transformations do not unintentionally change more than one path.
                // To prevent nodes in the tree from being used in multiple paths,
                // we copy the driving input on successive nodes.
                VarList newDrivingNodeVars;
                Node newDrivingNode;
                VarList newFlattenedElementVars;
                Op op;

                if (i > 1)
                {
                    newDrivingNode = OpCopier.Copy(Command, drivingNode, drivingNodeVars, out newDrivingNodeVars);
                    // 
                    // Bug 450245: If we copied the driver node, then references to driver node vars
                    // from the collection subquery must be patched up
                    //
                    VarRemapper varRemapper = new VarRemapper(this.Command);
                    for (int j = 0; j < drivingNodeVars.Count; j++)
                    {
                        varRemapper.AddMapping(drivingNodeVars[j], newDrivingNodeVars[j]);
                    }
                    // Remap all references in the current subquery
                    varRemapper.RemapSubtree(nestNode.Children[i]);

                    // Bug 479183: Remap the flattened element vars
                    newFlattenedElementVars = varRemapper.RemapVarList(nestOp.CollectionInfo[i - 1].FlattenedElementVars);

                    // Create a cross apply for all but the first collection
                    op = Command.CreateCrossApplyOp();
                }
                else
                {
                    newDrivingNode = drivingNode;
                    newDrivingNodeVars = drivingNodeVars;
                    newFlattenedElementVars = nestOp.CollectionInfo[i - 1].FlattenedElementVars;

                    // Create an outer apply for the first collection, 
                    // that way we ensure at least one row for each row in the driver node.
                    op = Command.CreateOuterApplyOp();
                }

                // Create an outer apply with the driver node and the nested collection.
                Node applyNode = Command.CreateNode(op, newDrivingNode, nestNode.Children[i]);

                // Now create a ProjectOp that augments the output from the OuterApplyOp
                // with nulls for each column from other collections

                // Build the VarDefList (the list of vars) for the Project, starting
                // with the collection discriminator var
                List<Node> varDefListChildren = new List<Node>();
                VarList projectOutputs = Command.CreateVarList();

                // Add the collection discriminator var to the output.
                projectOutputs.Add(discriminatorVarList[i]);

                // Add all columns from the driving node
                projectOutputs.AddRange(newDrivingNodeVars);

                // Add all the vars from all the nested collections;
                for (int j = 1; j < nestNode.Children.Count; j++)
                {
                    CollectionInfo otherCollectionInfo = nestOp.CollectionInfo[j - 1];
                    // For the current nested collection, we just pick the var that's
                    // coming from there and don't need have a new var defined, but for
                    // the rest we construct null values.
                    if (i == j)
                    {
                        projectOutputs.AddRange(newFlattenedElementVars);
                    }
                    else
                    {
                        foreach (Var v in otherCollectionInfo.FlattenedElementVars)
                        {
                            NullOp nullOp = Command.CreateNullOp(v.Type);
                            Node nullOpNode = Command.CreateNode(nullOp);
                            Var nullOpVar;
                            Node nullOpVarDefNode = Command.CreateVarDefNode(nullOpNode, out nullOpVar);
                            varDefListChildren.Add(nullOpVarDefNode);
                            projectOutputs.Add(nullOpVar);
                        }
                    }
                }

                Node varDefListNode = Command.CreateNode(Command.CreateVarDefListOp(), varDefListChildren);

                // Now, build up the projectOp
                VarVec projectOutputsVarSet = Command.CreateVarVec(projectOutputs);
                ProjectOp projectOp = Command.CreateProjectOp(projectOutputsVarSet);
                Node projectNode = Command.CreateNode(projectOp, applyNode, varDefListNode);

                // finally, build the union all
                if (unionAllNode == null)
                {
                    unionAllNode = projectNode;
                    unionAllOutputs = projectOutputs;
                }
                else
                {
                    VarMap unionAllMap = new VarMap();
                    VarMap projectMap = new VarMap();
                    for (int idx = 0; idx < unionAllOutputs.Count; idx++)
                    {
                        Var outputVar = Command.CreateSetOpVar(unionAllOutputs[idx].Type);
                        unionAllMap.Add(outputVar, unionAllOutputs[idx]);
                        projectMap.Add(outputVar, projectOutputs[idx]);
                    }
                    UnionAllOp unionAllOp = Command.CreateUnionAllOp(unionAllMap, projectMap);
                    unionAllNode = Command.CreateNode(unionAllOp, unionAllNode, projectNode);

                    // Get the output vars from the union-op. This must be in the same order
                    // as the original list of Vars
                    unionAllOutputs = GetUnionOutputs(unionAllOp, unionAllOutputs);
                }
            }

            // We're done building the node, but now we have to build a mapping from
            // the before-Vars to the after-Vars
            varMapList = new List<Dictionary<Var, Var>>();
            IEnumerator<Var> outputVarsEnumerator = unionAllOutputs.GetEnumerator();
            if (!outputVarsEnumerator.MoveNext())
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 4); // more columns from children than are on the unionAll?
            }
            // The discriminator var is always first
            discriminatorVar = outputVarsEnumerator.Current;

            // Build a map for each input
            for (int i = 0; i < nestNode.Children.Count; i++)
            {
                Dictionary<Var, Var> varMap = new Dictionary<Var, Var>();
                VarList varList = (i == 0) ? drivingNodeVars : nestOp.CollectionInfo[i - 1].FlattenedElementVars;
                foreach (Var v in varList)
                {
                    if (!outputVarsEnumerator.MoveNext())
                    {
                        throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 5); // more columns from children than are on the unionAll?
                    }
                    varMap[v] = outputVarsEnumerator.Current;
                }
                varMapList.Add(varMap);
            }
            if (outputVarsEnumerator.MoveNext())
            {
                throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 6); // at this point, we better be done with both lists...
            }

            return unionAllNode;
        }

        /// <summary>
        /// Get back an ordered list of outputs from a union-all op. The ordering should
        /// be identical to the ordered list "leftVars" which describes the left input of
        /// the unionAllOp
        /// </summary>
        /// <param name="unionOp">the unionall Op</param>
        /// <param name="leftVars">vars of the left input</param>
        /// <returns>output vars ordered in the same way as the left input</returns>
        private static VarList GetUnionOutputs(UnionAllOp unionOp, VarList leftVars)
        {
            VarMap varMap = unionOp.VarMap[0];
            Dictionary<Var, Var> reverseVarMap = varMap.GetReverseMap();

            VarList unionAllVars = Command.CreateVarList();
            foreach (Var v in leftVars)
            {
                Var newVar = reverseVarMap[v];
                unionAllVars.Add(newVar);
            }

            return unionAllVars;
        }

        #endregion

        #endregion
    }

    #region Class OpCopierTrackingCollectionVars
    /// <summary>
    /// Wrapper around OpCopier to keep track of the defining subtrees
    /// of collection vars defined in the subtree being returned as a copy.
    /// </summary>
    internal class OpCopierTrackingCollectionVars : OpCopier
    {
        #region Private State
        private Dictionary<Var, Node> m_newCollectionVarDefinitions = new Dictionary<Var, Node>();
        #endregion

        #region Private Constructor
        private OpCopierTrackingCollectionVars(Command cmd)
            : base(cmd)
        {
        }
        #endregion

        #region Public Surface
        /// <summary>
        /// Equivalent to OpCopier.Copy, only in addition it keeps track of the defining subtrees
        /// of collection vars defined in the subtree rooted at the copy of the input node n.
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="n"></param>
        /// <param name="varMap"></param>
        /// <param name="newCollectionVarDefinitions"></param>
        /// <returns></returns>
        internal static Node Copy(Command cmd, Node n, out VarMap varMap, out Dictionary<Var, Node> newCollectionVarDefinitions)
        {
            OpCopierTrackingCollectionVars oc = new OpCopierTrackingCollectionVars(cmd);
            Node newNode = oc.CopyNode(n);
            varMap = oc.m_varMap;
            newCollectionVarDefinitions = oc.m_newCollectionVarDefinitions;
            return newNode;
        }
        #endregion

        #region Visitor Members
        /// <summary>
        /// Tracks the collection vars after calling the base implementation
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(MultiStreamNestOp op, Node n)
        {
            Node result = base.Visit(op, n);
            MultiStreamNestOp newOp = (MultiStreamNestOp)result.Op;

            for (int i = 0; i < newOp.CollectionInfo.Count; i++)
            {
                m_newCollectionVarDefinitions.Add(newOp.CollectionInfo[i].CollectionVar, result.Children[i + 1]);
            }
            return result;
        }
        #endregion
    }
    #endregion

    #region Class SortRemover
    /// <summary>
    /// Removes all sort nodes from the given command except for the top most one 
    /// (the child of the root PhysicalProjectOp node) if any
    /// </summary>
    internal class SortRemover : BasicOpVisitorOfNode
    {
        #region Private members
        private Command m_command;

        /// <summary>
        /// The only sort node that should not be removed, if any
        /// </summary>
        private Node m_topMostSort = null;

        /// <summary>
        /// Keeps track of changed nodes to allow to only recompute node info when needed.
        /// </summary>
        private HashSet<Node> changedNodes = new HashSet<Node>();
        #endregion

        #region Constructor
        private SortRemover(Command command, Node topMostSort)
        {
            this.m_command = command;
            this.m_topMostSort = topMostSort;
        }
        #endregion

        #region Entry point
        internal static void Process(Command command)
        {
            Node topMostSort;
            if (command.Root.Child0 != null && command.Root.Child0.Op.OpType == OpType.Sort)
            {
                topMostSort = command.Root.Child0;
            }
            else
            {
                topMostSort = null;
            }
            SortRemover sortRemover = new SortRemover(command, topMostSort);
            command.Root = sortRemover.VisitNode(command.Root);
        }
        #endregion

        #region Visitor Helpers
        /// <summary>
        /// Iterates over all children.
        /// If any of the children changes, update the node info.
        /// This is safe to do because the only way a child can change is 
        /// if it is a sort node that needs to be removed. The nodes whose children have
        /// chagnged also get tracked.
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildren(Node n)
        {
            bool anyChanged = false;
            for (int i = 0; i < n.Children.Count; i++)
            {
                Node originalChild = n.Children[i];
                n.Children[i] = VisitNode(n.Children[i]);
                if (!Object.ReferenceEquals(originalChild, n.Children[i]) || changedNodes.Contains(originalChild))
                {
                    anyChanged = true;
                }
            }
            if (anyChanged)
            {
                m_command.RecomputeNodeInfo(n);
                changedNodes.Add(n);
            }
        }
        #endregion

        #region Visitors
        /// <summary>
        /// If the given node is not the top most SortOp node remove it. 
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(SortOp op, Node n)
        {
            VisitChildren(n);
            Node result;

            if (Object.ReferenceEquals(n, m_topMostSort))
            {
                result = n;
            }
            else
            {
                result = n.Child0;
            }
            return result;
        }
        #endregion
        #region
        #endregion
    }
    #endregion

}

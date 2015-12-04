//---------------------------------------------------------------------
// <copyright file="TransformationRules.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{
    internal class TransformationRulesContext : RuleProcessingContext
    {
        #region public methods and properties

        /// <summary>
        /// Whether any rule was applied that may have caused modifications such that projection pruning 
        /// may be useful
        /// </summary>
        internal bool ProjectionPrunningRequired { get { return this.m_projectionPrunningRequired; } }

        /// <summary>
        /// Whether any rule was applied that may have caused modifications such that reapplying
        /// the nullability rules may be useful
        /// </summary>
        internal bool ReapplyNullabilityRules { get { return this.m_reapplyNullabilityRules; } }

        /// <summary>
        /// Remap the given subree using the current remapper
        /// </summary>
        /// <param name="subTree"></param>
        internal void RemapSubtree(Node subTree)
        {
            this.m_remapper.RemapSubtree(subTree);
        }

        /// <summary>
        /// Adds a mapping from oldVar to newVar
        /// </summary>
        /// <param name="oldVar"></param>
        /// <param name="newVar"></param>
        internal void AddVarMapping(Var oldVar, Var newVar)
        {
            m_remapper.AddMapping(oldVar, newVar);
            m_remappedVars.Set(oldVar);
        }

        /// <summary>
        /// "Remap" an expression tree, replacing all references to vars in varMap with
        /// copies of the corresponding expression
        /// The subtree is modified *inplace* - it is the caller's responsibility to make
        /// a copy of the subtree if necessary. 
        /// The "replacement" expression (the replacement for the VarRef) is copied and then
        /// inserted into the appropriate location into the subtree. 
        /// 
        /// Note: we only support replacements in simple ScalarOp trees. This must be 
        /// validated by the caller.
        /// 
        /// </summary>
        /// <param name="node">Current subtree to process</param>
        /// <param name="varMap"></param>
        /// <returns>The updated subtree</returns>
        internal Node ReMap(Node node, Dictionary<Var, Node> varMap)
        {
            PlanCompiler.Assert(node.Op.IsScalarOp, "Expected a scalarOp: Found " + Dump.AutoString.ToString(node.Op.OpType));

            // Replace varRefOps by the corresponding expression in the map, if any
            if (node.Op.OpType == OpType.VarRef)
            {
                VarRefOp varRefOp = node.Op as VarRefOp;
                Node newNode = null;
                if (varMap.TryGetValue(varRefOp.Var, out newNode))
                {
                    newNode = this.Copy(newNode);
                    return newNode;
                }
                else
                {
                    return node;
                }
            }

            // Simply process the result of the children.
            for (int i = 0; i < node.Children.Count; i++)
            {
                node.Children[i] = ReMap(node.Children[i], varMap);
            }

            // We may have changed something deep down
            this.Command.RecomputeNodeInfo(node);
            return node;
        }

        /// <summary>
        /// Makes a copy of the appropriate subtree - with a simple accelerator for VarRefOp
        /// since that's likely to be the most command case
        /// </summary>
        /// <param name="node">the subtree to copy</param>
        /// <returns>the copy of the subtree</returns>
        internal Node Copy(Node node)
        {
            if (node.Op.OpType == OpType.VarRef)
            {
                VarRefOp op = node.Op as VarRefOp;
                return this.Command.CreateNode(this.Command.CreateVarRefOp(op.Var));
            }
            else
            {
                return OpCopier.Copy(this.Command, node);
            }
        }

        /// <summary>
        /// Checks to see if the current subtree only contains ScalarOps
        /// </summary>
        /// <param name="node">current subtree</param>
        /// <returns>true, if the subtree contains only ScalarOps</returns>
        internal bool IsScalarOpTree(Node node)
        {
            int nodeCount = 0;
            return IsScalarOpTree(node, null, ref nodeCount);
        }

        /// <summary>
        /// Is the given var guaranteed to be non-nullable with regards to the node
        /// that is currently being processed.
        /// True, if it is listed as such on any on the node infos on any of the 
        /// current relop ancestors.
        /// </summary>
        /// <param name="var"></param>
        /// <returns></returns>
        internal bool IsNonNullable(Var var)
        {
            foreach (Node relOpAncestor in m_relOpAncestors)
            {
                // Rules applied to the children of the relOpAncestor may have caused it change. 
                // Thus, if the node is used, it has to have its node info recomputed
                Command.RecomputeNodeInfo(relOpAncestor);
                ExtendedNodeInfo nodeInfo = Command.GetExtendedNodeInfo(relOpAncestor);
                if (nodeInfo.NonNullableVisibleDefinitions.IsSet(var))
                {
                    return true;
                }
                else if (nodeInfo.LocalDefinitions.IsSet(var))
                {
                    //The var is defined on this ancestor but is not non-nullable,
                    // therefore there is no need to further check other ancestors
                    return false;
                }
            }
            return false;
        }

        /// <summary>
        /// Is it safe to use a null sentinel with any value?
        /// It may not be safe if:
        /// 1. The top most sort includes null sentinels. If the null sentinel is replaced with a different value
        /// and is used as a sort key it may change the sorting results 
        /// 2. If any of the ancestors is Distinct, GroupBy, Intersect or Except,
        /// because the null sentinel may be used as a key.  
        /// 3. If the null sentinel is defined in the left child of an apply it may be used at the right side, 
        /// thus in these cases we also verify that the right hand side does not have any Distinct, GroupBy, 
        /// Intersect or Except.
        /// </summary>
        internal bool CanChangeNullSentinelValue
        {
            get
            {
                //Is there a sort that includes null sentinels
                if (this.m_compilerState.HasSortingOnNullSentinels)
                {
                    return false;
                }

                //Is any of the ancestors Distinct, GroupBy, Intersect or Except
                if (this.m_relOpAncestors.Any(a => IsOpNotSafeForNullSentinelValueChange(a.Op.OpType)))
                {
                    return false;
                }

                // Is the null sentinel defined in the left child of an apply and if so, 
                // does the right hand side have any Distinct, GroupBy, Intersect or Except.
                var applyAncestors = this.m_relOpAncestors.Where(a =>
                         a.Op.OpType == OpType.CrossApply ||
                         a.Op.OpType == OpType.OuterApply);

                //If the sentinel comes from the right hand side it is ok.
                foreach (Node applyAncestor in applyAncestors)
                {
                    if (!this.m_relOpAncestors.Contains(applyAncestor.Child1) && HasOpNotSafeForNullSentinelValueChange(applyAncestor.Child1))
                    {
                        return false;
                    }
                }
                return true;
            }
        }

        /// <summary>
        /// Is the op not safe for null sentinel value change
        /// </summary>
        /// <param name="optype"></param>
        /// <returns></returns>
        internal static bool IsOpNotSafeForNullSentinelValueChange(OpType optype)
        {
            return optype == OpType.Distinct ||
                    optype == OpType.GroupBy ||
                    optype == OpType.Intersect ||
                    optype == OpType.Except;
        }

        /// <summary>
        /// Does the given subtree contain a node with an op that
        /// is not safer for null sentinel value change
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static bool HasOpNotSafeForNullSentinelValueChange(Node n)
        {
            if (IsOpNotSafeForNullSentinelValueChange(n.Op.OpType))
            {
                return true;
            }
            foreach (Node child in n.Children)
            {
                if (HasOpNotSafeForNullSentinelValueChange(child))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Is this is a scalar-op tree? Also return a dictionary of var refcounts (ie)
        /// for each var encountered in the tree, determine the number of times it has
        /// been seen
        /// </summary>
        /// <param name="node">current subtree</param>
        /// <param name="varRefMap">dictionary of var refcounts to fill in</param>
        /// <returns></returns>
        internal bool IsScalarOpTree(Node node, Dictionary<Var, int> varRefMap)
        {
            PlanCompiler.Assert(varRefMap != null, "Null varRef map");

            int nodeCount = 0;
            return IsScalarOpTree(node, varRefMap, ref nodeCount);
        }

        /// <summary>
        /// Get a mapping from Var->Expression for a VarDefListOp tree. This information
        /// will be used by later stages to replace all references to the Vars by the 
        /// corresponding expressions
        /// 
        /// This function uses a few heuristics along the way. It uses the varRefMap
        /// parameter to determine if a computed Var (defined by this VarDefListOp)
        /// has been referenced multiple times, and if it has, it checks to see if
        /// the defining expression is too big (> 100 nodes). This is to avoid 
        /// bloating up the entire query tree with too many copies. 
        /// 
        /// </summary>
        /// <param name="varDefListNode">The varDefListOp subtree</param>
        /// <param name="varRefMap">ref counts for each referenced var</param>
        /// <returns>mapping from Var->replacement xpressions</returns>
        internal Dictionary<Var, Node> GetVarMap(Node varDefListNode, Dictionary<Var, int> varRefMap)
        {
            VarDefListOp varDefListOp = (VarDefListOp)varDefListNode.Op;

            Dictionary<Var, Node> varMap = new Dictionary<Var, Node>();
            foreach (Node chi in varDefListNode.Children)
            {
                VarDefOp varDefOp = (VarDefOp)chi.Op;
                int nonLeafNodeCount = 0;
                int refCount = 0;
                if (!IsScalarOpTree(chi.Child0, null, ref nonLeafNodeCount))
                {
                    return null;
                }
                //
                // More heuristics. If there are multiple references to this Var *and*
                // the defining expression for the Var is "expensive" (ie) has larger than
                // 100 nodes, then simply pretend that this is too hard to do
                // Note: we check for more than 2 references, (rather than just more than 1) - this
                // is simply to let some additional cases through
                // 
                if ((nonLeafNodeCount > 100) &&
                    (varRefMap != null) &&
                    varRefMap.TryGetValue(varDefOp.Var, out refCount) &&
                    (refCount > 2))
                {
                    return null;
                }

                Node n;
                if (varMap.TryGetValue(varDefOp.Var, out n))
                {
                    PlanCompiler.Assert(n == chi.Child0, "reusing varDef for different Node?");
                }
                else
                {
                    varMap.Add(varDefOp.Var, chi.Child0);
                }
            }

            return varMap;
        }

        /// <summary>
        /// Builds a NULLIF expression (ie) a Case expression that looks like
        ///    CASE WHEN v is null THEN null ELSE expr END
        /// where v is the conditionVar parameter, and expr is the value of the expression
        /// when v is non-null
        /// </summary>
        /// <param name="conditionVar">null discriminator var</param>
        /// <param name="expr">expression</param>
        /// <returns></returns>
        internal Node BuildNullIfExpression(Var conditionVar, Node expr)
        {
            VarRefOp varRefOp = this.Command.CreateVarRefOp(conditionVar);
            Node varRefNode = this.Command.CreateNode(varRefOp);
            Node whenNode = this.Command.CreateNode(this.Command.CreateConditionalOp(OpType.IsNull), varRefNode);
            Node elseNode = expr;
            Node thenNode = this.Command.CreateNode(this.Command.CreateNullOp(elseNode.Op.Type));
            Node caseNode = this.Command.CreateNode(this.Command.CreateCaseOp(elseNode.Op.Type), whenNode, thenNode, elseNode);

            return caseNode;
        }

        #region Rule Interactions
        /// <summary>
        /// Shut off filter pushdown for this subtree
        /// </summary>
        /// <param name="n"></param>
        internal void SuppressFilterPushdown(Node n)
        {
            m_suppressions[n] = n;
        }

        /// <summary>
        /// Is filter pushdown shut off for this subtree?
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        internal bool IsFilterPushdownSuppressed(Node n)
        {
            return m_suppressions.ContainsKey(n);
        }

        /// <summary>
        /// Given a list of vars try to get one that is of type Int32
        /// </summary>
        /// <param name="varList"></param>
        /// <param name="int32Var"></param>
        /// <returns></returns>
        internal static bool TryGetInt32Var(IEnumerable<Var> varList, out Var int32Var)
        {
            foreach (Var v in varList)
            {
                // Any Int32 var regardless of the fasets will do
                System.Data.Metadata.Edm.PrimitiveTypeKind typeKind;
                if (System.Data.Common.TypeHelpers.TryGetPrimitiveTypeKind(v.Type, out typeKind) && typeKind == System.Data.Metadata.Edm.PrimitiveTypeKind.Int32)
                {
                    int32Var = v;
                    return true;
                }
            }
            int32Var = null;
            return false;
        }

        #endregion

        #endregion

        #region constructors
        internal TransformationRulesContext(PlanCompiler compilerState)
            : base(compilerState.Command)
        {
            m_compilerState = compilerState;
            m_remapper = new VarRemapper(compilerState.Command);
            m_suppressions = new Dictionary<Node, Node>();
            m_remappedVars = compilerState.Command.CreateVarVec();
        }

        #endregion

        #region private state
        private readonly PlanCompiler m_compilerState;
        private readonly VarRemapper m_remapper;
        private readonly Dictionary<Node, Node> m_suppressions;
        private readonly VarVec m_remappedVars;
        private bool m_projectionPrunningRequired = false;
        private bool m_reapplyNullabilityRules = false;
        private Stack<Node> m_relOpAncestors = new Stack<Node>();
#if DEBUG
        /// <summary>
        /// Used to see all the applied rules. 
        /// One way to use it is to put a conditional breakpoint at the end of
        /// PostProcessSubTree with the condition m_relOpAncestors.Count == 0
        /// </summary>
        internal readonly System.Text.StringBuilder appliedRules = new System.Text.StringBuilder();
#endif
        #endregion

        #region RuleProcessingContext Overrides
        /// <summary>
        /// Callback function to invoke *before* rules are applied. 
        /// Calls the VarRemapper to update any Vars in this node, and recomputes 
        /// the nodeinfo
        /// </summary>
        /// <param name="n"></param>
        internal override void PreProcess(Node n)
        {
            m_remapper.RemapNode(n);
            Command.RecomputeNodeInfo(n);
        }

        /// <summary>
        /// Callback function to invoke *before* rules are applied. 
        /// Calls the VarRemapper to update any Vars in the entire subtree
        /// If the given node has a RelOp it is pushed on the relOp ancestors stack.
        /// </summary>
        /// <param name="subTree"></param>
        internal override void PreProcessSubTree(Node subTree)
        {
            if (subTree.Op.IsRelOp)
            {
                m_relOpAncestors.Push(subTree);
            }

            if (m_remappedVars.IsEmpty)
            {
                return;
            }

            NodeInfo nodeInfo = this.Command.GetNodeInfo(subTree);

            //We need to do remapping only if m_remappedVars overlaps with nodeInfo.ExternalReferences
            foreach (Var v in nodeInfo.ExternalReferences)
            {
                if (m_remappedVars.IsSet(v))
                {
                    m_remapper.RemapSubtree(subTree);
                    break;
                }
            }
        }

        /// <summary>
        /// If the given node has a RelOp it is popped from the relOp ancestors stack.
        /// </summary>
        /// <param name="subtree"></param>
        internal override void PostProcessSubTree(Node subtree)
        {
            if (subtree.Op.IsRelOp)
            {
                PlanCompiler.Assert(m_relOpAncestors.Count != 0, "The RelOp ancestors stack is empty when post processing a RelOp subtree");
                Node poppedNode = m_relOpAncestors.Pop();
                PlanCompiler.Assert(Object.ReferenceEquals(subtree, poppedNode), "The popped ancestor is not equal to the root of the subtree being post processed");
            }
        }

        /// <summary>
        /// Callback function to invoke *after* rules are applied
        /// Recomputes the node info, if this node has changed
        /// If the rule is among the rules after which projection pruning may be beneficial, 
        /// m_projectionPrunningRequired is set to true.
        /// If the rule is among the rules after which reapplying the nullability rules may be beneficial,
        /// m_reapplyNullabilityRules is set to true.
        /// </summary>
        /// <param name="n"></param>
        /// <param name="rule">the rule that was applied</param>
        internal override void PostProcess(Node n, InternalTrees.Rule rule)
        {
            if (rule != null)
            {
#if DEBUG
                appliedRules.Append(rule.MethodName);
                appliedRules.AppendLine();
#endif
                if (!this.m_projectionPrunningRequired && TransformationRules.RulesRequiringProjectionPruning.Contains(rule))
                {
                    this.m_projectionPrunningRequired = true;
                }
                if (!this.m_reapplyNullabilityRules && TransformationRules.RulesRequiringNullabilityRulesToBeReapplied.Contains(rule))
                {
                    this.m_reapplyNullabilityRules = true;
                }
                Command.RecomputeNodeInfo(n);
            }
        }

        /// <summary>
        /// Get the hash value for this subtree
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        internal override int GetHashCode(Node node)
        {
            NodeInfo nodeInfo = Command.GetNodeInfo(node);
            return nodeInfo.HashValue;
        }
        #endregion

        #region private methods
        /// <summary>
        /// Check to see if the current subtree is a scalar-op subtree (ie) does
        /// the subtree only comprise of scalarOps?
        /// Additionally, compute the number of non-leaf nodes (ie) nodes with at least one child
        /// that are found in the subtree. Note that this count is approximate - it is only
        /// intended to be used as a hint. It is the caller's responsibility to initialize
        /// nodeCount to a sane value on entry into this function
        /// And finally, if the varRefMap parameter is non-null, we keep track of 
        /// how often a Var is referenced within the subtree
        /// 
        /// The non-leaf-node count and the varRefMap are used by GetVarMap to determine
        /// if expressions can be composed together
        /// </summary>
        /// <param name="node">root of the subtree</param>
        /// <param name="varRefMap">Ref counts for each Var encountered in the subtree</param>
        /// <param name="nonLeafNodeCount">count of non-leaf nodes encountered in the subtree</param>
        /// <returns>true, if this node only contains scalarOps</returns>
        private bool IsScalarOpTree(Node node, Dictionary<Var, int> varRefMap, ref int nonLeafNodeCount)
        {
            if (!node.Op.IsScalarOp)
            {
                return false;
            }

            if (node.HasChild0)
            {
                nonLeafNodeCount++;
            }

            if (varRefMap != null && node.Op.OpType == OpType.VarRef)
            {
                VarRefOp varRefOp = (VarRefOp)node.Op;
                int refCount;
                if (!varRefMap.TryGetValue(varRefOp.Var, out refCount))
                {
                    refCount = 1;
                }
                else
                {
                    refCount++;
                }
                varRefMap[varRefOp.Var] = refCount;
            }

            foreach (Node chi in node.Children)
            {
                if (!IsScalarOpTree(chi, varRefMap, ref nonLeafNodeCount))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion
    }

    /// <summary>
    /// The list of all transformation rules to apply
    /// </summary>
    internal static class TransformationRules
    {
        /// <summary>
        /// A lookup table for built from all rules
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> AllRulesTable = BuildLookupTableForRules(AllRules);

        /// <summary>
        /// A lookup table for built only from ProjectRules
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> ProjectRulesTable = BuildLookupTableForRules(ProjectOpRules.Rules);


        /// <summary>
        /// A lookup table built only from rules that use key info
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> PostJoinEliminationRulesTable = BuildLookupTableForRules(PostJoinEliminationRules);

        /// <summary>
        /// A lookup table built only from rules that rely on nullability of vars and other rules 
        /// that may be able to perform simplificatios if these have been applied.
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        internal static readonly ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> NullabilityRulesTable = BuildLookupTableForRules(NullabilityRules);

        /// <summary>
        /// A look-up table of rules that may cause modifications such that projection pruning may be useful
        /// after they have been applied.
        /// </summary>
        internal static readonly HashSet<InternalTrees.Rule> RulesRequiringProjectionPruning = InitializeRulesRequiringProjectionPruning();

        /// <summary>
        /// A look-up table of rules that may cause modifications such that reapplying the nullability rules
        /// may be useful after they have been applied.
        /// </summary>
        internal static readonly HashSet<InternalTrees.Rule> RulesRequiringNullabilityRulesToBeReapplied = InitializeRulesRequiringNullabilityRulesToBeReapplied();


        #region private state maintenance
        private static List<InternalTrees.Rule> allRules;
        private static List<InternalTrees.Rule> AllRules
        {
            get
            {
                if (allRules == null)
                {
                    allRules = new List<InternalTrees.Rule>();
                    allRules.AddRange(ScalarOpRules.Rules);
                    allRules.AddRange(FilterOpRules.Rules);
                    allRules.AddRange(ProjectOpRules.Rules);
                    allRules.AddRange(ApplyOpRules.Rules);
                    allRules.AddRange(JoinOpRules.Rules);
                    allRules.AddRange(SingleRowOpRules.Rules);
                    allRules.AddRange(SetOpRules.Rules);
                    allRules.AddRange(GroupByOpRules.Rules);
                    allRules.AddRange(SortOpRules.Rules);
                    allRules.AddRange(ConstrainedSortOpRules.Rules);
                    allRules.AddRange(DistinctOpRules.Rules);
                }
                return allRules;
            }
        }

        private static List<InternalTrees.Rule> postJoinEliminationRules;
        private static List<InternalTrees.Rule> PostJoinEliminationRules
        {
            get
            {
                if (postJoinEliminationRules == null)
                {
                    postJoinEliminationRules = new List<InternalTrees.Rule>();
                    postJoinEliminationRules.AddRange(ProjectOpRules.Rules); //these don't use key info per-se, but can help after the distinct op rules.
                    postJoinEliminationRules.AddRange(DistinctOpRules.Rules);
                    postJoinEliminationRules.AddRange(FilterOpRules.Rules);
                    postJoinEliminationRules.AddRange(JoinOpRules.Rules);
                    postJoinEliminationRules.AddRange(NullabilityRules);
                }
                return postJoinEliminationRules;
            }
        }

        private static List<InternalTrees.Rule> nullabilityRules;
        private static List<InternalTrees.Rule> NullabilityRules
        {
            get
            {
                if (nullabilityRules == null)
                {
                    nullabilityRules = new List<InternalTrees.Rule>();
                    nullabilityRules.Add(ScalarOpRules.Rule_IsNullOverVarRef);
                    nullabilityRules.Add(ScalarOpRules.Rule_AndOverConstantPred1);
                    nullabilityRules.Add(ScalarOpRules.Rule_AndOverConstantPred2);
                    nullabilityRules.Add(ScalarOpRules.Rule_SimplifyCase);
                    nullabilityRules.Add(ScalarOpRules.Rule_NotOverConstantPred);
                }
                return nullabilityRules;
            }
        }

        private static ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> BuildLookupTableForRules(IEnumerable<InternalTrees.Rule> rules)
        {
            ReadOnlyCollection<InternalTrees.Rule> NoRules = new ReadOnlyCollection<InternalTrees.Rule>(new InternalTrees.Rule[0]);

            List<InternalTrees.Rule>[] lookupTable = new List<InternalTrees.Rule>[(int)OpType.MaxMarker];

            foreach (InternalTrees.Rule rule in rules)
            {
                List<InternalTrees.Rule> opRules = lookupTable[(int)rule.RuleOpType];
                if (opRules == null)
                {
                    opRules = new List<InternalTrees.Rule>();
                    lookupTable[(int)rule.RuleOpType] = opRules;
                }
                opRules.Add(rule);
            }

            ReadOnlyCollection<InternalTrees.Rule>[] rulesPerType = new ReadOnlyCollection<InternalTrees.Rule>[lookupTable.Length];
            for (int i = 0; i < lookupTable.Length; ++i)
            {
                if (null != lookupTable[i])
                {
                    rulesPerType[i] = new ReadOnlyCollection<InternalTrees.Rule>(lookupTable[i].ToArray());
                }
                else
                {
                    rulesPerType[i] = NoRules;
                }
            }
            return new ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>>(rulesPerType);
        }

        private static HashSet<InternalTrees.Rule> InitializeRulesRequiringProjectionPruning()
        {
            HashSet<InternalTrees.Rule> rulesRequiringProjectionPruning = new HashSet<InternalTrees.Rule>();

            rulesRequiringProjectionPruning.Add(ApplyOpRules.Rule_OuterApplyOverProject);

            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_CrossJoinOverProject1);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_CrossJoinOverProject2);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_InnerJoinOverProject1);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_InnerJoinOverProject2);
            rulesRequiringProjectionPruning.Add(JoinOpRules.Rule_OuterJoinOverProject2);

            rulesRequiringProjectionPruning.Add(ProjectOpRules.Rule_ProjectWithNoLocalDefs);

            rulesRequiringProjectionPruning.Add(FilterOpRules.Rule_FilterOverProject);
            rulesRequiringProjectionPruning.Add(FilterOpRules.Rule_FilterWithConstantPredicate);

            rulesRequiringProjectionPruning.Add(GroupByOpRules.Rule_GroupByOverProject);
            rulesRequiringProjectionPruning.Add(GroupByOpRules.Rule_GroupByOpWithSimpleVarRedefinitions);

            return rulesRequiringProjectionPruning;
        }

        private static HashSet<InternalTrees.Rule> InitializeRulesRequiringNullabilityRulesToBeReapplied()
        {
            HashSet<InternalTrees.Rule> rulesRequiringNullabilityRulesToBeReapplied = new HashSet<InternalTrees.Rule>();

            rulesRequiringNullabilityRulesToBeReapplied.Add(FilterOpRules.Rule_FilterOverLeftOuterJoin);

            return rulesRequiringNullabilityRulesToBeReapplied;
        }
        
        #endregion


        /// <summary>
        /// Apply the rules that belong to the specified group to the given query tree.
        /// </summary>
        /// <param name="compilerState"></param>
        /// <param name="rulesGroup"></param>
        internal static bool Process(PlanCompiler compilerState, TransformationRulesGroup rulesGroup)
        {
            ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> rulesTable = null;
            switch (rulesGroup)
            {
                case TransformationRulesGroup.All:
                    rulesTable = AllRulesTable;
                    break;
                case TransformationRulesGroup.PostJoinElimination:
                    rulesTable = PostJoinEliminationRulesTable;
                    break;
                case TransformationRulesGroup.Project:
                    rulesTable = ProjectRulesTable;
                    break;
            }
           
            // If any rule has been applied after which reapplying nullability rules may be useful,
            // reapply nullability rules.
            bool projectionPrunningRequired;
            if (Process(compilerState, rulesTable, out projectionPrunningRequired))
            {
                bool projectionPrunningRequired2;
                Process(compilerState, NullabilityRulesTable, out projectionPrunningRequired2);
                projectionPrunningRequired = projectionPrunningRequired || projectionPrunningRequired2;
            }
            return projectionPrunningRequired;
        }

        /// <summary>
        /// Apply the rules that belong to the specified rules table to the given query tree.
        /// </summary>
        /// <param name="compilerState"></param>
        /// <param name="rulesTable"></param>
        /// <param name="projectionPruningRequired">is projection pruning  required after the rule application</param>
        /// <returns>Whether any rule has been applied after which reapplying nullability rules may be useful</returns>
        private static bool Process(PlanCompiler compilerState, ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> rulesTable, out bool projectionPruningRequired)
        {
            RuleProcessor ruleProcessor = new RuleProcessor();
            TransformationRulesContext context = new TransformationRulesContext(compilerState);
            compilerState.Command.Root = ruleProcessor.ApplyRulesToSubtree(context, rulesTable, compilerState.Command.Root);
            projectionPruningRequired = context.ProjectionPrunningRequired;
            return context.ReapplyNullabilityRules;
        }
    }

    /// <summary>
    /// Available groups of rules, not necessarily mutually exclusive
    /// </summary>
    internal enum TransformationRulesGroup
    {
        All,
        Project,
        PostJoinElimination
    }

    #region ScalarOpRules
    /// <summary>
    /// Transformation rules for ScalarOps
    /// </summary>
    internal static class ScalarOpRules
    {
        #region CaseOp Rules
        internal static readonly SimpleRule Rule_SimplifyCase = new SimpleRule(OpType.Case, ProcessSimplifyCase);
        internal static readonly SimpleRule Rule_FlattenCase = new SimpleRule(OpType.Case, ProcessFlattenCase);
        /// <summary>
        /// We perform the following simple transformation for CaseOps. If every single
        /// then/else expression in the CaseOp is equivalent, then we can simply replace
        /// the Op with the first then/expression. Specifically,
        /// case when w1 then t1 when w2 then t2 ... when wn then tn else e end
        ///   => t1
        /// assuming that t1 is equivalent to t2 is equivalent to ... to e
        /// </summary>
        /// <param name="context">Rule Processing context</param>
        /// <param name="caseOpNode">The current subtree for the CaseOp</param>
        /// <param name="newNode">the (possibly) modified subtree</param>
        /// <returns>true, if we performed any transformations</returns>
        static bool ProcessSimplifyCase(RuleProcessingContext context, Node caseOpNode, out Node newNode)
        {
            CaseOp caseOp = (CaseOp)caseOpNode.Op;
            newNode = caseOpNode;

            //
            // Can I collapse the entire case-expression into a single expression - yes, 
            // if all the then/else clauses are the same expression
            //
            if (ProcessSimplifyCase_Collapse(caseOp, caseOpNode, out newNode))
            {
                return true;
            }

            //
            // Can I remove any unnecessary when-then pairs ?
            //
            if (ProcessSimplifyCase_EliminateWhenClauses(context, caseOp, caseOpNode, out newNode))
            {
                return true;
            }

            // Nothing else I can think of
            return false;
        }

        /// <summary>
        /// Try and collapse the case expression into a single expression. 
        /// If every single then/else expression in the CaseOp is equivalent, then we can 
        /// simply replace the CaseOp with the first then/expression. Specifically,
        /// case when w1 then t1 when w2 then t2 ... when wn then tn else e end
        ///   => t1
        ///  if t1 is equivalent to t2 is equivalent to ... to e
        /// </summary>
        /// <param name="caseOp">the current caseOp</param>
        /// <param name="caseOpNode">current subtree</param>
        /// <param name="newNode">new subtree</param>
        /// <returns>true, if we performed a transformation</returns>
        private static bool ProcessSimplifyCase_Collapse(CaseOp caseOp, Node caseOpNode, out Node newNode)
        {
            newNode = caseOpNode;
            Node firstThenNode = caseOpNode.Child1;
            Node elseNode = caseOpNode.Children[caseOpNode.Children.Count - 1];
            if (!firstThenNode.IsEquivalent(elseNode))
            {
                return false;
            }
            for (int i = 3; i < caseOpNode.Children.Count - 1; i += 2)
            {
                if (!caseOpNode.Children[i].IsEquivalent(firstThenNode))
                {
                    return false;
                }
            }

            // All nodes are equivalent - simply return the first then node
            newNode = firstThenNode;
            return true;
        }

        /// <summary>
        /// Try and remove spurious branches from the case expression. 
        /// If any of the WHEN clauses is the 'FALSE' expression, simply remove that 
        /// branch (when-then pair) from the case expression.
        /// If any of the WHEN clauses is the 'TRUE' expression, then all branches to the 
        /// right of it are irrelevant - eliminate them. Eliminate this branch as well, 
        /// and make the THEN expression of this branch the ELSE expression for the entire
        /// Case expression. If the WHEN expression represents the first branch, then 
        /// replace the entire case expression by the corresponding THEN expression
        /// </summary>
        /// <param name="context">rule processing context</param>
        /// <param name="caseOp">current caseOp</param>
        /// <param name="caseOpNode">Current subtree</param>
        /// <param name="newNode">the new subtree</param>
        /// <returns>true, if there was a transformation</returns>
        private static bool ProcessSimplifyCase_EliminateWhenClauses(RuleProcessingContext context, CaseOp caseOp, Node caseOpNode, out Node newNode)
        {
            List<Node> newNodeArgs = null;
            newNode = caseOpNode;

            for (int i = 0; i < caseOpNode.Children.Count; )
            {
                // Special handling for the else clause
                if (i == caseOpNode.Children.Count - 1)
                {
                    // If the else clause is a SoftCast then we do not attempt to simplify
                    // the case operation, since this may change the result type.
                    // This really belongs in more general SoftCastOp logic in the CTreeGenerator
                    // that converts SoftCasts that could affect the result type of the query into
                    // a real cast or a trivial case statement, to preserve the result type.
                    // This is tracked by SQL PT Work Item #300003327.
                    if (OpType.SoftCast == caseOpNode.Children[i].Op.OpType)
                    {
                        return false;
                    }

                    if (newNodeArgs != null)
                    {
                        newNodeArgs.Add(caseOpNode.Children[i]);
                    }
                    break;
                }

                // If the current then clause is a SoftCast then we do not attempt to simplify
                // the case operation, since this may change the result type.
                // Again, this really belongs in the CTreeGenerator as per SQL PT Work Item #300003327.
                if (OpType.SoftCast == caseOpNode.Children[i + 1].Op.OpType)
                {
                    return false;
                }

                // Check to see if the when clause is a ConstantPredicate
                if (caseOpNode.Children[i].Op.OpType != OpType.ConstantPredicate)
                {
                    if (newNodeArgs != null)
                    {
                        newNodeArgs.Add(caseOpNode.Children[i]);
                        newNodeArgs.Add(caseOpNode.Children[i + 1]);
                    }
                    i += 2;
                    continue;
                }

                // Found a when-clause which is a constant predicate
                ConstantPredicateOp constPred = (ConstantPredicateOp)caseOpNode.Children[i].Op;
                // Create the newArgs list, if we haven't done so already
                if (newNodeArgs == null)
                {
                    newNodeArgs = new List<Node>();
                    for (int j = 0; j < i; j++)
                    {
                        newNodeArgs.Add(caseOpNode.Children[j]);
                    }
                }

                // If the when-clause is the "true" predicate, then we simply ignore all
                // the succeeding arguments. We make the "then" clause of this when-clause
                // as the "else-clause" of the resulting caseOp
                if (constPred.IsTrue)
                {
                    newNodeArgs.Add(caseOpNode.Children[i + 1]);
                    break;
                }
                else
                {
                    // Otherwise, we simply skip the when-then pair
                    PlanCompiler.Assert(constPred.IsFalse, "constant predicate must be either true or false");
                    i += 2;
                    continue;
                }
            }

            // Did we see any changes? Simply return
            if (newNodeArgs == null)
            {
                return false;
            }

            // Otherwise, we did do some processing
            PlanCompiler.Assert(newNodeArgs.Count > 0, "new args list must not be empty");
            // Is there only one expression in the args list - simply return that expression
            if (newNodeArgs.Count == 1)
            {
                newNode = newNodeArgs[0];
            }
            else
            {
                newNode = context.Command.CreateNode(caseOp, newNodeArgs);
            }

            return true;
        }

        /// <summary>
        /// If the else clause of the CaseOp is another CaseOp, when two can be collapsed into one. 
        /// In particular, 
        /// 
        /// CASE 
        ///     WHEN W1 THEN T1 
        ///     WHEN W2 THEN T2 ... 
        ///     ELSE (CASE 
        ///             WHEN WN1 THEN TN1, … 
        ///             ELSE E) 
        ///             
        /// Is transformed into 
        /// 
        /// CASE 
        ///     WHEN W1 THEN T1 
        ///     WHEN W2 THEN T2 ...
        ///     WHEN WN1  THEN TN1 ...
        ///     ELSE E
        /// </summary>
        /// <param name="caseOp">the current caseOp</param>
        /// <param name="caseOpNode">current subtree</param>
        /// <param name="newNode">new subtree</param>
        /// <returns>true, if we performed a transformation</returns>
        static bool ProcessFlattenCase(RuleProcessingContext context, Node caseOpNode, out Node newNode)
        {
            newNode = caseOpNode;
            Node elseChild = caseOpNode.Children[caseOpNode.Children.Count - 1];
            if (elseChild.Op.OpType != OpType.Case)
            {
                return false;
            }

            // 
            // Flatten the case statements.
            // The else child is removed from the outer CaseOp op
            // and the else child's children are reparented to the outer CaseOp
            // Node info recomputation does not need to happen, the outer CaseOp
            // node still has the same descendants.
            //
            caseOpNode.Children.RemoveAt(caseOpNode.Children.Count - 1);
            caseOpNode.Children.AddRange(elseChild.Children);

            return true;
        }

        #endregion

        #region EqualsOverConstant Rules
        internal static readonly PatternMatchRule Rule_EqualsOverConstant =
            new PatternMatchRule(new Node(ComparisonOp.PatternEq,
                                          new Node(InternalConstantOp.Pattern),
                                          new Node(InternalConstantOp.Pattern)),
                                 ProcessComparisonsOverConstant);
        /// <summary>
        /// Convert an Equals(X, Y) to a "true" predicate if X=Y, or a "false" predicate if X!=Y
        /// Convert a NotEquals(X,Y) in the reverse fashion
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="node">current node</param>
        /// <param name="newNode">possibly modified subtree</param>
        /// <returns>true, if transformation was successful</returns>
        static bool ProcessComparisonsOverConstant(RuleProcessingContext context, Node node, out Node newNode)
        {
            newNode = node;
            PlanCompiler.Assert(node.Op.OpType == OpType.EQ || node.Op.OpType == OpType.NE, "unexpected comparison op type?");

            bool? comparisonStatus = node.Child0.Op.IsEquivalent(node.Child1.Op);
            // Don't mess with nulls or with non-internal constants
            if (comparisonStatus == null)
            {
                return false;
            }
            bool result = (node.Op.OpType == OpType.EQ) ? (bool)comparisonStatus : !((bool)comparisonStatus);
            ConstantPredicateOp newOp = context.Command.CreateConstantPredicateOp(result);
            newNode = context.Command.CreateNode(newOp);
            return true;
        }
        #endregion

        #region LikeOp Rules
        private static bool? MatchesPattern(string str, string pattern)
        {
            // What we're trying to see is if the pattern is something that ends with a '%'
            // And if the "str" is something that matches everything before that

            // Make sure that the terminal character of the pattern is a '%' character. Also
            // ensure that this character does not occur anywhere else. And finally, ensure
            // that the pattern is atmost one character longer than the string itself
            int wildCardIndex = pattern.IndexOf('%');
            if ((wildCardIndex == -1) ||
                (wildCardIndex != pattern.Length - 1) ||
                (pattern.Length > str.Length + 1))
            {
                return null;
            }

            bool match = true;

            int i = 0;
            for (i = 0; i < str.Length && i < pattern.Length - 1; i++)
            {
                if (pattern[i] != str[i])
                {
                    match = false;
                    break;
                }
            }

            return match;
        }

        internal static readonly PatternMatchRule Rule_LikeOverConstants =
            new PatternMatchRule(new Node(LikeOp.Pattern,
                                          new Node(InternalConstantOp.Pattern),
                                          new Node(InternalConstantOp.Pattern),
                                          new Node(NullOp.Pattern)),
                                 ProcessLikeOverConstant);
        static bool ProcessLikeOverConstant(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            InternalConstantOp patternOp = (InternalConstantOp)n.Child1.Op;
            InternalConstantOp strOp = (InternalConstantOp)n.Child0.Op;

            string str = (string)strOp.Value;
            string pattern = (string)patternOp.Value;

            bool? match = MatchesPattern((string)strOp.Value, (string)patternOp.Value);
            if (match == null)
            {
                return false;
            }

            ConstantPredicateOp constOp = context.Command.CreateConstantPredicateOp((bool)match);
            newNode = context.Command.CreateNode(constOp);
            return true;
        }

        #endregion

        #region LogicalOp (and,or,not) Rules
        internal static readonly PatternMatchRule Rule_AndOverConstantPred1 =
            new PatternMatchRule(new Node(ConditionalOp.PatternAnd,
                                          new Node(LeafOp.Pattern),
                                          new Node(ConstantPredicateOp.Pattern)),
                                 ProcessAndOverConstantPredicate1);
        internal static readonly PatternMatchRule Rule_AndOverConstantPred2 =
            new PatternMatchRule(new Node(ConditionalOp.PatternAnd,
                                          new Node(ConstantPredicateOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessAndOverConstantPredicate2);
        internal static readonly PatternMatchRule Rule_OrOverConstantPred1 =
            new PatternMatchRule(new Node(ConditionalOp.PatternOr,
                                          new Node(LeafOp.Pattern),
                                          new Node(ConstantPredicateOp.Pattern)),
                                 ProcessOrOverConstantPredicate1);
        internal static readonly PatternMatchRule Rule_OrOverConstantPred2 =
            new PatternMatchRule(new Node(ConditionalOp.PatternOr,
                                          new Node(ConstantPredicateOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessOrOverConstantPredicate2);
        internal static readonly PatternMatchRule Rule_NotOverConstantPred =
            new PatternMatchRule(new Node(ConditionalOp.PatternNot,
                                          new Node(ConstantPredicateOp.Pattern)),
                                 ProcessNotOverConstantPredicate);
        /// <summary>
        /// Transform 
        ///   AND(x, true) => x;
        ///   AND(true, x) => x
        ///   AND(x, false) => false
        ///   AND(false, x) => false
        /// 
        /// </summary>
        /// <param name="context">Rule Processing context</param>
        /// <param name="node">Current LogOp (And, Or, Not) node</param>
        /// <param name="constantPredicateNode">constant predicate node</param>
        /// <param name="otherNode">The other child of the LogOp (possibly null)</param>
        /// <param name="newNode">new subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessLogOpOverConstant(RuleProcessingContext context, Node node,
            Node constantPredicateNode, Node otherNode,
            out Node newNode)
        {
            PlanCompiler.Assert(constantPredicateNode != null, "null constantPredicateOp?");
            ConstantPredicateOp pred = (ConstantPredicateOp)constantPredicateNode.Op;

            switch (node.Op.OpType)
            {
                case OpType.And:
                    newNode = pred.IsTrue ? otherNode : constantPredicateNode;
                    break;
                case OpType.Or:
                    newNode = pred.IsTrue ? constantPredicateNode : otherNode;
                    break;
                case OpType.Not:
                    PlanCompiler.Assert(otherNode == null, "Not Op with more than 1 child. Gasp!");
                    newNode = context.Command.CreateNode(context.Command.CreateConstantPredicateOp(!pred.Value));
                    break;
                default:
                    PlanCompiler.Assert(false, "Unexpected OpType - " + node.Op.OpType);
                    newNode = null;
                    break;
            }
            return true;
        }

        static bool ProcessAndOverConstantPredicate1(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child1, node.Child0, out newNode);
        }
        static bool ProcessAndOverConstantPredicate2(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child0, node.Child1, out newNode);
        }
        static bool ProcessOrOverConstantPredicate1(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child1, node.Child0, out newNode);
        }
        static bool ProcessOrOverConstantPredicate2(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child0, node.Child1, out newNode);
        }
        static bool ProcessNotOverConstantPredicate(RuleProcessingContext context, Node node, out Node newNode)
        {
            return ProcessLogOpOverConstant(context, node, node.Child0, null, out newNode);
        }
        #endregion

        #region IsNull Rules
        internal static readonly PatternMatchRule Rule_IsNullOverConstant =
            new PatternMatchRule(new Node(ConditionalOp.PatternIsNull,
                                          new Node(InternalConstantOp.Pattern)),
                                 ProcessIsNullOverConstant);
        internal static readonly PatternMatchRule Rule_IsNullOverNullSentinel =
            new PatternMatchRule(new Node(ConditionalOp.PatternIsNull,
                                          new Node(NullSentinelOp.Pattern)),
                                 ProcessIsNullOverConstant);
        /// <summary>
        /// Convert a 
        ///    IsNull(constant) 
        /// to just the 
        ///    False predicate
        /// </summary>
        /// <param name="context"></param>
        /// <param name="isNullNode"></param>
        /// <param name="newNode">new subtree</param>
        /// <returns></returns>
        static bool ProcessIsNullOverConstant(RuleProcessingContext context, Node isNullNode, out Node newNode)
        {
            newNode = context.Command.CreateNode(context.Command.CreateFalseOp());
            return true;
        }

        internal static readonly PatternMatchRule Rule_IsNullOverNull =
            new PatternMatchRule(new Node(ConditionalOp.PatternIsNull,
                                          new Node(NullOp.Pattern)),
                         ProcessIsNullOverNull);
        /// <summary>
        /// Convert an IsNull(null) to just the 'true' predicate
        /// </summary>
        /// <param name="context"></param>
        /// <param name="isNullNode"></param>
        /// <param name="newNode">new subtree</param>
        /// <returns></returns>
        static bool ProcessIsNullOverNull(RuleProcessingContext context, Node isNullNode, out Node newNode)
        {
            newNode = context.Command.CreateNode(context.Command.CreateTrueOp());
            return true;
        }
        #endregion

        #region CastOp(NullOp) Rule
        internal static readonly PatternMatchRule Rule_NullCast = new PatternMatchRule(
                                                            new Node(CastOp.Pattern,
                                                                    new Node(NullOp.Pattern)),
                                                            ProcessNullCast);

        /// <summary>
        /// eliminates nested null casts into a single cast of the outermost cast type.
        /// basically the transformation applied is: cast(null[x] as T) => null[t]
        /// </summary>
        /// <param name="context"></param>
        /// <param name="castNullOp"></param>
        /// <param name="newNode">modified subtree</param>
        /// <returns></returns>
        static bool ProcessNullCast(RuleProcessingContext context, Node castNullOp, out Node newNode)
        {
            newNode = context.Command.CreateNode(context.Command.CreateNullOp(castNullOp.Op.Type));
            return true;
        }
        #endregion

        #region IsNull over VarRef
        internal static readonly PatternMatchRule Rule_IsNullOverVarRef =
            new PatternMatchRule(new Node(ConditionalOp.PatternIsNull,
                                          new Node(VarRefOp.Pattern)),
                                 ProcessIsNullOverVarRef);
        /// <summary>
        /// Convert a 
        ///    IsNull(VarRef(v)) 
        /// to just the 
        ///    False predicate
        ///    
        /// if v is guaranteed to be non nullable.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="isNullNode"></param>
        /// <param name="newNode">new subtree</param>
        /// <returns></returns>
        static bool ProcessIsNullOverVarRef(RuleProcessingContext context, Node isNullNode, out Node newNode)
        {
            Command command = context.Command;
            TransformationRulesContext trc = (TransformationRulesContext)context;

            Var v = ((VarRefOp)isNullNode.Child0.Op).Var;
                    
            if (trc.IsNonNullable(v))
            {

                newNode = command.CreateNode(context.Command.CreateFalseOp());
                return true;
            }
            else
            {
                newNode = isNullNode;
                return false;
            }
        }
        #endregion 

        #region All ScalarOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
            Rule_SimplifyCase,
            Rule_FlattenCase,
            Rule_LikeOverConstants,
            Rule_EqualsOverConstant,
            Rule_AndOverConstantPred1,
            Rule_AndOverConstantPred2,
            Rule_OrOverConstantPred1,
            Rule_OrOverConstantPred2,
            Rule_NotOverConstantPred,
            Rule_IsNullOverConstant,
            Rule_IsNullOverNullSentinel,
            Rule_IsNullOverNull,
            Rule_NullCast,
            Rule_IsNullOverVarRef,
        };
        #endregion
    }
    #endregion

    #region Filter Rules
    /// <summary>
    /// Transformation rules for FilterOps
    /// </summary>
    internal static class FilterOpRules
    {
        #region Helpers
        /// <summary>
        /// Split up a predicate into 2 parts - the pushdown and the non-pushdown predicate. 
        /// 
        /// If the filter node has no external references *and* the "columns" parameter is null,
        /// then the entire predicate can be pushed down
        /// 
        /// We then compute the set of valid column references - if the "columns" parameter
        /// is non-null, this set is used. Otherwise, we get the definitions of the 
        /// input relop node of the filterOp, and use that.
        /// 
        /// We use this list of valid column references to identify which parts of the filter
        /// predicate can be pushed down - only those parts of the predicate that do not 
        /// reference anything beyond these columns are considered for pushdown. The rest are
        /// stuffed into the nonPushdownPredicate output parameter
        /// 
        /// </summary>
        /// <param name="command">Command object</param>
        /// <param name="filterNode">the FilterOp subtree</param>
        /// <param name="columns">(Optional) List of columns to consider for "pushdown"</param>
        /// <param name="nonPushdownPredicateNode">(output) Part of the predicate that cannot be pushed down</param>
        /// <returns>part of the predicate that can be pushed down</returns>
        private static Node GetPushdownPredicate(Command command, Node filterNode, VarVec columns, out Node nonPushdownPredicateNode)
        {
            Node pushdownPredicateNode = filterNode.Child1;
            nonPushdownPredicateNode = null;
            ExtendedNodeInfo filterNodeInfo = command.GetExtendedNodeInfo(filterNode);
            if (columns == null && filterNodeInfo.ExternalReferences.IsEmpty)
            {
                return pushdownPredicateNode;
            }

            if (columns == null)
            {
                ExtendedNodeInfo inputNodeInfo = command.GetExtendedNodeInfo(filterNode.Child0);
                columns = inputNodeInfo.Definitions;
            }

            Predicate predicate = new Predicate(command, pushdownPredicateNode);
            Predicate nonPushdownPredicate;
            predicate = predicate.GetSingleTablePredicates(columns, out nonPushdownPredicate);
            pushdownPredicateNode = predicate.BuildAndTree();
            nonPushdownPredicateNode = nonPushdownPredicate.BuildAndTree();
            return pushdownPredicateNode;
        }

        #endregion

        #region FilterOverFilter
        internal static readonly PatternMatchRule Rule_FilterOverFilter =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessFilterOverFilter);
        /// <summary>
        /// Convert Filter(Filter(X, p1), p2) => Filter(X, (p1 and p2))
        /// </summary>
        /// <param name="context">rule processing context</param>
        /// <param name="filterNode">FilterOp node</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>transformed subtree</returns>
        static bool ProcessFilterOverFilter(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            Node newAndNode = context.Command.CreateNode(
                context.Command.CreateConditionalOp(OpType.And),
                filterNode.Child0.Child1, filterNode.Child1);

            newNode = context.Command.CreateNode(context.Command.CreateFilterOp(), filterNode.Child0.Child0, newAndNode);
            return true;
        }
        #endregion

        #region FilterOverProject
        internal static readonly PatternMatchRule Rule_FilterOverProject =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessFilterOverProject);
        /// <summary>
        /// Convert Filter(Project(X, ...), p) => Project(Filter(X, p'), ...)
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="filterNode">FilterOp subtree</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>transformed subtree</returns>
        static bool ProcessFilterOverProject(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            Node predicateNode = filterNode.Child1;

            //
            // If the filter is a constant predicate, then don't push the filter below the
            // project
            //
            if (predicateNode.Op.OpType == OpType.ConstantPredicate)
            {
                // There's a different rule to process this case. Simply return
                return false;
            }

            TransformationRulesContext trc = (TransformationRulesContext)context;
            //
            // check to see that this is a simple predicate
            //
            Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
            if (!trc.IsScalarOpTree(predicateNode, varRefMap))
            {
                return false;
            }
            //
            // check to see if all expressions in the project can be inlined
            //
            Node projectNode = filterNode.Child0;
            Dictionary<Var, Node> varMap = trc.GetVarMap(projectNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false;
            }

            //
            // Try to remap the predicate in terms of the definitions of the Vars
            //
            Node remappedPredicateNode = trc.ReMap(predicateNode, varMap);

            //
            // Now push the filter below the project
            //
            Node newFilterNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), projectNode.Child0, remappedPredicateNode);
            Node newProjectNode = trc.Command.CreateNode(projectNode.Op, newFilterNode, projectNode.Child1);

            newNode = newProjectNode;
            return true;
        }
        #endregion

        #region FilterOverSetOp
        internal static readonly PatternMatchRule Rule_FilterOverUnionAll =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                          new Node(UnionAllOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessFilterOverSetOp);
        internal static readonly PatternMatchRule Rule_FilterOverIntersect =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                          new Node(IntersectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessFilterOverSetOp);
        internal static readonly PatternMatchRule Rule_FilterOverExcept =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                          new Node(ExceptOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessFilterOverSetOp);
        /// <summary>
        /// Transform Filter(UnionAll(X1, X2), p) => UnionAll(Filter(X1, p1), Filter(X, p2))
        ///           Filter(Intersect(X1, X2), p) => Intersect(Filter(X1, p1), Filter(X2, p2))
        ///           Filter(Except(X1, X2), p) => Except(Filter(X1, p1), X2)
        /// where p1 and p2 are the "mapped" versions of the predicate "p" for each branch
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="filterNode">FilterOp subtree</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>true, if successful transformation</returns>
        static bool ProcessFilterOverSetOp(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            TransformationRulesContext trc = (TransformationRulesContext)context;

            //
            // Identify parts of the filter predicate that can be pushed down, and parts that
            // cannot be. If nothing can be pushed down, then return
            // 
            Node nonPushdownPredicate;
            Node pushdownPredicate = GetPushdownPredicate(trc.Command, filterNode, null, out nonPushdownPredicate);
            if (pushdownPredicate == null)
            {
                return false;
            }
            // Handle only simple predicates
            if (!trc.IsScalarOpTree(pushdownPredicate))
            {
                return false;
            }

            //
            // Now push the predicate (the part that can be pushed down) into each of the
            // branches (as appropriate)
            // 
            Node setOpNode = filterNode.Child0;
            SetOp setOp = (SetOp)setOpNode.Op;
            List<Node> newSetOpChildren = new List<Node>();
            int branchId = 0;
            foreach (VarMap varMap in setOp.VarMap)
            {
                // For exceptOp, the filter should only be pushed below the zeroth child
                if (setOp.OpType == OpType.Except && branchId == 1)
                {
                    newSetOpChildren.Add(setOpNode.Child1);
                    break;
                }

                Dictionary<Var, Node> remapMap = new Dictionary<Var, Node>();
                foreach (KeyValuePair<Var, Var> kv in varMap)
                {
                    Node varRefNode = trc.Command.CreateNode(trc.Command.CreateVarRefOp(kv.Value));
                    remapMap.Add(kv.Key, varRefNode);
                }

                //
                // Now fix up the predicate.
                // Make a copy of the predicate first - except if we're dealing with the last
                // branch, in which case, we can simply reuse the predicate
                //
                Node predicateNode = pushdownPredicate;
                if (branchId == 0 && filterNode.Op.OpType != OpType.Except)
                {
                    predicateNode = trc.Copy(predicateNode);
                }
                Node newPredicateNode = trc.ReMap(predicateNode, remapMap);
                trc.Command.RecomputeNodeInfo(newPredicateNode);

                // create a new filter node below the setOp child
                Node newFilterNode = trc.Command.CreateNode(
                    trc.Command.CreateFilterOp(),
                    setOpNode.Children[branchId],
                    newPredicateNode);
                newSetOpChildren.Add(newFilterNode);

                branchId++;
            }
            Node newSetOpNode = trc.Command.CreateNode(setOpNode.Op, newSetOpChildren);

            //
            // We've now pushed down the relevant parts of the filter below the SetOps
            // We may still however some predicates left over - create a new filter node
            // to account for that
            // 
            if (nonPushdownPredicate != null)
            {
                newNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), newSetOpNode, nonPushdownPredicate);
            }
            else
            {
                newNode = newSetOpNode;
            }
            return true;
        }
        #endregion

        #region FilterOverDistinct
        internal static readonly PatternMatchRule Rule_FilterOverDistinct =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                  new Node(DistinctOp.Pattern,
                                           new Node(LeafOp.Pattern)),
                                  new Node(LeafOp.Pattern)),
                         ProcessFilterOverDistinct);
        /// <summary>
        /// Transforms Filter(Distinct(x), p) => Filter(Distinct(Filter(X, p1), p2)
        ///    where p2 is the part of the filter that can be pushed down, while p1 represents
        ///    any external references
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="filterNode">FilterOp subtree</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessFilterOverDistinct(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            //
            // Split up the filter predicate into two parts - the part that can be pushed down
            // and the part that can't. If there is no part that can be pushed down, simply return
            // 
            Node nonPushdownPredicate;
            Node pushdownPredicate = GetPushdownPredicate(context.Command, filterNode, null, out nonPushdownPredicate);
            if (pushdownPredicate == null)
            {
                return false;
            }

            //
            // Create a new filter node below the current distinct node for the predicate
            // that can be pushed down - create a new distinct node as well
            // 
            Node distinctNode = filterNode.Child0;
            Node pushdownFilterNode = context.Command.CreateNode(context.Command.CreateFilterOp(), distinctNode.Child0, pushdownPredicate);
            Node newDistinctNode = context.Command.CreateNode(distinctNode.Op, pushdownFilterNode);

            //
            // If we have a predicate part that cannot be pushed down, build up a new 
            // filter node above the new Distinct op that we just created
            // 
            if (nonPushdownPredicate != null)
            {
                newNode = context.Command.CreateNode(context.Command.CreateFilterOp(), newDistinctNode, nonPushdownPredicate);
            }
            else
            {
                newNode = newDistinctNode;
            }
            return true;
        }
        #endregion

        #region FilterOverGroupBy
        internal static readonly PatternMatchRule Rule_FilterOverGroupBy =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                  new Node(GroupByOp.Pattern,
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern)),
                                  new Node(LeafOp.Pattern)),
                         ProcessFilterOverGroupBy);
        /// <summary>
        /// Transforms Filter(GroupBy(X, k1.., a1...), p) => 
        ///            Filter(GroupBy(Filter(X, p1'), k1..., a1...), p2)
        ///   p1 and p2 represent the parts of p that can and cannot be pushed down 
        ///    respectively - specifically, p1 must only reference the key columns from
        ///    the GroupByOp. 
        ///   "p1'" is the mapped version of "p1", 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="filterNode">Current FilterOp subtree</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessFilterOverGroupBy(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            Node groupByNode = filterNode.Child0;
            GroupByOp groupByOp = (GroupByOp)groupByNode.Op;
            TransformationRulesContext trc = (TransformationRulesContext)context;

            // Check to see that we have a simple predicate
            Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
            if (!trc.IsScalarOpTree(filterNode.Child1, varRefMap))
            {
                return false;
            }

            // 
            // Split up the predicate into two parts - the part that can be pushed down below
            // the groupByOp (specifically, the part that only refers to keys of the groupByOp),
            // and the part that cannot be pushed below
            // If nothing can be pushed below, quit now
            // 
            Node nonPushdownPredicate;
            Node pushdownPredicate = GetPushdownPredicate(context.Command, filterNode, groupByOp.Keys, out nonPushdownPredicate);
            if (pushdownPredicate == null)
            {
                return false;
            }

            //
            // We need to push the filter down; but we need to remap the predicate, so
            // that any references to variables defined locally by the groupBy are fixed up
            // Make sure that the predicate is not too complex to remap
            //
            Dictionary<Var, Node> varMap = trc.GetVarMap(groupByNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false; // complex expressions
            }
            Node remappedPushdownPredicate = trc.ReMap(pushdownPredicate, varMap);

            //
            // Push the filter below the groupBy now
            //
            Node subFilterNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), groupByNode.Child0, remappedPushdownPredicate);
            Node newGroupByNode = trc.Command.CreateNode(groupByNode.Op, subFilterNode, groupByNode.Child1, groupByNode.Child2);

            //
            // If there was any part of the original predicate that could not be pushed down,
            // create a new filterOp node above the new groupBy node to represent that 
            // predicate
            //
            if (nonPushdownPredicate == null)
            {
                newNode = newGroupByNode;
            }
            else
            {
                newNode = trc.Command.CreateNode(trc.Command.CreateFilterOp(), newGroupByNode, nonPushdownPredicate);
            }
            return true;
        }
        #endregion

        #region FilterOverJoin
        internal static readonly PatternMatchRule Rule_FilterOverCrossJoin =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                  new Node(CrossJoinOp.Pattern,
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern)),
                                  new Node(LeafOp.Pattern)),
                         ProcessFilterOverJoin);
        internal static readonly PatternMatchRule Rule_FilterOverInnerJoin =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                  new Node(InnerJoinOp.Pattern,
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern)),
                                  new Node(LeafOp.Pattern)),
                         ProcessFilterOverJoin);
        internal static readonly PatternMatchRule Rule_FilterOverLeftOuterJoin =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                  new Node(LeftOuterJoinOp.Pattern,
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern)),
                                  new Node(LeafOp.Pattern)),
                         ProcessFilterOverJoin);
        /// <summary>
        /// Transform Filter()
        /// </summary>
        /// <param name="context">Rule Processing context</param>
        /// <param name="filterNode">Current FilterOp subtree</param>
        /// <param name="newNode">Modified subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessFilterOverJoin(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            TransformationRulesContext trc = (TransformationRulesContext)context;

            //
            // Have we shut off filter pushdown for this node? Return
            //
            if (trc.IsFilterPushdownSuppressed(filterNode))
            {
                return false;
            }

            Node joinNode = filterNode.Child0;
            Op joinOp = joinNode.Op;
            Node leftInputNode = joinNode.Child0;
            Node rightInputNode = joinNode.Child1;
            Command command = trc.Command;
            bool needsTransformation = false;

            //
            // If we're dealing with an outer-join, first check to see if the current 
            // predicate preserves nulls for the right table. 
            // If it doesn't then we can convert the outer join into an inner join,
            // and then continue with the rest of our processing here
            // 
            ExtendedNodeInfo rightTableNodeInfo = command.GetExtendedNodeInfo(rightInputNode);
            Predicate predicate = new Predicate(command, filterNode.Child1);
            if (joinOp.OpType == OpType.LeftOuterJoin)
            {
                if (!predicate.PreservesNulls(rightTableNodeInfo.Definitions, true))
                {
                    joinOp = command.CreateInnerJoinOp();
                    needsTransformation = true;
                }
            }
            ExtendedNodeInfo leftTableInfo = command.GetExtendedNodeInfo(leftInputNode);

            //
            // Check to see if the predicate contains any "single-table-filters". In those
            // cases, we could simply push that filter down to the child. 
            // We can do this for inner joins and cross joins - for both inputs.
            // For left-outer joins, however, we can only do this for the left-side input
            // Further note that we only want to do the pushdown if it will help us - if 
            // the join input is a ScanTable (or some other cases), then it doesn't help us.
            // 
            Node leftSingleTablePredicateNode = null;
            if (leftInputNode.Op.OpType != OpType.ScanTable)
            {
                Predicate leftSingleTablePredicates = predicate.GetSingleTablePredicates(leftTableInfo.Definitions, out predicate);
                leftSingleTablePredicateNode = leftSingleTablePredicates.BuildAndTree();
            }

            Node rightSingleTablePredicateNode = null;
            if ((rightInputNode.Op.OpType != OpType.ScanTable) &&
                (joinOp.OpType != OpType.LeftOuterJoin))
            {
                Predicate rightSingleTablePredicates = predicate.GetSingleTablePredicates(rightTableNodeInfo.Definitions, out predicate);
                rightSingleTablePredicateNode = rightSingleTablePredicates.BuildAndTree();
            }

            //
            // Now check to see if the predicate contains some "join predicates". We can
            // add these to the existing join predicate (if any). 
            // We can only do this for inner joins and cross joins - not for LOJs
            //
            Node newJoinPredicateNode = null;
            if (joinOp.OpType == OpType.CrossJoin || joinOp.OpType == OpType.InnerJoin)
            {
                Predicate joinPredicate = predicate.GetJoinPredicates(leftTableInfo.Definitions, rightTableNodeInfo.Definitions, out predicate);
                newJoinPredicateNode = joinPredicate.BuildAndTree();
            }

            //
            // Now for the dirty work. We've identified some predicates that could be pushed
            // into the left table, some predicates that could be pushed into the right table
            // and some that could become join predicates. 
            // 
            if (leftSingleTablePredicateNode != null)
            {
                leftInputNode = command.CreateNode(command.CreateFilterOp(), leftInputNode, leftSingleTablePredicateNode);
                needsTransformation = true;
            }
            if (rightSingleTablePredicateNode != null)
            {
                rightInputNode = command.CreateNode(command.CreateFilterOp(), rightInputNode, rightSingleTablePredicateNode);
                needsTransformation = true;
            }

            // Identify the new join predicate
            if (newJoinPredicateNode != null)
            {
                needsTransformation = true;
                if (joinOp.OpType == OpType.CrossJoin)
                {
                    joinOp = command.CreateInnerJoinOp();
                }
                else
                {
                    PlanCompiler.Assert(joinOp.OpType == OpType.InnerJoin, "unexpected non-InnerJoin?");
                    newJoinPredicateNode = PlanCompilerUtil.CombinePredicates(joinNode.Child2, newJoinPredicateNode, command);
                }
            }
            else
            {
                newJoinPredicateNode = (joinOp.OpType == OpType.CrossJoin) ? null : joinNode.Child2;
            }

            // 
            // If nothing has changed, then just return the current node. Otherwise, 
            // we will loop forever
            //
            if (!needsTransformation)
            {
                return false;
            }

            Node newJoinNode;
            // 
            // Finally build up a new join node
            // 
            if (joinOp.OpType == OpType.CrossJoin)
            {
                newJoinNode = command.CreateNode(joinOp, leftInputNode, rightInputNode);
            }
            else
            {
                newJoinNode = command.CreateNode(joinOp, leftInputNode, rightInputNode, newJoinPredicateNode);
            }

            //
            // Build up a new filterNode above this join node. But only if we have a filter left
            // 
            Node newFilterPredicateNode = predicate.BuildAndTree();
            if (newFilterPredicateNode == null)
            {
                newNode = newJoinNode;
            }
            else
            {
                newNode = command.CreateNode(command.CreateFilterOp(), newJoinNode, newFilterPredicateNode);
            }
            return true;
        }
        #endregion

        #region Filter over OuterApply
        internal static readonly PatternMatchRule Rule_FilterOverOuterApply =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                                  new Node(OuterApplyOp.Pattern,
                                           new Node(LeafOp.Pattern),
                                           new Node(LeafOp.Pattern)),
                                  new Node(LeafOp.Pattern)),
                         ProcessFilterOverOuterApply);
        /// <summary>
        /// Convert Filter(OuterApply(X,Y), p) into 
        ///    Filter(CrossApply(X,Y), p)
        /// if "p" is not null-preserving for Y (ie) "p" does not preserve null values from Y
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="filterNode">Filter node</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessFilterOverOuterApply(RuleProcessingContext context, Node filterNode, out Node newNode)
        {
            newNode = filterNode;
            Node applyNode = filterNode.Child0;
            Op applyOp = applyNode.Op;
            Node applyRightInputNode = applyNode.Child1;
            TransformationRulesContext trc = (TransformationRulesContext)context;
            Command command = trc.Command;

            //
            // Check to see if the current predicate preserves nulls for the right table. 
            // If it doesn't then we can convert the outer apply into a cross-apply,
            // 
            ExtendedNodeInfo rightTableNodeInfo = command.GetExtendedNodeInfo(applyRightInputNode);
            Predicate predicate = new Predicate(command, filterNode.Child1);
            if (!predicate.PreservesNulls(rightTableNodeInfo.Definitions, true))
            {
                Node newApplyNode = command.CreateNode(command.CreateCrossApplyOp(), applyNode.Child0, applyRightInputNode);
                Node newFilterNode = command.CreateNode(command.CreateFilterOp(), newApplyNode, filterNode.Child1);
                newNode = newFilterNode;
                return true;
            }

            return false;
        }

        #endregion

        #region FilterWithConstantPredicate
        internal static readonly PatternMatchRule Rule_FilterWithConstantPredicate =
            new PatternMatchRule(new Node(FilterOp.Pattern,
                          new Node(LeafOp.Pattern),
                          new Node(ConstantPredicateOp.Pattern)),
                 ProcessFilterWithConstantPredicate);
        /// <summary>
        /// Convert 
        ///    Filter(X, true)  => X
        ///    Filter(X, false) => Project(Filter(SingleRowTableOp, ...), false)
        /// where ... represent variables that are equivalent to the table columns
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">Current subtree</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessFilterWithConstantPredicate(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            ConstantPredicateOp predOp = (ConstantPredicateOp)n.Child1.Op;

            // If we're dealing with a "true" predicate, then simply return the RelOp
            // input to the filter
            if (predOp.IsTrue)
            {
                newNode = n.Child0;
                return true;
            }

            PlanCompiler.Assert(predOp.IsFalse, "unexpected non-false predicate?");
            // We're dealing with a "false" predicate, then we can get rid of the 
            // input, and replace it with a dummy project

            //
            // If the input is already a singlerowtableOp, then there's nothing 
            // further to do
            //
            if (n.Child0.Op.OpType == OpType.SingleRowTable ||
                (n.Child0.Op.OpType == OpType.Project &&
                 n.Child0.Child0.Op.OpType == OpType.SingleRowTable))
            {
                return false;
            }

            TransformationRulesContext trc = (TransformationRulesContext)context;
            ExtendedNodeInfo childNodeInfo = trc.Command.GetExtendedNodeInfo(n.Child0);
            List<Node> varDefNodeList = new List<Node>();
            VarVec newVars = trc.Command.CreateVarVec();
            foreach (Var v in childNodeInfo.Definitions)
            {
                NullOp nullConst = trc.Command.CreateNullOp(v.Type);
                Node constNode = trc.Command.CreateNode(nullConst);
                Var computedVar;
                Node varDefNode = trc.Command.CreateVarDefNode(constNode, out computedVar);
                trc.AddVarMapping(v, computedVar);
                newVars.Set(computedVar);
                varDefNodeList.Add(varDefNode);
            }
            // If no vars have been selected out, add a dummy var
            if (newVars.IsEmpty)
            {
                NullOp nullConst = trc.Command.CreateNullOp(trc.Command.BooleanType);
                Node constNode = trc.Command.CreateNode(nullConst);
                Var computedVar;
                Node varDefNode = trc.Command.CreateVarDefNode(constNode, out computedVar);
                newVars.Set(computedVar);
                varDefNodeList.Add(varDefNode);
            }

            Node singleRowTableNode = trc.Command.CreateNode(trc.Command.CreateSingleRowTableOp());
            n.Child0 = singleRowTableNode;

            Node varDefListNode = trc.Command.CreateNode(trc.Command.CreateVarDefListOp(), varDefNodeList);
            ProjectOp projectOp = trc.Command.CreateProjectOp(newVars);           
            Node projectNode = trc.Command.CreateNode(projectOp, n, varDefListNode); 

            projectNode.Child0 = n;
            newNode = projectNode;
            return true;
        }

        #endregion

        #region All FilterOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
                 FilterOpRules.Rule_FilterWithConstantPredicate,     
                 FilterOpRules.Rule_FilterOverCrossJoin,
                 FilterOpRules.Rule_FilterOverDistinct,
                 FilterOpRules.Rule_FilterOverExcept,
                 FilterOpRules.Rule_FilterOverFilter,
                 FilterOpRules.Rule_FilterOverGroupBy,
                 FilterOpRules.Rule_FilterOverInnerJoin,
                 FilterOpRules.Rule_FilterOverIntersect,
                 FilterOpRules.Rule_FilterOverLeftOuterJoin,
                 FilterOpRules.Rule_FilterOverProject,
                 FilterOpRules.Rule_FilterOverUnionAll,
                 FilterOpRules.Rule_FilterOverOuterApply,
        };

        #endregion
    }
    #endregion

    #region Project Rules
    /// <summary>
    /// Transformation rules for ProjectOp
    /// </summary>
    internal static class ProjectOpRules
    {
        #region ProjectOverProject
        internal static readonly PatternMatchRule Rule_ProjectOverProject =
            new PatternMatchRule(new Node(ProjectOp.Pattern,
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessProjectOverProject);
        /// <summary>
        /// Converts a Project(Project(X, c1,...), d1,...) => 
        ///            Project(X, d1', d2'...)
        /// where d1', d2' etc. are the "mapped" versions of d1, d2 etc.
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="projectNode">Current ProjectOp node</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessProjectOverProject(RuleProcessingContext context, Node projectNode, out Node newNode)
        {
            newNode = projectNode;
            ProjectOp projectOp = (ProjectOp)projectNode.Op;
            Node varDefListNode = projectNode.Child1;
            Node subProjectNode = projectNode.Child0;
            ProjectOp subProjectOp = (ProjectOp)subProjectNode.Op;
            TransformationRulesContext trc = (TransformationRulesContext)context;

            // If any of the defining expressions is not a scalar op tree, then simply
            // quit
            Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
            foreach (Node varDefNode in varDefListNode.Children)
            {
                if (!trc.IsScalarOpTree(varDefNode.Child0, varRefMap))
                {
                    return false;
                }
            }

            Dictionary<Var, Node> varMap = trc.GetVarMap(subProjectNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false;
            }

            // create a new varDefList node...
            Node newVarDefListNode = trc.Command.CreateNode(trc.Command.CreateVarDefListOp());

            // Remap any local definitions, I have
            foreach (Node varDefNode in varDefListNode.Children)
            {
                // update the defining expression
                varDefNode.Child0 = trc.ReMap(varDefNode.Child0, varMap);
                trc.Command.RecomputeNodeInfo(varDefNode);
                newVarDefListNode.Children.Add(varDefNode);
            }

            // Now, pull up any definitions of the subProject that I publish myself
            ExtendedNodeInfo projectNodeInfo = trc.Command.GetExtendedNodeInfo(projectNode);
            foreach (Node chi in subProjectNode.Child1.Children)
            {
                VarDefOp varDefOp = (VarDefOp)chi.Op;
                if (projectNodeInfo.Definitions.IsSet(varDefOp.Var))
                {
                    newVarDefListNode.Children.Add(chi);
                }
            }

            //
            // now that we have remapped all our computed vars, simply bypass the subproject
            // node
            //
            projectNode.Child0 = subProjectNode.Child0;
            projectNode.Child1 = newVarDefListNode;
            return true;
        }
        #endregion

        #region ProjectWithNoLocalDefinitions
        internal static readonly PatternMatchRule Rule_ProjectWithNoLocalDefs =
            new PatternMatchRule(new Node(ProjectOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(VarDefListOp.Pattern)),
                                 ProcessProjectWithNoLocalDefinitions);
        /// <summary>
        /// Eliminate a ProjectOp that has no local definitions at all and 
        /// no external references, (ie) if Child1
        /// of the ProjectOp (the VarDefListOp child) has no children, then the ProjectOp
        /// is serving no useful purpose. Get rid of the ProjectOp, and replace it with its
        /// child
        /// </summary>
        /// <param name="context">rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessProjectWithNoLocalDefinitions(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            NodeInfo nodeInfo = context.Command.GetNodeInfo(n);

            // We cannot eliminate this node because it can break other rules, 
            // e.g. ProcessApplyOverAnything which relies on existance of external refs to substitute
            // CrossApply(x, y) => CrossJoin(x, y). See SQLBU #481719.
            if (!nodeInfo.ExternalReferences.IsEmpty)
            {
                return false;
            }

            newNode = n.Child0;
            return true;
        }

        #endregion

        #region ProjectOpWithSimpleVarRedefinitions
        internal static readonly SimpleRule Rule_ProjectOpWithSimpleVarRedefinitions = new SimpleRule(OpType.Project, ProcessProjectWithSimpleVarRedefinitions);
        /// <summary>
        /// If the ProjectOp defines some computedVars, but those computedVars are simply 
        /// redefinitions of other Vars, then eliminate the computedVars. 
        /// 
        /// Project(X, VarDefList(VarDef(cv1, VarRef(v1)), ...))
        ///    can be transformed into
        /// Project(X, VarDefList(...))
        /// where cv1 has now been replaced by v1
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessProjectWithSimpleVarRedefinitions(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            ProjectOp projectOp = (ProjectOp)n.Op;

            if (n.Child1.Children.Count == 0)
            {
                return false;
            }

            TransformationRulesContext trc = (TransformationRulesContext)context;
            Command command = trc.Command;

            ExtendedNodeInfo nodeInfo = command.GetExtendedNodeInfo(n);

            //
            // Check to see if any of the computed Vars defined by this ProjectOp
            // are simple redefinitions of other VarRefOps. Consider only those 
            // VarRefOps that are not "external" references
            bool canEliminateSomeVars = false;
            foreach (Node varDefNode in n.Child1.Children)
            {
                Node definingExprNode = varDefNode.Child0;
                if (definingExprNode.Op.OpType == OpType.VarRef)
                {
                    VarRefOp varRefOp = (VarRefOp)definingExprNode.Op;
                    if (!nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                    {
                        // this is a Var that we should remove 
                        canEliminateSomeVars = true;
                        break;
                    }
                }
            }

            // Did we have any redefinitions
            if (!canEliminateSomeVars)
            {
                return false;
            }

            //
            // OK. We've now identified a set of vars that are simple redefinitions.
            // Try and replace the computed Vars with the Vars that they're redefining
            //

            // Lets now build up a new VarDefListNode
            List<Node> newVarDefNodes = new List<Node>();
            foreach (Node varDefNode in n.Child1.Children)
            {
                VarDefOp varDefOp = (VarDefOp)varDefNode.Op;
                VarRefOp varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp != null && !nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                {
                    projectOp.Outputs.Clear(varDefOp.Var);
                    projectOp.Outputs.Set(varRefOp.Var);
                    trc.AddVarMapping(varDefOp.Var, varRefOp.Var);
                }
                else
                {
                    newVarDefNodes.Add(varDefNode);
                }
            }

            // Note: Even if we don't have any local var definitions left, we should not remove
            // this project yet because: 
            //  (1) this project node may be prunning out some outputs;
            //  (2) the rule Rule_ProjectWithNoLocalDefs, would do that later anyway.

            // Create a new vardeflist node, and set that as Child1 for the projectOp
            Node newVarDefListNode = command.CreateNode(command.CreateVarDefListOp(), newVarDefNodes);
            n.Child1 = newVarDefListNode;
            return true; // some part of the subtree was modified
        }


        #endregion

        #region ProjectOpWithNullSentinel
        internal static readonly SimpleRule Rule_ProjectOpWithNullSentinel = new SimpleRule(OpType.Project, ProcessProjectOpWithNullSentinel);
        /// <summary>
        /// Tries to remove null sentinel definitions by replacing them to vars that are guaranteed 
        /// to be non-nullable and of integer type, or with reference to other constants defined in the 
        /// same project. In particular, 
        /// 
        ///  - If based on the ancestors, the value of the null sentinel can be changed and the 
        /// input of the project has a var that is guaranteed to be non-nullable and 
        /// is of integer type, then the definitions of the vars defined as NullSentinels in the ProjectOp 
        /// are replaced with a reference to that var. I.eg:
        /// 
        /// Project(X, VarDefList(VarDef(ns_var, NullSentinel), ...))
        ///    can be transformed into
        /// Project(X, VarDefList(VarDef(ns_var, VarRef(v))...))
        /// where v is known to be non-nullable
        /// 
        /// - Else, if based on the ancestors, the value of the null sentinel can be changed and 
        /// the project already has definitions of other int constants, the definitions of the null sentinels
        /// are removed and the respective vars are remapped to the var representing the constant.
        /// 
        /// - Else, the definitions of the all null sentinels except for one are removed, and the
        /// the respective vars are remapped to the remaining null sentinel. 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessProjectOpWithNullSentinel(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            ProjectOp projectOp = (ProjectOp)n.Op;
            Node varDefListNode = n.Child1;

            if (varDefListNode.Children.Where(c => c.Child0.Op.OpType == OpType.NullSentinel).Count() == 0)
            {
                return false;
            }

            TransformationRulesContext trc = (TransformationRulesContext)context;
            Command command = trc.Command;
            ExtendedNodeInfo relOpInputNodeInfo = command.GetExtendedNodeInfo(n.Child0);
            Var inputSentinel;
            bool reusingConstantFromSameProjectAsSentinel = false;

            bool canChangeNullSentinelValue = trc.CanChangeNullSentinelValue;
            
            if (!canChangeNullSentinelValue || !TransformationRulesContext.TryGetInt32Var(relOpInputNodeInfo.NonNullableDefinitions, out inputSentinel))
            {
                reusingConstantFromSameProjectAsSentinel = true;
                if (!canChangeNullSentinelValue || !TransformationRulesContext.TryGetInt32Var(n.Child1.Children.Where(child => child.Child0.Op.OpType == OpType.Constant || child.Child0.Op.OpType == OpType.InternalConstant).Select(child => ((VarDefOp)(child.Op)).Var), out inputSentinel))
                {
                    inputSentinel = n.Child1.Children.Where(child => child.Child0.Op.OpType == OpType.NullSentinel).Select(child => ((VarDefOp)(child.Op)).Var).FirstOrDefault();
                    if (inputSentinel == null)
                    {
                        return false;
                    }
                }
            }

            bool modified = false;
            
            for (int i = n.Child1.Children.Count-1; i >= 0; i--)
            {
                Node varDefNode = n.Child1.Children[i];
                Node definingExprNode = varDefNode.Child0;
                if (definingExprNode.Op.OpType == OpType.NullSentinel)
                { 
                    if (!reusingConstantFromSameProjectAsSentinel)
                    {
                        VarRefOp varRefOp = command.CreateVarRefOp(inputSentinel);
                        varDefNode.Child0 = command.CreateNode(varRefOp);
                        command.RecomputeNodeInfo(varDefNode);
                        modified = true;
                    }
                    else if (!inputSentinel.Equals(((VarDefOp)varDefNode.Op).Var))
                    {
                        projectOp.Outputs.Clear(((VarDefOp)varDefNode.Op).Var);
                        n.Child1.Children.RemoveAt(i);
                        trc.AddVarMapping(((VarDefOp)varDefNode.Op).Var, inputSentinel);
                        modified = true;
                    }
                }
            }

            if (modified)
            {
                command.RecomputeNodeInfo(n.Child1);
            }
            return modified; 
        }
        #endregion

        #region All ProjectOp Rules
        //The order of the rules is important
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
                 ProjectOpRules.Rule_ProjectOpWithNullSentinel,
                 ProjectOpRules.Rule_ProjectOpWithSimpleVarRedefinitions,
                 ProjectOpRules.Rule_ProjectOverProject,
                 ProjectOpRules.Rule_ProjectWithNoLocalDefs,             
        };
        #endregion
    }
    #endregion

    #region Apply Rules
    /// <summary>
    /// Transformation rules for ApplyOps - CrossApply, OuterApply
    /// </summary>
    internal static class ApplyOpRules
    {
        #region ApplyOverFilter
        internal static readonly PatternMatchRule Rule_CrossApplyOverFilter =
            new PatternMatchRule(new Node(CrossApplyOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern))),
                                 ProcessApplyOverFilter);
        internal static readonly PatternMatchRule Rule_OuterApplyOverFilter =
            new PatternMatchRule(new Node(OuterApplyOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern))),
                                 ProcessApplyOverFilter);
        /// <summary>
        /// Convert CrossApply(X, Filter(Y, p)) => InnerJoin(X, Y, p)
        ///         OuterApply(X, Filter(Y, p)) => LeftOuterJoin(X, Y, p)
        /// if "Y" has no external references to X
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="applyNode">Current ApplyOp</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessApplyOverFilter(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            Node filterNode = applyNode.Child1;
            Command command = context.Command;

            NodeInfo filterInputNodeInfo = command.GetNodeInfo(filterNode.Child0);
            ExtendedNodeInfo applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);

            //
            // check to see if the inputNode to the FilterOp has any external references 
            // to the left child of the ApplyOp. If it does, we simply return, we 
            // can't do much more here
            //
            if (filterInputNodeInfo.ExternalReferences.Overlaps(applyLeftChildNodeInfo.Definitions))
            {
                return false;
            }

            //
            // We've now gotten to the stage where the only external references (if any)
            // are from the filter predicate. 
            // We can now simply convert the apply into an inner/leftouter join with the 
            // filter predicate acting as the join condition
            //
            JoinBaseOp joinOp = null;
            if (applyNode.Op.OpType == OpType.CrossApply)
            {
                joinOp = command.CreateInnerJoinOp();
            }
            else
            {
                joinOp = command.CreateLeftOuterJoinOp();
            }

            newNode = command.CreateNode(joinOp, applyNode.Child0, filterNode.Child0, filterNode.Child1);
            return true;
        }

        internal static readonly PatternMatchRule Rule_OuterApplyOverProjectInternalConstantOverFilter =
             new PatternMatchRule(new Node(OuterApplyOp.Pattern,
                                           new Node(LeafOp.Pattern),
                                           new Node(ProjectOp.Pattern,
                                                    new Node(FilterOp.Pattern,
                                                             new Node(LeafOp.Pattern),
                                                             new Node(LeafOp.Pattern)),
                                                    new Node(VarDefListOp.Pattern,
                                                             new Node(VarDefOp.Pattern,
                                                                      new Node(InternalConstantOp.Pattern))))),
                         ProcessOuterApplyOverDummyProjectOverFilter);

        internal static readonly PatternMatchRule Rule_OuterApplyOverProjectNullSentinelOverFilter =
           new PatternMatchRule(new Node(OuterApplyOp.Pattern,
                                         new Node(LeafOp.Pattern),
                                         new Node(ProjectOp.Pattern,
                                                  new Node(FilterOp.Pattern,
                                                           new Node(LeafOp.Pattern),
                                                           new Node(LeafOp.Pattern)),
                                                  new Node(VarDefListOp.Pattern,
                                                           new Node(VarDefOp.Pattern,
                                                                    new Node(NullSentinelOp.Pattern))))),
                       ProcessOuterApplyOverDummyProjectOverFilter);

        /// <summary>
        /// Convert OuterApply(X, Project(Filter(Y, p), constant)) => 
        ///     LeftOuterJoin(X, Project(Y, constant), p)
        /// if "Y" has no external references to X
        /// 
        /// In an ideal world, we would be able to push the Project below the Filter, 
        /// and then have the normal ApplyOverFilter rule handle this - but that causes us
        /// problems because we always try to pull up ProjectOp's as high as possible. Hence,
        /// the special case for this rule
        /// 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="applyNode">Current ApplyOp</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessOuterApplyOverDummyProjectOverFilter(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            Node projectNode = applyNode.Child1;
            ProjectOp projectOp = (ProjectOp)projectNode.Op;
            Node filterNode = projectNode.Child0;
            Node filterInputNode = filterNode.Child0;
            Command command = context.Command;

            ExtendedNodeInfo filterInputNodeInfo = command.GetExtendedNodeInfo(filterInputNode);
            ExtendedNodeInfo applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);

            //
            // Check if the outputs of the ProjectOp or the inputNode to the FilterOp 
            // have any external references to the left child of the ApplyOp. 
            // If they do, we simply return, we can't do much more here
            //
            if (projectOp.Outputs.Overlaps(applyLeftChildNodeInfo.Definitions) || filterInputNodeInfo.ExternalReferences.Overlaps(applyLeftChildNodeInfo.Definitions))
            {
                return false;
            }

            //
            // We've now gotten to the stage where the only external references (if any)
            // are from the filter predicate. 
            // First, push the Project node down below the filter - but make sure that
            // all the Vars needed by the Filter are projected out 
            //
            bool capWithProject = false;
            Node joinNodeRightInput = null;

            //
            // Check to see whether there is a sentinel var available - if there is, then
            // we can simply move the ProjectOp above the join we're going to construct 
            // and of course, build a NullIf expression for the constant.
            // Otherwise, the ProjectOp will need to be the child of the joinOp that we're
            // building - and we'll need to make sure that the ProjectOp projects out
            // any vars that are required for the Filter in the first place
            //
            TransformationRulesContext trc = (TransformationRulesContext)context;
            Var sentinelVar;
            bool sentinelIsInt32;

            if (TransformationRulesContext.TryGetInt32Var(filterInputNodeInfo.NonNullableDefinitions, out sentinelVar))
            {
                sentinelIsInt32 = true;
            }
            else
            {
                sentinelVar = filterInputNodeInfo.NonNullableDefinitions.First;
                sentinelIsInt32 = false;
            }
          
            if (sentinelVar != null)
            {
                capWithProject = true;
                Node varDefNode = projectNode.Child1.Child0;
                if (varDefNode.Child0.Op.OpType == OpType.NullSentinel && sentinelIsInt32 && trc.CanChangeNullSentinelValue)
                {
                    varDefNode.Child0 = context.Command.CreateNode(context.Command.CreateVarRefOp(sentinelVar));
                }
                else
                {
                    varDefNode.Child0 = trc.BuildNullIfExpression(sentinelVar, varDefNode.Child0);
                }
                command.RecomputeNodeInfo(varDefNode);
                command.RecomputeNodeInfo(projectNode.Child1);
                joinNodeRightInput = filterInputNode;
            }
            else
            {
                // We need to keep the projectNode - unfortunately
                joinNodeRightInput = projectNode;
                //
                // Make sure that every Var that is needed for the filter predicate
                // is captured in the projectOp outputs list
                //
                NodeInfo filterPredicateNodeInfo = command.GetNodeInfo(filterNode.Child1);
                foreach (Var v in filterPredicateNodeInfo.ExternalReferences)
                {
                    if (filterInputNodeInfo.Definitions.IsSet(v))
                    {
                        projectOp.Outputs.Set(v);
                    }
                }
                projectNode.Child0 = filterInputNode;
            }

            context.Command.RecomputeNodeInfo(projectNode);

            //
            // We can now simply convert the apply into an inner/leftouter join with the 
            // filter predicate acting as the join condition
            //
            Node joinNode = command.CreateNode(command.CreateLeftOuterJoinOp(), applyNode.Child0, joinNodeRightInput, filterNode.Child1);
            if (capWithProject)
            {
                ExtendedNodeInfo joinNodeInfo = command.GetExtendedNodeInfo(joinNode);
                projectNode.Child0 = joinNode;
                projectOp.Outputs.Or(joinNodeInfo.Definitions);
                newNode = projectNode;
            }
            else
            {
                newNode = joinNode;
            }
            return true;
        }
        #endregion

        #region ApplyOverProject
        internal static readonly PatternMatchRule Rule_CrossApplyOverProject =
            new PatternMatchRule(new Node(CrossApplyOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern))),
                                 ProcessCrossApplyOverProject);

        /// <summary>
        /// Converts a CrossApply(X, Project(Y, ...)) => Project(CrossApply(X, Y), ...)
        /// where the projectVars are simply pulled up
        /// </summary>
        /// <param name="context">RuleProcessing context</param>
        /// <param name="applyNode">The ApplyOp subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>Transfomation status</returns>
        static bool ProcessCrossApplyOverProject(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            Node projectNode = applyNode.Child1;
            ProjectOp projectOp = (ProjectOp)projectNode.Op as ProjectOp;
            Command command = context.Command;

            // We can simply pull up the project over the apply; provided we make sure 
            // that all the definitions of the apply are represented in the projectOp
            ExtendedNodeInfo applyNodeInfo = command.GetExtendedNodeInfo(applyNode);
            VarVec vec = command.CreateVarVec(projectOp.Outputs);
            vec.Or(applyNodeInfo.Definitions);
            projectOp.Outputs.InitFrom(vec);

            // pull up the project over the apply node
            applyNode.Child1 = projectNode.Child0;
            context.Command.RecomputeNodeInfo(applyNode);
            projectNode.Child0 = applyNode;

            newNode = projectNode;
            return true;
        }

        internal static readonly PatternMatchRule Rule_OuterApplyOverProject =
             new PatternMatchRule(new Node(OuterApplyOp.Pattern,
                                           new Node(LeafOp.Pattern),
                                           new Node(ProjectOp.Pattern,
                                                    new Node(LeafOp.Pattern),
                                                    new Node(LeafOp.Pattern))),
                         ProcessOuterApplyOverProject);
        /// <summary>
        /// Converts a 
        ///     OuterApply(X, Project(Y, ...)) 
        /// => 
        ///     Project(OuterApply(X, Project(Y, ...)), ...) or
        ///     Project(OuterApply(X, Y), ...)
        /// 
        /// The second (simpler) form is used if a "sentinel" var can be located (ie)
        /// some Var of Y that is guaranteed to be non-null. Otherwise, we create a 
        /// dummy ProjectNode as the right child of the Apply - which
        /// simply projects out all the vars of the Y, and adds on a constant (say "1"). This
        /// constant is now treated as the sentinel var
        /// 
        /// Then the existing ProjectOp is pulled up above the the outer-apply, but all the locally defined
        /// Vars have their defining expressions now expressed as 
        ///     case when sentinelVar is null then null else oldDefiningExpr end
        /// where oldDefiningExpr represents the original defining expression
        /// This allows us to get nulls for the appropriate columns when necessary. 
        /// 
        /// Special cases. 
        /// * If the oldDefiningExpr is itself an internal constant equivalent to the null sentinel ("1"),
        ///   we simply project a ref to the null sentinel, no need for cast
        /// * If the ProjectOp contained exactly one locally defined Var, and it was a constant, then 
        ///   we simply return - we will be looping endlessly otherwise
        /// * If the ProjectOp contained no local definitions, then we don't need to create the 
        ///   dummy projectOp - we can simply pull up the Project
        /// * If any of the defining expressions of the local definitions was simply a VarRefOp 
        ///   referencing a Var that was defined by Y, then there is no need to add the case
        ///   expression for that.
        /// 
        /// </summary>
        /// <param name="context">RuleProcessing context</param>
        /// <param name="applyNode">The ApplyOp subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>Transfomation status</returns>
        static bool ProcessOuterApplyOverProject(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            Node projectNode = applyNode.Child1;
            Node varDefListNode = projectNode.Child1;

            TransformationRulesContext trc = (TransformationRulesContext)context;
            ExtendedNodeInfo inputNodeInfo = context.Command.GetExtendedNodeInfo(projectNode.Child0);
            Var sentinelVar = inputNodeInfo.NonNullableDefinitions.First;

            //
            // special case handling first - we'll end up in an infinite loop otherwise.
            // If the ProjectOp is the dummy ProjectOp that we would be building (ie)
            // it defines only 1 var - and the defining expression is simply a constant
            // 
            if (sentinelVar == null &&
                varDefListNode.Children.Count == 1 &&
                (varDefListNode.Child0.Child0.Op.OpType == OpType.InternalConstant || varDefListNode.Child0.Child0.Op.OpType == OpType.NullSentinel))
            {
                return false;
            }

            Command command = context.Command;
            Node dummyProjectNode = null;
            InternalConstantOp nullSentinelDefinitionOp = null;

            // get node information for the project's child
            ExtendedNodeInfo projectInputNodeInfo = command.GetExtendedNodeInfo(projectNode.Child0);

            //
            // Build up a dummy project node. 
            // Walk through each local definition of the current project Node, and convert
            // all expressions into case expressions whose value depends on the var
            // produced by the dummy project node
            //

            // Dev10 #480443: If any of the definitions changes we need to recompute the node info.
            bool anyVarDefChagned = false;
            foreach (Node varDefNode in varDefListNode.Children)
            {
                PlanCompiler.Assert(varDefNode.Op.OpType == OpType.VarDef, "Expected VarDefOp. Found " + varDefNode.Op.OpType + " instead");
                VarRefOp varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp == null || !projectInputNodeInfo.Definitions.IsSet(varRefOp.Var))
                {
                    // do we need to build a dummy project node
                    if (sentinelVar == null)
                    {
                        nullSentinelDefinitionOp = command.CreateInternalConstantOp(command.IntegerType, 1);
                        Node dummyConstantExpr = command.CreateNode(nullSentinelDefinitionOp);
                        Node dummyProjectVarDefListNode = command.CreateVarDefListNode(dummyConstantExpr, out sentinelVar);
                        ProjectOp dummyProjectOp = command.CreateProjectOp(sentinelVar);
                        dummyProjectOp.Outputs.Or(projectInputNodeInfo.Definitions);
                        dummyProjectNode = command.CreateNode(dummyProjectOp, projectNode.Child0, dummyProjectVarDefListNode);
                    }

                    Node currentDefinition;

                    // If the null sentinel was just created, and the local definition of the current project Node 
                    // is an internal constant equivalent to the null sentinel, it can be rewritten as a reference
                    // to the null sentinel.
                    if (nullSentinelDefinitionOp != null && ((true == nullSentinelDefinitionOp.IsEquivalent(varDefNode.Child0.Op)) ||
                        //The null sentinel has the same value of 1, thus it is safe.        
                        varDefNode.Child0.Op.OpType == OpType.NullSentinel))
                    {
                        currentDefinition = command.CreateNode(command.CreateVarRefOp(sentinelVar));
                    }
                    else
                    {
                        currentDefinition = trc.BuildNullIfExpression(sentinelVar, varDefNode.Child0);
                    }
                    varDefNode.Child0 = currentDefinition;
                    command.RecomputeNodeInfo(varDefNode);
                    anyVarDefChagned = true;
                }
            }

            // Recompute node info if needed
            if (anyVarDefChagned)
            {
                command.RecomputeNodeInfo(varDefListNode);
            }

            //
            // If we've created a dummy project node, make that the new child of the applyOp
            //
            applyNode.Child1 = dummyProjectNode != null ? dummyProjectNode : projectNode.Child0;
            command.RecomputeNodeInfo(applyNode);

            //
            // Pull up the project node above the apply node now. Also, make sure that every Var of 
            // the applyNode's definitions actually shows up in the new Project
            //
            projectNode.Child0 = applyNode;
            ExtendedNodeInfo applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);
            ProjectOp projectOp = (ProjectOp)projectNode.Op;
            projectOp.Outputs.Or(applyLeftChildNodeInfo.Definitions);

            newNode = projectNode;
            return true;
        }
        #endregion

        #region ApplyOverAnything
        internal static readonly PatternMatchRule Rule_CrossApplyOverAnything =
            new PatternMatchRule(new Node(CrossApplyOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessApplyOverAnything);
        internal static readonly PatternMatchRule Rule_OuterApplyOverAnything =
            new PatternMatchRule(new Node(OuterApplyOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessApplyOverAnything);

        /// <summary>
        /// Converts a CrossApply(X,Y) => CrossJoin(X,Y)
        ///            OuterApply(X,Y) => LeftOuterJoin(X, Y, true)
        ///  only if Y has no external references to X
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="applyNode">The ApplyOp subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>the transformation status</returns>
        static bool ProcessApplyOverAnything(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            Node applyLeftChild = applyNode.Child0;
            Node applyRightChild = applyNode.Child1;
            ApplyBaseOp applyOp = (ApplyBaseOp)applyNode.Op;
            Command command = context.Command;

            ExtendedNodeInfo applyRightChildNodeInfo = command.GetExtendedNodeInfo(applyRightChild);
            ExtendedNodeInfo applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyLeftChild);

            //
            // If we're currently dealing with an OuterApply, and the right child is guaranteed
            // to produce at least one row, then we can convert the outer-apply into a cross apply
            //
            bool convertedToCrossApply = false;
            if (applyOp.OpType == OpType.OuterApply &&
                applyRightChildNodeInfo.MinRows >= RowCount.One)
            {
                applyOp = command.CreateCrossApplyOp();
                convertedToCrossApply = true;
            }

            //
            // Does the right child reference any of the definitions of the left child? If it
            // does, then simply return from this function
            //
            if (applyRightChildNodeInfo.ExternalReferences.Overlaps(applyLeftChildNodeInfo.Definitions))
            {
                if (convertedToCrossApply)
                {
                    newNode = command.CreateNode(applyOp, applyLeftChild, applyRightChild);
                    return true;
                }
                else
                {
                    return false;
                }
            }

            //
            // So, we now know that the right child does not reference any definitions
            // from the left. 
            // So, we simply convert the apply into an appropriate join Op
            //
            if (applyOp.OpType == OpType.CrossApply)
            {
                //
                // Convert "x CrossApply y" into "x CrossJoin y"
                //
                newNode = command.CreateNode(command.CreateCrossJoinOp(),
                    applyLeftChild, applyRightChild);
            }
            else // outer apply
            {
                //
                // Convert "x OA y" into "x LOJ y on (true)"
                //
                LeftOuterJoinOp joinOp = command.CreateLeftOuterJoinOp();
                ConstantPredicateOp trueOp = command.CreateTrueOp();
                Node trueNode = command.CreateNode(trueOp);
                newNode = command.CreateNode(joinOp, applyLeftChild, applyRightChild, trueNode);
            }
            return true;
        }
        #endregion

        #region ApplyIntoScalarSubquery
        internal static readonly PatternMatchRule Rule_CrossApplyIntoScalarSubquery =
            new PatternMatchRule(new Node(CrossApplyOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessApplyIntoScalarSubquery);
        internal static readonly PatternMatchRule Rule_OuterApplyIntoScalarSubquery =
            new PatternMatchRule(new Node(OuterApplyOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessApplyIntoScalarSubquery);

        /// <summary>
        /// Converts a Apply(X,Y) => Project(X, Y1), where Y1 is a scalar subquery version of Y
        /// The transformation is valid only if all of the following conditions hold:
        ///     1. Y produces only one output
        ///     2. Y produces at most one row
        ///     3. Y produces at least one row, or the Apply operator in question is an OuterApply
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="applyNode">The ApplyOp subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>the transformation status</returns>
        static bool ProcessApplyIntoScalarSubquery(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            Command command = context.Command;
            ExtendedNodeInfo applyRightChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child1);
            OpType applyKind = applyNode.Op.OpType;

            if (!CanRewriteApply(applyNode.Child1, applyRightChildNodeInfo, applyKind))
            {
                newNode = applyNode;
                return false;
            }

            // Create the project node over the original input with element over the apply as new projected var
            ExtendedNodeInfo applyLeftChildNodeInfo = command.GetExtendedNodeInfo(applyNode.Child0);

            Var oldVar = applyRightChildNodeInfo.Definitions.First;

            // Project all the outputs from the left child
            VarVec projectOpOutputs = command.CreateVarVec(applyLeftChildNodeInfo.Definitions);

            //
            // Remap the var defining tree to get it into a consistent state
            // and then remove all references to oldVar from it to avoid them being wrongly remapped to newVar 
            // in subsequent remappings.
            //
            TransformationRulesContext trc = (TransformationRulesContext)context;
            trc.RemapSubtree(applyNode.Child1);
            VarDefinitionRemapper.RemapSubtree(applyNode.Child1, command, oldVar);

            Node elementNode = command.CreateNode(command.CreateElementOp(oldVar.Type), applyNode.Child1);

            Var newVar;
            Node varDefListNode = command.CreateVarDefListNode(elementNode, out newVar);
            projectOpOutputs.Set(newVar);

            newNode = command.CreateNode(
                command.CreateProjectOp(projectOpOutputs),
                applyNode.Child0,
                varDefListNode);

            // Add the var mapping from oldVar to newVar
            trc.AddVarMapping(oldVar, newVar);
            return true;
        }

        /// <summary>
        /// Determines whether an applyNode can be rewritten into a projection with a scalar subquery.
        /// It can be done if all of the following conditions hold:
        ///     1. The right child or the apply has only one output
        ///     2. The right child of the apply produces at most one row
        ///     3. The right child of the apply produces at least one row, or the Apply operator in question is an OuterApply
        /// </summary>
        /// <param name="rightChild"></param>
        /// <param name="applyRightChildNodeInfo"></param>
        /// <param name="applyKind"></param>
        /// <returns></returns>
        private static bool CanRewriteApply(Node rightChild, ExtendedNodeInfo applyRightChildNodeInfo, OpType applyKind)
        {
            //Check whether it produces only one definition
            if (applyRightChildNodeInfo.Definitions.Count != 1)
            {
                return false;
            }

            //Check whether it produces at most one row
            if (applyRightChildNodeInfo.MaxRows != RowCount.One)
            {
                return false;
            }

            //For cross apply it must also return exactly one row
            if (applyKind == OpType.CrossApply && (applyRightChildNodeInfo.MinRows != RowCount.One))
            {
                return false;
            }

            //Dev10 #488632: Make sure the right child not only declares to produce only one definition,
            // but has exactly one output. For example, ScanTableOp really outputs all the columns from the table, 
            // but in its ExtendedNodeInfo.Definitions only these that are referenced are shown.
            // This is to allow for projection pruning of the unreferenced columns. 
            if (OutputCountVisitor.CountOutputs(rightChild) != 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// A visitor that calculates the number of output columns for a subree 
        /// with a given root
        /// </summary>
        internal class OutputCountVisitor : BasicOpVisitorOfT<int>
        {
            #region Constructors
            internal OutputCountVisitor()
            {
            }
            #endregion

            #region Public Methods
            /// <summary>
            /// Calculates the number of output columns for the subree 
            /// rooted at the given node
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            internal static int CountOutputs(Node node)
            {
                OutputCountVisitor visitor = new OutputCountVisitor();
                return visitor.VisitNode(node);
            }

            #endregion

            #region Visitor Methods

            #region Helpers
            /// <summary>
            /// Visitor for children. Simply visit all children,
            /// and sum the number of their outputs.
            /// </summary>
            /// <param name="n">Current node</param>
            /// <returns></returns>
            internal new int VisitChildren(Node n)
            {
                int result = 0;
                foreach (Node child in n.Children)
                {
                    result += VisitNode(child);
                }
                return result;
            }

            /// <summary>
            /// A default processor for any node. 
            /// Returns the sum of the children outputs
            /// </summary>
            /// <param name="n"></param>
            /// <returns>/returns>
            protected override int VisitDefault(Node n)
            {
                return VisitChildren(n);
            }

            #endregion

            #region RelOp Visitors

            #region SetOp Visitors

            /// <summary>
            /// The number of outputs is same as for any of the inputs
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            protected override int VisitSetOp(SetOp op, Node n)
            {
                return op.Outputs.Count;
            }

            #endregion

            /// <summary>
            /// Distinct
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override int Visit(DistinctOp op, Node n)
            {
                return op.Keys.Count;
            }

            /// <summary>
            /// FilterOp
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override int Visit(FilterOp op, Node n)
            {
                return VisitNode(n.Child0);
            }

            /// <summary>
            /// GroupByOp
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override int Visit(GroupByOp op, Node n)
            {
                return op.Outputs.Count;
            }

            /// <summary>
            /// ProjectOp
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override int Visit(ProjectOp op, Node n)
            {
                return op.Outputs.Count;
            }

            #region TableOps
            /// <summary>
            /// ScanTableOp
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override int Visit(ScanTableOp op, Node n)
            {
                return op.Table.Columns.Count;
            }

            /// <summary>
            /// SingleRowTableOp
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override int Visit(SingleRowTableOp op, Node n)
            {
                return 0;
            }

            /// <summary>
            /// Same as the input
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            protected override int VisitSortOp(SortBaseOp op, Node n)
            {
                return VisitNode(n.Child0);
            }
            #endregion
            #endregion

            #endregion
        }

        /// <summary>
        /// A utility class that remaps a given var at its definition and also remaps all its references.  
        /// The given var is remapped to an arbitrary new var.
        /// If the var is defined by a ScanTable, all the vars defined by that table and all their references
        /// are remapped as well.  
        /// </summary>
        internal class VarDefinitionRemapper : VarRemapper
        {
            private readonly Var m_oldVar;

            private VarDefinitionRemapper(Var oldVar, Command command)
                : base(command)
            {
                this.m_oldVar = oldVar;
            }

            /// <summary>
            /// Public entry point.
            /// Remaps the subree rooted at the given tree
            /// </summary>
            /// <param name="root"></param>
            /// <param name="command"></param>
            /// <param name="oldVar"></param>
            internal static void RemapSubtree(Node root, Command command, Var oldVar)
            {
                VarDefinitionRemapper remapper = new VarDefinitionRemapper(oldVar, command);
                remapper.RemapSubtree(root);
            }

            /// <summary>
            /// Update vars in this subtree. Recompute the nodeinfo along the way
            /// Unlike the base implementation, we want to visit the childrent, even if no vars are in the 
            /// remapping dictionary.
            /// </summary>
            /// <param name="subTree"></param>
            internal override void RemapSubtree(Node subTree)
            {
                foreach (Node chi in subTree.Children)
                {
                    RemapSubtree(chi);
                }

                VisitNode(subTree);
                m_command.RecomputeNodeInfo(subTree);
            }

            /// <summary>
            /// If the node defines the node that needs to be remapped, 
            /// it remaps it to a new var.
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override void Visit(VarDefOp op, Node n)
            {
                if (op.Var == m_oldVar)
                {
                    Var newVar = m_command.CreateComputedVar(n.Child0.Op.Type);
                    n.Op = m_command.CreateVarDefOp(newVar);
                    AddMapping(m_oldVar, newVar);
                }
            }

            /// <summary>
            /// If the columnVars defined by the table contain the var that needs to be remapped
            /// all the column vars produces by the table are remaped to new vars.  
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            /// <returns></returns>
            public override void Visit(ScanTableOp op, Node n)
            {
                if (op.Table.Columns.Contains(m_oldVar))
                {
                    ScanTableOp newScanTableOp = m_command.CreateScanTableOp(op.Table.TableMetadata);
                    VarDefListOp varDefListOp = m_command.CreateVarDefListOp();
                    for (int i = 0; i < op.Table.Columns.Count; i++)
                    {
                        AddMapping(op.Table.Columns[i], newScanTableOp.Table.Columns[i]);
                    }
                    n.Op = newScanTableOp;
                }
            }

            /// <summary>
            /// The var that needs to be remapped may be produced by a set op,
            /// in which case the varmaps need to be updated too. 
            /// </summary>
            /// <param name="op"></param>
            /// <param name="n"></param>
            protected override void VisitSetOp(SetOp op, Node n)
            {
                base.VisitSetOp(op, n);

                if (op.Outputs.IsSet(m_oldVar))
                {
                    Var newVar = m_command.CreateSetOpVar(m_oldVar.Type);
                    op.Outputs.Clear(m_oldVar);
                    op.Outputs.Set(newVar);
                    RemapVarMapKey(op.VarMap[0], newVar);
                    RemapVarMapKey(op.VarMap[1], newVar);
                    AddMapping(m_oldVar, newVar);
                }                
            }

            /// <summary>
            /// Replaces the entry in the varMap in which m_oldVar is a key
            /// with an entry in which newVAr is the key and the value remains the same.
            /// </summary>
            /// <param name="varMap"></param>
            /// <param name="newVar"></param>
            private void RemapVarMapKey(VarMap varMap, Var newVar)
            {
                Var value = varMap[m_oldVar];
                varMap.Remove(m_oldVar);
                varMap.Add(newVar, value);
            }
        }
        #endregion

        #region CrossApply over LeftOuterJoin of SingleRowTable with anything and with constant predicate
        internal static readonly PatternMatchRule Rule_CrossApplyOverLeftOuterJoinOverSingleRowTable =
            new PatternMatchRule(new Node(CrossApplyOp.Pattern,
                new Node(LeafOp.Pattern),
                new Node(LeftOuterJoinOp.Pattern,
                                          new Node(SingleRowTableOp.Pattern),
                                          new Node(LeafOp.Pattern),
                                          new Node(ConstantPredicateOp.Pattern))),
                                 ProcessCrossApplyOverLeftOuterJoinOverSingleRowTable);
        /// <summary>
        /// Convert a CrossApply(X, LeftOuterJoin(SingleRowTable, Y, on true))
        ///    into just OuterApply(X, Y)
        /// </summary>
        /// <param name="context">rule processing context</param>
        /// <param name="joinNode">the join node</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessCrossApplyOverLeftOuterJoinOverSingleRowTable(RuleProcessingContext context, Node applyNode, out Node newNode)
        {
            newNode = applyNode;
            Node joinNode = applyNode.Child1;

            //Check the value of the predicate
            ConstantPredicateOp joinPredicate = (ConstantPredicateOp)joinNode.Child2.Op;
            if (joinPredicate.IsFalse)
            {
                return false;
            }

            applyNode.Op = context.Command.CreateOuterApplyOp();
            applyNode.Child1 = joinNode.Child1;
            return true;
        }
        #endregion

        #region All ApplyOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
                 ApplyOpRules.Rule_CrossApplyOverAnything,
                 ApplyOpRules.Rule_CrossApplyOverFilter,
                 ApplyOpRules.Rule_CrossApplyOverProject,
                 ApplyOpRules.Rule_OuterApplyOverAnything,
                 ApplyOpRules.Rule_OuterApplyOverProjectInternalConstantOverFilter,
                 ApplyOpRules.Rule_OuterApplyOverProjectNullSentinelOverFilter,
                 ApplyOpRules.Rule_OuterApplyOverProject,
                 ApplyOpRules.Rule_OuterApplyOverFilter,
                 ApplyOpRules.Rule_CrossApplyOverLeftOuterJoinOverSingleRowTable,
                 ApplyOpRules.Rule_CrossApplyIntoScalarSubquery,
                 ApplyOpRules.Rule_OuterApplyIntoScalarSubquery,
        };
        #endregion
    }
    #endregion

    #region Join Rules
    /// <summary>
    /// Transformation rules for JoinOps
    /// </summary>
    internal static class JoinOpRules
    {
        #region JoinOverProject
        internal static readonly PatternMatchRule Rule_CrossJoinOverProject1 =
            new PatternMatchRule(new Node(CrossJoinOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern))),
                                 ProcessJoinOverProject);
        internal static readonly PatternMatchRule Rule_CrossJoinOverProject2 =
            new PatternMatchRule(new Node(CrossJoinOp.Pattern,
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverProject);
        internal static readonly PatternMatchRule Rule_InnerJoinOverProject1 =
            new PatternMatchRule(new Node(InnerJoinOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverProject);
        internal static readonly PatternMatchRule Rule_InnerJoinOverProject2 =
            new PatternMatchRule(new Node(InnerJoinOp.Pattern,
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverProject);
        internal static readonly PatternMatchRule Rule_OuterJoinOverProject2 =
            new PatternMatchRule(new Node(LeftOuterJoinOp.Pattern,
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverProject);
        /// <summary>
        /// CrossJoin(Project(A), B) => Project(CrossJoin(A, B), modifiedvars)
        /// InnerJoin(Project(A), B, p) => Project(InnerJoin(A, B, p'), modifiedvars)
        /// LeftOuterJoin(Project(A), B, p) => Project(LeftOuterJoin(A, B, p'), modifiedvars)
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="joinNode">Current JoinOp tree to process</param>
        /// <param name="newNode">Transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessJoinOverProject(RuleProcessingContext context, Node joinNode, out Node newNode)
        {
            newNode = joinNode;

            TransformationRulesContext trc = (TransformationRulesContext)context;
            Command command = trc.Command;

            Node joinConditionNode = joinNode.HasChild2 ? joinNode.Child2 : (Node)null;
            Dictionary<Var, int> varRefMap = new Dictionary<Var, int>();
            if (joinConditionNode != null && !trc.IsScalarOpTree(joinConditionNode, varRefMap))
            {
                return false;
            }

            Node newJoinNode;
            Node newProjectNode;

            // Now locate the ProjectOps
            VarVec newVarSet = command.CreateVarVec();
            List<Node> varDefNodes = new List<Node>();

            //
            // Try and handle "project" on both sides only if we're not dealing with 
            // an LOJ. 
            //
            if ((joinNode.Op.OpType != OpType.LeftOuterJoin) &&
                (joinNode.Child0.Op.OpType == OpType.Project) &&
                (joinNode.Child1.Op.OpType == OpType.Project))
            {
                ProjectOp projectOp1 = (ProjectOp)joinNode.Child0.Op;
                ProjectOp projectOp2 = (ProjectOp)joinNode.Child1.Op;

                Dictionary<Var, Node> varMap1 = trc.GetVarMap(joinNode.Child0.Child1, varRefMap);
                Dictionary<Var, Node> varMap2 = trc.GetVarMap(joinNode.Child1.Child1, varRefMap);
                if (varMap1 == null || varMap2 == null)
                {
                    return false;
                }

                if (joinConditionNode != null)
                {
                    joinConditionNode = trc.ReMap(joinConditionNode, varMap1);
                    joinConditionNode = trc.ReMap(joinConditionNode, varMap2);
                    newJoinNode = context.Command.CreateNode(joinNode.Op, joinNode.Child0.Child0, joinNode.Child1.Child0, joinConditionNode);
                }
                else
                {
                    newJoinNode = context.Command.CreateNode(joinNode.Op, joinNode.Child0.Child0, joinNode.Child1.Child0);
                }

                newVarSet.InitFrom(projectOp1.Outputs);
                foreach (Var v in projectOp2.Outputs)
                {
                    newVarSet.Set(v);
                }
                ProjectOp newProjectOp = command.CreateProjectOp(newVarSet);
                varDefNodes.AddRange(joinNode.Child0.Child1.Children);
                varDefNodes.AddRange(joinNode.Child1.Child1.Children);
                Node varDefListNode = command.CreateNode(
                    command.CreateVarDefListOp(),
                    varDefNodes);
                newProjectNode = command.CreateNode(newProjectOp,
                    newJoinNode, varDefListNode);
                newNode = newProjectNode;
                return true;
            }

            int projectNodeIdx = -1;
            int otherNodeIdx = -1;
            if (joinNode.Child0.Op.OpType == OpType.Project)
            {
                projectNodeIdx = 0;
                otherNodeIdx = 1;
            }
            else
            {
                PlanCompiler.Assert(joinNode.Op.OpType != OpType.LeftOuterJoin, "unexpected non-LeftOuterJoin");
                projectNodeIdx = 1;
                otherNodeIdx = 0;
            }
            Node projectNode = joinNode.Children[projectNodeIdx];

            ProjectOp projectOp = projectNode.Op as ProjectOp;
            Dictionary<Var, Node> varMap = trc.GetVarMap(projectNode.Child1, varRefMap);
            if (varMap == null)
            {
                return false;
            }
            ExtendedNodeInfo otherChildInfo = command.GetExtendedNodeInfo(joinNode.Children[otherNodeIdx]);
            VarVec vec = command.CreateVarVec(projectOp.Outputs);
            vec.Or(otherChildInfo.Definitions);
            projectOp.Outputs.InitFrom(vec);
            if (joinConditionNode != null)
            {
                joinConditionNode = trc.ReMap(joinConditionNode, varMap);
                joinNode.Child2 = joinConditionNode;
            }
            joinNode.Children[projectNodeIdx] = projectNode.Child0; // bypass the projectOp
            context.Command.RecomputeNodeInfo(joinNode);

            newNode = context.Command.CreateNode(projectOp, joinNode, projectNode.Child1);
            return true;
        }
        #endregion

        #region JoinOverFilter
        internal static readonly PatternMatchRule Rule_CrossJoinOverFilter1 =
            new PatternMatchRule(new Node(CrossJoinOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern))),
                                 ProcessJoinOverFilter);
        internal static readonly PatternMatchRule Rule_CrossJoinOverFilter2 =
            new PatternMatchRule(new Node(CrossJoinOp.Pattern,
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverFilter);
        internal static readonly PatternMatchRule Rule_InnerJoinOverFilter1 =
            new PatternMatchRule(new Node(InnerJoinOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverFilter);
        internal static readonly PatternMatchRule Rule_InnerJoinOverFilter2 =
            new PatternMatchRule(new Node(InnerJoinOp.Pattern,
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverFilter);
        internal static readonly PatternMatchRule Rule_OuterJoinOverFilter2 =
            new PatternMatchRule(new Node(LeftOuterJoinOp.Pattern,
                                          new Node(FilterOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverFilter);
        /// <summary>
        /// CrossJoin(Filter(A,p), B) => Filter(CrossJoin(A, B), p)
        /// CrossJoin(A, Filter(B,p)) => Filter(CrossJoin(A, B), p)
        /// 
        /// InnerJoin(Filter(A,p), B, c) => Filter(InnerJoin(A, B, c), p)
        /// InnerJoin(A, Filter(B,p), c) => Filter(InnerJoin(A, B, c), p)
        /// 
        /// LeftOuterJoin(Filter(A,p), B, c) => Filter(LeftOuterJoin(A, B, c), p)
        /// 
        /// Note that the predicate on the right table in a left-outer-join cannot be pulled
        /// up above the join.
        /// 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="joinNode">Current JoinOp tree to process</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessJoinOverFilter(RuleProcessingContext context, Node joinNode, out Node newNode)
        {
            newNode = joinNode;
            TransformationRulesContext trc = (TransformationRulesContext)context;
            Command command = trc.Command;

            Node predicateNode = null;
            Node newLeftInput = joinNode.Child0;
            // get the predicate from the first filter
            if (joinNode.Child0.Op.OpType == OpType.Filter)
            {
                predicateNode = joinNode.Child0.Child1;
                newLeftInput = joinNode.Child0.Child0; // bypass the filter
            }

            // get the predicate from the second filter
            Node newRightInput = joinNode.Child1;
            if (joinNode.Child1.Op.OpType == OpType.Filter && joinNode.Op.OpType != OpType.LeftOuterJoin)
            {
                if (predicateNode == null)
                {
                    predicateNode = joinNode.Child1.Child1;
                }
                else
                {
                    predicateNode = command.CreateNode(
                        command.CreateConditionalOp(OpType.And),
                        predicateNode, joinNode.Child1.Child1);
                }
                newRightInput = joinNode.Child1.Child0; // bypass the filter
            }

            // No optimizations to perform if we can't locate the appropriate predicate
            if (predicateNode == null)
            {
                return false;
            }

            //
            // Create a new join node with the new inputs
            //
            Node newJoinNode;
            if (joinNode.Op.OpType == OpType.CrossJoin)
            {
                newJoinNode = command.CreateNode(joinNode.Op, newLeftInput, newRightInput);
            }
            else
            {
                newJoinNode = command.CreateNode(joinNode.Op, newLeftInput, newRightInput, joinNode.Child2);
            }

            //
            // create a new filterOp with the combined predicates, and with the 
            // newjoinNode as the input
            //
            FilterOp newFilterOp = command.CreateFilterOp();
            newNode = command.CreateNode(newFilterOp, newJoinNode, predicateNode);

            //
            // Mark this subtree so that we don't try to push filters down again
            // 
            trc.SuppressFilterPushdown(newNode);
            return true;
        }
        #endregion

        #region Join over SingleRowTable
        internal static readonly PatternMatchRule Rule_CrossJoinOverSingleRowTable1 =
            new PatternMatchRule(new Node(CrossJoinOp.Pattern,
                                          new Node(SingleRowTableOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessJoinOverSingleRowTable);
        internal static readonly PatternMatchRule Rule_CrossJoinOverSingleRowTable2 =
            new PatternMatchRule(new Node(CrossJoinOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(SingleRowTableOp.Pattern)),
                                 ProcessJoinOverSingleRowTable);

        internal static readonly PatternMatchRule Rule_LeftOuterJoinOverSingleRowTable =
           new PatternMatchRule(new Node(LeftOuterJoinOp.Pattern,
                                         new Node(LeafOp.Pattern),
                                         new Node(SingleRowTableOp.Pattern),
                                         new Node(LeafOp.Pattern)),
                                ProcessJoinOverSingleRowTable);
        /// <summary>
        /// Convert a CrossJoin(SingleRowTable, X) or CrossJoin(X, SingleRowTable) or LeftOuterJoin(X, SingleRowTable)
        ///    into just "X"
        /// </summary>
        /// <param name="context">rule processing context</param>
        /// <param name="joinNode">the join node</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessJoinOverSingleRowTable(RuleProcessingContext context, Node joinNode, out Node newNode)
        {
            newNode = joinNode;

            if (joinNode.Child0.Op.OpType == OpType.SingleRowTable)
            {
                newNode = joinNode.Child1;
            }
            else
            {
                newNode = joinNode.Child0;
            }
            return true;
        }
        #endregion

        #region Misc
        #endregion

        #region All JoinOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
            Rule_CrossJoinOverProject1,
            Rule_CrossJoinOverProject2,
            Rule_InnerJoinOverProject1,
            Rule_InnerJoinOverProject2,
            Rule_OuterJoinOverProject2,

            Rule_CrossJoinOverFilter1,
            Rule_CrossJoinOverFilter2,
            Rule_InnerJoinOverFilter1,
            Rule_InnerJoinOverFilter2,
            Rule_OuterJoinOverFilter2,

            Rule_CrossJoinOverSingleRowTable1,
            Rule_CrossJoinOverSingleRowTable2,
            Rule_LeftOuterJoinOverSingleRowTable,
        };

        #endregion
    }
    #endregion

    #region SingleRowOp Rules
    /// <summary>
    /// Rules for SingleRowOp
    /// </summary>
    internal static class SingleRowOpRules
    {
        internal static readonly PatternMatchRule Rule_SingleRowOpOverAnything =
            new PatternMatchRule(new Node(SingleRowOp.Pattern,
                                     new Node(LeafOp.Pattern)),
                                 ProcessSingleRowOpOverAnything);
        /// <summary>
        /// Convert a 
        ///    SingleRowOp(X) => X
        /// if X produces at most one row
        /// </summary>
        /// <param name="context">Rule Processing context</param>
        /// <param name="singleRowNode">Current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessSingleRowOpOverAnything(RuleProcessingContext context, Node singleRowNode, out Node newNode)
        {
            newNode = singleRowNode;
            TransformationRulesContext trc = (TransformationRulesContext)context;
            ExtendedNodeInfo childNodeInfo = context.Command.GetExtendedNodeInfo(singleRowNode.Child0);

            // If the input to this Op can produce at most one row, then we don't need the
            // singleRowOp - simply return the input
            if (childNodeInfo.MaxRows <= RowCount.One)
            {
                newNode = singleRowNode.Child0;
                return true;
            }

            //
            // if the current node is a FilterOp, then try and determine if the FilterOp
            // produces one row at most
            //
            if (singleRowNode.Child0.Op.OpType == OpType.Filter)
            {
                Predicate predicate = new Predicate(context.Command, singleRowNode.Child0.Child1);
                if (predicate.SatisfiesKey(childNodeInfo.Keys.KeyVars, childNodeInfo.Definitions))
                {
                    childNodeInfo.MaxRows = RowCount.One;
                    newNode = singleRowNode.Child0;
                    return true;
                }
            }

            // we couldn't do anything
            return false;
        }

        internal static readonly PatternMatchRule Rule_SingleRowOpOverProject =
           new PatternMatchRule(new Node(SingleRowOp.Pattern,
                             new Node(ProjectOp.Pattern,
                                   new Node(LeafOp.Pattern), new Node(LeafOp.Pattern))),
                         ProcessSingleRowOpOverProject);
        /// <summary>
        /// Convert 
        ///    SingleRowOp(Project) => Project(SingleRowOp)
        /// </summary>
        /// <param name="context">Rule Processing context</param>
        /// <param name="singleRowNode">current subtree</param>
        /// <param name="newNode">transformeed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessSingleRowOpOverProject(RuleProcessingContext context, Node singleRowNode, out Node newNode)
        {
            newNode = singleRowNode;
            Node projectNode = singleRowNode.Child0;
            Node projectNodeInput = projectNode.Child0;

            // Simply push the SingleRowOp below the ProjectOp
            singleRowNode.Child0 = projectNodeInput;
            context.Command.RecomputeNodeInfo(singleRowNode);
            projectNode.Child0 = singleRowNode;

            newNode = projectNode;
            return true; // subtree modified internally
        }

        #region All SingleRowOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
            Rule_SingleRowOpOverAnything,
            Rule_SingleRowOpOverProject,
        };
        #endregion
    }
    #endregion

    #region SetOp Rules
    /// <summary>
    /// SetOp Transformation Rules
    /// </summary>
    internal static class SetOpRules
    {
        #region SetOpOverFilters
        internal static readonly SimpleRule Rule_UnionAllOverEmptySet =
            new SimpleRule(OpType.UnionAll, ProcessSetOpOverEmptySet);
        internal static readonly SimpleRule Rule_IntersectOverEmptySet =
            new SimpleRule(OpType.Intersect, ProcessSetOpOverEmptySet);
        internal static readonly SimpleRule Rule_ExceptOverEmptySet =
            new SimpleRule(OpType.Except, ProcessSetOpOverEmptySet);

        /// <summary>
        /// Process a SetOp when one of the inputs is an emptyset. 
        /// 
        /// An emptyset is represented by a Filter(X, ConstantPredicate)
        ///    where the ConstantPredicate has a value of "false"
        /// 
        /// The general rules are
        ///    UnionAll(X, EmptySet) => X
        ///    UnionAll(EmptySet, X) => X
        ///    Intersect(EmptySet, X) => EmptySet
        ///    Intersect(X, EmptySet) => EmptySet
        ///    Except(EmptySet, X) => EmptySet
        ///    Except(X, EmptySet) => X
        /// 
        /// These rules then translate into 
        ///    UnionAll: return the non-empty input
        ///    Intersect: return the empty input
        ///    Except: return the "left" input 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="setOpNode">the current setop tree</param>
        /// <param name="filterNodeIndex">Index of the filter node in the setop</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        private static bool ProcessSetOpOverEmptySet(RuleProcessingContext context, Node setOpNode, out Node newNode)
        {
            bool leftChildIsEmptySet = context.Command.GetExtendedNodeInfo(setOpNode.Child0).MaxRows == RowCount.Zero;
            bool rightChildIsEmptySet = context.Command.GetExtendedNodeInfo(setOpNode.Child1).MaxRows == RowCount.Zero;

            if (!leftChildIsEmptySet && !rightChildIsEmptySet)
            {
                newNode = setOpNode;
                return false;
            }
    
            int indexToReturn;
            SetOp setOp = (SetOp)setOpNode.Op;
            if (!rightChildIsEmptySet && setOp.OpType == OpType.UnionAll ||
                !leftChildIsEmptySet && setOp.OpType == OpType.Intersect)
            {
                indexToReturn = 1;
            }
            else
            {
                indexToReturn = 0;
            }

            newNode = setOpNode.Children[indexToReturn];           

            TransformationRulesContext trc = (TransformationRulesContext)context;
            foreach (KeyValuePair<Var, Var> kv in setOp.VarMap[indexToReturn])
            {
                trc.AddVarMapping(kv.Key, kv.Value);
            }
            return true;
        }

        #endregion

        #region All SetOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
            Rule_UnionAllOverEmptySet,
            Rule_IntersectOverEmptySet,
            Rule_ExceptOverEmptySet,
        };
        #endregion
    }
    #endregion

    #region GroupByOp Rules
    /// <summary>
    /// Transformation Rules for GroupByOps
    /// </summary>
    internal static class GroupByOpRules
    {
        #region GroupByOpWithSimpleVarRedefinitions
        internal static readonly SimpleRule Rule_GroupByOpWithSimpleVarRedefinitions = new SimpleRule(OpType.GroupBy, ProcessGroupByWithSimpleVarRedefinitions);
        /// <summary>
        /// If the GroupByOp defines some computedVars as part of its keys, but those computedVars are simply 
        /// redefinitions of other Vars, then eliminate the computedVars. 
        /// 
        /// GroupBy(X, VarDefList(VarDef(cv1, VarRef(v1)), ...), VarDefList(...))
        ///    can be transformed into
        /// GroupBy(X, VarDefList(...))
        /// where cv1 has now been replaced by v1
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessGroupByWithSimpleVarRedefinitions(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            GroupByOp groupByOp = (GroupByOp)n.Op;
            // no local keys? nothing to do
            if (n.Child1.Children.Count == 0)
            {
                return false;
            }

            TransformationRulesContext trc = (TransformationRulesContext)context;
            Command command = trc.Command;

            ExtendedNodeInfo nodeInfo = command.GetExtendedNodeInfo(n);

            //
            // Check to see if any of the computed Vars defined by this GroupByOp
            // are simple redefinitions of other VarRefOps. Consider only those 
            // VarRefOps that are not "external" references
            //
            bool canEliminateSomeVars = false;
            foreach (Node varDefNode in n.Child1.Children)
            {
                Node definingExprNode = varDefNode.Child0;
                if (definingExprNode.Op.OpType == OpType.VarRef)
                {
                    VarRefOp varRefOp = (VarRefOp)definingExprNode.Op;
                    if (!nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                    {
                        // this is a Var that we should remove 
                        canEliminateSomeVars = true;
                    }
                }
            }

            // Did we have any redefinitions
            if (!canEliminateSomeVars)
            {
                return false;
            }

            //
            // OK. We've now identified a set of vars that are simple redefinitions.
            // Try and replace the computed Vars with the Vars that they're redefining
            //

            // Lets now build up a new VarDefListNode
            List<Node> newVarDefNodes = new List<Node>();
            foreach (Node varDefNode in n.Child1.Children)
            {
                VarDefOp varDefOp = (VarDefOp)varDefNode.Op;
                VarRefOp varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp != null && !nodeInfo.ExternalReferences.IsSet(varRefOp.Var))
                {
                    groupByOp.Outputs.Clear(varDefOp.Var);
                    groupByOp.Outputs.Set(varRefOp.Var);
                    groupByOp.Keys.Clear(varDefOp.Var);
                    groupByOp.Keys.Set(varRefOp.Var);
                    trc.AddVarMapping(varDefOp.Var, varRefOp.Var);
                }
                else
                {
                    newVarDefNodes.Add(varDefNode);
                }
            }

            // Create a new vardeflist node, and set that as Child1 for the group by op
            Node newVarDefListNode = command.CreateNode(command.CreateVarDefListOp(), newVarDefNodes);
            n.Child1 = newVarDefListNode;
            return true; // subtree modified
        }
        #endregion

        #region GroupByOverProject
        internal static readonly PatternMatchRule Rule_GroupByOverProject =
            new PatternMatchRule(new Node(GroupByOp.Pattern,
                                          new Node(ProjectOp.Pattern,
                                                   new Node(LeafOp.Pattern),
                                                   new Node(LeafOp.Pattern)),
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern)),
                                 ProcessGroupByOverProject);
        /// <summary>
        /// Converts a GroupBy(Project(X, c1,..ck), agg1, agg2, .. aggm) => 
        ///            GroupBy(X, agg1', agg2', .. aggm')
        /// where agg1', agg2', .. aggm'  are the "mapped" versions 
        /// of agg1, agg2, .. aggm, such that the references to c1, ... ck are 
        /// replaced by their definitions.
        /// 
        /// We only do this if each c1, ..ck is refereneced (in aggregates) at most once or it is a constant. 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="projectNode">Current ProjectOp node</param>
        /// <param name="newNode">modified subtree</param>
        /// <returns>Transformation status</returns>
        static bool ProcessGroupByOverProject(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;
            GroupByOp op = (GroupByOp)n.Op;
            Command command = ((TransformationRulesContext)context).Command;
            Node projectNode = n.Child0;
            Node projectNodeVarDefList = projectNode.Child1;

            Node keys = n.Child1;
            Node aggregates = n.Child2;

            // If there are any keys, we should not remove the inner project
            if (keys.Children.Count > 0)
            {
                return false;
            }

            //Get a list of all defining vars
            VarVec projectDefinitions = command.GetExtendedNodeInfo(projectNode).LocalDefinitions;

            //If any of the defined vars is output, than we need the extra project anyway.
            if (op.Outputs.Overlaps(projectDefinitions))
            {
                return false;
            }

            bool createdNewProjectDefinitions = false;

            //If there are any constants remove them from the list that needs to be tested,
            //These can safely be replaced
            for (int i = 0; i < projectNodeVarDefList.Children.Count; i++)
            {
                Node varDefNode = projectNodeVarDefList.Children[i];
                if (varDefNode.Child0.Op.OpType == OpType.Constant || varDefNode.Child0.Op.OpType == OpType.InternalConstant || varDefNode.Child0.Op.OpType == OpType.NullSentinel)
                {
                    //We shouldn't modify the original project definitions, thus we copy it  
                    // the first time we encounter a constant
                    if (!createdNewProjectDefinitions)
                    {
                        projectDefinitions = command.CreateVarVec(projectDefinitions);
                        createdNewProjectDefinitions = true;
                    }
                    projectDefinitions.Clear(((VarDefOp)varDefNode.Op).Var);
                }
            }

            if (VarRefUsageFinder.AnyVarUsedMoreThanOnce(projectDefinitions, aggregates, command))
            {
                return false;
            }

            //If we got here it means that all vars were either constants, or used at most once
            // Create a dictionary to be used for remapping the keys and the aggregates
            Dictionary<Var, Node> varToDefiningNode = new Dictionary<Var, Node>(projectNodeVarDefList.Children.Count);
            for (int j = 0; j < projectNodeVarDefList.Children.Count; j++)
            {
                Node varDefNode = projectNodeVarDefList.Children[j];
                Var var = ((VarDefOp)varDefNode.Op).Var;
                varToDefiningNode.Add(var, varDefNode.Child0);
            }

            newNode.Child2 = VarRefReplacer.Replace(varToDefiningNode, aggregates, command);

            newNode.Child0 = projectNode.Child0;
            return true;
        }

        /// <summary>
        /// Replaces each occurance of the given vars with their definitions.
        /// </summary>
        internal class VarRefReplacer : BasicOpVisitorOfNode
        {
            private Dictionary<Var, Node> m_varReplacementTable;
            private Command m_command;

            private VarRefReplacer(Dictionary<Var, Node> varReplacementTable, Command command)
            {
                this.m_varReplacementTable = varReplacementTable;
                this.m_command = command;
            }

            /// <summary>
            /// "Public" entry point. In the subtree rooted at the given root, 
            /// replace each occurance of the given vars with their definitions, 
            /// where each key-value pair in the dictionary is a var-definition pair.
            /// </summary>
            /// <param name="varReplacementTable"></param>
            /// <param name="root"></param>
            /// <param name="command"></param>
            /// <returns></returns>
            internal static Node Replace(Dictionary<Var, Node> varReplacementTable, Node root, Command command)
            {
                VarRefReplacer replacer = new VarRefReplacer(varReplacementTable, command);
                return replacer.VisitNode(root);
            }

            public override Node Visit(VarRefOp op, Node n)
            {
                Node replacementNode;
                if (m_varReplacementTable.TryGetValue(op.Var, out replacementNode))
                {
                    return replacementNode;
                }
                else
                {
                    return n;
                }
            }

            /// <summary>
            /// Recomputes node info post regular processing.
            /// </summary>
            /// <param name="n"></param>
            /// <returns></returns>
            protected override Node VisitDefault(Node n)
            {
                Node result = base.VisitDefault(n);
                m_command.RecomputeNodeInfo(result);
                return result;
            }
        }

        /// <summary>
        /// Used to determine whether any of the given vars occurs more than once 
        /// in a given subtree.
        /// </summary>
        internal class VarRefUsageFinder : BasicOpVisitor
        {
            private bool m_anyUsedMoreThenOnce = false;
            private VarVec m_varVec;
            private VarVec m_usedVars;

            private VarRefUsageFinder(VarVec varVec, Command command)
            {
                this.m_varVec = varVec;
                this.m_usedVars = command.CreateVarVec();
            }

            /// <summary>
            /// Public entry point. Returns true if at least one of the given vars occurs more than 
            /// once in the subree rooted at the given root.
            /// </summary>
            /// <param name="varVec"></param>
            /// <param name="root"></param>
            /// <param name="command"></param>
            /// <returns></returns>
            internal static bool AnyVarUsedMoreThanOnce(VarVec varVec, Node root, Command command)
            {
                VarRefUsageFinder usageFinder = new VarRefUsageFinder(varVec, command);
                usageFinder.VisitNode(root);
                return usageFinder.m_anyUsedMoreThenOnce;
            }

            public override void Visit(VarRefOp op, Node n)
            {
                Var referencedVar = op.Var;
                if (m_varVec.IsSet(referencedVar))
                {
                    if (m_usedVars.IsSet(referencedVar))
                    {
                        this.m_anyUsedMoreThenOnce = true;
                    }
                    else
                    {
                        m_usedVars.Set(referencedVar);
                    }
                }
            }

            protected override void VisitChildren(Node n)
            {
                //small optimization: no need to continue if we have the answer
                if (m_anyUsedMoreThenOnce)
                {
                    return;
                }
                base.VisitChildren(n);
            }
        }
        #endregion

        #region GroupByOpWithNoAggregates
        internal static readonly PatternMatchRule Rule_GroupByOpWithNoAggregates =
            new PatternMatchRule(new Node(GroupByOp.Pattern,
                                          new Node(LeafOp.Pattern),
                                          new Node(LeafOp.Pattern),
                                          new Node(VarDefListOp.Pattern)),
                                 ProcessGroupByOpWithNoAggregates);        
        /// <summary>
        /// If the GroupByOp has no aggregates:
        /// 
        /// (1) and if it includes all all the keys of the input, than it is unnecessary
        /// GroupBy (X, keys) -> Project(X, keys) where keys includes all keys of X.
        /// 
        /// (2) else it can be turned into a Distinct:
        /// GroupBy (X, keys) -> Distinct(X, keys)
        /// 
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessGroupByOpWithNoAggregates(RuleProcessingContext context, Node n, out Node newNode)
        {
            Command command = context.Command;
            GroupByOp op = (GroupByOp)n.Op;

            ExtendedNodeInfo nodeInfo = command.GetExtendedNodeInfo(n.Child0);
            ProjectOp newOp = command.CreateProjectOp(op.Keys);

            VarDefListOp varDefListOp = command.CreateVarDefListOp();
            Node varDefListNode = command.CreateNode(varDefListOp);

            newNode = command.CreateNode(newOp, n.Child0, n.Child1);
            
            //If we know the keys of the input and the list of keys includes them all, 
            // this is the result, otherwise add distinct
            if (nodeInfo.Keys.NoKeys || !op.Keys.Subsumes(nodeInfo.Keys.KeyVars))
            {
                newNode = command.CreateNode(command.CreateDistinctOp(command.CreateVarVec(op.Keys)), newNode);
            }
            return true;       
        }
        #endregion

        #region GroupByOpOnAllInputColumnsWithAggregateOperation

        internal static readonly SimpleRule Rule_GroupByOpOnAllInputColumnsWithAggregateOperation = new SimpleRule(
            OpType.GroupBy, ProcessGroupByOpOnAllInputColumnsWithAggregateOperation);

        /// <summary>
        /// Converts a GroupBy(X, Y, Z) => OuterApply(X', GroupBy(Filter(X, key(X') == key(X)), Y, Z))
        /// if and only if X is a ScanTableOp, and Z is the upper node of an aggregate function and
        /// the group by operation uses all the columns of X as the key.
        /// Additionally, the top-level physical projection must only expose one variable. If it exposes
        /// more than one (more than just the aggregate itself), then this rule must not apply.
        /// This is a fix for devdiv bug 851732. Since now we're supporting NewRecordOp nodes as
        /// part of the GroupBy aggregate variable computations, we are also respecting the fact that
        /// group by (e => e) means that we're grouping by all columns of entity e. This was not a
        /// problem when the NewRecordOp node was not being processed since this caused the GroupBy
        /// statement to be simplified to a form with no keys and no output columns. The generated SQL
        /// is correct, but it is different from what it used to be and may be incompatible if the
        /// entity contains fields with datatypes that do not support being grouped by, such as blobs
        /// and images.
        /// This rule simplifies the tree so that we remain compatible with the way we were generating
        /// queries that contain group by (e => e).
        /// What this does is enabling the tree to take a shape that further optimization can convert
        /// into an expression that groups by the key of the table and calls the aggregate function
        /// as expected.
        /// </summary>
        /// <param name="context"> Rule processing context </param>
        /// <param name="n"> Current ProjectOp node </param>
        /// <param name="newNode"> modified subtree </param>
        /// <returns> Transformation status </returns>
        private static bool ProcessGroupByOpOnAllInputColumnsWithAggregateOperation(RuleProcessingContext context, Node n, out Node newNode)
        {
            newNode = n;

            var rootOp = context.Command.Root.Op as PhysicalProjectOp;
            if (rootOp == null ||
                rootOp.Outputs.Count > 1)
            {
                return false;
            }

            if (n.Child0.Op.OpType != OpType.ScanTable)
            {
                return false;
            }

            if (n.Child2 == null
                || n.Child2.Child0 == null
                || n.Child2.Child0.Child0 == null
                || n.Child2.Child0.Child0.Op.OpType != OpType.Aggregate)
            {
                return false;
            }

            var groupByOp = (GroupByOp)n.Op;

            var sourceTable = ((ScanTableOp)n.Child0.Op).Table;
            var allInputColumns = sourceTable.Columns;

            // Exit if the group's keys do not contain all the columns defined by Child0
            foreach (var column in allInputColumns)
            {
                if (!groupByOp.Keys.IsSet(column))
                {
                    return false;
                }
            }

            // All the columns of Child0 are used, so remove them from the outputs and the keys
            foreach (var column in allInputColumns)
            {
                groupByOp.Outputs.Clear(column);
                groupByOp.Keys.Clear(column);
            }

            // Build the OuterApply and also set the filter around the GroupBy's scan table.
            var command = context.Command;

            var scanTableOp = command.CreateScanTableOp(sourceTable.TableMetadata);
            var scanTable = command.CreateNode(scanTableOp);
            var outerApplyNode = command.CreateNode(command.CreateOuterApplyOp(), scanTable, n);

            Var newVar;
            var varDefListNode = command.CreateVarDefListNode(command.CreateNode(command.CreateVarRefOp(groupByOp.Outputs.First)), out newVar);

            newNode = command.CreateNode(
                    command.CreateProjectOp(newVar),
                    outerApplyNode,
                    varDefListNode);

            Node equality = null;
            var leftKeys = scanTableOp.Table.Keys.GetEnumerator();
            var rightKeys = sourceTable.Keys.GetEnumerator();
            for (int i = 0; i < sourceTable.Keys.Count; ++i)
            {
                leftKeys.MoveNext();
                rightKeys.MoveNext();
                var comparison = command.CreateNode(
                                    command.CreateComparisonOp(OpType.EQ),
                                    command.CreateNode(command.CreateVarRefOp(leftKeys.Current)),
                                    command.CreateNode(command.CreateVarRefOp(rightKeys.Current)));
                if (equality != null)
                {
                    equality = command.CreateNode(
                                    command.CreateConditionalOp(OpType.And),
                                    equality, comparison);
                }
                else
                {
                    equality = comparison;
                }
            }

            var filter = command.CreateNode(command.CreateFilterOp(),
                        n.Child0,
                        equality);
            n.Child0 = filter;

            return true; // subtree modified
        }

        #endregion

        #region All GroupByOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
                 GroupByOpRules.Rule_GroupByOpWithSimpleVarRedefinitions,
                 GroupByOpRules.Rule_GroupByOverProject,
                 GroupByOpRules.Rule_GroupByOpWithNoAggregates,
                 GroupByOpRules.Rule_GroupByOpOnAllInputColumnsWithAggregateOperation,
        };
        #endregion
    }
    #endregion

    #region Sorting Rules
    /// <summary>
    /// Transformation Rules for SortOp
    /// </summary>
    internal static class SortOpRules
    {
        #region SortOpOverAtMostOneRow
        internal static readonly SimpleRule Rule_SortOpOverAtMostOneRow = new SimpleRule(OpType.Sort, ProcessSortOpOverAtMostOneRow);
        /// <summary>
        /// If the SortOp's input is guaranteed to produce at most 1 row, remove the node with the SortOp:
        ///  Sort(X) => X, if X is guaranteed to produce no more than 1 row
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessSortOpOverAtMostOneRow(RuleProcessingContext context, Node n, out Node newNode)
        {
            ExtendedNodeInfo nodeInfo = ((TransformationRulesContext)context).Command.GetExtendedNodeInfo(n.Child0);

            //If the input has at most one row, omit the SortOp
            if (nodeInfo.MaxRows == RowCount.Zero || nodeInfo.MaxRows == RowCount.One)
            {
                newNode = n.Child0;
                return true;
            }

            //Otherwise return the node as is
            newNode = n;
            return false;
        }
        #endregion

        #region All SortOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
                 SortOpRules.Rule_SortOpOverAtMostOneRow,
        };
        #endregion
    }

    /// <summary>
    /// Transformation Rules for ConstrainedSortOp
    /// </summary>
    internal static class ConstrainedSortOpRules
    {
        #region ConstrainedSortOpOverEmptySet
        internal static readonly SimpleRule Rule_ConstrainedSortOpOverEmptySet = new SimpleRule(OpType.ConstrainedSort, ProcessConstrainedSortOpOverEmptySet);
        /// <summary>
        /// If the ConstrainedSortOp's input is guaranteed to produce no rows, remove the ConstrainedSortOp completly:
        ///    CSort(EmptySet) => EmptySet
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessConstrainedSortOpOverEmptySet(RuleProcessingContext context, Node n, out Node newNode)
        {
            ExtendedNodeInfo nodeInfo = ((TransformationRulesContext)context).Command.GetExtendedNodeInfo(n.Child0);

            //If the input has no rows, remove the ConstraintSortOp node completly
            if (nodeInfo.MaxRows == RowCount.Zero)
            {
                newNode = n.Child0;
                return true;
            }

            newNode = n;
            return false;
        }
        #endregion

        #region All ConstrainedSortOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
                 ConstrainedSortOpRules.Rule_ConstrainedSortOpOverEmptySet,
        };
        #endregion
    }
    #endregion

    #region DistinctOp Rules
    /// <summary>
    /// Transformation Rules for DistinctOp
    /// </summary>
    internal static class DistinctOpRules
    {
        #region DistinctOpOfKeys
        internal static readonly SimpleRule Rule_DistinctOpOfKeys = new SimpleRule(OpType.Distinct, ProcessDistinctOpOfKeys);
        /// <summary>
        /// If the DistinctOp includes all all the keys of the input, than it is unnecessary.
        /// Distinct (X, distinct_keys) -> Project( X, distinct_keys) where distinct_keys includes all keys of X.
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="n">current subtree</param>
        /// <param name="newNode">transformed subtree</param>
        /// <returns>transformation status</returns>
        static bool ProcessDistinctOpOfKeys(RuleProcessingContext context, Node n, out Node newNode)
        {
            Command command = context.Command;

            ExtendedNodeInfo nodeInfo = command.GetExtendedNodeInfo(n.Child0);

            DistinctOp op = (DistinctOp)n.Op;

            //If we know the keys of the input and the list of distinct keys includes them all, omit the distinct
            if (!nodeInfo.Keys.NoKeys && op.Keys.Subsumes(nodeInfo.Keys.KeyVars))
            {
                ProjectOp newOp = command.CreateProjectOp(op.Keys);

                //Create empty vardef list
                VarDefListOp varDefListOp = command.CreateVarDefListOp();
                Node varDefListNode = command.CreateNode(varDefListOp);

                newNode = command.CreateNode(newOp, n.Child0, varDefListNode);
                return true;
            }

            //Otherwise return the node as is
            newNode = n;
            return false;
        }
        #endregion

        #region All DistinctOp Rules
        internal static readonly InternalTrees.Rule[] Rules = new InternalTrees.Rule[] {
                 DistinctOpRules.Rule_DistinctOpOfKeys,
        };
        #endregion
    }
    #endregion
}

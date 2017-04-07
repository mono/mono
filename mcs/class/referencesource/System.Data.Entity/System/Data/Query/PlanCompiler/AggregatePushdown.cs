//---------------------------------------------------------------------
// <copyright file="AggregatePushdown.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
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

using System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{
    internal delegate bool TryGetValue(Node key, out Node value);

    #region Helper Classes
    /// <summary>
    /// Helper class to track the aggregate nodes that are candidates to be 
    /// pushed into the definingGroupByNode.
    /// </summary>
    internal class GroupAggregateVarInfo
    {
        #region Private Fields
        private readonly Node _definingGroupByNode;
        private HashSet<KeyValuePair<Node, Node>> _candidateAggregateNodes;
        private readonly Var _groupAggregateVar;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="defingingGroupNode">The GroupIntoOp node</param>
        /// <param name="groupAggregateVar"></param>
        internal GroupAggregateVarInfo(Node defingingGroupNode, Var groupAggregateVar)
        {
            _definingGroupByNode = defingingGroupNode;
            _groupAggregateVar = groupAggregateVar;
        }
        #endregion

        #region 'Public' Properties
        /// <summary>
        /// Each key value pair represents a candidate aggregate. 
        /// The key is the function aggregate subtree and the value is a 'template' of translation of the 
        /// function aggregate's argument over the var representing the group aggregate.
        /// A valid candidate has an argument that does not have any external references
        /// except for the group aggregate corresponding to the DefiningGroupNode.
        /// </summary>
        internal HashSet<KeyValuePair<Node, Node>> CandidateAggregateNodes
        {
            get
            {
                if (_candidateAggregateNodes == null)
                {
                    _candidateAggregateNodes = new HashSet<KeyValuePair<Node, Node>>();
                }
                return _candidateAggregateNodes;
            }
        }

        /// <summary>
        /// Are there are agregates that are candidates to be pushed into the DefiningGroupNode
        /// </summary>
        internal bool HasCandidateAggregateNodes
        {
            get
            {
                return (_candidateAggregateNodes != null && _candidateAggregateNodes.Count != 0);
            }
        }

        /// <summary>
        /// The GroupIntoOp node that this GroupAggregateVarInfo represents
        /// </summary>
        internal Node DefiningGroupNode
        {
            get { return _definingGroupByNode; }
        }

        internal Var GroupAggregateVar
        {
            get { return _groupAggregateVar; }
        }
        #endregion
    }

    /// <summary>
    /// Helper class to track usage of GroupAggregateVarInfo
    /// It represents the usage of a single GroupAggregateVar.
    /// The usage is defined by the computation, it should be a subree whose only 
    /// external reference is the group var represented by the GroupAggregateVarInfo.
    /// </summary>
    internal class GroupAggregateVarRefInfo
    {
        #region Private fields
        private readonly Node _computation;
        private readonly GroupAggregateVarInfo _groupAggregateVarInfo;
        private readonly bool _isUnnested;
        #endregion

        #region Constructor
        /// <summary>
        /// Public constructor
        /// </summary>
        /// <param name="groupAggregateVarInfo"></param>
        /// <param name="computation"></param>
        internal GroupAggregateVarRefInfo(GroupAggregateVarInfo groupAggregateVarInfo, Node computation, bool isUnnested)
        {
            this._groupAggregateVarInfo = groupAggregateVarInfo;
            this._computation = computation;
            this._isUnnested = isUnnested;
        }

        #endregion

        #region 'Public' Properties
        /// <summary>
        /// Subtree whose only external reference is 
        /// the group var represented by the GroupAggregateVarInfo
        /// </summary>
        internal Node Computation
        {
            get { return _computation; }
        }

        /// <summary>
        /// The GroupAggregateVarInfo (possibly) referenced by the computation
        /// </summary>
        internal GroupAggregateVarInfo GroupAggregateVarInfo
        {
            get { return _groupAggregateVarInfo; }
        }

        /// <summary>
        /// Is the computation over unnested group aggregate var
        /// </summary>
        internal bool IsUnnested
        {
            get { return _isUnnested; }
        }
        #endregion
    }

    /// <summary>
    /// Manages refereces to groupAggregate variables.
    /// </summary>
    internal class GroupAggregateVarInfoManager
    {
        #region Private state
        private readonly Dictionary<Var, GroupAggregateVarRefInfo> _groupAggregateVarRelatedVarToInfo = new Dictionary<Var, GroupAggregateVarRefInfo>();
        private Dictionary<Var, Dictionary<EdmMember, GroupAggregateVarRefInfo>> _groupAggregateVarRelatedVarPropertyToInfo;
        private HashSet<GroupAggregateVarInfo> _groupAggregateVarInfos = new HashSet<GroupAggregateVarInfo>();
        #endregion

        #region Public Surface
        /// <summary>
        /// Get all the groupAggregateVarInfos
        /// </summary>
        internal IEnumerable<GroupAggregateVarInfo> GroupAggregateVarInfos
        {
            get
            {
                return _groupAggregateVarInfos;
            }
        }

        /// <summary>
        /// Add an entry that var is a computation represented by the computationTemplate
        /// over the var represented by the given groupAggregateVarInfo
        /// </summary>
        /// <param name="var"></param>
        /// <param name="groupAggregateVarInfo"></param>
        /// <param name="computationTemplate"></param>
        /// <param name="isUnnested"></param>
        internal void Add(Var var, GroupAggregateVarInfo groupAggregateVarInfo, Node computationTemplate, bool isUnnested)
        {
            this._groupAggregateVarRelatedVarToInfo.Add(var, new GroupAggregateVarRefInfo(groupAggregateVarInfo, computationTemplate, isUnnested));
            _groupAggregateVarInfos.Add(groupAggregateVarInfo);
        }

        /// <summary>
        /// Add an entry that the given property of the given var is a computation represented 
        /// by the computationTemplate over the var represented by the given groupAggregateVarInfo        
        /// </summary>
        /// <param name="var"></param>
        /// <param name="groupAggregateVarInfo"></param>
        /// <param name="computationTemplate"></param>
        /// <param name="isUnnested"></param>
        /// <param name="property"></param>
        internal void Add(Var var, GroupAggregateVarInfo groupAggregateVarInfo, Node computationTemplate, bool isUnnested, EdmMember property)
        {
            if (property == null)
            {
                Add(var, groupAggregateVarInfo, computationTemplate, isUnnested);
                return;
            }
            if (this._groupAggregateVarRelatedVarPropertyToInfo == null)
            {
                this._groupAggregateVarRelatedVarPropertyToInfo = new Dictionary<Var, Dictionary<System.Data.Metadata.Edm.EdmMember, GroupAggregateVarRefInfo>>();
            }
            Dictionary<EdmMember, GroupAggregateVarRefInfo> varPropertyDictionary;
            if (!_groupAggregateVarRelatedVarPropertyToInfo.TryGetValue(var, out varPropertyDictionary))
            {
                varPropertyDictionary = new Dictionary<System.Data.Metadata.Edm.EdmMember, GroupAggregateVarRefInfo>();
                _groupAggregateVarRelatedVarPropertyToInfo.Add(var, varPropertyDictionary);
            }
            varPropertyDictionary.Add(property, new GroupAggregateVarRefInfo(groupAggregateVarInfo, computationTemplate, isUnnested));

            // Note: The following line is not necessary with the current usage pattern, this method is 
            // never called with a new groupAggregateVarInfo thus it is a no-op.
            _groupAggregateVarInfos.Add(groupAggregateVarInfo);
        }

        /// <summary>
        /// Gets the groupAggregateVarRefInfo representing the definition of the given var over 
        /// a group aggregate var if any.
        /// </summary>
        /// <param name="var"></param>
        /// <param name="groupAggregateVarRefInfo"></param>
        /// <returns></returns>
        internal bool TryGetReferencedGroupAggregateVarInfo(Var var, out GroupAggregateVarRefInfo groupAggregateVarRefInfo)
        {
            return this._groupAggregateVarRelatedVarToInfo.TryGetValue(var, out groupAggregateVarRefInfo);
        }

        /// <summary>
        /// Gets the groupAggregateVarRefInfo representing the definition of the given property of the given
        /// var over a group aggregate var if any.        
        /// </summary>
        /// <param name="var"></param>
        /// <param name="property"></param>
        /// <param name="groupAggregateVarRefInfo"></param>
        /// <returns></returns>
        internal bool TryGetReferencedGroupAggregateVarInfo(Var var, EdmMember property, out GroupAggregateVarRefInfo groupAggregateVarRefInfo)
        {
            if (property == null)
            {
                return TryGetReferencedGroupAggregateVarInfo(var, out groupAggregateVarRefInfo);
            }

            Dictionary<EdmMember, GroupAggregateVarRefInfo> varPropertyDictionary;
            if (_groupAggregateVarRelatedVarPropertyToInfo == null || !_groupAggregateVarRelatedVarPropertyToInfo.TryGetValue(var, out varPropertyDictionary))
            {
                groupAggregateVarRefInfo = null;
                return false;
            }
            return varPropertyDictionary.TryGetValue(property, out groupAggregateVarRefInfo);
        }
        #endregion
    }
    #endregion

    /// <summary>
    /// Utility class that tries to produce an equivalent tree to the input tree over 
    /// a single group aggregate variable and no other external references
    /// </summary>
    internal class GroupAggregateVarComputationTranslator : BasicOpVisitorOfNode
    {
        #region Private State
        private GroupAggregateVarInfo _targetGroupAggregateVarInfo;
        private bool _isUnnested;
        private readonly Command _command;
        private readonly GroupAggregateVarInfoManager _groupAggregateVarInfoManager;
        #endregion

        #region Constructor
        /// <summary>
        /// Private constructor 
        /// </summary>
        /// <param name="command"></param>
        /// <param name="groupAggregateVarInfoManager"></param>
        private GroupAggregateVarComputationTranslator(
            Command command,
            GroupAggregateVarInfoManager groupAggregateVarInfoManager)
        {
            this._command = command;
            this._groupAggregateVarInfoManager = groupAggregateVarInfoManager;
        }
        #endregion

        #region 'Public' Surface
        /// <summary>
        /// Try to produce an equivalent tree to the input subtree, over a single group aggregate variable.
        /// Such translation can only be produced if all external references of the input subtree are to a 
        /// single group aggregate var, or to vars that are can be translated over that single group 
        /// aggregate var
        /// </summary>
        /// <param name="subtree">The input subtree</param>
        /// <param name="isVarDefinition"></param>
        /// <param name="command"></param>
        /// <param name="groupAggregateVarInfoManager"></param>
        /// <param name="groupAggregateVarInfo">The groupAggregateVarInfo over which the input subtree can be translated </param>
        /// <param name="templateNode">A tree that is equvalent to the input tree, but over the group aggregate variable
        /// represented by the groupAggregetVarInfo</param>
        /// <param name="isUnnested"></param>
        /// <returns>True, if the translation can be done, false otherwise</returns>
        public static bool TryTranslateOverGroupAggregateVar(
            Node subtree,
            bool isVarDefinition,
            Command command,
            GroupAggregateVarInfoManager groupAggregateVarInfoManager,
            out GroupAggregateVarInfo groupAggregateVarInfo,
            out Node templateNode,
            out bool isUnnested)
        {
            GroupAggregateVarComputationTranslator handler = new GroupAggregateVarComputationTranslator(command, groupAggregateVarInfoManager);

            Node inputNode = subtree;
            SoftCastOp softCastOp = null;
            bool isCollect;
            if (inputNode.Op.OpType == OpType.SoftCast)
            {
                softCastOp = (SoftCastOp)inputNode.Op;
                inputNode = inputNode.Child0;
            }

            if (inputNode.Op.OpType == OpType.Collect)
            {
                templateNode = handler.VisitCollect(inputNode);
                isCollect = true;
            }
            else
            {
                templateNode = handler.VisitNode(inputNode);
                isCollect = false;
            }

            groupAggregateVarInfo = handler._targetGroupAggregateVarInfo;
            isUnnested = handler._isUnnested;

            if (handler._targetGroupAggregateVarInfo == null || templateNode == null)
            {
                return false;
            }
            if (softCastOp != null)
            {
                SoftCastOp newSoftCastOp;
                // 
                // The type needs to be fixed only if the unnesting happened during this translation.
                // That can be recognized by these two cases: 
                //      1) if the input node was a collect, or 
                //      2) if the input did not represent a var definition, but a function aggregate argument and 
                //              the template is VarRef of a group aggregate var.
                //
                if (isCollect || !isVarDefinition && AggregatePushdownUtil.IsVarRefOverGivenVar(templateNode, handler._targetGroupAggregateVarInfo.GroupAggregateVar))
                {
                    newSoftCastOp = command.CreateSoftCastOp(TypeHelpers.GetEdmType<CollectionType>(softCastOp.Type).TypeUsage);
                }
                else
                {
                    newSoftCastOp = softCastOp;
                }
                templateNode = command.CreateNode(newSoftCastOp, templateNode);
            }
            return true;
        }
        #endregion

        #region Visitor Methods
        /// <summary>
        /// See <cref="TryTranslateOverGroupAggregateVar"/>
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(VarRefOp op, Node n)
        {
            return TranslateOverGroupAggregateVar(op.Var, null);
        }

        /// <summary>
        /// If the child is VarRef check if the subtree PropertyOp(VarRef) is reference to a 
        /// group aggregate var. 
        /// Otherwise do default processing
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(PropertyOp op, Node n)
        {
            if (n.Child0.Op.OpType != OpType.VarRef)
            {
                return base.Visit(op, n);
            }
            VarRefOp varRefOp = (VarRefOp)n.Child0.Op;
            return TranslateOverGroupAggregateVar(varRefOp.Var, op.PropertyInfo);
        }

        /// <summary>
        /// If the Subtree rooted at the collect is of the following structure:
        /// 
        /// PhysicalProject(outputVar)
        /// |
        /// Project(s)
        /// |
        /// Unnest
        /// 
        /// where the unnest is over the group aggregate var and the output var
        /// is either a reference to the group aggregate var or to a constant, it returns the 
        /// translation of the ouput var.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        private Node VisitCollect(Node n)
        {
            //Make sure the only children are projects over unnest
            Node currentNode = n.Child0;
            Dictionary<Var, Node> constantDefinitions = new Dictionary<Var, Node>();
            while (currentNode.Child0.Op.OpType == OpType.Project)
            {
                currentNode = currentNode.Child0;
                //Visit the VarDefListOp child
                if (VisitDefault(currentNode.Child1) == null)
                {
                    return null;
                }
                foreach (Node definitionNode in currentNode.Child1.Children)
                {
                    if (IsConstant(definitionNode.Child0))
                    {
                        constantDefinitions.Add(((VarDefOp)definitionNode.Op).Var, definitionNode.Child0);
                    }
                }
            }

            if (currentNode.Child0.Op.OpType != OpType.Unnest)
            {
                return null;
            }

            // Handle the unnest
            UnnestOp unnestOp = (UnnestOp)currentNode.Child0.Op;
            GroupAggregateVarRefInfo groupAggregateVarRefInfo;
            if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(unnestOp.Var, out groupAggregateVarRefInfo))
            {
                if (_targetGroupAggregateVarInfo == null)
                {
                    _targetGroupAggregateVarInfo = groupAggregateVarRefInfo.GroupAggregateVarInfo;
                }
                else if (_targetGroupAggregateVarInfo != groupAggregateVarRefInfo.GroupAggregateVarInfo)
                {
                    return null;
                }
                if (!_isUnnested)
                {
                    return null;
                }
            }
            else
            {
                return null;
            }

            PhysicalProjectOp physicalProjectOp = (PhysicalProjectOp)n.Child0.Op;
            PlanCompiler.Assert(physicalProjectOp.Outputs.Count == 1, "PhysicalProject should only have one output at this stage");
            Var outputVar = physicalProjectOp.Outputs[0];

            Node computationTemplate = TranslateOverGroupAggregateVar(outputVar, null);
            if (computationTemplate != null)
            {
                _isUnnested = true;
                return computationTemplate;
            }

            Node constantDefinitionNode;
            if (constantDefinitions.TryGetValue(outputVar, out constantDefinitionNode))
            {
                _isUnnested = true;
                return constantDefinitionNode;
            }
            return null;
        }

        /// <summary>
        /// Determines whether the given Node is a constant subtree
        /// It only recognizes any of the constant base ops
        /// and possibly casts over these nodes.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        private static bool IsConstant(Node node)
        {
            Node currentNode = node;
            while (currentNode.Op.OpType == OpType.Cast)
            {
                currentNode = currentNode.Child0;
            }
            return PlanCompilerUtil.IsConstantBaseOp(currentNode.Op.OpType);
        }

        /// <summary>
        /// (1) If the given var or the given property of the given var are defined over a group aggregate var, 
        /// (2) and if that group aggregate var matches the var represented by represented by _targetGroupAggregateVarInfo
        /// if any
        /// 
        /// it returns the corresponding translation over the group aggregate var. Also, if _targetGroupAggregateVarInfo
        /// is not set, it sets it to the group aggregate var representing the referenced var.
        /// </summary>
        /// <param name="var"></param>
        /// <param name="property"></param>
        /// <returns></returns>
        private Node TranslateOverGroupAggregateVar(Var var, EdmMember property)
        {
            GroupAggregateVarRefInfo groupAggregateVarRefInfo;
            EdmMember localProperty;
            if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(var, out groupAggregateVarRefInfo))
            {
                localProperty = property;
            }
            else if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(var, property, out  groupAggregateVarRefInfo))
            {
                localProperty = null;
            }
            else
            {
                return null;
            }

            if (_targetGroupAggregateVarInfo == null)
            {
                _targetGroupAggregateVarInfo = groupAggregateVarRefInfo.GroupAggregateVarInfo;
                _isUnnested = groupAggregateVarRefInfo.IsUnnested;
            }
            else if (_targetGroupAggregateVarInfo != groupAggregateVarRefInfo.GroupAggregateVarInfo || _isUnnested != groupAggregateVarRefInfo.IsUnnested)
            {
                return null;
            }

            Node computationTemplate = groupAggregateVarRefInfo.Computation;
            if (localProperty != null)
            {
                computationTemplate = this._command.CreateNode(this._command.CreatePropertyOp(localProperty), computationTemplate);
            }
            return computationTemplate;
        }

        /// <summary>
        /// Default processing for nodes. 
        /// Visits the children and if any child has changed it creates a new node 
        /// for the parent.
        /// If the reference of the child node did not change, the child node did not change either,
        /// this is because a node can only be reused "as is" when building a template.
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override Node VisitDefault(Node n)
        {
            List<Node> newChildren = new List<Node>(n.Children.Count);
            bool anyChildChanged = false;
            for (int i = 0; i < n.Children.Count; i++)
            {
                Node processedChild = VisitNode(n.Children[i]);
                if (processedChild == null)
                {
                    return null;
                }
                if (!anyChildChanged && !Object.ReferenceEquals(n.Children[i], processedChild))
                {
                    anyChildChanged = true;
                }
                newChildren.Add(processedChild);
            }

            if (!anyChildChanged)
            {
                return n;
            }
            else
            {
                return _command.CreateNode(n.Op, newChildren);
            }
        }

        #region Unsupported node types

        protected override Node VisitRelOpDefault(RelOp op, Node n)
        {
            return null;
        }

        public override Node Visit(AggregateOp op, Node n)
        {
            return null;
        }

        public override Node Visit(CollectOp op, Node n)
        {
            return null;
        }

        public override Node Visit(ElementOp op, Node n)
        {
            return null;
        }

        #endregion

        #endregion
    }

    /// <summary>
    /// A visitor that collects all group aggregates and the corresponding function aggregates 
    /// that are defined over them, referred to as 'candidate aggregates'. The candidate aggregates are aggregates
    /// that have an argument that has the corresponding group aggregate as the only external reference
    /// </summary>
    internal class GroupAggregateRefComputingVisitor : BasicOpVisitor
    {
        #region private state
        private readonly Command _command;
        private readonly GroupAggregateVarInfoManager _groupAggregateVarInfoManager = new GroupAggregateVarInfoManager();
        private readonly Dictionary<Node, Node> _childToParent = new Dictionary<Node, Node>();
        #endregion

        #region 'Public'
        /// <summary>
        /// Produces a list of all GroupAggregateVarInfos, each of which represents a single group aggregate 
        /// and it candidate function aggregates. It also produces a delegate that given a child node returns the parent node
        /// </summary>
        /// <param name="itree"></param>
        /// <param name="tryGetParent"></param>
        /// <returns></returns>
        internal static IEnumerable<GroupAggregateVarInfo> Process(Command itree, out TryGetValue tryGetParent)
        {
            GroupAggregateRefComputingVisitor groupRefComputingVisitor = new GroupAggregateRefComputingVisitor(itree);
            groupRefComputingVisitor.VisitNode(itree.Root);
            tryGetParent = groupRefComputingVisitor._childToParent.TryGetValue;

            return groupRefComputingVisitor._groupAggregateVarInfoManager.GroupAggregateVarInfos;
        }
        #endregion

        #region Private Constructor
        /// <summary>
        /// Private constructor
        /// </summary>
        /// <param name="itree"></param>
        private GroupAggregateRefComputingVisitor(Command itree)
        {
            this._command = itree;
        }
        #endregion

        #region Visitor Methods

        #region AncillaryOps
        /// <summary>
        /// Determines whether the var or a property of the var (if the var is defined as a NewRecord) 
        /// is defined exclusively over a single group aggregate. If so, it registers it as such with the
        /// group aggregate var info manager.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(VarDefOp op, Node n)
        {
            VisitDefault(n);

            Node definingNode = n.Child0;
            Op definingNodeOp = definingNode.Op;

            GroupAggregateVarInfo referencedVarInfo;
            Node templateNode;
            bool isUnnested;
            if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(definingNode, true, this._command, this._groupAggregateVarInfoManager, out  referencedVarInfo, out templateNode, out isUnnested))
            {
                _groupAggregateVarInfoManager.Add(op.Var, referencedVarInfo, templateNode, isUnnested);
            }
            else if (definingNodeOp.OpType == OpType.NewRecord)
            {
                NewRecordOp newRecordOp = (NewRecordOp)definingNodeOp;
                for (int i = 0; i < definingNode.Children.Count; i++)
                {
                    Node argumentNode = definingNode.Children[i];
                    if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(argumentNode, true, this._command, this._groupAggregateVarInfoManager, out referencedVarInfo, out templateNode, out isUnnested))
                    {
                        _groupAggregateVarInfoManager.Add(op.Var, referencedVarInfo, templateNode, isUnnested, newRecordOp.Properties[i]);
                    }
                }
            }
        }

        #endregion

        #region RelOp Visitors
        /// <summary>
        /// Registers the group aggregate var with the group aggregate var info manager
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(GroupByIntoOp op, Node n)
        {
            VisitGroupByOp(op, n);
            foreach (Node child in n.Child3.Children)
            {
                Var groupAggregateVar = ((VarDefOp)child.Op).Var;
                GroupAggregateVarRefInfo groupAggregateVarRefInfo;
                // If the group by is over a group, it may be already tracked as referencing a group var
                // An optimization would be to separately track this groupAggregateVar too, for the cases when the aggregate can 
                // not be pushed to the group by node over which this one is defined but can be propagated to this group by node.
                if (!_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(groupAggregateVar, out groupAggregateVarRefInfo))
                {
                    _groupAggregateVarInfoManager.Add(groupAggregateVar, new GroupAggregateVarInfo(n, groupAggregateVar), this._command.CreateNode(this._command.CreateVarRefOp(groupAggregateVar)), false);
                }
            }
        }

        /// <summary>
        /// If the unnestOp's var is defined as a reference of a group aggregate var,
        /// then the columns it produces should be registered too, but as 'unnested' references
        /// </summary>
        /// <param name="op">the unnestOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>modified subtree</returns>
        public override void Visit(UnnestOp op, Node n)
        {
            VisitDefault(n);
            GroupAggregateVarRefInfo groupAggregateVarRefInfo;
            if (_groupAggregateVarInfoManager.TryGetReferencedGroupAggregateVarInfo(op.Var, out groupAggregateVarRefInfo))
            {
                PlanCompiler.Assert(op.Table.Columns.Count == 1, "Expected one column before NTE");
                _groupAggregateVarInfoManager.Add(op.Table.Columns[0], groupAggregateVarRefInfo.GroupAggregateVarInfo, groupAggregateVarRefInfo.Computation, true);
            }
        }

        #endregion

        #region ScalarOps Visitors
        /// <summary>
        /// If the op is a collection aggregate function it checks whether its arguement can be translated over 
        /// a single group aggregate var. If so, it is tracked as a candidate to be pushed into that 
        /// group by into node.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        public override void Visit(FunctionOp op, Node n)
        {
            VisitDefault(n);
            if (!PlanCompilerUtil.IsCollectionAggregateFunction(op, n))
            {
                return;
            }
            PlanCompiler.Assert(n.Children.Count == 1, "Aggregate Function must have one argument");

            Node argumentNode = n.Child0;

            GroupAggregateVarInfo referencedGroupAggregateVarInfo;
            Node templateNode;
            bool isUnnested;
            if (GroupAggregateVarComputationTranslator.TryTranslateOverGroupAggregateVar(n.Child0, false, _command, _groupAggregateVarInfoManager, out referencedGroupAggregateVarInfo, out templateNode, out isUnnested)
                && (isUnnested || AggregatePushdownUtil.IsVarRefOverGivenVar(templateNode, referencedGroupAggregateVarInfo.GroupAggregateVar)))
            {
                referencedGroupAggregateVarInfo.CandidateAggregateNodes.Add(new KeyValuePair<Node, Node>(n, templateNode));
            }
        }

        #endregion

        /// <summary>
        /// Default visitor for nodes.
        /// It tracks the child-parent relationship.
        /// </summary>
        /// <param name="n"></param>
        protected override void VisitDefault(Node n)
        {
            VisitChildren(n);
            foreach (Node child in n.Children)
            {
                //No need to track terminal nodes, plus some of these may be reused.
                if (child.Op.Arity != 0)
                {
                    _childToParent.Add(child, n);
                }
            }
        }
        #endregion

    }

    /// <summary>
    /// Utility class to gather helper methods used by more than one class in the Aggregate Pushdown feature.
    /// </summary>
    internal static class AggregatePushdownUtil
    {
        /// <summary>
        /// Determines whether the given node is a VarRef over the given var
        /// </summary>
        /// <param name="node"></param>
        /// <param name="var"></param>
        /// <returns></returns>
        internal static bool IsVarRefOverGivenVar(Node node, Var var)
        {
            if (node.Op.OpType != OpType.VarRef)
            {
                return false;
            }
            return ((VarRefOp)node.Op).Var == var;
        }
    }

    /// <summary>
    /// The Aggregate Pushdown feature tries to identify function aggregates defined over a
    /// group aggregate and push their definitions in the group by into node corresponding to 
    /// the group aggregate.
    /// </summary>
    internal class AggregatePushdown
    {
        #region Private fields
        private readonly Command m_command;
        private TryGetValue m_tryGetParent;
        #endregion

        #region Private Constructor
        private AggregatePushdown(Command command)
        {
            this.m_command = command;
        }
        #endregion

        #region 'Public' Surface
        /// <summary>
        /// Apply Aggregate Pushdown over the tree in the given plan complier state.
        /// </summary>
        /// <param name="planCompilerState"></param>
        internal static void Process(PlanCompiler planCompilerState)
        {
            AggregatePushdown aggregatePushdown = new AggregatePushdown(planCompilerState.Command);
            aggregatePushdown.Process();
        }
        #endregion

        #region Private Methods

        /// <summary>
        /// The main driver
        /// </summary>
        private void Process()
        {
            IEnumerable<GroupAggregateVarInfo> groupAggregateVarInfos = GroupAggregateRefComputingVisitor.Process(m_command, out m_tryGetParent);
            foreach (GroupAggregateVarInfo groupAggregateVarInfo in groupAggregateVarInfos)
            {
                if (groupAggregateVarInfo.HasCandidateAggregateNodes)
                {
                    foreach (KeyValuePair<Node, Node> candidate in groupAggregateVarInfo.CandidateAggregateNodes)
                    {
                        TryProcessCandidate(candidate, groupAggregateVarInfo);
                    }
                }
            }
        }

        /// <summary>
        /// Try to push the given function aggregate candidate to the corresponding group into node.
        /// The candidate can be pushed if all ancestors of the group into node up to the least common 
        /// ancestor between the group into node and the function aggregate have one of the following node op types:  
        ///     Project
        ///     Filter
        ///     ConstraintSortOp    
        /// </summary>
        /// <param name="command"></param>
        /// <param name="candidate"></param>
        /// <param name="groupAggregateVarInfo"></param>
        /// <param name="m_childToParent"></param>
        private void TryProcessCandidate(
            KeyValuePair<Node, Node> candidate,
            GroupAggregateVarInfo groupAggregateVarInfo)
        {
            IList<Node> functionAncestors;
            IList<Node> groupByAncestors;
            Node definingGroupNode = groupAggregateVarInfo.DefiningGroupNode;
            FindPathsToLeastCommonAncestor(candidate.Key, definingGroupNode, out functionAncestors, out groupByAncestors);

            //Check whether all ancestors of the GroupByInto node are of type that we support propagating through
            if (!AreAllNodesSupportedForPropagation(groupByAncestors))
            {
                return;
            }

            //Add the function to the group by node
            GroupByIntoOp definingGroupOp = (GroupByIntoOp)definingGroupNode.Op;
            PlanCompiler.Assert(definingGroupOp.Inputs.Count == 1, "There should be one input var to GroupByInto at this stage");
            Var inputVar = definingGroupOp.Inputs.First;
            FunctionOp functionOp = (FunctionOp)candidate.Key.Op;

            //
            // Remap the template from referencing the groupAggregate var to reference the input to
            // the group by into
            //
            Node argumentNode = OpCopier.Copy(m_command, candidate.Value);
            Dictionary<Var, Var> dictionary = new Dictionary<Var, Var>(1);
            dictionary.Add(groupAggregateVarInfo.GroupAggregateVar, inputVar);
            VarRemapper remapper = new VarRemapper(m_command, dictionary);
            remapper.RemapSubtree(argumentNode);

            Node newFunctionDefiningNode = m_command.CreateNode(
                m_command.CreateAggregateOp(functionOp.Function, false),
                argumentNode);

            Var newFunctionVar;
            Node varDefNode = m_command.CreateVarDefNode(newFunctionDefiningNode, out newFunctionVar);

            // Add the new aggregate to the list of aggregates
            definingGroupNode.Child2.Children.Add(varDefNode);
            GroupByIntoOp groupByOp = (GroupByIntoOp)definingGroupNode.Op;
            groupByOp.Outputs.Set(newFunctionVar);

            //Propagate the new var throught the ancestors of the GroupByInto
            for (int i = 0; i < groupByAncestors.Count; i++)
            {
                Node groupByAncestor = groupByAncestors[i];
                if (groupByAncestor.Op.OpType == OpType.Project)
                {
                    ProjectOp ancestorProjectOp = (ProjectOp)groupByAncestor.Op;
                    ancestorProjectOp.Outputs.Set(newFunctionVar);
                }
            }

            //Update the functionNode
            candidate.Key.Op = m_command.CreateVarRefOp(newFunctionVar);
            candidate.Key.Children.Clear();
        }

        /// <summary>
        /// Check whether all nodes in the given list of nodes are of types 
        /// that we know how to propagate an aggregate through
        /// </summary>
        /// <param name="nodes"></param>
        /// <returns></returns>
        private static bool AreAllNodesSupportedForPropagation(IList<Node> nodes)
        {
            foreach (Node node in nodes)
            {
                if (node.Op.OpType != OpType.Project
                    && node.Op.OpType != OpType.Filter
                    && node.Op.OpType != OpType.ConstrainedSort
                    )
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Finds the paths from each of node1 and node2 to their least common ancestor
        /// </summary>
        /// <param name="node1"></param>
        /// <param name="node2"></param>
        /// <param name="ancestors1"></param>
        /// <param name="ancestors2"></param>
        private void FindPathsToLeastCommonAncestor(Node node1, Node node2, out IList<Node> ancestors1, out IList<Node> ancestors2)
        {
            ancestors1 = FindAncestors(node1);
            ancestors2 = FindAncestors(node2);

            int currentIndex1 = ancestors1.Count - 1;
            int currentIndex2 = ancestors2.Count - 1;
            while (ancestors1[currentIndex1] == ancestors2[currentIndex2])
            {
                currentIndex1--;
                currentIndex2--;
            }

            for (int i = ancestors1.Count - 1; i > currentIndex1; i--)
            {
                ancestors1.RemoveAt(i);
            }
            for (int i = ancestors2.Count - 1; i > currentIndex2; i--)
            {
                ancestors2.RemoveAt(i);
            }
        }

        /// <summary>
        /// Finds all ancestors of the given node. 
        /// </summary>
        /// <param name="node"></param>
        /// <returns>An ordered list of the all the ancestors of the given node starting from the immediate parent
        /// to the root of the tree</returns>
        private IList<Node> FindAncestors(Node node)
        {
            List<Node> ancestors = new List<Node>();
            Node currentNode = node;
            Node ancestor;
            while (m_tryGetParent(currentNode, out ancestor))
            {
                ancestors.Add(ancestor);
                currentNode = ancestor;
            }
            return ancestors;
        }
        #endregion
    }
}

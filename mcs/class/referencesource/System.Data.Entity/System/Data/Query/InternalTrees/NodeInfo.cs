//---------------------------------------------------------------------
// <copyright file="NodeInfo.cs" company="Microsoft">
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
using md=System.Data.Metadata.Edm;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// The KeySet class encapsulates all information about the keys of a RelOp node in
    /// the query tree.
    /// A KeyVec is logically a set of vars that uniquely identify the row of the current
    /// RelOp. Some RelOps may have no unique keys - such a state is identified by the
    /// "NoKeys" property
    /// </summary>
    internal class KeyVec
    {
        #region private state
        private VarVec m_keys;
        private bool m_noKeys;
        #endregion

        #region constructors
        internal KeyVec(Command itree)
        {
            m_keys = itree.CreateVarVec();
            m_noKeys = true;
        }
        #endregion

        internal void InitFrom(KeyVec keyset)
        {
            m_keys.InitFrom(keyset.m_keys);
            m_noKeys = keyset.m_noKeys;
        }

        internal void InitFrom(IEnumerable<Var> varSet)
        {
            InitFrom(varSet, false);
        }

        internal void InitFrom(IEnumerable<Var> varSet, bool ignoreParameters)
        {
            m_keys.InitFrom(varSet, ignoreParameters);
            // 

            m_noKeys = false;
        }
        internal void InitFrom(KeyVec left, KeyVec right)
        {
            if (left.m_noKeys || right.m_noKeys)
            {
                m_noKeys = true;
            }
            else
            {
                m_noKeys = false;
                m_keys.InitFrom(left.m_keys);
                m_keys.Or(right.m_keys);
            }
        }
        internal void InitFrom(List<KeyVec> keyVecList)
        {
            m_noKeys = false;
            m_keys.Clear();
            foreach (KeyVec keyVec in keyVecList)
            {
                if (keyVec.m_noKeys)
                {
                    m_noKeys = true;
                    return;
                }
                m_keys.Or(keyVec.m_keys);
            }
        }
        internal void Clear()
        {
            m_noKeys = true;
            m_keys.Clear();
        }

        internal VarVec KeyVars { get { return m_keys; } }
        internal bool NoKeys { get { return m_noKeys; } set { m_noKeys = value; } }
    }

    /// <summary>
    /// The NodeInfo class represents additional information about a node in the tree.
    /// By default, this includes a set of external references for each node (ie) references
    /// to Vars that are not defined in the same subtree
    /// The NodeInfo class also includes a "hashValue" that is a hash value for the entire 
    /// subtree rooted at this node
    /// NOTE: When adding a new member to track inforation, make sure to update the Clear method 
    /// in this class to set that member to the default value.
    /// </summary>
    internal class NodeInfo
    {
        #region private state
        private VarVec m_externalReferences;
        protected int m_hashValue; // hash value for the node
        #endregion

        #region constructors
        internal NodeInfo(Command cmd)
        {
            m_externalReferences = cmd.CreateVarVec();
        }
        #endregion

        #region public methods
        /// <summary>
        /// Clear out all information - usually used by a Recompute
        /// </summary>
        internal virtual void Clear()
        {
            m_externalReferences.Clear();
            m_hashValue = 0;
        }

        /// <summary>
        /// All external references from this node
        /// </summary>
        internal VarVec ExternalReferences
        {
            get { return m_externalReferences; }
        }

        /// <summary>
        /// Get the hash value for this nodeInfo
        /// </summary>
        internal int HashValue
        {
            get { return m_hashValue; }
        }

        /// <summary>
        /// Compute the hash value for a Vec
        /// </summary>
        /// <param name="vec"></param>
        /// <returns></returns>
        internal static int GetHashValue(VarVec vec)
        {
            int hashValue = 0;
            foreach (Var v in vec)
            {
                hashValue ^= v.GetHashCode();
            }
            return hashValue;
        }

        /// <summary>
        /// Computes the hash value for this node. The hash value is simply the 
        /// local hash value for this node info added with the hash values of the child 
        /// nodes
        /// </summary>
        /// <param name="cmd">current command</param>
        /// <param name="n">current node</param>
        internal virtual void ComputeHashValue(Command cmd, Node n)
        {
            m_hashValue = 0;
            foreach (Node chi in n.Children)
            {
                NodeInfo chiNodeInfo = cmd.GetNodeInfo(chi);
                m_hashValue ^= chiNodeInfo.HashValue;
            }

            m_hashValue = (m_hashValue << 4) ^ ((int)n.Op.OpType); // include the optype somehow
            // Now compute my local hash value
            m_hashValue = (m_hashValue << 4) ^ GetHashValue(m_externalReferences);
        }
        #endregion
    }

    /// <summary>
    /// Enum describing row counts
    /// </summary>
    internal enum RowCount : byte
    {
        /// <summary>
        /// Zero rows
        /// </summary>
        Zero = 0,

        /// <summary>
        /// One row
        /// </summary>
        One = 1,

        /// <summary>
        /// Unbounded (unknown number of rows)
        /// </summary>
        Unbounded = 2,
    }

    /// <summary>
    /// An ExtendedNodeInfo class adds additional information to a standard NodeInfo.
    /// This class is usually applicable only to RelOps and PhysicalOps.
    /// The ExtendedNodeInfo class has in addition to the information maintained by NodeInfo
    /// the following
    /// - a set of local definitions
    /// - a set of definitions
    /// - a set of keys
    /// - a set of non-nullable definitions 
    /// - a set of non-nullable definitions that are visible at this node
    /// NOTE: When adding a new member to track inforation, make sure to update the Clear method 
    /// in this class to set that member to the default value.
    /// </summary>
    internal class ExtendedNodeInfo : NodeInfo
    {
        #region private
        private VarVec m_localDefinitions;
        private VarVec m_definitions;
        private KeyVec m_keys;
        private VarVec m_nonNullableDefinitions;
        private VarVec m_nonNullableVisibleDefinitions;
        private RowCount m_minRows;
        private RowCount m_maxRows;
        #endregion

        #region constructors
        internal ExtendedNodeInfo(Command cmd)
            : base(cmd)
        {
            m_localDefinitions = cmd.CreateVarVec();
            m_definitions = cmd.CreateVarVec();
            m_nonNullableDefinitions = cmd.CreateVarVec();
            m_nonNullableVisibleDefinitions = cmd.CreateVarVec();
            m_keys = new KeyVec(cmd);
            m_minRows = RowCount.Zero;
            m_maxRows = RowCount.Unbounded;
        }
        #endregion

        #region public methods

        internal override void Clear()
        {
            base.Clear();
            m_definitions.Clear();
            m_localDefinitions.Clear();
            m_nonNullableDefinitions.Clear();
            m_nonNullableVisibleDefinitions.Clear();
            m_keys.Clear();
            m_minRows = RowCount.Zero;
            m_maxRows = RowCount.Unbounded;
        }

        /// <summary>
        /// Compute the hash value for this node
        /// </summary>
        /// <param name="cmd"></param>
        /// <param name="n"></param>
        internal override void ComputeHashValue(Command cmd, Node n)
        {
            base.ComputeHashValue(cmd, n);
            m_hashValue = (m_hashValue << 4) ^ NodeInfo.GetHashValue(this.Definitions);
            m_hashValue = (m_hashValue << 4) ^ NodeInfo.GetHashValue(this.Keys.KeyVars);
            return;
        }

        /// <summary>
        /// Definitions made specifically by this node
        /// </summary>
        internal VarVec LocalDefinitions { get { return m_localDefinitions; } }
        /// <summary>
        /// All definitions visible as outputs of this node
        /// </summary>
        internal VarVec Definitions { get { return m_definitions; } }
        /// <summary>
        /// The keys for this node
        /// </summary>
        internal KeyVec Keys { get { return m_keys; } }
        /// <summary>
        /// The definitions of vars that are guaranteed to be non-nullable when output from this node
        /// </summary>
        internal VarVec NonNullableDefinitions { get { return m_nonNullableDefinitions; } }
        /// <summary>
        /// The definitions that come from the rel-op inputs of this node that are guaranteed to be non-nullable
        /// </summary>
        internal VarVec NonNullableVisibleDefinitions { get { return m_nonNullableVisibleDefinitions; } }
        /// <summary>
        /// Min number of rows returned from this node
        /// </summary>
        internal RowCount MinRows
        {
            get { return m_minRows; }
            set { m_minRows = value; ValidateRowCount(); }
        }
        /// <summary>
        /// Max rows returned from this node
        /// </summary>
        internal RowCount MaxRows
        {
            get { return m_maxRows; }
            set { m_maxRows = value; ValidateRowCount(); }
        }

        /// <summary>
        /// Set the rowcount for this node
        /// </summary>
        /// <param name="minRows">min rows produced by this node</param>
        /// <param name="maxRows">max rows produced by this node</param>
        internal void SetRowCount(RowCount minRows, RowCount maxRows)
        {
            m_minRows = minRows;
            m_maxRows = maxRows;
            ValidateRowCount();
        }

        /// <summary>
        /// Initialize the rowcounts for this node from the source node
        /// </summary>
        /// <param name="source">nodeinfo of source</param>
        internal void InitRowCountFrom(ExtendedNodeInfo source)
        {
            m_minRows = source.m_minRows;
            m_maxRows = source.m_maxRows;
        }

        #endregion

        #region private methods
        private void ValidateRowCount()
        {
            Debug.Assert(m_maxRows >= m_minRows, "MaxRows less than MinRows?");
        }
        #endregion
    }

    /// <summary>
    /// The NodeInfoVisitor is a simple class (ab)using the Visitor pattern to define
    /// NodeInfo semantics for various nodes in the tree
    /// </summary>
    internal class NodeInfoVisitor : BasicOpVisitorOfT<NodeInfo>
    {
        #region public methods
        /// <summary>
        /// The only public method. Recomputes the nodeInfo for a node in the tree, 
        /// but only if the node info has already been computed.  
        /// Assumes that the NodeInfo for each child (if computed already) is valid
        /// </summary>
        /// <param name="n">Node to get NodeInfo for</param>
        internal void RecomputeNodeInfo(Node n)
        {
            if (n.IsNodeInfoInitialized)
            {
                NodeInfo nodeInfo = VisitNode(n);
                nodeInfo.ComputeHashValue(this.m_command, n); // compute the hash value for this node
            }
        }
        #endregion

        #region constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="command"></param>
        internal NodeInfoVisitor(Command command)
        {
            m_command = command;
        }
        #endregion

        #region private state
        private Command m_command;
        #endregion

        #region private methods
        private NodeInfo GetNodeInfo(Node n)
        {
            return n.GetNodeInfo(m_command);
        }
        private ExtendedNodeInfo GetExtendedNodeInfo(Node n)
        {
            return n.GetExtendedNodeInfo(m_command);
        }
        private NodeInfo InitNodeInfo(Node n)
        {
            NodeInfo nodeInfo = GetNodeInfo(n);
            nodeInfo.Clear();
            return nodeInfo;
        }
        private ExtendedNodeInfo InitExtendedNodeInfo(Node n)
        {
            ExtendedNodeInfo nodeInfo = GetExtendedNodeInfo(n);
            nodeInfo.Clear();
            return nodeInfo;
        }
        #endregion

        #region VisitorHelpers
        /// <summary>
        /// Default implementation for scalarOps. Simply adds up external references
        /// from each child
        /// </summary>
        /// <param name="n"></param>
        /// <returns></returns>
        protected override NodeInfo VisitDefault(Node n)
        {
            Debug.Assert(n.Op.IsScalarOp || n.Op.IsAncillaryOp, "not a supported optype");

            NodeInfo nodeInfo = InitNodeInfo(n);
            // My external references are simply the combination of external references
            // of all my children
            foreach (Node chi in n.Children)
            {
                NodeInfo childNodeInfo = GetNodeInfo(chi);
                nodeInfo.ExternalReferences.Or(childNodeInfo.ExternalReferences);
            }
            return nodeInfo;
        }

        /// <summary>
        /// The given definition is non nullable if it is a non-null constant
        /// or a reference to non-nullable input
        /// </summary>
        /// <param name="definition"></param>
        /// <param name="nonNullableInputs"></param>
        /// <returns></returns>
        private bool IsDefinitionNonNullable(Node definition, VarVec nonNullableInputs)
        {
            return (definition.Op.OpType == OpType.Constant
                || definition.Op.OpType == OpType.InternalConstant
                || definition.Op.OpType == OpType.NullSentinel
                || definition.Op.OpType == OpType.VarRef
                    && nonNullableInputs.IsSet(((VarRefOp)definition.Op).Var));      
        }
        #endregion

        #region IOpVisitor<NodeInfo> Members

        #region MiscOps
        #endregion

        #region AncillarOps
        #endregion

        #region ScalarOps
        /// <summary>
        /// The only special case among all scalar and ancillaryOps. Simply adds
        /// its var to the list of unreferenced Ops
        /// </summary>
        /// <param name="op">The VarRefOp</param>
        /// <param name="n">Current node</param>
        /// <returns></returns>
        public override NodeInfo Visit(VarRefOp op, Node n)
        {
            NodeInfo nodeInfo = InitNodeInfo(n);
            nodeInfo.ExternalReferences.Set(op.Var);
            return nodeInfo;
        }

        #endregion

        #region RelOps
        protected override NodeInfo VisitRelOpDefault(RelOp op, Node n)
        {
            return Unimplemented(n);
        }

        /// <summary>
        /// Definitions = Local Definitions = referenced table columns
        /// External References = none
        /// Keys = keys of entity type
        /// RowCount (default): MinRows = 0, MaxRows = * 
        /// NonNullableDefinitions : non nullable table columns that are definitions
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op">ScanTable/ScanView op</param>
        /// <param name="n">current subtree</param>
        /// <returns>nodeinfo for this subtree</returns>
        protected override NodeInfo VisitTableOp(ScanTableBaseOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            // #479372 - only the "referenced" columns of the table should
            // show up in the definitions
            nodeInfo.LocalDefinitions.Or(op.Table.ReferencedColumns);
            nodeInfo.Definitions.Or(op.Table.ReferencedColumns);

            // get table's keys - but only if the key columns have been referenced
            if (op.Table.ReferencedColumns.Subsumes(op.Table.Keys))
            {
                nodeInfo.Keys.InitFrom(op.Table.Keys);
            }
            // no external references

            //non-nullable definitions
            nodeInfo.NonNullableDefinitions.Or(op.Table.NonNullableColumns);
            nodeInfo.NonNullableDefinitions.And(nodeInfo.Definitions);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for an UnnestOp.
        /// Definitions = columns of the table produced by this Op
        /// Keys = none
        /// External References = the unnestVar + any external references of the
        ///   computed Var (if any)
        /// RowCount (default): MinRows = 0; MaxRows = *
        /// NonNullableDefinitions: default(empty) 
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override NodeInfo Visit(UnnestOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            foreach (Var v in op.Table.Columns)
            {
                nodeInfo.LocalDefinitions.Set(v);
                nodeInfo.Definitions.Set(v);
            }

            // Process keys if it's a TVF with inferred keys, otherwise - no keys.
            if (n.Child0.Op.OpType == OpType.VarDef && n.Child0.Child0.Op.OpType == OpType.Function && op.Table.Keys.Count > 0)
            {
                // This is a TVF case. 
                // Get table's keys - but only if they have been referenced.
                if (op.Table.ReferencedColumns.Subsumes(op.Table.Keys))
                {
                    nodeInfo.Keys.InitFrom(op.Table.Keys);
                }
            }
            else
            {
                // no keys
                Debug.Assert(nodeInfo.Keys.NoKeys, "UnnestOp should have no keys in all cases except TVFs mapped to entities.");
            }

            // If I have a child, then my external references are my child's external references.
            // Otherwise, my external reference is my unnestVar
            if (n.HasChild0)
            {
                NodeInfo childNodeInfo = GetNodeInfo(n.Child0);
                nodeInfo.ExternalReferences.Or(childNodeInfo.ExternalReferences);
            }
            else
            {
                nodeInfo.ExternalReferences.Set(op.Var);
            }

            return nodeInfo;
        }

        /// <summary>
        /// Walk through the computed vars defined by a VarDefListNode, and look for
        /// "simple" Var renames. Build up a mapping from original Vars to the renamed Vars
        /// </summary>
        /// <param name="varDefListNode">the varDefListNode subtree</param>
        /// <returns>A dictionary of Var->Var renames</returns>
        internal static Dictionary<Var, Var> ComputeVarRemappings(Node varDefListNode)
        {
            Debug.Assert(varDefListNode.Op.OpType == OpType.VarDefList);

            Dictionary<Var, Var> varMap = new Dictionary<Var, Var>();
            foreach (Node varDefNode in varDefListNode.Children)
            {
                VarRefOp varRefOp = varDefNode.Child0.Op as VarRefOp;
                if (varRefOp != null)
                {
                    VarDefOp varDefOp = varDefNode.Op as VarDefOp;
                    Debug.Assert(varDefOp != null);
                    varMap[varRefOp.Var] = varDefOp.Var;
                }
            }
            return varMap;
        }

        /// <summary>
        /// Computes a NodeInfo for a ProjectOp.
        /// Definitions = the Vars property of this Op
        /// LocalDefinitions = list of computed Vars produced by this node
        /// Keys = Keys of the input Relop (if they are all preserved)
        /// External References = any external references from the computed Vars
        /// RowCount = Input's RowCount
        /// NonNullabeDefinitions = Outputs that are either among the NonNullableDefinitions of the child or
        ///                         are constants defined on this node
        /// NonNullableInputDefinitions = NonNullableDefinitions of the child 
        /// </summary>
        /// <param name="op">The ProjectOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(ProjectOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);

            // Walk through my outputs and identify my "real" definitions
            ExtendedNodeInfo relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            // In the first pass, only definitions of the child are considered
            // to be definitions - everything else is an external reference
            foreach (Var v in op.Outputs)
            {
                if (relOpChildNodeInfo.Definitions.IsSet(v))
                {
                    nodeInfo.Definitions.Set(v);
                }
                else
                {
                    nodeInfo.ExternalReferences.Set(v);
                }
            }

            //Nonnullable definitions 
            nodeInfo.NonNullableDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(op.Outputs);          
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);

            // Local definitions
            foreach (Node chi in n.Child1.Children)
            {
                VarDefOp varDefOp = chi.Op as VarDefOp;
                NodeInfo chiNodeInfo = GetNodeInfo(chi.Child0);
                nodeInfo.LocalDefinitions.Set(varDefOp.Var);
                nodeInfo.ExternalReferences.Clear(varDefOp.Var);
                nodeInfo.Definitions.Set(varDefOp.Var);
                nodeInfo.ExternalReferences.Or(chiNodeInfo.ExternalReferences);

                if (IsDefinitionNonNullable(chi.Child0, nodeInfo.NonNullableVisibleDefinitions))
                {
                    nodeInfo.NonNullableDefinitions.Set(varDefOp.Var);
                }
            }
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);

            // Get the set of keys - simply the list of my child's keys, unless
            // they're not all defined
            nodeInfo.Keys.NoKeys = true;
            if (!relOpChildNodeInfo.Keys.NoKeys)
            {
                // Check to see if any of my child's keys have been left by the wayside
                // in that case, mark this node as having no keys
                VarVec keyVec = m_command.CreateVarVec(relOpChildNodeInfo.Keys.KeyVars);
                Dictionary<Var, Var> varRenameMap = ComputeVarRemappings(n.Child1);
                VarVec mappedKeyVec = keyVec.Remap(varRenameMap);
                VarVec mappedKeyVecClone = mappedKeyVec.Clone();
                VarVec opVars = m_command.CreateVarVec(op.Outputs);
                mappedKeyVec.Minus(opVars);
                if (mappedKeyVec.IsEmpty)
                {
                    nodeInfo.Keys.InitFrom(mappedKeyVecClone);
                }
            }

            nodeInfo.InitRowCountFrom(relOpChildNodeInfo);
            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a FilterOp.
        /// Definitions = Definitions of the input Relop
        /// LocalDefinitions = None
        /// Keys = Keys of the input Relop
        /// External References = any external references from the input + any external
        ///    references from the predicate
        /// MaxOneRow = Input's RowCount
        ///    If the predicate is a "false" predicate, then max RowCount is zero
        ///    If we can infer additional info from the key-selector, we may be 
        ///     able to get better estimates
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp
        /// NonNullableInputDefinitions = NonNullabeDefinitions of the input RelOp
        /// </summary>
        /// <param name="op">The FilterOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(FilterOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            ExtendedNodeInfo relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            NodeInfo predNodeInfo = GetNodeInfo(n.Child1);

            // definitions are my child's definitions
            nodeInfo.Definitions.Or(relOpChildNodeInfo.Definitions);
            // No local definitions

            // My external references are my child's external references + those made
            // by my predicate
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(predNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);

            // my keys are my child's keys
            nodeInfo.Keys.InitFrom(relOpChildNodeInfo.Keys);

            //The non-nullable definitions are same as these of the child
            nodeInfo.NonNullableDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            
            // inherit max RowCount from child; set min RowCount to 0, because 
            // we require way more analysis to do anything smarter
            nodeInfo.MinRows = RowCount.Zero;
            // If the predicate is a "false" predicate, then we know that MaxRows 
            // is zero as well
            ConstantPredicateOp predicate = n.Child1.Op as ConstantPredicateOp;
            if (predicate != null && predicate.IsFalse)
            {
                nodeInfo.MaxRows = RowCount.Zero;
            }
            else
            {
                nodeInfo.MaxRows = relOpChildNodeInfo.MaxRows;
            }
            return nodeInfo;
        }
        
        /// <summary>
        /// Computes a NodeInfo for a GroupByOp.
        /// Definitions = Keys + aggregates
        /// LocalDefinitions = Keys + Aggregates
        /// Keys = GroupBy Keys
        /// External References = any external references from the input + any external
        ///    references from the local computed Vars
        /// RowCount = 
        ///          (1,1) if no group-by keys; 
        ///          otherwise if input MinRows is 1 then (1, input MaxRows); 
        ///          otherwise (0, input MaxRows)
        /// NonNullableDefinitions: non-nullable keys
        /// NonNullableInputDefinitions : default(empty)        
        /// </summary>
        /// <param name="op">The GroupByOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            ExtendedNodeInfo relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);

            // all definitions are my outputs
            nodeInfo.Definitions.InitFrom(op.Outputs);
            nodeInfo.LocalDefinitions.InitFrom(nodeInfo.Definitions);
            // my definitions are the keys and aggregates I define myself

            // My references are my child's external references + those made
            // by my keys and my aggregates
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);
            foreach (Node chi in n.Child1.Children)
            {
                NodeInfo keyExprNodeInfo = GetNodeInfo(chi.Child0);
                nodeInfo.ExternalReferences.Or(keyExprNodeInfo.ExternalReferences);
                if (IsDefinitionNonNullable(chi.Child0, relOpChildNodeInfo.NonNullableDefinitions))
                {
                    nodeInfo.NonNullableDefinitions.Set(((VarDefOp)chi.Op).Var);
                }
            }

            // Non-nullable definitions: also all the keys that come from the input
            nodeInfo.NonNullableDefinitions.Or(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(op.Keys);

            //Handle all aggregates
            for (int i = 2; i < n.Children.Count; i++)
            {
                foreach (Node chi in n.Children[i].Children)
                {
                    NodeInfo aggExprNodeInfo = GetNodeInfo(chi.Child0);
                    nodeInfo.ExternalReferences.Or(aggExprNodeInfo.ExternalReferences);
                }
            }

            // eliminate definitions of my input
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);

            // my keys are my grouping keys
            nodeInfo.Keys.InitFrom(op.Keys);

            // row counts
            nodeInfo.MinRows = op.Keys.IsEmpty ? RowCount.One : (relOpChildNodeInfo.MinRows == RowCount.One ? RowCount.One : RowCount.Zero);
            nodeInfo.MaxRows = op.Keys.IsEmpty ? RowCount.One : relOpChildNodeInfo.MaxRows;

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a CrossJoinOp.
        /// Definitions = Definitions of my children
        /// LocalDefinitions = None
        /// Keys = Concatenation of the keys of my children (if every one of them has keys; otherwise, null)
        /// External References = any external references from the inputs
        /// RowCount: MinRows: min(min-rows of each child)
        ///              MaxRows: max(max-rows of each child)
        /// NonNullableDefinitions : The NonNullableDefinitions of the children
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op">The CrossJoinOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(CrossJoinOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);

            // No definitions of my own. Simply inherit from my children
            // My external references are the union of my children's external
            // references
            // And my keys are the concatenation of the keys of each of my
            // inputs
            List<KeyVec> keyVecList = new List<KeyVec>();
            RowCount maxCard = RowCount.Zero;
            RowCount minCard = RowCount.One;
            foreach (Node chi in n.Children)
            {
                ExtendedNodeInfo chiNodeInfo = GetExtendedNodeInfo(chi);
                nodeInfo.Definitions.Or(chiNodeInfo.Definitions);
                nodeInfo.ExternalReferences.Or(chiNodeInfo.ExternalReferences);
                keyVecList.Add(chiNodeInfo.Keys);

                nodeInfo.NonNullableDefinitions.Or(chiNodeInfo.NonNullableDefinitions);

                // Not entirely precise, but good enough
                if (chiNodeInfo.MaxRows > maxCard)
                {
                    maxCard = chiNodeInfo.MaxRows; 
                }
                if (chiNodeInfo.MinRows < minCard)
                {
                    minCard = chiNodeInfo.MinRows;
                }
            }
            nodeInfo.Keys.InitFrom(keyVecList);

            nodeInfo.SetRowCount(minCard, maxCard);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for an Inner/LeftOuter/FullOuter JoinOp.
        /// Definitions = Definitions of my children
        /// LocalDefinitions = None
        /// Keys = Concatenation of the keys of my children (if every one of them has keys; otherwise, null)
        /// External References = any external references from the inputs + any external
        ///    references from the join predicates
        /// RowCount: 
        ///    FullOuterJoin: MinRows = 0, MaxRows = N
        ///    InnerJoin: MinRows = 0; 
        ///               MaxRows = N; if both inputs have RowCount lesser than (or equal to) 1, then maxCard = 1
        ///    OuterJoin: MinRows = leftInput.MinRows
        ///               MaxRows = N; if both inputs have RowCount lesser than (or equal to) 1, then maxCard = 1
        /// NonNullableDefinitions:
        ///    FullOuterJoin: None.
        ///    InnerJoin: NonNullableDefinitions of both children
        ///    LeftOuterJoin: NonNullableDefinitions of the left child
        /// NonNullableInputDefinitions : NonNullabeDefinitions of both children  
        /// </summary>
        /// <param name="op">The JoinOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitJoinOp(JoinBaseOp op, Node n)
        {
            if (!(op.OpType == OpType.InnerJoin ||
                  op.OpType == OpType.LeftOuterJoin ||
                  op.OpType == OpType.FullOuterJoin))
            {
                return Unimplemented(n);
            }

            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);

            // No definitions of my own. Simply inherit from my children
            // My external references are the union of my children's external
            // references
            // And my keys are the concatenation of the keys of each of my
            // inputs
            ExtendedNodeInfo leftRelOpNodeInfo = GetExtendedNodeInfo(n.Child0);
            ExtendedNodeInfo rightRelOpNodeInfo = GetExtendedNodeInfo(n.Child1);
            NodeInfo predNodeInfo = GetNodeInfo(n.Child2);

            nodeInfo.Definitions.Or(leftRelOpNodeInfo.Definitions);
            nodeInfo.Definitions.Or(rightRelOpNodeInfo.Definitions);

            nodeInfo.ExternalReferences.Or(leftRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(rightRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(predNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(nodeInfo.Definitions);

            nodeInfo.Keys.InitFrom(leftRelOpNodeInfo.Keys, rightRelOpNodeInfo.Keys);

            //Non-nullable definitions
            if (op.OpType == OpType.InnerJoin || op.OpType == OpType.LeftOuterJoin)
            {
                nodeInfo.NonNullableDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);
            }
            if (op.OpType == OpType.InnerJoin)
            {
                nodeInfo.NonNullableDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);
            }
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);

            RowCount maxRows;
            RowCount minRows;
            if (op.OpType == OpType.FullOuterJoin)
            {
                minRows = RowCount.Zero;
                maxRows = RowCount.Unbounded;
            }
            else
            {
                if ((leftRelOpNodeInfo.MaxRows > RowCount.One) ||
                    (rightRelOpNodeInfo.MaxRows > RowCount.One))
                {
                    maxRows = RowCount.Unbounded;
                }
                else
                {
                    maxRows = RowCount.One;
                }

                if (op.OpType == OpType.LeftOuterJoin)
                {
                    minRows = leftRelOpNodeInfo.MinRows;
                }
                else
                {
                    minRows = RowCount.Zero;
                }
            }

            nodeInfo.SetRowCount(minRows, maxRows);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a CrossApply/OuterApply op.
        /// Definitions = Definitions of my children
        /// LocalDefinitions = None
        /// Keys = Concatenation of the keys of my children (if every one of them has keys; otherwise, null)
        /// External References = any external references from the inputs 
        /// RowCount:
        ///    CrossApply: minRows=0; MaxRows=Unbounded 
        ///         (MaxRows = 1, if both inputs have MaxRow less than or equal to 1)
        ///    OuterApply: minRows=leftInput.MinRows; MaxRows=Unbounded
        ///         (MaxRows = 1, if both inputs have MaxRow less than or equal to 1)
        /// NonNullableDefinitions = 
        ///    CrossApply: NonNullableDefinitions of both children
        ///    OuterApply: NonNullableDefinitions of the left child
        /// NonNullableInputDefinitions = NonNullabeDefinitions of both children  
        /// </summary>
        /// <param name="op">The ApplyOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitApplyOp(ApplyBaseOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);

            ExtendedNodeInfo leftRelOpNodeInfo = GetExtendedNodeInfo(n.Child0);
            ExtendedNodeInfo rightRelOpNodeInfo = GetExtendedNodeInfo(n.Child1);

            nodeInfo.Definitions.Or(leftRelOpNodeInfo.Definitions);
            nodeInfo.Definitions.Or(rightRelOpNodeInfo.Definitions);

            nodeInfo.ExternalReferences.Or(leftRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(rightRelOpNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(nodeInfo.Definitions);

            nodeInfo.Keys.InitFrom(leftRelOpNodeInfo.Keys, rightRelOpNodeInfo.Keys);

            //NonNullableDefinitions
            nodeInfo.NonNullableDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);          
            if (op.OpType == OpType.CrossApply)
            {
                nodeInfo.NonNullableDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);
            }
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(leftRelOpNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.Or(rightRelOpNodeInfo.NonNullableDefinitions);

            RowCount maxRows;
            if (leftRelOpNodeInfo.MaxRows <= RowCount.One &&
                rightRelOpNodeInfo.MaxRows <= RowCount.One)
            {
                maxRows = RowCount.One;
            }
            else
            {
                maxRows = RowCount.Unbounded;
            }
            RowCount minRows = (op.OpType == OpType.CrossApply) ? RowCount.Zero : leftRelOpNodeInfo.MinRows;
            nodeInfo.SetRowCount(minRows, maxRows);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for SetOps (UnionAll, Intersect, Except).
        /// Definitions = OutputVars
        /// LocalDefinitions = OutputVars
        /// Keys = Output Vars for Intersect, Except. For UnionAll ??
        /// External References = any external references from the inputs 
        /// RowCount: Min = 0, Max = unbounded.
        ///    For UnionAlls, MinRows = max(MinRows of left and right inputs)
        /// NonNullable definitions =   
        ///     UnionAll - Columns that are NonNullableDefinitions on both (children) sides
        ///     Except  - Columns that are NonNullableDefinitions on the left child side
        ///     Intersect - Columns that are NonNullableDefinitions on either side
        /// NonNullableInputDefinitions = default(empty) because cannot be used
        /// </summary>
        /// <param name="op">The SetOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitSetOp(SetOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);

            // My definitions and my "all" definitions are simply my outputs
            nodeInfo.Definitions.InitFrom(op.Outputs);
            nodeInfo.LocalDefinitions.InitFrom(op.Outputs);

            ExtendedNodeInfo leftChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            ExtendedNodeInfo rightChildNodeInfo = GetExtendedNodeInfo(n.Child1);

            RowCount minRows = RowCount.Zero;
            
            // My external references are the external references of both of 
            // my inputs
            nodeInfo.ExternalReferences.Or(leftChildNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Or(rightChildNodeInfo.ExternalReferences);

            if (op.OpType == OpType.UnionAll) 
            {
                minRows = (leftChildNodeInfo.MinRows > rightChildNodeInfo.MinRows) ? leftChildNodeInfo.MinRows  : rightChildNodeInfo.MinRows;
            }

            // for intersect, and exceptOps, the keys are simply the outputs.
            if (op.OpType == OpType.Intersect || op.OpType == OpType.Except)
            {
                nodeInfo.Keys.InitFrom(op.Outputs);
            }
            else
            {
                // UnionAlls are a lot more complicated.  If we've gone through
                // keyPullup, we will have set some keys on it's input branches and
                // what we need to do here is get the keys from each branch and re-map
                // them to the output vars.
                //
                // If the branchDiscriminator is not set on the unionAllOp, then
                // we haven't been through key pullup and we can't look at the keys
                // that the child nodes have, because they're not discriminated.
                //
                // See the logic in KeyPullup, where we make sure that there are
                // actually branch discriminators on the input branches.
                UnionAllOp unionAllOp = (UnionAllOp)op;

                if (null == unionAllOp.BranchDiscriminator) 
                {
                    nodeInfo.Keys.NoKeys = true;
                }
                else 
                {
                    VarVec nodeKeys = m_command.CreateVarVec();
                    VarVec mappedKeyVec;
                    for (int i = 0; i < n.Children.Count; i++)
                    {
                        ExtendedNodeInfo childNodeInfo = n.Children[i].GetExtendedNodeInfo(m_command);
                        if (!childNodeInfo.Keys.NoKeys && !childNodeInfo.Keys.KeyVars.IsEmpty)
                        {
                            mappedKeyVec = childNodeInfo.Keys.KeyVars.Remap(unionAllOp.VarMap[i].GetReverseMap());
                            nodeKeys.Or(mappedKeyVec);
                        }
                        else
                        {
                            // Each branch had better have keys, or we can't continue.
                            nodeKeys.Clear();
                            break;
                        }
                    }
                    
                    // You might be tempted to ask: "Don't we need to add the branch discriminator 
                    // to the keys as well?"  The reason we don't is that we wouldn't be here unless 
                    // we have a branch discriminator variable, which implies we've pulled up keys on
                    // the inputs, and they'll already have the branch descriminator set in the keys
                    // of each input, so we don't need to add that...
                    if (nodeKeys.IsEmpty) 
                    {
                        nodeInfo.Keys.NoKeys = true;
                    }
                    else 
                    {
                        nodeInfo.Keys.InitFrom(nodeKeys);
                    }
                }
            }

            //Non-nullable definitions
            VarVec leftNonNullableVars = leftChildNodeInfo.NonNullableDefinitions.Remap(op.VarMap[0].GetReverseMap());
            nodeInfo.NonNullableDefinitions.InitFrom(leftNonNullableVars);
            
            if (op.OpType != OpType.Except)
            {
                VarVec rightNonNullableVars = rightChildNodeInfo.NonNullableDefinitions.Remap(op.VarMap[1].GetReverseMap());
                if (op.OpType == OpType.Intersect)
                {
                    nodeInfo.NonNullableDefinitions.Or(rightNonNullableVars);
                }
                else  //Union all
                {
                    nodeInfo.NonNullableDefinitions.And(rightNonNullableVars);
                }
            }

            nodeInfo.NonNullableDefinitions.And(op.Outputs);

            nodeInfo.MinRows = minRows;
            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a ConstrainedSortOp/SortOp.
        /// Definitions = Definitions of the input Relop
        /// LocalDefinitions = not allowed
        /// Keys = Keys of the input Relop
        /// External References = any external references from the input + any external
        ///    references from the keys
        /// RowCount = Input's RowCount
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp
        /// NonNullableInputDefinitions = NonNullabeDefinitions of the input RelOp
        /// </summary>
        /// <param name="op">The SortOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitSortOp(SortBaseOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            ExtendedNodeInfo relOpChildNodeInfo = GetExtendedNodeInfo(n.Child0);

            // definitions are my child's definitions
            nodeInfo.Definitions.Or(relOpChildNodeInfo.Definitions);
            
            // My references are my child's external references + those made
            // by my sort keys
            nodeInfo.ExternalReferences.Or(relOpChildNodeInfo.ExternalReferences);
            nodeInfo.ExternalReferences.Minus(relOpChildNodeInfo.Definitions);

            // my keys are my child's keys
            nodeInfo.Keys.InitFrom(relOpChildNodeInfo.Keys);

            //Non-nullable definitions are same as the input
            nodeInfo.NonNullableDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableVisibleDefinitions.InitFrom(relOpChildNodeInfo.NonNullableDefinitions);
            
            //Row counts are same as the input
            nodeInfo.InitRowCountFrom(relOpChildNodeInfo);

            // For constrained sort, if the Limit value is Constant(1) and WithTies is false,
            // then MinRows and MaxRows can be adjusted to 0, 1.
            if (OpType.ConstrainedSort == op.OpType &&
                OpType.Constant == n.Child2.Op.OpType &&
                !((ConstrainedSortOp)op).WithTies)
            {
                ConstantBaseOp constOp = (ConstantBaseOp)n.Child2.Op;
                if(TypeHelpers.IsIntegerConstant(constOp.Type, constOp.Value, 1))
                {
                    nodeInfo.SetRowCount(RowCount.Zero, RowCount.One);
                }
            }

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for Distinct.
        /// Definitions = OutputVars that are not external references
        /// LocalDefinitions = None
        /// Keys = Output Vars 
        /// External References = any external references from the inputs 
        /// RowCount = Input's RowCount
        /// NonNullabeDefinitions : NonNullabeDefinitions of the input RelOp that are outputs
        /// NonNullableInputDefinitions : default(empty) because cannot be used
        /// </summary>
        /// <param name="op">The DistinctOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(DistinctOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);

            //#497217 - The parameters should not be included as keys
            nodeInfo.Keys.InitFrom(op.Keys, true);

            // external references - inherit from child
            ExtendedNodeInfo childNodeInfo = GetExtendedNodeInfo(n.Child0);
            nodeInfo.ExternalReferences.InitFrom(childNodeInfo.ExternalReferences);

            // no local definitions - definitions are just the keys that are not external references
            foreach (Var v in op.Keys)
            {
                if (childNodeInfo.Definitions.IsSet(v))
                {
                    nodeInfo.Definitions.Set(v);
                }
                else
                {
                    nodeInfo.ExternalReferences.Set(v);
                }
            }

            //Non-nullable definitions
            nodeInfo.NonNullableDefinitions.InitFrom(childNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(op.Keys);

            nodeInfo.InitRowCountFrom(childNodeInfo);
            return nodeInfo;
        }

        /// <summary>
        /// Compute NodeInfo for a SingleRowOp.
        /// Definitions = child's definitions
        /// Keys = child's keys
        /// Local Definitions = none
        /// External references = child's external references
        /// RowCount=(0,1)
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp
        /// NonNullableInputDefinitions : default(empty) because cannot be used        
        /// </summary>
        /// <param name="op">The SingleRowOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>NodeInfo for this node</returns>
        public override NodeInfo Visit(SingleRowOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            ExtendedNodeInfo childNodeInfo = GetExtendedNodeInfo(n.Child0);
            nodeInfo.Definitions.InitFrom(childNodeInfo.Definitions);
            nodeInfo.Keys.InitFrom(childNodeInfo.Keys);
            nodeInfo.ExternalReferences.InitFrom(childNodeInfo.ExternalReferences);
            nodeInfo.NonNullableDefinitions.InitFrom(childNodeInfo.NonNullableDefinitions);
            nodeInfo.SetRowCount(RowCount.Zero, RowCount.One);
            return nodeInfo;
        }

        /// <summary>
        /// SingleRowTableOp
        /// No definitions, external references, non-nullable definitions
        /// Keys = empty list (not the same as "no keys")
        /// RowCount = (1,1)
        /// </summary>
        /// <param name="op">the SingleRowTableOp</param>
        /// <param name="n">current subtree</param>
        /// <returns>nodeInfo for this subtree</returns>
        public override NodeInfo Visit(SingleRowTableOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            nodeInfo.Keys.NoKeys = false;
            nodeInfo.SetRowCount(RowCount.One, RowCount.One);
            return nodeInfo;
        }

        #endregion

        #region PhysicalOps
        /// <summary>
        /// Computes a NodeInfo for a PhysicalProjectOp.
        /// Definitions = OutputVars
        /// LocalDefinitions = None
        /// Keys = None
        /// External References = any external references from the inputs
        /// RowCount=default
        /// NonNullabeDefinitions = NonNullabeDefinitions of the input RelOp that are among the definitions
        /// NonNullableInputDefinitions = NonNullabeDefinitions of the input RelOp
        /// </summary>
        /// <param name="op">The PhysicalProjectOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        public override NodeInfo Visit(PhysicalProjectOp op, Node n)
        {
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);
            foreach (Node chi in n.Children)
            {
                NodeInfo childNodeInfo = GetNodeInfo(chi);
                nodeInfo.ExternalReferences.Or(childNodeInfo.ExternalReferences);
            }
            nodeInfo.Definitions.InitFrom(op.Outputs);
            nodeInfo.LocalDefinitions.InitFrom(nodeInfo.Definitions);

            //
            // Inherit the keys from the child - but only if all the columns were projected
            // out
            // 
            ExtendedNodeInfo driverChildNodeInfo = GetExtendedNodeInfo(n.Child0);
            if (!driverChildNodeInfo.Keys.NoKeys)
            {
                VarVec missingKeys = m_command.CreateVarVec(driverChildNodeInfo.Keys.KeyVars);
                missingKeys.Minus(nodeInfo.Definitions);
                if (missingKeys.IsEmpty)
                {
                    nodeInfo.Keys.InitFrom(driverChildNodeInfo.Keys);
                }
            }

            //Non-nullable definitions
            nodeInfo.NonNullableDefinitions.Or(driverChildNodeInfo.NonNullableDefinitions);
            nodeInfo.NonNullableDefinitions.And(nodeInfo.Definitions);
            nodeInfo.NonNullableVisibleDefinitions.Or(driverChildNodeInfo.NonNullableVisibleDefinitions);

            return nodeInfo;
        }

        /// <summary>
        /// Computes a NodeInfo for a NestOp (SingleStream/MultiStream).
        /// Definitions = OutputVars
        /// LocalDefinitions = Collection Vars
        /// Keys = Keys of my child
        /// External References = any external references from the inputs 
        /// RowCount=default
        /// </summary>
        /// <param name="op">The NestOp</param>
        /// <param name="n">corresponding Node</param>
        /// <returns></returns>
        protected override NodeInfo VisitNestOp(NestBaseOp op, Node n)
        {
            SingleStreamNestOp ssnOp = op as SingleStreamNestOp;
            ExtendedNodeInfo nodeInfo = InitExtendedNodeInfo(n);

            foreach (CollectionInfo ci in op.CollectionInfo)
            {
                nodeInfo.LocalDefinitions.Set(ci.CollectionVar);
            }
            nodeInfo.Definitions.InitFrom(op.Outputs);

            // get external references from each child
            foreach (Node chi in n.Children)
            {
                nodeInfo.ExternalReferences.Or(GetExtendedNodeInfo(chi).ExternalReferences);
            }

            // eliminate things I may have defined already (left correlation)
            nodeInfo.ExternalReferences.Minus(nodeInfo.Definitions);
            
            // Keys are from the driving node only.
            if (ssnOp == null) 
            {
                nodeInfo.Keys.InitFrom(GetExtendedNodeInfo(n.Child0).Keys);
            }
            else 
            {
                nodeInfo.Keys.InitFrom(ssnOp.Keys);
            } 
            return nodeInfo;
        }

        #endregion

        #endregion
    }
}

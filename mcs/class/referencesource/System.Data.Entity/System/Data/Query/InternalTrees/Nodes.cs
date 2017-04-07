//---------------------------------------------------------------------
// <copyright file="Nodes.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Metadata.Edm;
using System.Globalization;
using System.Diagnostics;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// A Node describes a node in a query tree. Each node has an operator, and
    /// a list of zero or more children of that operator.
    /// </summary>
    internal class Node
    {
        #region private state
        private int m_id;
        private List<Node> m_children;
        private Op m_op;
        private NodeInfo m_nodeInfo;
        #endregion

        #region constructors
        /// <summary>
        /// Basic constructor. 
        /// 
        /// NEVER call this routine directly - you should always use the Command.CreateNode
        /// factory methods.
        /// </summary>
        /// <param name="nodeId">id for the node</param>
        /// <param name="op">The operator</param>
        /// <param name="children">List of child nodes</param>
        internal Node(int nodeId, Op op, List<Node> children)
        {
            m_id = nodeId;
            m_op = op;
            m_children = children;
        }

        /// <summary>
        /// This routine is only used for building up rule patterns. 
        /// NEVER use this routine for building up nodes in a user command tree.
        /// </summary>
        /// <param name="op"></param>
        /// <param name="children"></param>
        internal Node(Op op, params Node[] children)
            : this(-1, op, new List<Node>(children))
        {
        }
        #endregion

        #region public properties and methods

#if DEBUG
        internal int Id { get { return m_id; } }
#endif

        /// <summary>
        /// Get the list of children
        /// </summary>
        internal List<Node> Children { get { return m_children; } }

        /// <summary>
        /// Gets or sets the node's operator
        /// </summary>
        internal Op Op { get { return m_op; } set { m_op = value; } }

        /// <summary>
        /// Simpler (?) getter/setter routines
        /// </summary>
        internal Node Child0 { get { return m_children[0]; } set { m_children[0] = value; } }
        /// <summary>
        /// Do I have a zeroth child?
        /// </summary>
        internal bool HasChild0 { get { return m_children.Count > 0; } }

        /// <summary>
        /// Get/set first child
        /// </summary>
        internal Node Child1 { get { return m_children[1]; } set { m_children[1] = value; } }
        /// <summary>
        /// Do I have a child1?
        /// </summary>
        internal bool HasChild1 { get { return m_children.Count > 1; } }

        /// <summary>
        /// get/set second child
        /// </summary>
        internal Node Child2 { get { return m_children[2]; } set { m_children[2] = value; } }
        /// <summary>
        /// get/set second child
        /// </summary>
        internal Node Child3 { get { return m_children[3]; } /* commented out because of fxcop - there are no upstream callers -- set { m_children[3] = value; }*/ }
        /// <summary>
        /// Do I have a child2 (third child really)
        /// </summary>
        internal bool HasChild2 { get { return m_children.Count > 2; } }
        /// <summary>
        /// Do I have a child3 (fourth child really)
        /// </summary>
        internal bool HasChild3 { get { return m_children.Count > 3; } }

        #region equivalence functions
        /// <summary>
        /// Is this subtree equivalent to another subtree
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        internal bool IsEquivalent(Node other)
        {
            if (this.Children.Count != other.Children.Count)
            {
                return false;
            }
            bool? opEquivalent = this.Op.IsEquivalent(other.Op);
            if (opEquivalent != true)
            {
                return false;
            }
            for (int i = 0; i < this.Children.Count; i++)
            {
                if (!this.Children[i].IsEquivalent(other.Children[i]))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #region NodeInfo methods and properties
        /// <summary>
        /// Has the node info been initialized, i.e. previously computed
        /// </summary>
        internal bool IsNodeInfoInitialized { get { return (m_nodeInfo != null);  } }

        /// <summary>
        /// Get the nodeInfo for a node. Initializes it, if it has not yet been initialized
        /// </summary>
        /// <param name="command">Current command object</param>
        /// <returns>NodeInfo for this node</returns>
        internal NodeInfo GetNodeInfo(Command command)
        {
            if (m_nodeInfo == null)
            {
                InitializeNodeInfo(command);
            }
            return m_nodeInfo;
        }
        /// <summary>
        /// Gets the "extended" nodeinfo for a node; if it has not yet been initialized, then it will be
        /// </summary>
        /// <param name="command">current command object</param>
        /// <returns>extended nodeinfo for this node</returns>
        internal ExtendedNodeInfo GetExtendedNodeInfo(Command command)
        {
            if (m_nodeInfo == null)
            {
                InitializeNodeInfo(command);
            }
            ExtendedNodeInfo extendedNodeInfo = m_nodeInfo as ExtendedNodeInfo;
            Debug.Assert(extendedNodeInfo != null);
            return extendedNodeInfo;
        }

        private void InitializeNodeInfo(Command command)
        {
            if (this.Op.IsRelOp || this.Op.IsPhysicalOp)
            {
                m_nodeInfo = new ExtendedNodeInfo(command);
            }
            else
            {
                m_nodeInfo = new NodeInfo(command);
            }
            command.RecomputeNodeInfo(this);
        }
        #endregion

        #endregion
    }
}

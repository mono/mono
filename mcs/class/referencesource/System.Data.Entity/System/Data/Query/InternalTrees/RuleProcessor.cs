//---------------------------------------------------------------------
// <copyright file="RuleProcessor.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Diagnostics;

namespace System.Data.Query.InternalTrees
{
    #region RuleProcessor
    /// <summary>
    /// The RuleProcessor helps apply a set of rules to a query tree
    /// </summary>
    internal class RuleProcessor
    {
        #region private state
        /// <summary>
        /// A lookup table for rules.
        /// The lookup table is an array indexed by OpType and each entry has a list of rules.
        /// </summary>
        private Dictionary<SubTreeId, SubTreeId> m_processedNodeMap;
        #endregion

        #region constructors
        /// <summary>
        /// Initializes a new RuleProcessor
        /// </summary>
        internal RuleProcessor()
        {
            // Build up the accelerator tables
            m_processedNodeMap = new Dictionary<SubTreeId, SubTreeId>();
        }
        #endregion

        #region private methods

        private static bool ApplyRulesToNode(RuleProcessingContext context, ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> rules, Node currentNode, out Node newNode)
        {
            newNode = currentNode;

            // Apply any pre-rule delegates
            context.PreProcess(currentNode);

            foreach (Rule r in rules[(int)currentNode.Op.OpType])
            {
                if (!r.Match(currentNode))
                {
                    continue;
                }

                // Did the rule modify the subtree?
                if (r.Apply(context, currentNode, out newNode))
                {
                    // The node has changed; don't try to apply any more rules
                    context.PostProcess(newNode, r);
                    return true;
                }
                else
                {
                    Debug.Assert(newNode == currentNode, "Liar! This rule should have returned 'true'");
                }
            }

            context.PostProcess(currentNode, null);
            return false;
        }

        /// <summary>
        /// Apply rules to the current subtree in a bottom-up fashion. 
        /// </summary>
        /// <param name="context">Current rule processing context</param>
        /// <param name="rules">The look-up table with the rules to be applied</param>
        /// <param name="subTreeRoot">Current subtree</param>
        /// <param name="parent">Parent node</param>
        /// <param name="childIndexInParent">Index of this child within the parent</param>
        /// <returns>the result of the transformation</returns>
        private Node ApplyRulesToSubtree(RuleProcessingContext context, 
            ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> rules,
            Node subTreeRoot, Node parent, int childIndexInParent)
        {
            int loopCount = 0;
            Dictionary<SubTreeId, SubTreeId> localProcessedMap = new Dictionary<SubTreeId, SubTreeId>();
            SubTreeId subTreeId;

            while (true)
            {
                // Am I looping forever
                Debug.Assert(loopCount < 12, "endless loops?");
                loopCount++;

                //
                // We may need to update state regardless of whether this subTree has 
                // changed after it has been processed last. For example, it may be 
                // affected by transformation in its siblings due to external references.
                //
                context.PreProcessSubTree(subTreeRoot);
                subTreeId = new SubTreeId(context, subTreeRoot, parent, childIndexInParent);
   
                // Have I seen this subtree already? Just return, if so
                if (m_processedNodeMap.ContainsKey(subTreeId))
                {
                    break;
                }

                // Avoid endless loops here - avoid cycles of 2 or more
                if (localProcessedMap.ContainsKey(subTreeId))
                {
                    // mark this subtree as processed
                    m_processedNodeMap[subTreeId] = subTreeId;
                    break;
                }
                // Keep track of this one
                localProcessedMap[subTreeId] = subTreeId;

                // Walk my children
                for (int i = 0; i < subTreeRoot.Children.Count; i++)
                {
                    subTreeRoot.Children[i] = ApplyRulesToSubtree(context, rules, subTreeRoot.Children[i], subTreeRoot, i);
                }

                // Apply rules to myself. If no transformations were performed, 
                // then mark this subtree as processed, and break out
                Node newSubTreeRoot;
                if (!ApplyRulesToNode(context, rules, subTreeRoot, out newSubTreeRoot))
                {
                    Debug.Assert(subTreeRoot == newSubTreeRoot);
                    // mark this subtree as processed
                    m_processedNodeMap[subTreeId] = subTreeId;
                    break;
                }
                context.PostProcessSubTree(subTreeRoot);
                subTreeRoot = newSubTreeRoot;
            }

            context.PostProcessSubTree(subTreeRoot);
            return subTreeRoot;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Apply a set of rules to the subtree
        /// </summary>
        /// <param name="context">Rule processing context</param>
        /// <param name="subTreeRoot">current subtree</param>
        /// <returns>transformed subtree</returns>
        internal Node ApplyRulesToSubtree(RuleProcessingContext context, ReadOnlyCollection<ReadOnlyCollection<InternalTrees.Rule>> rules, Node subTreeRoot)
        {
            return ApplyRulesToSubtree(context, rules, subTreeRoot, null, 0);
        }

        #endregion
    }
    #endregion

    #region SubTreeId
    internal class SubTreeId
    {
        #region private state
        public Node m_subTreeRoot;
        private int m_hashCode;
        private Node m_parent;
        private int m_parentHashCode;
        private int m_childIndex;
        #endregion

        #region constructors
        internal SubTreeId(RuleProcessingContext context, Node node, Node parent, int childIndex)
        {
            m_subTreeRoot = node;
            m_parent = parent;
            m_childIndex = childIndex;
            m_hashCode = context.GetHashCode(node);
            m_parentHashCode = parent == null ? 0 : context.GetHashCode(parent);
        }
        #endregion

        #region public surface
        public override int GetHashCode()
        {
            return m_hashCode;
        }
        public override bool Equals(object obj)
        {
            SubTreeId other = obj as SubTreeId;
            return ((other != null) && (m_hashCode == other.m_hashCode) &&
                ((other.m_subTreeRoot == this.m_subTreeRoot) ||
                  ((other.m_parent == this.m_parent) && (other.m_childIndex == this.m_childIndex))));
        }
        #endregion
    }
    #endregion

    #region RuleProcessingContext

    /// <summary>
    /// Delegate that describes the processing 
    /// </summary>
    /// <param name="context">RuleProcessing context</param>
    /// <param name="node">Node to process</param>
    internal delegate void OpDelegate(RuleProcessingContext context, Node node);

    /// <summary>
    /// A RuleProcessingContext encapsulates information needed by various rules to process
    /// the query tree.
    /// </summary>
    internal abstract class RuleProcessingContext
    {
        #region public surface
        internal Command Command
        {
            get { return m_command; }
        }

        /// <summary>
        /// Callback function to be applied to a node before any rules are applied
        /// </summary>
        /// <param name="node">the node</param>
        internal virtual void PreProcess(Node node)
        {

        }

        /// <summary>
        /// Callback function to be applied to the subtree rooted at the given 
        /// node before any rules are applied
        /// </summary>
        /// <param name="node">the node that is the root of the subtree</param>
        internal virtual void PreProcessSubTree(Node node)
        {
        }

        /// <summary>
        /// Callback function to be applied on a node after a rule has been applied
        /// that has modified the node
        /// </summary>
        /// <param name="node">current node</param>
        /// <param name="rule">the rule that modified the node</param>
        internal virtual void PostProcess(Node node, Rule rule)
        {
        }

        /// <summary>
        /// Callback function to be applied to the subtree rooted at the given 
        /// node after any rules are applied
        /// </summary>
        /// <param name="node">the node that is the root of the subtree</param>
        internal virtual void PostProcessSubTree(Node node)
        {
        }

        /// <summary>
        /// Get the hashcode for this node - to ensure that we don't loop forever
        /// </summary>
        /// <param name="node">current node</param>
        /// <returns>int hashcode</returns>
        internal virtual int GetHashCode(Node node)
        {
            return node.GetHashCode();
        }
        #endregion

        #region constructors
        internal RuleProcessingContext(Command command)
        {
            m_command = command;
        }
        #endregion

        #region private state
        private Command m_command;
        #endregion
    }
    #endregion
}

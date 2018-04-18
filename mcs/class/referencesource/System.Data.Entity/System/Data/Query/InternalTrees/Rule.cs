//---------------------------------------------------------------------
// <copyright file="Rule.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;

namespace System.Data.Query.InternalTrees
{
    /// <summary>
    /// A Rule - more specifically, a transformation rule - describes an action that is to
    /// be taken when a specific kind of subtree is found in the tree
    /// </summary>
    internal abstract class Rule
    {
        /// <summary>
        /// The "callback" function for each rule. 
        /// Every callback function must return true if the subtree has
        /// been modified (or a new subtree has been returned); and must return false
        /// otherwise. If the root of the subtree has not changed, but some internal details
        /// of the subtree have changed, it is the responsibility of the rule to update any
        /// local bookkeeping information.
        /// </summary>
        /// <param name="context">The rule processing context</param>
        /// <param name="subTree">the subtree to operate on</param>
        /// <param name="newSubTree">possibly transformed subtree</param>
        /// <returns>transformation status - true, if there was some change; false otherwise</returns>
        internal delegate bool ProcessNodeDelegate(RuleProcessingContext context, Node subTree, out Node newSubTree);

        #region private state
        private ProcessNodeDelegate m_nodeDelegate;
        private OpType m_opType;
        #endregion

        #region Constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="opType">The OpType we're interested in processing</param>
        /// <param name="nodeProcessDelegate">The callback to invoke</param>
        protected Rule(OpType opType, ProcessNodeDelegate nodeProcessDelegate)
        {
            Debug.Assert(nodeProcessDelegate != null, "null process delegate");
            Debug.Assert(opType != OpType.NotValid, "bad OpType");
            Debug.Assert(opType != OpType.Leaf, "bad OpType - Leaf");

            m_opType = opType;
            m_nodeDelegate = nodeProcessDelegate;
        }
        #endregion

        #region protected methods

        #endregion

        #region public methods
        /// <summary>
        /// Does the rule match the current node?
        /// </summary>
        /// <param name="node">the node in question</param>
        /// <returns>true, if a match was found</returns>
        internal abstract bool Match(Node node);

        /// <summary>
        /// We need to invoke the specified callback on the subtree in question - but only
        /// if the match succeeds
        /// </summary>
        /// <param name="ruleProcessingContext">Current rule processing context</param>
        /// <param name="node">The node (subtree) to process</param>
        /// <param name="newNode">the (possibly) modified subtree</param>
        /// <returns>true, if the subtree was modified</returns>
        internal bool Apply(RuleProcessingContext ruleProcessingContext, Node node, out Node newNode)
        {
            // invoke the real callback
            return m_nodeDelegate(ruleProcessingContext, node, out newNode);
        }

        /// <summary>
        /// The OpType we're interested in transforming
        /// </summary>
        internal OpType RuleOpType
        {
            get { return m_opType; }
        }

#if DEBUG
        /// <summary>
        /// The method name for the rule
        /// </summary>
        internal string MethodName
        {
            get { return m_nodeDelegate.Method.Name; }
        }
#endif

        #endregion
    }

    /// <summary>
    /// A SimpleRule is a rule that specifies a specific OpType to look for, and an
    /// appropriate action to take when such an Op is identified
    /// </summary>
    internal sealed class SimpleRule : Rule
    {
        #region private state
        #endregion

        #region constructors
        /// <summary>
        /// Basic constructor.
        /// </summary>
        /// <param name="opType">The OpType we're interested in</param>
        /// <param name="processDelegate">The callback to invoke when we see such an Op</param>
        internal SimpleRule(OpType opType, ProcessNodeDelegate processDelegate)
            : base(opType, processDelegate)
        {
        }
        #endregion

        #region overriden methods
        internal override bool Match(Node node)
        {
            return node.Op.OpType == this.RuleOpType;
        }
        #endregion
    }

    /// <summary>
    /// A PatternMatchRule allows for a pattern to be specified to identify interesting
    /// subtrees, rather than just an OpType
    /// </summary>
    internal sealed class PatternMatchRule: Rule
    {
        #region private state
        private Node m_pattern;
        #endregion

        #region constructors
        /// <summary>
        /// Basic constructor
        /// </summary>
        /// <param name="pattern">The pattern to look for</param>
        /// <param name="processDelegate">The callback to invoke when such a pattern is identified</param>
        internal PatternMatchRule(Node pattern, ProcessNodeDelegate processDelegate)
            : base(pattern.Op.OpType, processDelegate)
        {
            Debug.Assert(pattern != null, "null pattern");
            Debug.Assert(pattern.Op != null, "null pattern Op");
            m_pattern = pattern;
        }
        #endregion

        #region private methods
        private bool Match(Node pattern, Node original)
        {
            if (pattern.Op.OpType == OpType.Leaf)
                return true;
            if (pattern.Op.OpType != original.Op.OpType)
                return false;
            if (pattern.Children.Count != original.Children.Count)
                return false;
            for (int i = 0; i < pattern.Children.Count; i++)
                if (!Match(pattern.Children[i], original.Children[i]))
                    return false;
            return true;
        }
        #endregion

        #region overridden methods
        internal override bool Match(Node node)
        {
            return Match(m_pattern, node);
        }
        #endregion
    }
}

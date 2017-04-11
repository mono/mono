//---------------------------------------------------------------------
// <copyright file="VarRefManager.cs" company="Microsoft">
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
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// This is a halper module for <see cref="JoinElimination"/>
    /// The VarRefManager keeps track of the child-parent relationships in order to be able
    /// to decide whether a given var is referenced by children on right-side relatives of a given node.
    /// It is used in JoinElimination when deciding whether it is possible to eliminate the child table participating
    /// in a left-outer join when there is a 1 - 0..1 FK relationship.
    /// </summary>
    internal class VarRefManager
    {
        #region Internal State
        private Dictionary<Node, Node> m_nodeToParentMap;   //child-parent mapping
        private Dictionary<Node, int> m_nodeToSiblingNumber;   //the index of the given node among its siblings, i.e. 0 for a first child
        Command m_command;
        #endregion

        #region Constructor
        /// <summary>
        /// Constructs a new VarRefManager given a command.
        /// </summary>
        /// <param name="command"></param>
        internal VarRefManager(Command command)
        {
            m_nodeToParentMap = new Dictionary<Node, Node>();
            m_nodeToSiblingNumber = new Dictionary<Node, int>();
            m_command = command;
        }
        #endregion

        #region Public Methods

        /// <summary>
        /// Tracks the information that the given node is a parent of its children (one level only)
        /// </summary>
        /// <param name="parent"></param>
        internal void AddChildren(Node parent)
        {
            for(int i=0; i< parent.Children.Count; i++)
            {                
                //We do not use add on purpose, we may be updating a child's parent after join elimination in a subtree
                m_nodeToParentMap[parent.Children[i]] = parent;
                m_nodeToSiblingNumber[parent.Children[i]] = i;
            }
        }

        /// <summary>
        /// Determines whether any var from a given list of keys is referenced by any of defining node's right relatives, 
        /// with the exception of the relatives brunching at the given targetJoinNode.
        /// </summary>
        /// <param name="keys">A list of vars to check for</param>
        /// <param name="definingNode">The node considered to be the defining node</param>
        /// <param name="targetJoinNode">The relatives branching at this node are skipped</param>
        /// <returns>False, only it can determine that not a single var from a given list of keys is referenced by any 
        /// of defining node's right relatives, with the exception of the relatives brunching at the given targetJoinNode. </returns>
        internal bool HasKeyReferences(VarVec keys, Node definingNode, Node targetJoinNode)
        {
            Node currentChild = definingNode;
            Node parent;
            bool continueUp = true;

            while (continueUp & m_nodeToParentMap.TryGetValue(currentChild, out parent))
            {
                if (parent != targetJoinNode)
                {
                    // Check the parent
                    if (HasVarReferencesShallow(parent, keys, m_nodeToSiblingNumber[currentChild], out continueUp))
                    {
                        return true;
                    }

                    //Check all the siblings to the right
                    for (int i = m_nodeToSiblingNumber[currentChild] + 1; i < parent.Children.Count; i++)
                    {
                        if (parent.Children[i].GetNodeInfo(m_command).ExternalReferences.Overlaps(keys))
                        {
                            return true;
                        }
                    }
                }
                currentChild = parent;
            }
            return false;
        }

        #endregion

        #region Private Methods
        /// <summary>
        /// Checks whether the given node has references to any of the vars in the given VarVec.
        /// It only checks the given node, not its children.
        /// </summary>
        /// <param name="node">The node to check</param>
        /// <param name="vars">The list of vars to check for</param>
        /// <param name="childIndex">The index of the node's subree from which this var is coming.
        /// This is used for SetOp-s, to be able to locate the appropriate var map that will give the
        /// vars corresponding to the given once</param>
        /// <param name="continueUp">If the OpType of the node's Op is such that it 'hides' the input, i.e.
        /// the decision of whether the given vars are referenced can be made on this level, it returns true,
        /// false otherwise</param>
        /// <returns>True if the given node has references to any of the vars in the given VarVec, false otherwise</returns>
        private static bool HasVarReferencesShallow(Node node, VarVec vars, int childIndex, out bool continueUp)
        {
            switch (node.Op.OpType)
            {
                case OpType.ConstrainedSort:
                case OpType.Sort:
                    continueUp = true;
                    return HasVarReferences(((SortBaseOp)node.Op).Keys, vars);

                case OpType.Distinct:
                    continueUp = false;
                    return HasVarReferences(((DistinctOp)node.Op).Keys, vars);

                case OpType.Except:
                case OpType.Intersect:
                case OpType.UnionAll:
                    continueUp = false;
                    return HasVarReferences((SetOp)node.Op, vars, childIndex);

                case OpType.GroupBy:
                    continueUp = false;
                    return HasVarReferences(((GroupByOp)node.Op).Keys, vars);

                case OpType.PhysicalProject:
                    continueUp = false;
                    return HasVarReferences(((PhysicalProjectOp)node.Op).Outputs, vars);

                case OpType.Project:
                    continueUp = false;
                    return HasVarReferences(((ProjectOp)node.Op).Outputs, vars);

                default:
                    continueUp = true;
                    return false;
            }
        }

        /// <summary>
        /// Does the gvien VarList overlap with the given VarVec
        /// </summary>
        /// <param name="listToCheck"></param>
        /// <param name="vars"></param>
        /// <returns></returns>
        private static bool HasVarReferences(VarList listToCheck, VarVec vars)
        {
            foreach (Var var in vars)
            {
                if (listToCheck.Contains(var))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Do the two given varVecs overlap
        /// </summary>
        /// <param name="listToCheck"></param>
        /// <param name="vars"></param>
        /// <returns></returns>
        private static bool HasVarReferences(VarVec listToCheck, VarVec vars)
        {
            return listToCheck.Overlaps(vars);
        }

        /// <summary>
        /// Does the given list of sort keys contain a key with a var that is the given VarVec
        /// </summary>
        /// <param name="listToCheck"></param>
        /// <param name="vars"></param>
        /// <returns></returns>
        private static bool HasVarReferences(List<InternalTrees.SortKey> listToCheck, VarVec vars)
        {
            foreach (InternalTrees.SortKey key in listToCheck)
            {
                if (vars.IsSet(key.Var))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Does the list of outputs of the given SetOp contain a var 
        /// from the given VarVec defined by the SetOp's child with the given index
        /// </summary>
        /// <param name="op"></param>
        /// <param name="vars"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        private static bool HasVarReferences(SetOp op, VarVec vars, int index)
        {
            foreach (Var var in op.VarMap[index].Values)
            {
                if (vars.IsSet(var))
                {
                    return true;
                }
            }
            return false;
        }
        #endregion
    }
}

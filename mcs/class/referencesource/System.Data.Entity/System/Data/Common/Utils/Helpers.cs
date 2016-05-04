//---------------------------------------------------------------------
// <copyright file="Util.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner [....]
// @backupOwner [....]
//---------------------------------------------------------------------


using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using System.Globalization;
using System.Diagnostics;

namespace System.Data.Common.Utils {
    
    // Miscellaneous helper routines
    internal static class Helpers {

        #region Trace methods
        // effects: Trace args according to the CLR format string with a new line
        internal static void FormatTraceLine(string format, params object[] args) {
            Trace.WriteLine(String.Format(CultureInfo.InvariantCulture, format, args));
        }

        // effects: Trace the string with a new line
        internal static void StringTrace(string arg) {
            Trace.Write(arg);
        }

        // effects: Trace the string without adding a new line
        internal static void StringTraceLine(string arg) {
            Trace.WriteLine(arg);
        }
        #endregion

        #region Misc Helpers
        // effects: compares two sets using the given comparer - removes
        // duplicates if they exist
        internal static bool IsSetEqual<Type>(IEnumerable<Type> list1, IEnumerable<Type> list2, IEqualityComparer<Type> comparer)
        {
            Set<Type> set1 = new Set<Type>(list1, comparer);
            Set<Type> set2 = new Set<Type>(list2, comparer);

            return set1.SetEquals(set2);
        }

        // effects: Given a stream of values of type "SubType", returns a
        // stream of values of type "SuperType" where SuperType is a
        // superclass/supertype of SubType
        internal static IEnumerable<SuperType> AsSuperTypeList<SubType, SuperType>(IEnumerable<SubType> values)
            where SubType : SuperType {
            foreach (SubType value in values) {
                yield return value;
            }
        }

        /// <summary>
        /// Returns a new array with the first element equal to <paramref name="arg"/> and the remaining
        /// elements taken from <paramref name="args"/>.
        /// </summary>
        /// <typeparam name="TElement">The element type of the arrays</typeparam>
        /// <param name="args">An array that provides the successive elements of the new array</param>
        /// <param name="arg">An instance the provides the first element of the new array</param>
        /// <returns>A new array containing the specified argument as the first element and the specified successive elements</returns>
        internal static TElement[] Prepend<TElement>(TElement[] args, TElement arg)
        {
            Debug.Assert(args != null, "Ensure 'args' is non-null before calling Prepend");

            TElement[] retVal = new TElement[args.Length + 1];
            retVal[0] = arg;
            for (int idx = 0; idx < args.Length; idx++)
            {
                retVal[idx + 1] = args[idx];
            }

            return retVal;
        }

        /// <summary>
        /// Builds a balanced binary tree with the specified nodes as leaves. 
        /// Note that the current elements of <paramref name="nodes"/> MAY be overwritten
        /// as the leaves are combined to produce the tree.
        /// </summary>
        /// <typeparam name="TNode">The type of each node in the tree</typeparam>
        /// <param name="nodes">The leaf nodes to combine into an balanced binary tree</param>
        /// <param name="combinator">A function that produces a new node that is the combination of the two specified argument nodes</param>
        /// <returns>The single node that is the root of the balanced binary tree</returns>
        internal static TNode BuildBalancedTreeInPlace<TNode>(IList<TNode> nodes, Func<TNode, TNode, TNode> combinator)
        {
            EntityUtil.CheckArgumentNull(nodes, "nodes");
            EntityUtil.CheckArgumentNull(combinator, "combinator");

            Debug.Assert(nodes.Count > 0, "At least one node is required");

            // If only one node is present, return the single node.
            if (nodes.Count == 1)
            {
                return nodes[0];
            }

            // For the two-node case, simply combine the two nodes and return the result.
            if (nodes.Count == 2)
            {
                return combinator(nodes[0], nodes[1]);
            }

            //
            // Build the balanced tree in a bottom-up fashion.
            // On each iteration, an even number of nodes are paired off using the
            // combinator function, reducing the total number of available leaf nodes
            // by half each time. If the number of nodes in an iteration is not even,
            // the 'last' node in the set is omitted, then combined with the last pair
            // that is produced.
            // Nodes are collected from left to right with newly combined nodes overwriting
            // nodes from the previous iteration that have already been consumed (as can
            // be seen by 'writePos' lagging 'readPos' in the main statement of the loop below).
            // When a single available leaf node remains, this node is the root of the
            // balanced binary tree and can be returned to the caller.
            //
            int nodesToPair = nodes.Count;
            while (nodesToPair != 1)
            {
                bool combineModulo = ((nodesToPair & 0x1) == 1);
                if (combineModulo)
                {
                    nodesToPair--;
                }

                int writePos = 0;
                for (int readPos = 0; readPos < nodesToPair; readPos += 2)
                {
                    nodes[writePos++] = combinator(nodes[readPos], nodes[readPos + 1]);
                }

                if (combineModulo)
                {
                    int updatePos = writePos - 1;
                    nodes[updatePos] = combinator(nodes[updatePos], nodes[nodesToPair]);
                }

                nodesToPair /= 2;
            }

            return nodes[0];
        }

        /// <summary>
        /// Uses a stack to non-recursively traverse a given tree structure and retrieve the leaf nodes.
        /// </summary>
        /// <typeparam name="TNode">The type of each node in the tree structure</typeparam>
        /// <param name="root">The node that represents the root of the tree</param>
        /// <param name="isLeaf">A function that determines whether or not a given node should be considered a leaf node</param>
        /// <param name="getImmediateSubNodes">A function that traverses the tree by retrieving the <b>immediate</b> descendants of a (non-leaf) node.</param>
        /// <returns>An enumerable containing the leaf nodes (as determined by <paramref name="isLeaf"/>) retrieved by traversing the tree from <paramref name="root"/> using <paramref name="getImmediateSubNodes"/>.</returns>
        internal static IEnumerable<TNode> GetLeafNodes<TNode>(TNode root, Func<TNode, bool> isLeaf, Func<TNode, IEnumerable<TNode>> getImmediateSubNodes)
        {
            EntityUtil.CheckArgumentNull(isLeaf, "isLeaf");
            EntityUtil.CheckArgumentNull(getImmediateSubNodes, "getImmediateSubNodes");

            Stack<TNode> nodes = new Stack<TNode>();
            nodes.Push(root);

            while (nodes.Count > 0)
            {
                TNode current = nodes.Pop();
                if (isLeaf(current))
                {
                    yield return current;
                }
                else
                {
                    List<TNode> childNodes = new List<TNode>(getImmediateSubNodes(current));
                    for (int idx = childNodes.Count - 1; idx > -1; idx--)
                    {
                        nodes.Push(childNodes[idx]);
                    }
                }
            }
        }

        #endregion
    }
}
    

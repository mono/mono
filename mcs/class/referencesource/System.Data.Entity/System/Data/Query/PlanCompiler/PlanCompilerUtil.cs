//---------------------------------------------------------------------
// <copyright file="PlanCompilerUtil.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common.Utils;
using System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// Utility class for the methods shared among the classes comprising the plan compiler
    /// </summary>
    internal static class PlanCompilerUtil
    {
        /// <summary>
        /// Utility method that determines whether a given CaseOp subtree can be optimized.
        /// Called by both PreProcessor and NominalTypeEliminator.
        /// 
        /// If the case statement is of the shape:
        ///     case when X then NULL else Y, or
        ///     case when X then Y else NULL,
        /// where Y is of row type, and the types of the input CaseOp, the NULL and Y are the same,
        /// return true
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        internal static bool IsRowTypeCaseOpWithNullability(CaseOp op, Node n, out bool thenClauseIsNull)
        {
            thenClauseIsNull = false;  //any default value will do

            if (!TypeSemantics.IsRowType(op.Type))
            {
                return false;
            }
            if (n.Children.Count != 3)
            {
                return false;
            }

            //All three types must be equal
            if (!n.Child1.Op.Type.EdmEquals(op.Type) || !n.Child2.Op.Type.EdmEquals(op.Type))
            {
                return false;
            }

            //At least one of Child1 and Child2 needs to be a null
            if (n.Child1.Op.OpType == OpType.Null)
            {
                thenClauseIsNull = true;
                return true;
            }
            if (n.Child2.Op.OpType == OpType.Null)
            {
                // thenClauseIsNull stays false
                return true;
            }

            return false;
        }

        /// <summary>
        /// Is this function a collection aggregate function. It is, if
        ///   - it has exactly one child
        ///   - that child is a collection type
        ///   - and the function has been marked with the aggregate attribute
        /// </summary>
        /// <param name="op">the function op</param>
        /// <param name="n">the current subtree</param>
        /// <returns>true, if this was a collection aggregate function</returns>
        internal static bool IsCollectionAggregateFunction(FunctionOp op, Node n)
        {
            return ((n.Children.Count == 1) &&
                    TypeSemantics.IsCollectionType(n.Child0.Op.Type) &&
                    TypeSemantics.IsAggregateFunction(op.Function));
        }

        /// <summary>
        /// Is the given op one of the ConstantBaseOp-s
        /// </summary>
        /// <param name="opType"></param>
        /// <returns></returns>
        internal static bool IsConstantBaseOp(OpType opType)
        {
            return opType == OpType.Constant ||
                    opType == OpType.InternalConstant ||
                    opType == OpType.Null ||
                    opType == OpType.NullSentinel;
        }

        /// <summary>
        /// Combine two predicates by trying to avoid the predicate parts of the 
        /// second one that are already present in the first one.
        /// 
        /// In particular, given two nodes, predicate1 and predicate2, 
        /// it creates a combined predicate logically equivalent to 
        ///     predicate1 AND predicate2,
        /// but it does not include any AND parts of predicate2 that are present
        /// in predicate1.
        /// </summary>
        /// <param name="predicate1"></param>
        /// <param name="predicate2"></param>
        /// <param name="command"></param>
        /// <returns></returns>
        internal static Node CombinePredicates(Node predicate1, Node predicate2, Command command)
        {
            IEnumerable<Node> andParts1 = BreakIntoAndParts(predicate1);
            IEnumerable<Node> andParts2 = BreakIntoAndParts(predicate2);

            Node result = predicate1;

            foreach (Node predicatePart2 in andParts2)
            {
                bool foundMatch = false;
                foreach (Node predicatePart1 in andParts1)
                {
                    if (predicatePart1.IsEquivalent(predicatePart2))
                    {
                        foundMatch = true;
                        break;
                    }
                }
                if (!foundMatch)
                {
                    result = command.CreateNode(command.CreateConditionalOp(OpType.And), result, predicatePart2);
                }
            }
            return result;
        }

        /// <summary>
        /// Create a list of AND parts for a given predicate. 
        /// For example, if the predicate is of the shape:
        /// ((p1 and p2) and (p3 and p4)) the list is p1, p2, p3, p4
        /// The predicates p1,p2, p3, p4 may be roots of subtrees that 
        /// have nodes with AND ops, but 
        /// would not be broken unless they are the AND nodes themselves.
        /// </summary>
        /// <param name="predicate"></param>
        /// <param name="andParts"></param>
        private static IEnumerable<Node> BreakIntoAndParts(Node predicate)
        {
            return Helpers.GetLeafNodes<Node>(predicate,
                node => (node.Op.OpType != OpType.And),
                node => (new[] {node.Child0, node.Child1}));
        }
    }
}

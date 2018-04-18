//---------------------------------------------------------------------
// <copyright file="Predicate.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
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

using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// The Predicate class represents a condition (predicate) in CNF. 
    /// A predicate consists of a number of "simple" parts, and the parts are considered to be 
    /// ANDed together
    /// 
    /// This class provides a number of useful functions related to
    ///   - Single Table predicates
    ///   - Join predicates
    ///   - Key preservation
    ///   - Null preservation
    /// etc.
    /// 
    /// Note: This class doesn't really convert node trees into CNF form. It looks for
    /// basic CNF patterns, and reasons about them. For example,
    ///    (a AND b) OR c
    /// can technically be translated into (a OR c) AND (b OR c),
    /// but we don't bother.
    /// At some future point of time, it might be appropriate to consider this
    /// 
    /// </summary>
    internal class Predicate
    {
        #region private state
        private Command m_command;
        private List<Node> m_parts;
        #endregion

        #region constructors
        /// <summary>
        /// Create an empty predicate
        /// </summary>
        /// <param name="command"></param>
        internal Predicate(Command command)
        {
            m_command = command;
            m_parts = new List<Node>();
        }

        /// <summary>
        /// Create a predicate from a node tree
        /// </summary>
        /// <param name="command">current iqt command</param>
        /// <param name="andTree">the node tree</param>
        internal Predicate(Command command, Node andTree)
            : this(command)
        {
            PlanCompiler.Assert(andTree != null, "null node passed to Predicate() constructor");
            InitFromAndTree(andTree);
        }
        #endregion

        #region public surface

        #region construction APIs
        /// <summary>
        /// Add a new "part" (simple predicate) to the current list of predicate parts
        /// </summary>
        /// <param name="n">simple predicate</param>
        internal void AddPart(Node n)
        {
            m_parts.Add(n);
        }
        #endregion

        #region Reconstruction (of node tree)
        /// <summary>
        /// Build up an AND tree based on the current parts. 
        /// Specifically, if I have parts (p1, p2, ..., pn), we build up a tree that looks like
        ///    p1 AND p2 AND ... AND pn
        /// 
        /// If we have no parts, we return a null reference
        /// If we have only one part, then we return just that part
        /// </summary>
        /// <returns>the and subtree</returns>
        internal Node BuildAndTree()
        {
            Node andNode = null;
            foreach (Node n in m_parts)
            {
                if (andNode == null)
                {
                    andNode = n;
                }
                else
                {
                    andNode = m_command.CreateNode(m_command.CreateConditionalOp(OpType.And),
                        andNode, n);
                }
            }
            return andNode;
        }
        #endregion

        #region SingleTable (Filter) Predicates

        /// <summary>
        /// Partition the current predicate into predicates that only apply 
        /// to the specified table (single-table-predicates), and others
        /// </summary>
        /// <param name="tableDefinitions">current columns defined by the table</param>
        /// <param name="otherPredicates">non-single-table predicates</param>
        /// <returns>single-table-predicates</returns>
        internal Predicate GetSingleTablePredicates(VarVec tableDefinitions, 
            out Predicate otherPredicates)
        {
            List<VarVec> tableDefinitionList = new List<VarVec>();
            tableDefinitionList.Add(tableDefinitions);
            List<Predicate> singleTablePredicateList;
            GetSingleTablePredicates(tableDefinitionList, out singleTablePredicateList, out otherPredicates);
            return singleTablePredicateList[0];
        }
        #endregion

        #region EquiJoins
        /// <summary>
        /// Get the set of equi-join columns from this predicate
        /// </summary>
        /// <param name="leftTableDefinitions"></param>
        /// <param name="rightTableDefinitions"></param>
        /// <param name="leftTableEquiJoinColumns"></param>
        /// <param name="rightTableEquiJoinColumns"></param>
        /// <param name="otherPredicates"></param>
        internal void GetEquiJoinPredicates(VarVec leftTableDefinitions, VarVec rightTableDefinitions,
            out List<Var> leftTableEquiJoinColumns, out List<Var> rightTableEquiJoinColumns,
            out Predicate otherPredicates)
        {
            otherPredicates = new Predicate(m_command);
            leftTableEquiJoinColumns = new List<Var>();
            rightTableEquiJoinColumns = new List<Var>();
            foreach (Node part in m_parts)
            {
                Var leftTableVar;
                Var rightTableVar;

                if (IsEquiJoinPredicate(part, leftTableDefinitions, rightTableDefinitions, out leftTableVar, out rightTableVar))
                {
                    leftTableEquiJoinColumns.Add(leftTableVar);
                    rightTableEquiJoinColumns.Add(rightTableVar);
                }
                else
                {
                    otherPredicates.AddPart(part);
                }
            }
        }

        internal Predicate GetJoinPredicates(VarVec leftTableDefinitions, VarVec rightTableDefinitions,
            out Predicate otherPredicates)
        {
            Predicate joinPredicate = new Predicate(m_command);
            otherPredicates = new Predicate(m_command);

            foreach (Node part in m_parts)
            {
                Var leftTableVar;
                Var rightTableVar;

                if (Predicate.IsEquiJoinPredicate(part, leftTableDefinitions, rightTableDefinitions, out leftTableVar, out rightTableVar))
                {
                    joinPredicate.AddPart(part);
                }
                else
                {
                    otherPredicates.AddPart(part);
                }
            }
            return joinPredicate;
        }
        #endregion

        #region Keys
        /// <summary>
        /// Is the current predicate a "key-satisfying" predicate? 
        /// </summary>
        /// <param name="keyVars">list of keyVars</param>
        /// <param name="definitions">current table definitions</param>
        /// <returns>true, if this predicate satisfies the keys</returns>
        internal bool SatisfiesKey(VarVec keyVars, VarVec definitions)
        {
            if (keyVars.Count > 0)
            {
                VarVec missingKeys = keyVars.Clone();
                foreach (Node part in m_parts)
                {
                    if (part.Op.OpType != OpType.EQ)
                    {
                        continue;
                    }
                    Var keyVar;
                    if (IsKeyPredicate(part.Child0, part.Child1, keyVars, definitions, out keyVar))
                    {
                        missingKeys.Clear(keyVar);
                    }
                    else if (IsKeyPredicate(part.Child1, part.Child0, keyVars, definitions, out keyVar))
                    {
                        missingKeys.Clear(keyVar);
                    }
                }

                return missingKeys.IsEmpty;
            }
            return false;
        }
        #endregion

        #region Nulls
        /// <summary>
        /// Does this predicate preserve nulls for the table columns? 
        /// 
        /// If the ansiNullSemantics parameter is set, then we simply return true
        /// always - this shuts off most optimizations
        /// 
        /// </summary>
        /// <param name="tableColumns">list of columns to consider</param>
        /// <param name="ansiNullSemantics">use ansi null semantics</param>
        /// <returns>true, if the predicate preserves nulls</returns>
        internal bool PreservesNulls(VarVec tableColumns, bool ansiNullSemantics)
        {
            // Don't mess with non-ansi semantics
            if (!ansiNullSemantics)
            {
                return true;
            }

            // If at least one part does not preserve nulls, then we simply return false
            foreach (Node part in m_parts)
            {
                if (!PreservesNulls(part, tableColumns))
                {
                    return false;
                }
            }
            return true;
        }
        #endregion

        #endregion

        #region private methods
        #region construction
        private void InitFromAndTree(Node andTree)
        {
            if (andTree.Op.OpType == OpType.And)
            {
                InitFromAndTree(andTree.Child0);
                InitFromAndTree(andTree.Child1);
            }
            else
            {
                m_parts.Add(andTree);
            }
        }
        #endregion

        #region Single Table Predicates

        private void GetSingleTablePredicates(List<VarVec> tableDefinitions,
            out List<Predicate> singleTablePredicates, out Predicate otherPredicates)
        {
            singleTablePredicates = new List<Predicate>();
            foreach (VarVec vec in tableDefinitions)
            {
                singleTablePredicates.Add(new Predicate(m_command));
            }
            otherPredicates = new Predicate(m_command);
            VarVec externalRefs = m_command.CreateVarVec();

            foreach (Node part in m_parts)
            {
                NodeInfo nodeInfo = m_command.GetNodeInfo(part);

                bool singleTablePart = false;
                for (int i = 0; i < tableDefinitions.Count; i++)
                {
                    VarVec tableColumns = tableDefinitions[i];
                    if (tableColumns != null)
                    {
                        externalRefs.InitFrom(nodeInfo.ExternalReferences);
                        externalRefs.Minus(tableColumns);
                        if (externalRefs.IsEmpty)
                        {
                            singleTablePart = true;
                            singleTablePredicates[i].AddPart(part);
                            break;
                        }
                    }
                }
                if (!singleTablePart)
                {
                    otherPredicates.AddPart(part);
                }
            }
        }

        #endregion

        #region EquiJoins
        /// <summary>
        /// Is this "simple" predicate an equi-join predicate? 
        ///   (ie) is it of the form "var1 = var2" 
        /// Return "var1" and "var2"
        /// </summary>
        /// <param name="simplePredicateNode">the simple predicate</param>
        /// <param name="leftVar">var on the left-side</param>
        /// <param name="rightVar">var on the right</param>
        /// <returns>true, if this is an equijoin predicate</returns>
        private static bool IsEquiJoinPredicate(Node simplePredicateNode, out Var leftVar, out Var rightVar)
        {
            leftVar = null;
            rightVar = null;
            if (simplePredicateNode.Op.OpType != OpType.EQ)
            {
                return false;
            }

            VarRefOp leftVarOp = simplePredicateNode.Child0.Op as VarRefOp;
            if (leftVarOp == null)
            {
                return false;
            }
            VarRefOp rightVarOp = simplePredicateNode.Child1.Op as VarRefOp;
            if (rightVarOp == null)
            {
                return false;
            }

            leftVar = leftVarOp.Var;
            rightVar = rightVarOp.Var;
            return true;
        }

        /// <summary>
        /// Is this an equi-join predicate involving columns from the specified tables?
        /// On output, if this was indeed an equijoin predicate, "leftVar" is the 
        /// column of the left table, while "rightVar" is the column of the right table
        /// and the predicate itself is of the form "leftVar = rightVar"
        /// </summary>
        /// <param name="simplePredicateNode">the simple predicate node</param>
        /// <param name="leftTableDefinitions">interesting columns of the left table</param>
        /// <param name="rightTableDefinitions">interesting columns of the right table</param>
        /// <param name="leftVar">join column of the left table</param>
        /// <param name="rightVar">join column of the right table</param>
        /// <returns>true, if this is an equijoin predicate involving columns from the 2 tables</returns>
        private static bool IsEquiJoinPredicate(Node simplePredicateNode,
            VarVec leftTableDefinitions, VarVec rightTableDefinitions,
            out Var leftVar, out Var rightVar)
        {
            Var tempLeftVar;
            Var tempRightVar;

            leftVar = null;
            rightVar = null;
            if (!Predicate.IsEquiJoinPredicate(simplePredicateNode, out tempLeftVar, out tempRightVar))
            {
                return false;
            }

            if (leftTableDefinitions.IsSet(tempLeftVar) &&
                rightTableDefinitions.IsSet(tempRightVar))
            {
                leftVar = tempLeftVar;
                rightVar = tempRightVar;
            }
            else if (leftTableDefinitions.IsSet(tempRightVar) &&
                     rightTableDefinitions.IsSet(tempLeftVar))
            {
                leftVar = tempRightVar;
                rightVar = tempLeftVar;
            }
            else
            {
                return false;
            }

            return true;
        }
        #endregion

        #region Nulls
        /// <summary>
        /// Does this predicate preserve nulls on the specified columns of the table?
        /// If any of the columns participates in a comparison predicate, or in a 
        /// not-null predicate, then, nulls are not preserved
        /// </summary>
        /// <param name="simplePredNode">the "simple" predicate node</param>
        /// <param name="tableColumns">list of table columns</param>
        /// <returns>true, if nulls are preserved</returns>
        private static bool PreservesNulls(Node simplePredNode, VarVec tableColumns)
        {
            VarRefOp varRefOp;

            switch (simplePredNode.Op.OpType)
            {
                case OpType.EQ:
                case OpType.NE:
                case OpType.GT:
                case OpType.GE:
                case OpType.LT:
                case OpType.LE:
                    varRefOp = simplePredNode.Child0.Op as VarRefOp;
                    if (varRefOp != null && tableColumns.IsSet(varRefOp.Var))
                    {
                        return false;
                    }
                    varRefOp = simplePredNode.Child1.Op as VarRefOp;
                    if (varRefOp != null && tableColumns.IsSet(varRefOp.Var))
                    {
                        return false;
                    }
                    return true;

                case OpType.Not:
                    if (simplePredNode.Child0.Op.OpType != OpType.IsNull)
                    {
                        return true;
                    }
                    varRefOp = simplePredNode.Child0.Child0.Op as VarRefOp;
                    return (varRefOp == null || !tableColumns.IsSet(varRefOp.Var));

                case OpType.Like:
                    // If the predicate is "column LIKE constant ...", then the
                    // predicate does not preserve nulls
                    ConstantBaseOp constantOp = simplePredNode.Child1.Op as ConstantBaseOp;
                    if (constantOp == null || (constantOp.OpType == OpType.Null))
                    {
                        return true;
                    }
                    varRefOp = simplePredNode.Child0.Op as VarRefOp;
                    if (varRefOp != null && tableColumns.IsSet(varRefOp.Var))
                    {
                        return false;
                    }
                    return true;

                default:
                    return true;
            }
        }
        #endregion

        #region Keys
        private bool IsKeyPredicate(Node left, Node right, VarVec keyVars, VarVec definitions, out Var keyVar)
        {
            keyVar = null;

            // If the left-side is not a Var, then return false
            if (left.Op.OpType != OpType.VarRef)
            {
                return false;
            }
            VarRefOp varRefOp = (VarRefOp)left.Op;
            keyVar = varRefOp.Var;

            // Not a key of this table?
            if (!keyVars.IsSet(keyVar))
            {
                return false;
            }

            // Make sure that the other side is either a constant, or has no
            // references at all to us
            NodeInfo otherNodeInfo = m_command.GetNodeInfo(right);
            VarVec otherVarExternalReferences = otherNodeInfo.ExternalReferences.Clone();
            otherVarExternalReferences.And(definitions);
            return otherVarExternalReferences.IsEmpty;
        }
        #endregion

        #endregion
    }
}

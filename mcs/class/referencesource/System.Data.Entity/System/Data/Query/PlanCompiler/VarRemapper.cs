//---------------------------------------------------------------------
// <copyright file="VarRemapper.cs" company="Microsoft">
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

using System.Data.Query.InternalTrees;

namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// The VarRemapper is a utility class that can be used to "remap" Var references
    /// in a node, or a subtree. 
    /// </summary>
    internal class VarRemapper : BasicOpVisitor
    {
        #region Private state
        private readonly Dictionary<Var, Var> m_varMap;
        protected readonly Command m_command;
        #endregion

        #region Constructors
        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="command">Current iqt command</param>
        internal VarRemapper(Command command)
            :this(command, new Dictionary<Var,Var>())
        {
        }

        /// <summary>
        /// Internal constructor
        /// </summary>
        /// <param name="command">Current iqt command</param>
        /// <param name="varMap">Var map to be used</param>
        internal VarRemapper(Command command, Dictionary<Var, Var> varMap)
        {
            m_command = command;
            m_varMap = varMap;
        }
        #endregion

        #region Public surface
        /// <summary>
        /// Add a mapping for "oldVar" - when the replace methods are invoked, they
        /// will replace all references to "oldVar" by "newVar"
        /// </summary>
        /// <param name="oldVar">var to replace</param>
        /// <param name="newVar">the replacement var</param>
        internal void AddMapping(Var oldVar, Var newVar)
        {
            m_varMap[oldVar] = newVar;
        }

        /// <summary>
        /// Update vars in just this node (and not the entire subtree) 
        /// Does *not* recompute the nodeinfo - there are at least some consumers of this
        /// function that do not want the recomputation - transformation rules, for example
        /// </summary>
        /// <param name="node">current node</param>
        internal virtual void RemapNode(Node node)
        {
            if (m_varMap.Count == 0)
            {
                return;
            }
            VisitNode(node);
        }

        /// <summary>
        /// Update vars in this subtree. Recompute the nodeinfo along the way
        /// </summary>
        /// <param name="subTree">subtree to "remap"</param>
        internal virtual void RemapSubtree(Node subTree)
        {
            if (m_varMap.Count == 0)
            {
                return;
            }

            foreach (Node chi in subTree.Children)
            {
                RemapSubtree(chi);
            }

            RemapNode(subTree);
            m_command.RecomputeNodeInfo(subTree);
        }

        /// <summary>
        /// Produce a a new remapped varList
        /// </summary>
        /// <param name="varList"></param>
        /// <returns>remapped varList</returns>
        internal VarList RemapVarList(VarList varList)
        {
            return Command.CreateVarList(MapVars(varList));
        }

        /// <summary>
        /// Remap the given varList using the given varMap
        /// </summary>
        /// <param name="command"></param>
        /// <param name="varMap"></param>
        /// <param name="varList"></param>
        internal static VarList RemapVarList(Command command, Dictionary<Var, Var> varMap, VarList varList)
        {
            VarRemapper varRemapper = new VarRemapper(command, varMap);
            return varRemapper.RemapVarList(varList);
        }
        #endregion

        #region Private methods
        /// <summary>
        /// Get the mapping for a Var - returns the var itself, mapping was found
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        private Var Map(Var v)
        {
            Var newVar;
            while (true)
            {
                if (!m_varMap.TryGetValue(v, out newVar))
                {
                    return v;
                }
                v = newVar;
            }
        }

        private IEnumerable<Var> MapVars(IEnumerable<Var> vars)
        {
            foreach (Var v in vars)
            {
                yield return Map(v);
            }
        }

        private void Map(VarVec vec)
        {
            VarVec newVec = m_command.CreateVarVec(MapVars(vec));
            vec.InitFrom(newVec);
        }

        private void Map(VarList varList)
        {
            VarList newList = Command.CreateVarList(MapVars(varList));
            varList.Clear();
            varList.AddRange(newList);
        }

        private void Map(VarMap varMap)
        {
            VarMap newVarMap = new VarMap();
            foreach (KeyValuePair<Var, Var> kv in varMap)
            {
                Var newVar = Map(kv.Value);
                newVarMap.Add(kv.Key, newVar);
            }
            varMap.Clear();
            foreach (KeyValuePair<Var, Var> kv in newVarMap)
            {
                varMap.Add(kv.Key, kv.Value);
            }
        }
        private void Map(List<InternalTrees.SortKey> sortKeys)
        {
            VarVec sortVars = m_command.CreateVarVec();
            bool hasDuplicates = false;

            // 
            // Map each var in the sort list. Remapping may introduce duplicates, and
            // we should get rid of duplicates, since sql doesn't like them
            //
            foreach (InternalTrees.SortKey sk in sortKeys)
            {
                sk.Var = Map(sk.Var);
                if (sortVars.IsSet(sk.Var))
                {
                    hasDuplicates = true;
                }
                sortVars.Set(sk.Var);
            }

            //
            // Get rid of any duplicates
            //
            if (hasDuplicates)
            {
                List<InternalTrees.SortKey> newSortKeys = new List<SortKey>(sortKeys);
                sortKeys.Clear();
                sortVars.Clear();
                foreach (InternalTrees.SortKey sk in newSortKeys)
                {
                    if (!sortVars.IsSet(sk.Var))
                    {
                        sortKeys.Add(sk);
                    }
                    sortVars.Set(sk.Var);
                }
            }
        }

        #region VisitorMethods
        /// <summary>
        ///  Default visitor for a node - does not visit the children 
        /// The reason we have this method is because the default VisitDefault
        /// actually visits the children, and we don't want to do that
        /// </summary>
        /// <param name="n"></param>
        protected override void VisitDefault(Node n)
        {
            // Do nothing. 
        }

        #region ScalarOps
        public override void Visit(VarRefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Var newVar = Map(op.Var);
            if (newVar != op.Var)
            {
                n.Op = m_command.CreateVarRefOp(newVar);
            }
        }
        #endregion

        #region AncillaryOps
        #endregion

        #region PhysicalOps
        protected override void VisitNestOp(NestBaseOp op, Node n)
        {
            throw EntityUtil.NotSupported();
        }

        public override void Visit(PhysicalProjectOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
            Map(op.Outputs);

            SimpleCollectionColumnMap newColumnMap = (SimpleCollectionColumnMap)ColumnMapTranslator.Translate(op.ColumnMap, m_varMap);
            n.Op = m_command.CreatePhysicalProjectOp(op.Outputs, newColumnMap);
        }
        #endregion

        #region RelOps
        protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Outputs);
            Map(op.Keys);
        }
        public override void Visit(GroupByIntoOp op, Node n)
        {
            VisitGroupByOp(op, n);
            Map(op.Inputs);
        }
        public override void Visit(DistinctOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Keys);
        }
        public override void Visit(ProjectOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Outputs);
        }
        public override void Visit(UnnestOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Var newVar = Map(op.Var);
            if (newVar != op.Var)
            {
                n.Op = m_command.CreateUnnestOp(newVar, op.Table);
            }
        }
        protected override void VisitSetOp(SetOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.VarMap[0]);
            Map(op.VarMap[1]);
        }
        protected override void VisitSortOp(SortBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            Map(op.Keys);
        }
        #endregion

        #endregion

        #endregion
    }
}

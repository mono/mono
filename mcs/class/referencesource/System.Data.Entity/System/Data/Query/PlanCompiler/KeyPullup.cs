//---------------------------------------------------------------------
// <copyright file="KeyPullup.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
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

//
// The KeyPullup module helps pull up keys from the leaves of a subtree. 
//
namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// The KeyPullup class subclasses the default visitor and pulls up keys
    /// for the different node classes below. 
    /// The only Op that really deserves special treatment is the ProjectOp. 
    /// </summary>
    internal class KeyPullup : BasicOpVisitor
    {
        #region private state
        private Command m_command;
        #endregion

        #region constructors
        internal KeyPullup(Command command)
        {
            m_command = command;
        }
        #endregion

        #region public methods
        /// <summary>
        /// Pull up keys (if possible) for the given node
        /// </summary>
        /// <param name="node">node to pull up keys for</param>
        /// <returns>Keys for the node</returns>
        internal KeyVec GetKeys(Node node)
        {
            ExtendedNodeInfo nodeInfo = node.GetExtendedNodeInfo(m_command);
            if (nodeInfo.Keys.NoKeys)
            {
                VisitNode(node);
            }
            return nodeInfo.Keys;
        }

        #endregion

        #region private methods

        #region Visitor Methods

        #region general helpers
        /// <summary>
        /// Default visitor for children. Simply visit all children, and 
        /// try to get keys for those nodes (relops, physicalOps) that 
        /// don't have keys as yet.
        /// </summary>
        /// <param name="n">Current node</param>
        protected override void VisitChildren(Node n)
        {
            foreach (Node chi in n.Children)
            {
                if (chi.Op.IsRelOp || chi.Op.IsPhysicalOp)
                {
                    GetKeys(chi);
                }
            }
        }
        #endregion

        #region RelOp Visitors

        /// <summary>
        /// Default visitor for RelOps. Simply visits the children, and 
        /// then tries to recompute the NodeInfo (with the fond hope that
        /// some keys have now shown up)
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        protected override void VisitRelOpDefault(RelOp op, Node n)
        {
            VisitChildren(n);
            m_command.RecomputeNodeInfo(n);
        }

        /// <summary>
        /// Visitor for a ScanTableOp. Simply ensures that the keys get 
        /// added to the list of referenced columns
        /// </summary>
        /// <param name="op">current ScanTableOp</param>
        /// <param name="n">current subtree</param>
        public override void Visit(ScanTableOp op, Node n)
        {
            // find the keys of the table. Make sure that they are 
            // all references
            op.Table.ReferencedColumns.Or(op.Table.Keys);
            // recompute the nodeinfo - keys won't get picked up otherwise
            m_command.RecomputeNodeInfo(n);
        }

        /// <summary>
        /// Pulls up keys for a ProjectOp. First visits its children to pull
        /// up its keys; then identifies any keys from the input that it may have 
        /// projected out - and adds them to the output list of vars
        /// </summary>
        /// <param name="op">Current ProjectOp</param>
        /// <param name="n">Current subtree</param>
        public override void Visit(ProjectOp op, Node n)
        {
            VisitChildren(n);

            ExtendedNodeInfo childNodeInfo = n.Child0.GetExtendedNodeInfo(m_command);
            if (!childNodeInfo.Keys.NoKeys)
            {
                VarVec outputVars = m_command.CreateVarVec(op.Outputs);
                // NOTE: This code appears in NodeInfoVisitor as well. Try to see if we
                //       can share this somehow.
                Dictionary<Var, Var> varRenameMap = NodeInfoVisitor.ComputeVarRemappings(n.Child1);
                VarVec mappedKeyVec = childNodeInfo.Keys.KeyVars.Remap(varRenameMap);
                outputVars.Or(mappedKeyVec);
                op.Outputs.InitFrom(outputVars);
            }
            m_command.RecomputeNodeInfo(n);
        }

        /// <summary>
        /// Comments from Murali:
        /// 
        ///   There are several cases to consider here. 
        ///     
        ///   Case 0:
        ///     Let’s assume that K1 is the set of keys ({k1, k2, ..., kn}) for the 
        ///     first input, and K2 ({l1, l2, …}) is the set of keys for the second
        ///     input.
        /// 
        ///     The best case is when both K1 and K2 have the same cardinality (hopefully
        ///     greater than 0), and the keys are in the same locations (ie) the corresponding
        ///     positions in the select-list.  Even in this case, its not enough to take
        ///     the keys, and treat them as the keys of the union-all. What we’ll need to 
        ///     do is to add a “branch” discriminator constant for each branch of the 
        ///     union-all, and use this as the prefix for the keys. 
        /// 
        ///     For example, if I had:
        /// 
        ///         Select c1, c2, c3... from ...
        ///         Union all
        ///         Select d1, d2, d3... from ...
        /// 
        ///     And for the sake of argument, lets say that {c2} and {d2} are the keys of 
        ///     each of the branches. What you’ll need to do is to translate this into
        ///     
        ///         Select 0 as bd, c1, c2, c3... from ...
        ///         Union all
        ///         Select 1 as bd, d1, d2, d3... from ...
        ///
        ///     And then treat {bd, c2/d2} as the key of the union-all 
        ///
        ///   Case 1:  (actually, a subcase of Case 0):
        ///     Now, if the keys don’t align, then we can simply take the union of the 
        ///     corresponding positions, and make them all the keys (we would still need 
        ///     the branch discriminator)
        ///
        ///   Case 2:
        ///     Finally, if you need to “pull” up keys from either of the branches, it is 
        ///     possible that the branches get out of whack.  We will then need to push up 
        ///     the keys (with nulls if the other branch doesn’t have the corresponding key) 
        ///     into the union-all. (We still need the branch discriminator).
        ///     
        /// Now, unfortunately, whenever we've got polymorphic entity types, we'll end up
        /// in case 2 way more often than we really want to, because when we're pulling up
        /// keys, we don't want to reason about a caseop (which is how polymorphic types
        /// wrap their key value).
        /// 
        /// To simplify all of this, we:
        /// 
        /// (1) Pulling up the keys for both branches of the UnionAll, and computing which
        ///     keys are in the outputs and which are missing from the outputs.
        /// 
        /// (2) Accumulate all the missing keys.
        ///
        /// (3) Slap a projectOp around each branch, adding a branch discriminator
        ///     var and all the missing keys.  When keys are missing from a different
        ///     branch, we'll construct null ops for them on the other branches.  If 
        ///     a branch already has a branch descriminator, we'll re-use it instead
        ///     of constructing a new one.  (Of course, if there aren't any keys to
        ///     add and it's already including the branch discriminator we won't 
        ///     need the projectOp)
        ///     
        /// </summary>
        /// <param name="op">the UnionAllOp</param>
        /// <param name="n">current subtree</param>
        public override void Visit(UnionAllOp op, Node n)
        {
#if DEBUG
            string input = Dump.ToXml(m_command, n);
#endif //DEBUG

            // Ensure we have keys pulled up on each branch of the union all.
            VisitChildren(n);

            // Create the setOp var we'll use to output the branch discriminator value; if 
            // any of the branches are already surfacing a branchDiscriminator var to the 
            // output of this operation then we won't need to use this but we construct it 
            // early to simplify logic.
            Var outputBranchDiscriminatorVar = m_command.CreateSetOpVar(m_command.IntegerType);

            // Now ensure that we're outputting the key vars from this op as well.
            VarList allKeyVarsMissingFromOutput = Command.CreateVarList();
            VarVec[] keyVarsMissingFromOutput = new VarVec[n.Children.Count];

            for (int i = 0; i < n.Children.Count; i++)
            {
                Node branchNode = n.Children[i];
                ExtendedNodeInfo branchNodeInfo = m_command.GetExtendedNodeInfo(branchNode);

                // Identify keys that aren't in the output list of this operation. We
                // determine these by remapping the keys that are found through the node's
                // VarMap, which gives us the keys in the same "varspace" as the outputs
                // of the UnionAll, then we subtract out the outputs of this UnionAll op,
                // leaving things that are not in the output vars.  Of course, if they're
                // not in the output vars, then we didn't really remap.
                VarVec existingKeyVars = branchNodeInfo.Keys.KeyVars.Remap(op.VarMap[i]);

                keyVarsMissingFromOutput[i] = m_command.CreateVarVec(existingKeyVars);
                keyVarsMissingFromOutput[i].Minus(op.Outputs);

                // Special Case: if the branch is a UnionAll, it will already have it's
                // branch discriminator var added in the keys; we don't want to add that
                // a second time...
                if (OpType.UnionAll == branchNode.Op.OpType)
                {
                    UnionAllOp branchUnionAllOp = (UnionAllOp)branchNode.Op;

                    keyVarsMissingFromOutput[i].Clear(branchUnionAllOp.BranchDiscriminator);
                }

                allKeyVarsMissingFromOutput.AddRange(keyVarsMissingFromOutput[i]);
            }

            // Construct the setOp vars we're going to map to output.
            VarList allKeyVarsToAddToOutput = Command.CreateVarList();

            foreach (Var v in allKeyVarsMissingFromOutput)
            {
                Var newKeyVar = m_command.CreateSetOpVar(v.Type);
                allKeyVarsToAddToOutput.Add(newKeyVar);
            }

            // Now that we've identified all the keys we need to add, ensure that each branch 
            // has both the branch discrimination var and the all the keys in them, even when 
            // the keys are just going to null (which we construct, as needed)
            for (int i = 0; i < n.Children.Count; i++)
            {
                Node branchNode = n.Children[i];
                ExtendedNodeInfo branchNodeInfo = m_command.GetExtendedNodeInfo(branchNode);

                VarVec branchOutputVars = m_command.CreateVarVec();
                List<Node> varDefNodes = new List<Node>();

                // If the branch is a UnionAllOp that has a branch discriminator var then we can
                // use it, otherwise we'll construct a new integer constant with the next value 
                // of the branch discriminator value from the command object.
                Var branchDiscriminatorVar;

                if (OpType.UnionAll == branchNode.Op.OpType && null != ((UnionAllOp)branchNode.Op).BranchDiscriminator)
                {
                    branchDiscriminatorVar = ((UnionAllOp)branchNode.Op).BranchDiscriminator;

                    // If the branch has a discriminator var, but we haven't added it to the
                    // varmap yet, then we do so now.
                    if (!op.VarMap[i].ContainsValue(branchDiscriminatorVar))
                    {
                        op.VarMap[i].Add(outputBranchDiscriminatorVar, branchDiscriminatorVar);
                        // We don't need to add this to the branch outputs, because it's already there,
                        // otherwise we wouln't have gotten here, yes?
                    }
                    else
                    {
                        // In this case, we're already outputting the branch discriminator var -- we'll 
                        // just use it for both sides.  We should never have a case where only one of the
                        // two branches are outputting the branch discriminator var, because it can only
                        // be constructed in this method, and we wouldn't need it for any other purpose.
                        PlanCompiler.Assert(0 == i, "right branch has a discriminator var that the left branch doesn't have?");
                        VarMap reverseVarMap = op.VarMap[i].GetReverseMap();
                        outputBranchDiscriminatorVar = reverseVarMap[branchDiscriminatorVar];
                    }
                }
                else
                {
                    // Not a unionAll -- we have to add a BranchDiscriminator var.
                    varDefNodes.Add(
                        m_command.CreateVarDefNode(
                            m_command.CreateNode(
                                m_command.CreateConstantOp(m_command.IntegerType, m_command.NextBranchDiscriminatorValue)), out branchDiscriminatorVar));

                    branchOutputVars.Set(branchDiscriminatorVar);
                    op.VarMap[i].Add(outputBranchDiscriminatorVar, branchDiscriminatorVar);
                }

                // Append all the missing keys to the branch outputs.  If the missing key
                // is not from this branch then create a null.
                for (int j = 0; j < allKeyVarsMissingFromOutput.Count; j++)
                {
                    Var keyVar = allKeyVarsMissingFromOutput[j];

                    if (!keyVarsMissingFromOutput[i].IsSet(keyVar))
                    {
                        varDefNodes.Add(
                            m_command.CreateVarDefNode(
                                m_command.CreateNode(
                                    m_command.CreateNullOp(keyVar.Type)), out keyVar));

                        branchOutputVars.Set(keyVar);
                    }

                    // In all cases, we're adding a key to the output so we need to update the
                    // varmap.
                    op.VarMap[i].Add(allKeyVarsToAddToOutput[j], keyVar);
                }

                // If we got this far and didn't add anything to the branch, then we're done.
                // Otherwise we'll have to construct the new projectOp around the input branch
                // to add the stuff we've added.
                if (branchOutputVars.IsEmpty)
                {
                    // Actually, we're not quite done -- we need to update the key vars for the
                    // branch to include the branch discriminator var we
                    branchNodeInfo.Keys.KeyVars.Set(branchDiscriminatorVar);
                }
                else
                {
                    PlanCompiler.Assert(varDefNodes.Count != 0, "no new nodes?");

                    // Start by ensuring all the existing outputs from the branch are in the list.
                    foreach (Var v in op.VarMap[i].Values)
                    {
                        branchOutputVars.Set(v);
                    }

                    // Now construct a project op to project out everything we've added, and
                    // replace the branchNode with it in the flattened ladder.
                    n.Children[i] = m_command.CreateNode(m_command.CreateProjectOp(branchOutputVars),
                                                        branchNode,
                                                        m_command.CreateNode(m_command.CreateVarDefListOp(), varDefNodes));

                    // Finally, ensure that we update the Key info for the projectOp to include
                    // the original branch's keys, along with the branch discriminator var.
                    m_command.RecomputeNodeInfo(n.Children[i]);
                    ExtendedNodeInfo projectNodeInfo = m_command.GetExtendedNodeInfo(n.Children[i]);
                    projectNodeInfo.Keys.KeyVars.InitFrom(branchNodeInfo.Keys.KeyVars);
                    projectNodeInfo.Keys.KeyVars.Set(branchDiscriminatorVar);
                }
            }

            // All done with the branches, now it's time to update the UnionAll op to indicate
            // that we've got a branch discriminator var.
            n.Op = m_command.CreateUnionAllOp(op.VarMap[0], op.VarMap[1], outputBranchDiscriminatorVar);

            // Finally, the thing we've all been waiting for -- computing the keys.  We cheat here and let 
            // nodeInfo do it so we don't have to duplicate the logic...
            m_command.RecomputeNodeInfo(n);

#if DEBUG
            input = input.Trim();
            string output = Dump.ToXml(m_command, n);
#endif //DEBUG
        }
        #endregion

        #endregion
        #endregion
    }
}

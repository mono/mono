//---------------------------------------------------------------------
// <copyright file="Normalizer.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  Microsoft
// @backupOwner Microsoft
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.Metadata.Edm;
using System.Data.Query.InternalTrees;
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

//
// The normalizer performs transformations of the tree to bring it to a 'normalized' format
// In particular it does the following: 
//  (a) Transforms collection aggregate functions into a GroupBy. 
//  (b) Translates Exists(X) into Exists(select 1 from X)
//
namespace System.Data.Query.PlanCompiler
{
    /// <summary>
    /// The normalizer performs transformations of the tree to bring it to a 'normalized' format
    /// </summary>
    internal class Normalizer : SubqueryTrackingVisitor
    {
        #region constructors
        private Normalizer(PlanCompiler planCompilerState)
            :base(planCompilerState)
        {
        }
        #endregion

        #region public methods
        /// <summary>
        /// The driver routine.
        /// </summary>
        /// <param name="planCompilerState">plan compiler state</param>
        internal static void Process(PlanCompiler planCompilerState)
        {
            Normalizer normalizer = new Normalizer(planCompilerState);
            normalizer.Process();
        }

        #endregion

        #region private methods

        #region driver
        private void Process()
        {
            m_command.Root = VisitNode(m_command.Root);
        }
        #endregion

        #region visitor methods

        #region ScalarOps

        /// <summary>
        /// Translate Exists(X) into Exists(select 1 from X)
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(ExistsOp op, Node n)
        {
            VisitChildren(n);

            // Build up a dummy project node over the input
            n.Child0 = BuildDummyProjectForExists(n.Child0);

            return n;
        }

        /// <summary>
        /// Build Project(select 1 from child).
        /// </summary>
        /// <param name="child"></param>
        /// <returns></returns>
        private Node BuildDummyProjectForExists(Node child)
        {
            Var newVar;
            Node projectNode = m_command.BuildProject(
                child,
                m_command.CreateNode(m_command.CreateInternalConstantOp(m_command.IntegerType, 1)),
                out newVar);
            return projectNode;
        }

        /// <summary>
        /// Build up an unnest above a scalar op node
        ///    X => unnest(X)
        /// </summary>
        /// <param name="collectionNode">the scalarop collection node</param>
        /// <returns>the unnest node</returns>
        private Node BuildUnnest(Node collectionNode)
        {
            PlanCompiler.Assert(collectionNode.Op.IsScalarOp, "non-scalar usage of Unnest?");
            PlanCompiler.Assert(TypeSemantics.IsCollectionType(collectionNode.Op.Type), "non-collection usage for Unnest?");

            Var newVar;
            Node varDefNode = m_command.CreateVarDefNode(collectionNode, out newVar);
            UnnestOp unnestOp = m_command.CreateUnnestOp(newVar);
            Node unnestNode = m_command.CreateNode(unnestOp, varDefNode);

            return unnestNode;
        }

        /// <summary>
        /// Converts the reference to a TVF as following: Collect(PhysicalProject(Unnest(Func)))
        /// </summary>
        /// <param name="op">current function op</param>
        /// <param name="n">current function subtree</param>
        /// <returns>the new expression that corresponds to the TVF</returns>
        private Node VisitCollectionFunction(FunctionOp op, Node n)
        {
            PlanCompiler.Assert(TypeSemantics.IsCollectionType(op.Type), "non-TVF function?");

            Node unnestNode = BuildUnnest(n);
            UnnestOp unnestOp = unnestNode.Op as UnnestOp;
            PhysicalProjectOp projectOp = m_command.CreatePhysicalProjectOp(unnestOp.Table.Columns[0]);
            Node projectNode = m_command.CreateNode(projectOp, unnestNode);
            CollectOp collectOp = m_command.CreateCollectOp(n.Op.Type);
            Node collectNode = m_command.CreateNode(collectOp, projectNode);

            return collectNode;
        }

        /// <summary>
        /// Converts a collection aggregate function count(X), where X is a collection into
        /// two parts. Part A is a groupby subquery that looks like
        ///    GroupBy(Unnest(X), empty, count(y))
        /// where "empty" describes the fact that the groupby has no keys, and y is an
        /// element var of the Unnest
        /// 
        /// Part 2 is a VarRef that refers to the aggregate var for count(y) described above.
        /// 
        /// Logically, we would replace the entire functionOp by element(GroupBy...). However,
        /// since we also want to translate element() into single-row-subqueries, we do this
        /// here as well.
        /// 
        /// The function itself is replaced by the VarRef, and the GroupBy is added to the list
        /// of scalar subqueries for the current relOp node on the stack
        /// 
        /// </summary>
        /// <param name="op">the functionOp for the collection agg</param>
        /// <param name="n">current subtree</param>
        /// <returns>the VarRef node that should replace the function</returns>
        private Node VisitCollectionAggregateFunction(FunctionOp op, Node n)
        {
            TypeUsage softCastType = null;
            Node argNode = n.Child0;
            if (OpType.SoftCast == argNode.Op.OpType)
            {
                softCastType = TypeHelpers.GetEdmType<CollectionType>(argNode.Op.Type).TypeUsage;
                argNode = argNode.Child0;

                while (OpType.SoftCast == argNode.Op.OpType)
                {
                    argNode = argNode.Child0;
                }
            }

            Node unnestNode = BuildUnnest(argNode);
            UnnestOp unnestOp = unnestNode.Op as UnnestOp;
            Var unnestOutputVar = unnestOp.Table.Columns[0];

            AggregateOp aggregateOp = m_command.CreateAggregateOp(op.Function, false);
            VarRefOp unnestVarRefOp = m_command.CreateVarRefOp(unnestOutputVar);
            Node unnestVarRefNode = m_command.CreateNode(unnestVarRefOp);
            if (softCastType != null)
            {
                unnestVarRefNode = m_command.CreateNode(m_command.CreateSoftCastOp(softCastType), unnestVarRefNode);
            }
            Node aggExprNode = m_command.CreateNode(aggregateOp, unnestVarRefNode);

            VarVec keyVars = m_command.CreateVarVec(); // empty keys
            Node keyVarDefListNode = m_command.CreateNode(m_command.CreateVarDefListOp());

            VarVec gbyOutputVars = m_command.CreateVarVec();
            Var aggVar;
            Node aggVarDefListNode = m_command.CreateVarDefListNode(aggExprNode, out aggVar);
            gbyOutputVars.Set(aggVar);
            GroupByOp gbyOp = m_command.CreateGroupByOp(keyVars, gbyOutputVars);
            Node gbySubqueryNode = m_command.CreateNode(gbyOp, unnestNode, keyVarDefListNode, aggVarDefListNode);

            // "Move" this subquery to my parent relop
            Node ret = AddSubqueryToParentRelOp(aggVar, gbySubqueryNode);

            return ret;
        }

        /// <summary>
        /// Pre-processing for a function. Does the default scalar op processing.
        /// If the function returns a collection (TVF), the method converts this expression into
        ///    Collect(PhysicalProject(Unnest(Func))).
        /// If the function is a collection aggregate, converts it into the corresponding group aggregate.   
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public override Node Visit(FunctionOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Node newNode = null;

            // Is this a TVF?
            if (TypeSemantics.IsCollectionType(op.Type))
            {
                newNode = VisitCollectionFunction(op, n);
            }
            // Is this a collection-aggregate function?
            else if (PlanCompilerUtil.IsCollectionAggregateFunction(op, n))
            {
                newNode = VisitCollectionAggregateFunction(op, n);
            }
            else
            {
                newNode = n;
            }

            PlanCompiler.Assert(newNode != null, "failure to construct a functionOp?");
            return newNode;
        }

        #endregion

        #region RelOps
        /// <summary>
        /// Processing for all JoinOps
        /// </summary>
        /// <param name="op">JoinOp</param>
        /// <param name="n">Current subtree</param>
        /// <returns></returns>
        protected override Node VisitJoinOp(JoinBaseOp op, Node n)
        {
            if (base.ProcessJoinOp(op, n))
            {
                // update the join condition
                // #479372: Build up a dummy project node over the input, as we always wrap the child of exists
                n.Child2.Child0 =  BuildDummyProjectForExists(n.Child2.Child0);
            }
            return n;
        }

        #endregion

        #endregion

        #endregion
    }
}

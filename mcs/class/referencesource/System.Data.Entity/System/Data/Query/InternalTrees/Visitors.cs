//---------------------------------------------------------------------
// <copyright file="Visitors.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

namespace System.Data.Query.InternalTrees
{
    using System;
    using pc = System.Data.Query.PlanCompiler; // To be able to use PlanCompiler.Assert instead of Debug.Assert in this class.

    /// <summary>
    /// Simple implemenation of the BasicOpVisitor interface. 
    /// </summary>
    internal abstract class BasicOpVisitor
    {
        /// <summary>
        /// Default constructor. 
        /// </summary>
        internal BasicOpVisitor() { }

        #region Visitor Helpers
        /// <summary>
        /// Visit the children of this Node
        /// </summary>
        /// <param name="n">The Node that references the Op</param>
        protected virtual void VisitChildren(Node n)
        {
            foreach (Node chi in n.Children)
            {
                VisitNode(chi);
            }
        }

        /// <summary>
        /// Visit the children of this Node. but in reverse order
        /// </summary>
        /// <param name="n">The current node</param>
        protected virtual void VisitChildrenReverse(Node n)
        {
            for (int i = n.Children.Count - 1; i >= 0; i--)
            {
                VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        /// Visit this node
        /// </summary>
        /// <param name="n"></param>
        internal virtual void VisitNode(Node n)
        {
            n.Op.Accept(this, n);
        }
        /// <summary>
        /// Default node visitor
        /// </summary>
        /// <param name="n"></param>
        protected virtual void VisitDefault(Node n)
        {
            VisitChildren(n);
        }

        /// <summary>
        /// Default handler for all constantOps
        /// </summary>
        /// <param name="op">the constant op</param>
        /// <param name="n">the node</param>
        protected virtual void VisitConstantOp(ConstantBaseOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Default handler for all TableOps
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        protected virtual void VisitTableOp(ScanTableBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Default handler for all JoinOps
        /// </summary>
        /// <param name="op">join op</param>
        /// <param name="n"></param>
        protected virtual void VisitJoinOp(JoinBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Default handler for all ApplyOps
        /// </summary>
        /// <param name="op">apply op</param>
        /// <param name="n"></param>
        protected virtual void VisitApplyOp(ApplyBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }
        /// <summary>
        /// Default handler for all SetOps
        /// </summary>
        /// <param name="op">set op</param>
        /// <param name="n"></param>
        protected virtual void VisitSetOp(SetOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Default handler for all SortOps
        /// </summary>
        /// <param name="op">sort op</param>
        /// <param name="n"></param>
        protected virtual void VisitSortOp(SortBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Default handler for all GroupBy ops
        /// </summary>
        /// <param name="op">sort op</param>
        /// <param name="n"></param>
        protected virtual void VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }
        #endregion

        #region BasicOpVisitor Members

        /// <summary>
        /// Trap method for unrecognized Op types
        /// </summary>
        /// <param name="op">The Op being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(Op op, Node n)
        {
            throw new NotSupportedException(System.Data.Entity.Strings.Iqt_General_UnsupportedOp(op.GetType().FullName));
        }

        #region ScalarOps
        protected virtual void VisitScalarOpDefault(ScalarOp op, Node n)
        {
            VisitDefault(n);
        }

        /// <summary>
        /// Visitor pattern method for ConstantOp
        /// </summary>
        /// <param name="op">The ConstantOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ConstantOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for NullOp
        /// </summary>
        /// <param name="op">The NullOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(NullOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for NullSentinelOp
        /// </summary>
        /// <param name="op">The NullSentinelOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(NullSentinelOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for InternalConstantOp
        /// </summary>
        /// <param name="op">The InternalConstantOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(InternalConstantOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ConstantPredicateOp
        /// </summary>
        /// <param name="op">The ConstantPredicateOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ConstantPredicateOp op, Node n)
        {
            VisitConstantOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for FunctionOp
        /// </summary>
        /// <param name="op">The FunctionOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(FunctionOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for PropertyOp
        /// </summary>
        /// <param name="op">The PropertyOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(PropertyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for RelPropertyOp
        /// </summary>
        /// <param name="op">The RelPropertyOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(RelPropertyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for CaseOp
        /// </summary>
        /// <param name="op">The CaseOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(CaseOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ComparisonOp
        /// </summary>
        /// <param name="op">The ComparisonOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ComparisonOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for LikeOp
        /// </summary>
        /// <param name="op">The LikeOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(LikeOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for AggregateOp
        /// </summary>
        /// <param name="op">The AggregateOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(AggregateOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for NewInstanceOp
        /// </summary>
        /// <param name="op">The NewInstanceOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(NewInstanceOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for NewEntityOp
        /// </summary>
        /// <param name="op">The NewEntityOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(NewEntityOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for DiscriminatedNewInstanceOp
        /// </summary>
        /// <param name="op">The DiscriminatedNewInstanceOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(DiscriminatedNewEntityOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for NewMultisetOp
        /// </summary>
        /// <param name="op">The NewMultisetOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(NewMultisetOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for NewRecordOp
        /// </summary>
        /// <param name="op">The NewRecordOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(NewRecordOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for RefOp
        /// </summary>
        /// <param name="op">The RefOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(RefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for VarRefOp
        /// </summary>
        /// <param name="op">The VarRefOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(VarRefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ConditionalOp
        /// </summary>
        /// <param name="op">The ConditionalOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ConditionalOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ArithmeticOp
        /// </summary>
        /// <param name="op">The ArithmeticOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ArithmeticOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for TreatOp
        /// </summary>
        /// <param name="op">The TreatOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(TreatOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for CastOp
        /// </summary>
        /// <param name="op">The CastOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(CastOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }
        /// <summary>
        /// Visitor pattern method for SoftCastOp
        /// </summary>
        /// <param name="op">The SoftCastOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(SoftCastOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for IsOp
        /// </summary>
        /// <param name="op">The IsOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(IsOfOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ExistsOp
        /// </summary>
        /// <param name="op">The ExistsOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ExistsOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ElementOp
        /// </summary>
        /// <param name="op">The ElementOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ElementOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for GetEntityRefOp
        /// </summary>
        /// <param name="op">The GetEntityRefOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(GetEntityRefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for GetRefKeyOp
        /// </summary>
        /// <param name="op">The GetRefKeyOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(GetRefKeyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for NestOp
        /// </summary>
        /// <param name="op">The NestOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(CollectOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        public virtual void Visit(DerefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        public virtual void Visit(NavigateOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        #endregion

        #region AncillaryOps
        protected virtual void VisitAncillaryOpDefault(AncillaryOp op, Node n)
        {
            VisitDefault(n);
        }

        /// <summary>
        /// Visitor pattern method for VarDefOp
        /// </summary>
        /// <param name="op">The VarDefOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(VarDefOp op, Node n)
        {
            VisitAncillaryOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for VarDefListOp
        /// </summary>
        /// <param name="op">The VarDefListOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(VarDefListOp op, Node n)
        {
            VisitAncillaryOpDefault(op, n);
        }

        #endregion

        #region RelOps

        protected virtual void VisitRelOpDefault(RelOp op, Node n)
        {
            VisitDefault(n);
        }

        /// <summary>
        /// Visitor pattern method for ScanTableOp
        /// </summary>
        /// <param name="op">The ScanTableOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ScanTableOp op, Node n)
        {
            VisitTableOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ScanViewOp
        /// </summary>
        /// <param name="op">The ScanViewOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ScanViewOp op, Node n)
        {
            VisitTableOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for UnnestOp
        /// </summary>
        /// <param name="op">The UnnestOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(UnnestOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ProjectOp
        /// </summary>
        /// <param name="op">The ProjectOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ProjectOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for FilterOp
        /// </summary>
        /// <param name="op">The FilterOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(FilterOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for SortOp
        /// </summary>
        /// <param name="op">The SortOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(SortOp op, Node n)
        {
            VisitSortOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ConstrainedSortOp
        /// </summary>
        /// <param name="op">The ConstrainedSortOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ConstrainedSortOp op, Node n)
        {
            VisitSortOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for GroupByOp
        /// </summary>
        /// <param name="op">The GroupByOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(GroupByOp op, Node n)
        {
            VisitGroupByOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for GroupByIntoOp
        /// </summary>
        /// <param name="op">The GroupByIntoOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(GroupByIntoOp op, Node n)
        {
            VisitGroupByOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for CrossJoinOp
        /// </summary>
        /// <param name="op">The CrossJoinOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(CrossJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for InnerJoinOp
        /// </summary>
        /// <param name="op">The InnerJoinOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(InnerJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for LeftOuterJoinOp
        /// </summary>
        /// <param name="op">The LeftOuterJoinOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(LeftOuterJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for FullOuterJoinOp
        /// </summary>
        /// <param name="op">The FullOuterJoinOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(FullOuterJoinOp op, Node n)
        {
            VisitJoinOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for CrossApplyOp
        /// </summary>
        /// <param name="op">The CrossApplyOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(CrossApplyOp op, Node n)
        {
            VisitApplyOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for OuterApplyOp
        /// </summary>
        /// <param name="op">The OuterApplyOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(OuterApplyOp op, Node n)
        {
            VisitApplyOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for UnionAllOp
        /// </summary>
        /// <param name="op">The UnionAllOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(UnionAllOp op, Node n)
        {
            VisitSetOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for IntersectOp
        /// </summary>
        /// <param name="op">The IntersectOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(IntersectOp op, Node n)
        {
            VisitSetOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for ExceptOp
        /// </summary>
        /// <param name="op">The ExceptOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(ExceptOp op, Node n)
        {
            VisitSetOp(op, n);
        }

        /// <summary>
        /// Visitor pattern method for DistinctOp
        /// </summary>
        /// <param name="op">The DistinctOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(DistinctOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for SingleRowOp
        /// </summary>
        /// <param name="op">The SingleRowOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(SingleRowOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for SingleRowTableOp
        /// </summary>
        /// <param name="op">The SingleRowTableOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(SingleRowTableOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }

        #endregion

        #region PhysicalOps
        protected virtual void VisitPhysicalOpDefault(PhysicalOp op, Node n)
        {
            VisitDefault(n);
        }

        /// <summary>
        /// Visitor pattern method for PhysicalProjectOp
        /// </summary>
        /// <param name="op">The op being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(PhysicalProjectOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
        }

        #region NestOps
        /// <summary>
        /// Common handling for all nestOps
        /// </summary>
        /// <param name="op">nest op</param>
        /// <param name="n"></param>
        protected virtual void VisitNestOp(NestBaseOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
        }
        /// <summary>
        /// Visitor pattern method for SingleStreamNestOp
        /// </summary>
        /// <param name="op">The op being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(SingleStreamNestOp op, Node n)
        {
            VisitNestOp(op, n);
        }
        /// <summary>
        /// Visitor pattern method for MultistreamNestOp
        /// </summary>
        /// <param name="op">The op being visited</param>
        /// <param name="n">The Node that references the Op</param>
        public virtual void Visit(MultiStreamNestOp op, Node n)
        {
            VisitNestOp(op, n);
        }
        #endregion
        #endregion

        #endregion
    }

    /// <summary>
    /// Simple implementation of the BasicOpVisitorOfT interface"/>
    /// </summary>
    /// <typeparam name="TResultType">type parameter</typeparam>
    internal abstract class BasicOpVisitorOfT<TResultType>
    {
        #region visitor helpers

        /// <summary>
        /// Simply iterates over all children, and manages any updates 
        /// </summary>
        /// <param name="n">The current node</param>
        protected virtual void VisitChildren(Node n)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        /// Simply iterates over all children, and manages any updates, but in reverse order
        /// </summary>
        /// <param name="n">The current node</param>
        protected virtual void VisitChildrenReverse(Node n)
        {
            for (int i = n.Children.Count - 1; i >= 0; i--)
            {
                VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        /// Simple wrapper to invoke the appropriate action on a node
        /// </summary>
        /// <param name="n">the node to process</param>
        /// <returns></returns>
        internal TResultType VisitNode(Node n)
        {
            // Invoke the visitor
            return n.Op.Accept<TResultType>(this, n);
        }

        /// <summary>
        /// A default processor for any node. Visits the children and returns itself unmodified.
        /// </summary>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially new node</returns>
        protected virtual TResultType VisitDefault(Node n)
        {
            VisitChildren(n);
            return default(TResultType);
        }

        #endregion

        /// <summary>
        /// No processing yet for this node - raises an exception
        /// </summary>
        /// <param name="n"></param>
        internal virtual TResultType Unimplemented(Node n)
        {
            pc.PlanCompiler.Assert(false, "Not implemented op type");
            return default(TResultType);
        }

        /// <summary>
        /// Catch-all processor - raises an exception
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(Op op, Node n)
        {
            return Unimplemented(n);
        }

        #region AncillaryOp Visitors

        /// <summary>
        /// A default processor for all AncillaryOps.
        /// 
        /// Allows new visitors to just override this to handle all AncillaryOps
        /// </summary>
        /// <param name="op">the AncillaryOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitAncillaryOpDefault(AncillaryOp op, Node n)
        {
            return VisitDefault(n);
        }

        /// <summary>
        /// VarDefOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(VarDefOp op, Node n)
        {
            return VisitAncillaryOpDefault(op, n);
        }

        /// <summary>
        /// VarDefListOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(VarDefListOp op, Node n)
        {
            return VisitAncillaryOpDefault(op, n);
        }

        #endregion

        #region PhysicalOp Visitors

        /// <summary>
        /// A default processor for all PhysicalOps.
        /// 
        /// Allows new visitors to just override this to handle all PhysicalOps
        /// </summary>
        /// <param name="op">the PhysicalOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitPhysicalOpDefault(PhysicalOp op, Node n)
        {
            return VisitDefault(n);
        }

        /// <summary>
        /// PhysicalProjectOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(PhysicalProjectOp op, Node n)
        {
            return VisitPhysicalOpDefault(op, n);
        }

        #region NestOp Visitors

        /// <summary>
        /// A default processor for all NestOps.
        /// 
        /// Allows new visitors to just override this to handle all NestOps
        /// </summary>
        /// <param name="op">the NestOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitNestOp(NestBaseOp op, Node n)
        {
            return VisitPhysicalOpDefault(op, n);
        }

        /// <summary>
        /// SingleStreamNestOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(SingleStreamNestOp op, Node n)
        {
            return VisitNestOp(op, n);
        }
        /// <summary>
        /// MultiStreamNestOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(MultiStreamNestOp op, Node n)
        {
            return VisitNestOp(op, n);
        }

        #endregion

        #endregion

        #region RelOp Visitors

        /// <summary>
        /// A default processor for all RelOps.
        /// 
        /// Allows new visitors to just override this to handle all RelOps
        /// </summary>
        /// <param name="op">the RelOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitRelOpDefault(RelOp op, Node n)
        {
            return VisitDefault(n);
        }

        #region ApplyOp Visitors

        /// <summary>
        /// Common handling for all ApplyOps
        /// </summary>
        /// <param name="op">the ApplyOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitApplyOp(ApplyBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// CrossApply
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(CrossApplyOp op, Node n)
        {
            return VisitApplyOp(op, n);
        }

        /// <summary>
        /// OuterApply
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(OuterApplyOp op, Node n)
        {
            return VisitApplyOp(op, n);
        }

        #endregion

        #region JoinOp Visitors

        /// <summary>
        /// A default processor for all JoinOps.
        /// 
        /// Allows new visitors to just override this to handle all JoinOps.
        /// </summary>
        /// <param name="op">the JoinOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitJoinOp(JoinBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// CrossJoin
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(CrossJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        /// <summary>
        /// FullOuterJoin
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(FullOuterJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        /// <summary>
        /// LeftOuterJoin
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(LeftOuterJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        /// <summary>
        /// InnerJoin
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(InnerJoinOp op, Node n)
        {
            return VisitJoinOp(op, n);
        }

        #endregion

        #region SetOp Visitors

        /// <summary>
        /// A default processor for all SetOps.
        /// 
        /// Allows new visitors to just override this to handle all SetOps.
        /// </summary>
        /// <param name="op">the SetOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitSetOp(SetOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Except
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ExceptOp op, Node n)
        {
            return VisitSetOp(op, n);
        }

        /// <summary>
        /// Intersect
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(IntersectOp op, Node n)
        {
            return VisitSetOp(op, n);
        }

        /// <summary>
        /// UnionAll
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(UnionAllOp op, Node n)
        {
            return VisitSetOp(op, n);
        }

        #endregion

        /// <summary>
        /// Distinct
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(DistinctOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// FilterOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(FilterOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// GroupByBaseOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected virtual TResultType VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }


        /// <summary>
        /// GroupByOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(GroupByOp op, Node n)
        {
            return VisitGroupByOp(op, n);
        }

        /// <summary>
        /// GroupByIntoOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(GroupByIntoOp op, Node n)
        {
            return VisitGroupByOp(op, n);
        }

        /// <summary>
        /// ProjectOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ProjectOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        #region TableOps
        /// <summary>
        /// Default handler for all TableOps
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected virtual TResultType VisitTableOp(ScanTableBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }
        /// <summary>
        /// ScanTableOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ScanTableOp op, Node n)
        {
            return VisitTableOp(op, n);
        }

        /// <summary>
        /// ScanViewOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ScanViewOp op, Node n)
        {
            return VisitTableOp(op, n);
        }
        #endregion

        /// <summary>
        /// Visitor pattern method for SingleRowOp
        /// </summary>
        /// <param name="op">The SingleRowOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns></returns>
        public virtual TResultType Visit(SingleRowOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// Visitor pattern method for SingleRowTableOp
        /// </summary>
        /// <param name="op">The SingleRowTableOp being visited</param>
        /// <param name="n">The Node that references the Op</param>
        /// <returns></returns>
        public virtual TResultType Visit(SingleRowTableOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// A default processor for all SortOps.
        /// 
        /// Allows new visitors to just override this to handle ConstrainedSortOp/SortOp.
        /// </summary>
        /// <param name="op">the SetOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected virtual TResultType VisitSortOp(SortBaseOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        /// <summary>
        /// SortOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(SortOp op, Node n)
        {
            return VisitSortOp(op, n);
        }

        /// <summary>
        /// ConstrainedSortOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ConstrainedSortOp op, Node n)
        {
            return VisitSortOp(op, n);
        }

        /// <summary>
        /// UnnestOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(UnnestOp op, Node n)
        {
            return VisitRelOpDefault(op, n);
        }

        #endregion

        #region ScalarOp Visitors

        /// <summary>
        /// A default processor for all ScalarOps.
        /// 
        /// Allows new visitors to just override this to handle all ScalarOps
        /// </summary>
        /// <param name="op">the ScalarOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially new node</returns>
        protected virtual TResultType VisitScalarOpDefault(ScalarOp op, Node n)
        {
            return VisitDefault(n);
        }

        /// <summary>
        /// Default handler for all constant Ops
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        protected virtual TResultType VisitConstantOp(ConstantBaseOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// AggregateOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(AggregateOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// ArithmeticOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ArithmeticOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// CaseOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(CaseOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// CastOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(CastOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// SoftCastOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(SoftCastOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// NestOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(CollectOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// ComparisonOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ComparisonOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// ConditionalOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ConditionalOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// ConstantOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ConstantOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        /// ConstantPredicateOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ConstantPredicateOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        /// ElementOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ElementOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// ExistsOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(ExistsOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// FunctionOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(FunctionOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// GetEntityRefOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(GetEntityRefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// GetRefKeyOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(GetRefKeyOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// InternalConstantOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(InternalConstantOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        /// IsOfOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(IsOfOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// LikeOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(LikeOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// NewEntityOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(NewEntityOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// NewInstanceOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(NewInstanceOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// DiscriminatedNewInstanceOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(DiscriminatedNewEntityOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// NewMultisetOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(NewMultisetOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// NewRecordOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(NewRecordOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// NullOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(NullOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        /// NullSentinelOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(NullSentinelOp op, Node n)
        {
            return VisitConstantOp(op, n);
        }

        /// <summary>
        /// PropertyOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(PropertyOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }
        /// <summary>
        /// RelPropertyOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(RelPropertyOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// RefOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(RefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// TreatOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(TreatOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        /// <summary>
        /// VarRefOp
        /// </summary>
        /// <param name="op"></param>
        /// <param name="n"></param>
        /// <returns></returns>
        public virtual TResultType Visit(VarRefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }

        public virtual TResultType Visit(DerefOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }
        public virtual TResultType Visit(NavigateOp op, Node n)
        {
            return VisitScalarOpDefault(op, n);
        }
        #endregion
    }

    /// <summary>
    /// A visitor implementation that allows subtrees to be modified (in a bottom-up
    /// fashion)
    /// </summary>
    internal abstract class BasicOpVisitorOfNode : BasicOpVisitorOfT<Node>
    {
        #region visitor helpers

        /// <summary>
        /// Simply iterates over all children, and manages any updates 
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildren(Node n)
        {
            for (int i = 0; i < n.Children.Count; i++)
            {
                n.Children[i] = VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        /// Simply iterates over all children, and manages any updates, but in reverse order
        /// </summary>
        /// <param name="n">The current node</param>
        protected override void VisitChildrenReverse(Node n)
        {
            for (int i = n.Children.Count - 1; i >= 0; i--)
            {
                n.Children[i] = VisitNode(n.Children[i]);
            }
        }

        /// <summary>
        /// A default processor for any node. Visits the children and returns itself unmodified.
        /// </summary>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially new node</returns>
        protected override Node VisitDefault(Node n)
        {
            VisitChildren(n);
            return n;
        }

        #endregion

        #region AncillaryOp Visitors

        /// <summary>
        /// A default processor for all AncillaryOps.
        /// 
        /// Allows new visitors to just override this to handle all AncillaryOps
        /// </summary>
        /// <param name="op">the AncillaryOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected override Node VisitAncillaryOpDefault(AncillaryOp op, Node n)
        {
            return VisitDefault(n);
        }

        #endregion

        #region PhysicalOp Visitors

        /// <summary>
        /// A default processor for all PhysicalOps.
        /// 
        /// Allows new visitors to just override this to handle all PhysicalOps
        /// </summary>
        /// <param name="op">the PhysicalOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected override Node VisitPhysicalOpDefault(PhysicalOp op, Node n)
        {
            return VisitDefault(n);
        }

        #endregion

        #region RelOp Visitors

        /// <summary>
        /// A default processor for all RelOps.
        /// 
        /// Allows new visitors to just override this to handle all RelOps
        /// </summary>
        /// <param name="op">the RelOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially modified subtree</returns>
        protected override Node VisitRelOpDefault(RelOp op, Node n)
        {
            return VisitDefault(n);
        }
        #endregion

        #region ScalarOp Visitors

        /// <summary>
        /// A default processor for all ScalarOps.
        /// 
        /// Allows new visitors to just override this to handle all ScalarOps
        /// </summary>
        /// <param name="op">the ScalarOp</param>
        /// <param name="n">the node to process</param>
        /// <returns>a potentially new node</returns>
        protected override Node VisitScalarOpDefault(ScalarOp op, Node n)
        {
            return VisitDefault(n);
        }
        #endregion

    }
}

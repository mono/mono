//---------------------------------------------------------------------
// <copyright file="BasicValidator.cs" company="Microsoft">
//      Copyright (c) Microsoft Corporation.  All rights reserved.
// </copyright>
//
// @owner  [....]
// @backupOwner [....]
//---------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Diagnostics;
using System.Data;
using System.Data.Common;
using System.Data.Metadata.Edm;

namespace System.Data.Query.InternalTrees
{
#if DEBUG
    /// <summary>
    /// The BasicValidator validates the shape of the IQT. It ensures that the 
    /// various Ops in the tree have the right kinds and number of arguments.
    /// </summary>
    internal class BasicValidator : BasicOpVisitor
    {
        #region constructors
        protected BasicValidator(Command command)
        {
            m_command = command;
        }
        #endregion

        #region private surface
        protected void Validate(Node node)
        {
            VisitNode(node);
        }

        #region AssertHelpers
        protected static void Assert(bool condition, string format, int arg0)
        {
            if (!condition)
            {
                Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, format, arg0));
            }
        }
        protected static void Assert(bool condition, string format, OpType op)
        {
            if (!condition)
            {
                Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, format, Dump.AutoString.ToString(op)));
            }
        }
        protected static void Assert(bool condition, string format, OpType op, object arg1)
        {
            if (!condition)
            {
                Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, format, Dump.AutoString.ToString(op), arg1));
            }
        }
        protected static void Assert(bool condition, string format, OpType op, object arg1, object arg2)
        {
            if (!condition)
            {
                Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, format, Dump.AutoString.ToString(op), arg1, arg2));
            }
        }
        protected static void Assert(bool condition, string format, params object[] args)
        {
            if (!condition)
            {
                Debug.Assert(false, String.Format(CultureInfo.InvariantCulture, format, args));
            }
        }
        protected static void AssertArity(Node n, int arity)
        {
            Assert(arity == n.Children.Count, "Op Arity mismatch for Op {0}: Expected {1} arguments; found {2} arguments", n.Op.OpType, arity, n.Children.Count);
        }
        protected static void AssertArity(Node n)
        {
            if (n.Op.Arity != Op.ArityVarying)
            {
                AssertArity(n, n.Op.Arity);
            }
        }
        protected static void AssertBoolean(TypeUsage type)
        {
            Assert(TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Boolean), "Type Mismatch: Expected Boolean; found {0} instead", TypeHelpers.GetFullName(type));
        }
        protected static void AssertCollectionType(TypeUsage type)
        {
            Assert(TypeSemantics.IsCollectionType(type), "Type Mismatch: Expected Collection type: Found {0}", TypeHelpers.GetFullName(type));
        }
        protected static void AssertEqualTypes(TypeUsage type1, TypeUsage type2)
        {
            Assert(Command.EqualTypes(type1, type2),
                "Type mismatch: " + type1.Identity + ", " + type2.Identity);
        }
        protected static void AssertEqualTypes(TypeUsage type1, EdmType type2)
        {
            AssertEqualTypes(type1, TypeUsage.Create(type2));
        }
        protected static void AssertBooleanOp(Op op)
        {
            AssertBoolean(op.Type);
        }
        protected static void AssertRelOp(Op op)
        {
            Assert(op.IsRelOp, "OpType Mismatch: Expected RelOp; found {0}", op.OpType);
        }
        protected static void AssertRelOpOrPhysicalOp(Op op)
        {
            Assert(op.IsRelOp || op.IsPhysicalOp, "OpType Mismatch: Expected RelOp or PhysicalOp; found {0}", op.OpType);
        }
        protected static void AssertScalarOp(Op op)
        {
            Assert(op.IsScalarOp, "OpType Mismatch: Expected ScalarOp; found {0}", op.OpType);
        }
        protected static void AssertOpType(Op op, OpType opType)
        {
            Assert(op.OpType == opType, "OpType Mismatch: Expected {0}; found {1}", op.OpType, Dump.AutoString.ToString(opType));
        }
        protected static void AssertUnexpectedOp(Op op)
        {
            Assert(false, "Unexpected OpType {0}", op.OpType);
        }
        #endregion

        #region Visitors

        protected override void VisitDefault(Node n)
        {
            Assert(n.Id >= 0, "Bad node id {0}", n.Id);
            VisitChildren(n);
            AssertArity(n);
        }

        #region ScalarOps
        protected override void VisitScalarOpDefault(ScalarOp op, Node n)
        {
            VisitDefault(n);
            Assert(op.Type != null, "ScalarOp {0} with no datatype!", op.OpType);
            if (op.OpType != OpType.Element &&
                op.OpType != OpType.Exists &&
                op.OpType != OpType.Collect)
            {
                foreach (Node chi in n.Children)
                {
                    AssertScalarOp(chi.Op);
                }
            }
        }

        public override void Visit(AggregateOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }
        public override void Visit(CaseOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Assert((n.Children.Count >= 3 && n.Children.Count % 2 == 1),
                   "CaseOp: Expected odd number of arguments, and at least 3; found {0}", n.Children.Count); 
           
            // Validate that each when statement is of type Boolean
            for (int i = 0; i < n.Children.Count - 1; i += 2)
            {
                Assert(TypeSemantics.IsBooleanType(n.Children[i].Op.Type), "Encountered a when node with a non-boolean return type");
            }

            // Ensure that the then clauses, the else clause and the result type are all the same
            for (int i = 1; i < n.Children.Count-1; i += 2)
            {
                AssertEqualTypes(n.Op.Type, n.Children[i].Op.Type);
            }
            AssertEqualTypes(n.Op.Type, n.Children[n.Children.Count - 1].Op.Type);
        }

        public override void Visit(ComparisonOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            AssertBooleanOp(op);
            AssertEqualTypes(n.Child0.Op.Type, n.Child1.Op.Type);
        }
        public override void Visit(ConditionalOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            switch(op.OpType)
            {
                case OpType.And: 
                case OpType.Or:
                    AssertArity(n, 2);
                    AssertBooleanOp(n.Child0.Op);
                    AssertBooleanOp(n.Child1.Op);
                    break;
                case OpType.Not:
                    AssertArity(n, 1);
                    AssertBooleanOp(n.Child0.Op);
                    break;
                case OpType.IsNull:
                    AssertArity(n, 1);
                    break;
                default:
                    AssertUnexpectedOp(op);
                    break;
            }
            AssertBooleanOp(op);
        }
        public override void Visit(ArithmeticOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            switch (op.OpType)
            {
                case OpType.Plus: 
                case OpType.Minus: 
                case OpType.Multiply: 
                case OpType.Divide:
                case OpType.Modulo:
                    AssertEqualTypes(n.Child0.Op.Type, n.Child1.Op.Type);
                    AssertEqualTypes(n.Op.Type, n.Child0.Op.Type);
                    AssertArity(n, 2);
                    break;
                case OpType.UnaryMinus: 
                    AssertArity(n, 1);
                    break;
                default:
                    AssertUnexpectedOp(op);
                    break;
            }
        }
        public override void Visit(ElementOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            AssertRelOp(n.Child0.Op);
        }

        public override void Visit(CollectOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            AssertOpType(n.Child0.Op, OpType.PhysicalProject);
            AssertCollectionType(op.Type);
        }

        public override void Visit(DerefOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Assert(TypeSemantics.IsEntityType(op.Type), "Expected an entity type. Found " + op.Type);
            Assert(TypeSemantics.IsReferenceType(n.Child0.Op.Type), "Expected a ref type. Found " + n.Child0.Op.Type);
            RefType r = n.Child0.Op.Type.EdmType as RefType;
            Assert(r.ElementType.EdmEquals(op.Type.EdmType), "Inconsistent types");
        }

        public override void Visit(ExistsOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            AssertRelOp(n.Child0.Op);
            AssertBooleanOp(op);
        }

        public override void Visit(PropertyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            AssertEqualTypes(n.Child0.Op.Type, op.PropertyInfo.DeclaringType);
        }

        public override void Visit(RelPropertyOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Assert(m_command.IsRelPropertyReferenced(op.PropertyInfo), "no such rel property:", op.PropertyInfo);
            Assert(TypeSemantics.IsEntityType(n.Child0.Op.Type), "argument to RelPropertyOp must be an entity type. Found: ", n.Child0.Op.Type);
        }

        public override void Visit(FunctionOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            Assert(op.Function.Parameters.Count == n.Children.Count, "FunctionOp: Argument count ({0}) does not match parameter count ({1})", n.Children.Count, op.Function.Parameters.Count);
            for (int idx = 0; idx < n.Children.Count; idx++)
            {
                AssertEqualTypes(n.Children[idx].Op.Type, op.Function.Parameters[idx].TypeUsage);
            }
        }

        public override void Visit(SoftCastOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
            // [....] 9/21/06 - temporarily removing check here 
            //  because the assert wrongly fails in some cases where the types are promotable,
            //  but the facets are not.  Put this back when that issue is solved.
            // Assert(TypeSemantics.IsEquivalentOrPromotableTo(n.Child0.Op.Type, op.Type), "Illegal SoftCastOp: Cannot promote input type {0} to target type {1}", n.Child0.Op.Type.Identity, op.Type.Identity);
        }

        public override void Visit(NavigateOp op, Node n)
        {
            VisitScalarOpDefault(op, n);
        }

        #endregion

        #region AncillaryOps
        protected override void VisitAncillaryOpDefault(AncillaryOp op, Node n)
        {
            VisitDefault(n);
        }

        public override void Visit(VarDefOp op, Node n)
        {
            VisitAncillaryOpDefault(op, n);
            AssertScalarOp(n.Child0.Op);
            VarDefOp varDefOp = (VarDefOp)op;
            AssertEqualTypes(varDefOp.Var.Type, n.Child0.Op.Type);
        }
        public override void Visit(VarDefListOp op, Node n)
        {
            VisitDefault(n);
            foreach (Node chi in n.Children)
            {
                AssertOpType(chi.Op, OpType.VarDef);
            }
        }
        #endregion

        #region RelOps
        protected override void VisitRelOpDefault(RelOp op, Node n)
        {
            VisitDefault(n);
        }
        protected override void VisitJoinOp(JoinBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            if (op.OpType == OpType.CrossJoin)
            {
                Assert(n.Children.Count >= 2, "CrossJoinOp needs at least 2 arguments; found only {0}", n.Children.Count);
                return;
            }
            AssertRelOpOrPhysicalOp(n.Child0.Op);
            AssertRelOpOrPhysicalOp(n.Child1.Op);
            AssertScalarOp(n.Child2.Op);
            AssertBooleanOp(n.Child2.Op);
        }
        protected override void VisitApplyOp(ApplyBaseOp op, Node n)
        {
             VisitRelOpDefault(op, n);
             AssertRelOpOrPhysicalOp(n.Child0.Op);
             AssertRelOpOrPhysicalOp(n.Child1.Op);
        }
        protected override void VisitSetOp(SetOp op, Node n)
        {
             VisitRelOpDefault(op, n);
             AssertRelOpOrPhysicalOp(n.Child0.Op);
             AssertRelOpOrPhysicalOp(n.Child1.Op);
             //
             // Ensure that the corresponding setOp Vars are all of the same
             // type
             //
             foreach (VarMap varMap in op.VarMap)
             {
                 foreach (KeyValuePair<Var, Var> kv in varMap)
                 {
                     AssertEqualTypes(kv.Key.Type, kv.Value.Type);
                 }
             }
        }
        protected override void VisitSortOp(SortBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertRelOpOrPhysicalOp(n.Child0.Op);
        }

        public override void Visit(ConstrainedSortOp op, Node n)
        {
            base.Visit(op, n);
            AssertScalarOp(n.Child1.Op);
            Assert(TypeSemantics.IsIntegerNumericType(n.Child1.Op.Type), "ConstrainedSortOp Skip Count Node must have an integer result type");
            AssertScalarOp(n.Child2.Op);
            Assert(TypeSemantics.IsIntegerNumericType(n.Child2.Op.Type), "ConstrainedSortOp Limit Node must have an integer result type");
        }

        public override void Visit(ScanTableOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }
        public override void Visit(ScanViewOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertRelOp(n.Child0.Op);
        }
        public override void Visit(FilterOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertRelOpOrPhysicalOp(n.Child0.Op);
            AssertScalarOp(n.Child1.Op);
            AssertBooleanOp(n.Child1.Op);
        }
        public override void Visit(ProjectOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertRelOpOrPhysicalOp(n.Child0.Op);
            AssertOpType(n.Child1.Op, OpType.VarDefList);
        }
        public override void Visit(UnnestOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertOpType(n.Child0.Op, OpType.VarDef);
        }
        protected override void VisitGroupByOp(GroupByBaseOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertRelOpOrPhysicalOp(n.Child0.Op);

            for (int i = 1; i < n.Children.Count; i++)
            {
                AssertOpType(n.Children[i].Op, OpType.VarDefList);
            }
        }
        public override void Visit(GroupByIntoOp op, Node n)
        {
            VisitGroupByOp(op, n);
            Assert(n.Child3.Children.Count > 0, "GroupByInto with no group aggregate vars");
        }
        
        public override void Visit(DistinctOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertRelOp(n.Child0.Op);
        }

        public override void Visit(SingleRowTableOp op, Node n)
        {
            VisitRelOpDefault(op, n);
        }
        public override void Visit(SingleRowOp op, Node n)
        {
            VisitRelOpDefault(op, n);
            AssertRelOpOrPhysicalOp(n.Child0.Op);
        }
        #endregion

        #region PhysicalOps
        protected override void VisitPhysicalOpDefault(PhysicalOp op, Node n)
        {
            VisitDefault(n);
        }
        public override void Visit(PhysicalProjectOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
            Assert(n.Children.Count >= 1, "PhysicalProjectOp needs at least 1 arg: found {0}", n.Children.Count);
            foreach (Node chi in n.Children)
            {
                AssertRelOpOrPhysicalOp(chi.Op);
            }
        }
        public override void Visit(SingleStreamNestOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
            AssertRelOp(n.Child0.Op);
        }
        public override void Visit(MultiStreamNestOp op, Node n)
        {
            VisitPhysicalOpDefault(op, n);
            Assert(n.Children.Count > 1, "MultiStreamNestOp needs at least 2 arguments: found {0}", n.Children.Count);
            foreach (Node chi in n.Children)
            {
                AssertRelOpOrPhysicalOp(chi.Op);
            }
        }
        #endregion

        #endregion

        #endregion

        #region private state
        private Command m_command;
        #endregion

    }
#endif // DEBUG
}

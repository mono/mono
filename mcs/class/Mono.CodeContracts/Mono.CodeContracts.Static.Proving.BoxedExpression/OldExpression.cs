using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class OldExpression : BoxedExpression
		{
			private const string ContractOldValueTemplate = "Contract.OldValue({0})";
			public readonly BoxedExpression Old;
			public readonly TypeNode Type;

			public OldExpression (BoxedExpression old, TypeNode type)
			{
				this.Old = old;
				this.Type = type;
			}

			#region Overrides of BoxedExpression
			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Old.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression old = this.Old.Substitute (map);
				if (old == this.Old)
					return this;
				if (old == null)
					return null;

				return new OldExpression (old, this.Type);
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				return this.Old.IsBinaryExpression (out op, out left, out right);
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				return this.Old.IsUnaryExpression (out op, out argument);
			}

			public override bool IsIsinstExpression (out BoxedExpression expr, out TypeNode type)
			{
				return this.Old.IsIsinstExpression (out expr, out type);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.EndOld (pc, new PC (pc.Node, 0), this.Type, Dummy.Value, Dummy.Value, data);
			}
			#endregion

			public override PathElement[] AccessPath {
				get { return this.Old.AccessPath; }
			}

			public override BoxedExpression BinaryLeftArgument {
				get { return this.Old.BinaryLeftArgument; }
			}

			public override BoxedExpression BinaryRightArgument {
				get { return this.Old.BinaryRightArgument; }
			}

			public override BinaryOperator BinaryOperator {
				get { return this.Old.BinaryOperator; }
			}

			public override object Constant {
				get { return this.Old.Constant; }
			}

			public override object ConstantType {
				get { return this.Old.ConstantType; }
			}

			public override bool IsBinary {
				get { return this.Old.IsBinary; }
			}

			public override bool IsConstant {
				get { return this.Old.IsConstant; }
			}

			public override bool IsSizeof {
				get { return this.Old.IsSizeof; }
			}

			public override bool IsNull {
				get { return this.Old.IsNull; }
			}

			public override bool IsIsinst {
				get { return this.Old.IsIsinst; }
			}

			public override bool IsUnary {
				get { return this.Old.IsUnary; }
			}

			public override bool IsVariable {
				get { return this.Old.IsVariable; }
			}

			public override BoxedExpression UnaryArgument {
				get { return this.Old.UnaryArgument; }
			}

			public override UnaryOperator UnaryOperator {
				get { return this.Old.UnaryOperator; }
			}

			public override object UnderlyingVariable {
				get { return this.Old.UnderlyingVariable; }
			}
		}
}


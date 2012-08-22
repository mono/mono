using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class ValueAtReturnExpression : BoxedExpression
		{
			private const string ContractValueAtReturnTemplate = "Contract.ValueAtReturn({0})";
			public readonly TypeNode Type;
			public readonly BoxedExpression Value;

			public ValueAtReturnExpression (BoxedExpression old, TypeNode type)
			{
				this.Value = old;
				this.Type = type;
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Value.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression value = this.Value.Substitute (map);
				if (value == this.Value)
					return this;
				if (value == null)
					return null;

				return new ValueAtReturnExpression (value, this.Type);
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				return this.Value.IsBinaryExpression (out op, out left, out right);
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				return this.Value.IsUnaryExpression (out op, out argument);
			}

			public override bool IsIsinstExpression (out BoxedExpression expr, out TypeNode type)
			{
				return this.Value.IsIsinstExpression (out expr, out type);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				throw new NotImplementedException ();
			}

			public override PathElement[] AccessPath {
				get { return this.Value.AccessPath; }
			}

			public override BoxedExpression BinaryLeftArgument {
				get { return this.Value.BinaryLeftArgument; }
			}

			public override BoxedExpression BinaryRightArgument {
				get { return this.Value.BinaryRightArgument; }
			}

			public override BinaryOperator BinaryOperator {
				get { return this.Value.BinaryOperator; }
			}

			public override object Constant {
				get { return this.Value.Constant; }
			}

			public override object ConstantType {
				get { return this.Value.ConstantType; }
			}

			public override bool IsBinary {
				get { return this.Value.IsBinary; }
			}

			public override bool IsConstant {
				get { return this.Value.IsConstant; }
			}

			public override bool IsSizeof {
				get { return this.Value.IsSizeof; }
			}

			public override bool IsNull {
				get { return this.Value.IsNull; }
			}

			public override bool IsIsinst {
				get { return this.Value.IsIsinst; }
			}

			public override bool IsUnary {
				get { return this.Value.IsUnary; }
			}

			public override bool IsVariable {
				get { return this.Value.IsVariable; }
			}

			public override BoxedExpression UnaryArgument {
				get { return this.Value.UnaryArgument; }
			}

			public override UnaryOperator UnaryOperator {
				get { return this.Value.UnaryOperator; }
			}

			public override object UnderlyingVariable {
				get { return this.Value.UnderlyingVariable; }
			}
		}
}


using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;
using Mono.CodeContracts.Static.AST.Visitors;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class BinaryExpression : BoxedExpressions
		{
			public readonly BoxedExpression Left;
			public readonly BinaryOperator Op;
			public readonly BoxedExpression Right;

			public BinaryExpression (BinaryOperator op, BoxedExpression left, BoxedExpression right)
			{
				this.Op = op;
				this.Left = left;
				this.Right = right;
			}

			public override bool IsBinary {
				get { return true; }
			}

			public override BoxedExpression BinaryLeftArgument {
				get { return this.Left; }
			}

			public override BoxedExpression BinaryRightArgument {
				get { return this.Right; }
			}

			public override BinaryOperator BinaryOperator {
				get { return this.Op; }
			}

			public override bool IsBinaryExpression (out BinaryOperator op, out BoxedExpression left, out BoxedExpression right)
			{
				op = this.Op;
				left = this.Left;
				right = this.Right;
				return true;
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Left.AddFreeVariables (set);
				this.Right.AddFreeVariables (set);
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				BoxedExpression left = this.Left.Substitute (what, replace);
				BoxedExpression right = this.Right.Substitute (what, replace);
				if (left == this.Left && right == this.Right)
					return this;

				return new BinaryExpression (this.Op, left, right);
			}

			public override BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression left = this.Left.Substitute (map);
				if (left == null)
					return null;

				BoxedExpression right = this.Right.Substitute (map);
				if (right == null)
					return null;

				if (this.Left == left && this.Right == right)
					return this;
				return new BinaryExpression (this.Op, left, right);
			}

			public override Result ForwardDecode<Data, Result, Local, Parametr,Field, Method, Type, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Binary(pc, this.Op, Dummy.Value, Dummy.Value, Dummy.Value, data);
			}
		}
}


using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.DataStructures;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class UnaryExpression : BoxedExpression
		{
			public readonly BoxedExpression Argument;
			public readonly UnaryOperator Op;

			public UnaryExpression (UnaryOperator op, BoxedExpression argument)
			{
				this.Op = op;
				this.Argument = argument;
			}

			public override bool IsUnary {
				get { return true; }
			}

			public override BoxedExpression UnaryArgument {
				get { return this.Argument; }
			}

			public override UnaryOperator UnaryOperator {
				get { return this.Op; }
			}

			public override bool IsUnaryExpression (out UnaryOperator op, out BoxedExpression argument)
			{
				op = this.Op;
				argument = this.Argument;
				return true;
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.Argument.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression argument = this.Argument.Substitute (map);
				if (argument == this.Argument)
					return this;
				if (argument == null)
					return null;

				return new UnaryExpression (this.Op, argument);
			}

			protected internal override BoxedExpression RecursiveSubstitute (BoxedExpression what, BoxedExpression replace)
			{
				BoxedExpression argument = this.Argument.Substitute (what, replace);

				if (argument == this.Argument)
					return this;

				return new UnaryExpression (this.Op, argument);
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Unary (pc, this.Op, false, Dummy.Value, Dummy.Value, data);
			}

			public override bool Equals (object obj)
			{
				if (this == obj)
					return true;

				var unary = obj as UnaryExpression;
				return unary != null && this.Op == unary.Op && this.Argument.Equals (unary.Argument);
			}

			public override int GetHashCode ()
			{
				return this.Op.GetHashCode () * 13 + (this.Argument == null ? 0 : this.Argument.GetHashCode ());
			}

		}
}


using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class IsinstExpression : BoxedExpression
		{
			private readonly BoxedExpression arg;
			private readonly TypeNode type;

			public IsinstExpression (BoxedExpression boxedExpression, TypeNode type)
			{
				this.arg = boxedExpression;
				this.type = type;
			}

			public override bool IsIsinst {
				get { return true; }
			}

			public override BoxedExpression UnaryArgument {
				get { return this.arg; }
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Isinst (pc, this.type, Dummy.Value, Dummy.Value, data);
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
				this.arg.AddFreeVariables (set);
			}

			public override BoxedExpression Substitute<Variable> (Func<Variable, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression arg = this.arg.Substitute (map);
				if (arg == this.arg)
					return this;
				if (arg == null)
					return null;

				return new IsinstExpression (arg, this.type);
			}
		}
}


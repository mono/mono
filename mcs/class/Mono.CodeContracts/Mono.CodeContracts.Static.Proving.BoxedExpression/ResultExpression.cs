using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class ResultExpression : BoxedExpression
		{
			private const string ContractResultTemplate = "Contract.Result<{0}>()";
			public readonly TypeNode Type;

			public ResultExpression (TypeNode type)
			{
				this.Type = type;
			}

			public override bool IsResult {
				get { return true; }
			}

			public override void AddFreeVariables (HashSet<BoxedExpression> set)
			{
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				return this;
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.LoadResult (pc, this.Type, Dummy.Value, Dummy.Value, data);
			}
		}
}


using System;
using System.Collections.Generic;
using Mono.CodeContracts.Static.AST;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class SizeOfExpression : BoxedExpression
		{
			public readonly int SizeAsConstant;
			public readonly TypeNode Type;

			public SizeOfExpression (TypeNode type, int sizeAsConstant)
			{
				this.Type = type;
				this.SizeAsConstant = sizeAsConstant;
			}

			public SizeOfExpression (TypeNode type)
				: this (type, -1)
			{
			}

			public override bool IsSizeof {
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
				return visitor.Sizeof (pc, this.Type, Dummy.Value, data);
			}
		}
}


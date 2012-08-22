using System;
using Mono.CodeContracts.Static.ControlFlow;
using Mono.CodeContracts.Static.DataStructures;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public class AssertExpression : ContractExpression
		{
			public AssertExpression (BoxedExpression condition, EdgeTag tag, APC pc) : base (condition, tag, pc)
			{
			}

			public override Result ForwardDecode<Data, Result, Visitor> (PC pc, Visitor visitor, Data data)
			{
				return visitor.Assert (pc, this.Tag, Dummy.Value, data);
			}

			public override BoxedExpression Substitute<Variable1> (Func<Variable1, BoxedExpression, BoxedExpression> map)
			{
				BoxedExpression cond = this.Condition.Substitute (map);
				if (cond == this.Condition)
					return this;
				if (cond == null)
					return null;

				return new AssertExpression (cond, this.Tag, this.Apc);
			}
		}

}


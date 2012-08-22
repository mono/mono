using System;
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Inference
{
	class FactForOverflow<Var> : IFactForOverflow<BoxedExpression>
	{
		private readonly IFactQuery<BoxedExpression, Var> facts;
		
		public FactForOverflow (IFactQuery<BoxedExpression, Var> facts)
		{
			this.facts = facts;
		}
		
		public bool Overflow(APC pc, BoxedExpression exp)
	    {
	      FactForOverflow<Var>.CanOverflowVisitor canOverflowVisitor = new FactQueryForOverflow<Variable>.CanOverflowVisitor(pc, this.FactQuery);
	      exp.Dispatch((IBoxedExpressionVisitor) canOverflowVisitor);
	      return canOverflowVisitor.CanOverflow;
	    }
	}
}


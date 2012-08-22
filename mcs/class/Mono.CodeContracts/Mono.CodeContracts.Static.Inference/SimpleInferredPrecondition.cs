using System;
using Mono.CodeContracts.Static.Inference.Interface;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions;

namespace Mono.CodeContracts.Static.Inference
{
	public class SimpleInferredPrecondition : IInferredPrecond
	{
		public SimpleInferredPrecondition(BoxedExpression expr, ExpressionInPreStateKind kind, bool isSufficient)
	    {
	      this.expr = expr;
	      this.kind = kind;
	      this.IsSufficientForTheWarning = isSufficient;
	    }

		public BoxedExpression expr { get; private set; }

    	public ExpressionInPreStateKind kind { get; private set; }

    	public bool IsSufficientForTheWarning { get; private set; }

		public override string ToString()
	    {
	      return ((object) this.expr).ToString();
	    }
	}
}


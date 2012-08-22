using System;
using Mono.CodeContracts.Static.Proving;
using Mono.CodeContracts.Static.Analysis.ExpressionAnalysis.Expressions;

namespace Mono.CodeContracts.Static.Inference.Interface
{
	interface IInferredPrecond
	{
		BoxedExpression expr { get; }

    	ExpressionInPreStateKind kind { get; }

    	bool IsSufficientForTheWarning { get; }
	}
}


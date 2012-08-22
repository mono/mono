using System;

namespace Mono.CodeContracts.Static.Inference
{
	enum CodeFixKind
	{
		ArrayInitialization,
		ArrayOffByOne,
		ConstantInitialization,
		ExpressionInitialization,
		FloatingPointCast,
		MethodCallResult,
		MethodCallResultNoCode,
		NonOverflowingExpression,
		OffByOne,
		RemoveConstructor,
		StrengthenTest,
		Test,
		Assume,
	}
}


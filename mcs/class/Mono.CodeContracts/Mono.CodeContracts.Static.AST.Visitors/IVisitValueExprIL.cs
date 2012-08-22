using System;

namespace Mono.CodeContracts.Static.AST.Visitors
{
	internal interface IVisitValueExprIL<Label, Type, Expression, SymbolicValue, Data, Result> : IVisitExprIL<Label, Type, Expression, SymbolicValue, Data, Result>
	{
		Result SymbolicConstant (Label pc, SymbolicValue symbol, Data data);
	}
}


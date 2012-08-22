using System;
using Mono.CodeContracts.Static.AST.Visitors;
using Mono.CodeContracts.Static.Lattices;

namespace Mono.CodeContracts.Static.Analysis
{
	internal interface IExpressionContextData<Local, Parameter, Method, Field, Type, Expression, SymbolicValue> where Type : IEquatable<Type>
	{
		Expression Refine (APC pc, SymbolicValue value);

		SymbolicValue Unrefine (Expression expr);

		Result Decode<Data, Result, Visitor> (Expression expr, Visitor visitor, Data data) where Visitor : IVisitValueExprIL<Expression, Type, Expression, SymbolicValue, Data, Result>;

		FlatDomain<Type> GetType (Expression expr);

		APC GetPC (Expression expr);

		Expression For (SymbolicValue value);

		bool IsZero (Expression exp);

		bool TryGetArrayLength (Expression array, out Expression length);

		bool TryGetWritableBytes (Expression pointer, out Expression writableBytes);
	}
}


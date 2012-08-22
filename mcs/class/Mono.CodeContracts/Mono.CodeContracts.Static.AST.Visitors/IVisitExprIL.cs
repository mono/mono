using System;

namespace Mono.CodeContracts.Static.AST.Visitors
{
	internal interface IVisitExprIL<Label, Type, Source, Dest, Data, Result>
	{
		Result Binary (Label pc, BinaryOperator op, Dest dest, Source s1, Source s2, Data data);

		Result Box (Label pc, Type type, Dest dest, Source source, Data data);

		Result Isinst (Label pc, Type type, Dest dest, Source obj, Data data);

		Result Ldconst (Label pc, object constant, Type type, Dest dest, Data data);

		Result Ldnull (Label pc, Dest dest, Data data);

		Result Sizeof (Label pc, Type type, Dest dest, Data data);

		Result Unary (Label pc, UnaryOperator op, bool overflow, bool unsigned, Dest dest, Source source, Data data);
	}
}


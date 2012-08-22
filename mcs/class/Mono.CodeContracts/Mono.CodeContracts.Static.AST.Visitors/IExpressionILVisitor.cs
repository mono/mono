using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.AST.Visitors
{
		interface IExpressionILVisitor<Label, Type, Source, Dest, Data, Result>
		{
				Result Binary(Label pc, BinaryOperator bop, Dest dest, Source src1, Source src2, Data data);

				Result Unary(Label pc, UnaryOperator uop, bool unsigned, Dest dest, Source source, Data data);

				Result LoadNull(Label pc, Dest dest, Data polarity);

				Result LoadConst(Label pc, TypeNode type, object constant, Dest dest, Data data);

				Result Sizeof(Label pc, TypeNode type, Dest dest, Data data);

				Result Isinst(Label pc, TypeNode type, Dest dest, Source obj, Data data);
		}
}

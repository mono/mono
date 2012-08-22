using System;
using Mono.CodeContracts.Static.Analysis;

namespace Mono.CodeContracts.Static.Proving
{
	interface IFactForOverflow<Expression>
	{
		bool Overflow(APC pc, BoxedExpression expression);
		bool Underflow(APC pc, BoxedExpression expression);
	}
}


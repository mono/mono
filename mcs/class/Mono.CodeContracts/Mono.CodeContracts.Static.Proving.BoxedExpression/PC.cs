using Mono.CodeContracts.Static.Proving;

namespace Mono.CodeContracts.Static.Proving.BoxedExpressions
{
		public struct PC
		{
			public readonly int Index;
			public readonly BoxedExpression Node;

			public PC (BoxedExpression expr, int index)
			{
				this.Node = expr;
				this.Index = index;
			}
		}
}


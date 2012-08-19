using Mono.CodeContracts.Static.AST.Visitors;

namespace Mono.CodeContracts.Static.Providers {
	interface ICodeProvider<TLabel> {
		TResult Decode<TVisitor, TData, TResult> (TLabel pc, TVisitor visitor, TData data)
			where TVisitor : IAggregateVisitor<TLabel, TData, TResult>;

		bool Next (TLabel current, out TLabel nextLabel);
		int GetILOffset (TLabel current);
	}
}

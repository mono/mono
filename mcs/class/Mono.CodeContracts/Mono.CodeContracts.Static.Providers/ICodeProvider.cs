using Mono.CodeContracts.Static.AST.Visitors;

namespace Mono.CodeContracts.Static.Providers {
	interface ICodeProvider<Label> {
		Result Decode<CodeVisitor, Data, Result> (Label pc, CodeVisitor visitor, Data data)
			where CodeVisitor : IAggregateVisitor<Label, Data, Result>;

		bool Next (Label current, out Label nextLabel);
		int GetILOffset (Label current);
	}
}

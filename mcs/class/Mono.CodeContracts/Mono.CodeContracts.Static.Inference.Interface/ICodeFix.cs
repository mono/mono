using System;

namespace Mono.CodeContracts.Static.Inference.Interface
{
	interface ICodeFix
	{
		CodeFixKind Kind { get; }

		string Suggest ();

		string SuggestCode ();
	}
}


using System;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using Mono.CodeContracts.Static.ControlFlow;

namespace Mono.CodeContracts.Static.Analysis
{
	interface IOutput
	{
		IFrameworkLogOptions LogOptions { get; }

    	void WriteLine(string format, params object[] args);

    	void Write(string format, params object[] args);

    	void Suggestion(string kind, APC pc, string suggestion, List<uint> causes);

    	void EmitError(CompilerError error);
	}
}


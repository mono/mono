// CS3005: Identifier `I.this[int]' differing only in case is not CLS-compliant
// Line: 12
// Compiler options: -warnaserror

using System.Runtime.CompilerServices;
using System;

[assembly: CLSCompliant (true)]

public interface I {
	[IndexerName ("blah")]
	int this [int a] {
            get;
	}

 	int BLAH { get; }
}

// CS3005: Identifier `I.BLAH.get' differing only in case is not CLS-compliant
// Line: 16
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

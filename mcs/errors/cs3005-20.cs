// cs3005.cs: Identifier 'I.BLAH.get' differing only in case is not CLS-compliant
// Line: 15

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

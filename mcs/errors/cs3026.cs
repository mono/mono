// CS3026: CLS-compliant field `Class.V' cannot be volatile
// Line: 9
// Compiler options: -warn:1 -warnaserror

using System;
[assembly: CLSCompliant (true)]

public class Class {
	protected volatile int V;
	static void Main () {}
}

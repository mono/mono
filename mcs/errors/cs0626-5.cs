// CS0626: `Test.OnFoo' is marked as an external but has no DllImport attribute. Consider adding a DllImport attribute to specify the external implementation
// Line: 9
// Compiler options: -warnaserror -warn:1


using System;

public delegate void Handler ();

class Test {
	extern event Handler OnFoo;
}


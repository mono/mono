// CS3019: CLS compliance checking will not be performed on `CLSClass.Foo()' because it is not visible from outside this assembly
// Line: 8
// Compiler options: -warnaserror -warn:2

using System;
[assembly:CLSCompliant (true)]

public partial class CLSClass
{
	[CLSCompliant (false)]
	partial void Foo ();
	
	partial void Foo ()
	{
	}
}

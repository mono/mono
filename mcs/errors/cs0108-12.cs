// CS0108: `Libs.MyLib' hides inherited member `Foo.MyLib'. Use the new keyword if hiding was intended
// Line: 18
// Compiler options: -warnaserror -warn:2

using System;
using System.Runtime.InteropServices;
 
class Test {
	[DllImport (Libs.MyLib)]
	private static extern void foo ();
 
	public static void Main ()
	{
	}
}
 
class Libs : Foo {
	internal const string MyLib = "SomeLibrary";
}
class Foo {
	internal const string MyLib = "Foo";
}

// cs0108-12.cs: The keyword new is required on 'Libs.MyLib' because it hides inherited member
// Line: 18
// Compiler options: -warnaserror -warn:1

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

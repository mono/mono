using System;
using System.Runtime.InteropServices;
 
class Test {
	[DllImport (Libs.MyLib)]
	private static extern void foo ();
 
	public static void Main ()
	{
	}
}
 
class Libs {
	internal const string MyLib = "SomeLibrary";
}

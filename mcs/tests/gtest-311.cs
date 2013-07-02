// Compiler options: -t:library

using System;
using System.Runtime.InteropServices;

class A
{
	[DllImport ("Dingus", CharSet=CharSet.Auto)]
	extern static void Do<T> ();

	[DllImport ("Dingus")]
	extern static void Do2<T> ();

	public static void Main ()
	{

	}
}
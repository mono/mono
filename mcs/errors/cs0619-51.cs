// cs0619-51.cs: `A.comparer' is obsolete: `Please use ...'
// Line: 16
// Compiler options: -reference:CS0619-51-lib.dll

using System;
using System.Collections;

public class B : A
{
	void test ()
	{
	}
	
	public void AA ()
	{
		comparer += new D (test);
	}
	
	public static void Main () {}
}

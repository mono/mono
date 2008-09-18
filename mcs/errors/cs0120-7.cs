// CS0120: An object reference is required to access non-static member `MemRefMonoBug.String'
// Line: 11

using System;

public class MemRefMonoBug {
	private string String;	// this member has the same name as System.String class
	public static void Main ()
	{
		new MemRefMonoBug ().String = "";	// this line causes no problem
		String = "";	// mcs crashes in this line
	}
}


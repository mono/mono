// cs0214.cs : Pointers may only be used in an unsafe context
// Line : 14

using System;
using System.Reflection;

public class Blah {
	
	public static void Main ()
	{
		int* i;
		int foo = 10;
		
		i = &foo;
		
		Console.WriteLine ("The pointer value is " + i);
	}	
}

		

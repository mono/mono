// CS0029: Cannot implicitly convert type `System.TypedReference' to `object'
// Line: 10

using System;

class Test
{
	public static void Main ()
	{
		var res = default (TypedReference).ToString ();
	}
}
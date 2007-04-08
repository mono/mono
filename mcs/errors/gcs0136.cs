// CS0136: A local variable named `ii' cannot be declared in this scope because it would give a different meaning to `ii', which is already used in a `parent' scope to denote something else
// Line: 14
// Compiler options: -langversion:linq

using System;
using System.Collections.Generic;
using System.Linq;

class Test
{
	public static void Main ()
	{
		int[] int_array = null;
		int ii = 0;
		IEnumerable<int> e = from int ii in int_array select "1";
	}
}

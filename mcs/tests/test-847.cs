using System;
using System.Collections.Generic;

interface IA
{
	int this[int arg] { get; set; }
}

interface I : IA
{
}

class C
{
	public static int Main ()
	{
		var attrs = typeof (I).GetCustomAttributes (false);

		// No DefaultMemberAttribute needed
		if (attrs.Length != 0)
			return 1;

		return 0;
	}
}
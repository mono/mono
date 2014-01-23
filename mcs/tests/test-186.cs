using System;

namespace TestBUG
{
	public class myAttribute : Attribute
	{
		public myAttribute(string p1, string p2, string p3, int p4) {}
	}

	//
	// Typecasts on attributes, fix for bug 37363
	//
	[myAttribute("stringArgument", (String)null, (String)null, 2)]
	public class Test
	{

		public static int Main  ()
		{
			return 0;
		}
	}
}







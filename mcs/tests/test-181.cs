//
// test-181.cs: Test whenever mcs correctly handles the MethodImplAttributes
// custom attribute.
//

using System;
using System.Reflection;
using System.Runtime.CompilerServices;

class Test
{
	[MethodImplAttribute(MethodImplOptions.Synchronized)]
	public void test ()
	{
	}

	public static int Main ()
	{
		MethodImplAttributes iflags = typeof (Test).GetMethod ("test").GetMethodImplementationFlags ();
		return ((iflags & MethodImplAttributes.Synchronized) != 0 ? 0 : 1);
	}
}
		
		

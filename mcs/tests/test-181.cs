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

	[MethodImplAttribute((short)MethodImplOptions.Synchronized)]
	public void test2 ()
	{
	}

	[MethodImplAttribute((byte)32)]
	public void test3 ()
	{
	}

	[MethodImpl(MethodImplOptions.NoInlining | MethodImplOptions.NoOptimization)]
	public void test4 ()
	{
	}

	public static int Main ()
	{
		MethodImplAttributes iflags;
		iflags = typeof (Test).GetMethod ("test").GetMethodImplementationFlags ();
		if ((iflags & MethodImplAttributes.Synchronized) == 0)
			return 1;

		iflags = typeof (Test).GetMethod ("test2").GetMethodImplementationFlags ();
		if ((iflags & MethodImplAttributes.Synchronized) == 0)
			return 2;

		iflags = typeof (Test).GetMethod ("test3").GetMethodImplementationFlags ();
		if ((iflags & MethodImplAttributes.Synchronized) == 0)
			return 3;

		iflags = typeof (Test).GetMethod ("test3").GetMethodImplementationFlags ();
		if ((iflags & MethodImplAttributes.Synchronized) == 0)
			return 4;

		return 0;
	}
}
		
		

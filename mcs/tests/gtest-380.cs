using System.Reflection;
using System;

class GTest<T>
{
	public static volatile string str = "Hello";
}

class Test
{
	public volatile int field;

	public static int Main ()
	{
		FieldInfo fi = typeof (Test).GetField ("field");
		if (fi.GetCustomAttributes (true).Length != 0)
			return 1;
		
		Type[] t = fi.GetRequiredCustomModifiers ();
		if (t.Length != 1)
			return 2;
		
		if (t [0] != typeof (System.Runtime.CompilerServices.IsVolatile))
			return 3;

		fi = typeof (GTest<>).GetField ("str");
		if (fi.GetCustomAttributes (true).Length != 0)
			return 10;
		
		t = fi.GetRequiredCustomModifiers ();
		if (t.Length != 1)
			return 11;
		
		if (t [0] != typeof (System.Runtime.CompilerServices.IsVolatile))
			return 12;

		Console.WriteLine ("OK");
		return 0;
	}
}

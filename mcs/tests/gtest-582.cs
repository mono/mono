// Compiler options: -r:gtest-582-lib.dll

using System;
using System.Reflection;

public class G1 : GC<C>
{
}

public class GC<T> where T : C
{
}

class Program
{
	public static int Main()
	{
		var constraints = typeof (GC<>).GetGenericArguments ()[0].GetGenericParameterConstraints ();
		if (constraints.Length != 1)
			return 1;
		if (constraints [0] != typeof (C))
			return 2;

		Console.WriteLine ("ok");
		return 0;
	}
}
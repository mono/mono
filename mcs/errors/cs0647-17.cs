// Cs0647: Error during emitting `System.Reflection.AssemblyVersionAttribute' attribute. The reason is `Specified version `0.0.0.80420' is not valid'
// Line: 7

using System;
using System.Reflection;

[assembly: AssemblyVersion ("0.0.0.80420")]

public class Test {

	public static int Main ()
	{
		return 1;
	}
}

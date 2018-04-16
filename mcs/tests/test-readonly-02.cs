// Compiler options: -langversion:latest
using System;

public delegate ref readonly int D (int x);

class X
{
	public static void Main ()
	{

	}

	Guid g;

	ref readonly Guid TestMethod ()
	{
		return ref g;
	}

	ref readonly Guid TestProp {
		get {
			ref readonly var rg = ref g;
			return ref rg;
		}
	}	

}
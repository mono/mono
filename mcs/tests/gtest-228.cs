using System;

[AttributeUsage(AttributeTargets.GenericParameter)]
class GenParAttribute : Attribute
{
}

class cons <[GenPar] A, [GenPar] B>
{
	public void abc <[GenPar] M> ()
	{
	}
}

class Test
{
	public static void Main ()
	{
	}
}

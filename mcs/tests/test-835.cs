using C = A.D;
using System;

class AA
{
	internal class D : Exception { }
}

class A : AA
{
	public static void Main()
	{
		object o = new C();
	}
}

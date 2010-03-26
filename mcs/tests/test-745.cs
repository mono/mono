using System;

interface ITest
{
}

class C : ITest
{
	public static void Main ()
	{
		ITest it = new C ();
		it.GetType ();

		IConvertible ic = 1 as IConvertible;
		var t = ic.GetType ();
	}
}

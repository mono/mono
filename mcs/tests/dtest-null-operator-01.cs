using System;
using System.Collections.Generic;
using System.Linq;
 
class X
{
	public string Prop;
	public A A = new A ();
}

class A
{
	public string B;
}

class MainClass
{
	static void NullCheckTest ()
	{
		dynamic dyn = null;
		dynamic res;

		res = dyn?.ToString ();
		res = dyn?.GetHashCode ();
		res = dyn?.DD.Length?.GetHashCode ();

		dyn?.ToString ();

		res = dyn?.Prop;
		res = dyn?.Prop?.Prop2;
		res = dyn?[0];
	}

	static void Test_1 ()
	{
		dynamic dyn = new X ();
		dynamic res;

		res = dyn.Prop?.Length;
		res = dyn.A.B?.C.D?.E.F;
	}

	static dynamic Test_2 (IEnumerable<dynamic> collection)
	{
		return collection?.FirstOrDefault ().Length;
	}	

	public static void Main ()
	{
		NullCheckTest ();

		Test_1 ();
		Test_2 (null);
	}
}

 
using System;
using System.Linq;

class A
{
	public int Value;
}

class C
{
	A[] Prop {
		get {
			return new A [1] { new A () };
		}
	}
	
	void Test ()
	{
		int i = 9;
		var c = new C ();
		var r = Prop.Select (l => l.Value).ToArray ();
	}
	
	public static int Main ()
	{
		new C().Test ();
		return 0;
	}
}

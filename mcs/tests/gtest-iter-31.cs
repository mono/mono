using System.Collections.Generic;
using System.Linq;

class B
{
	public object Foo (object obj)
	{
		return null;
	}
}

class C
{
	B ctx = new B ();

	public static void Main ()
	{
		foreach (var c in new C ().Test ()) {			
		}
	}

	IEnumerable<ushort> Test ()
	{
		string[] s = new[] { "a", "b", "c" };

		var m = s.Select (l => ctx.Foo (l)).ToArray ();

		yield break;
	}
}


using System;

class Test
{
	delegate int D (ref int i);

	public dynamic Foo;

	public static int Main ()
	{
		dynamic d = new Test ();

		d.Foo = (Func<int, int>) (l => 4 + l);

		var r1 = d.Foo (3);
		if (r1 != 7)
			return 1;

		d.Foo (2);

		d.Foo = (Action) (() => Console.WriteLine ("action"));
		d.Foo ();

		d.Foo = (D) ((ref int l) => { l = 9; return 4; });

		int ref_value = 3;
		var r2 = d.Foo (ref ref_value);
		if (r2 != 4)
			return 2;

		if (ref_value != 9)
			return 3;

		Console.WriteLine ("ok");
		return 0;
	}
}

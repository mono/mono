using System;

class TupleConversions
{
	public static void Main ()
	{
		object oarg = 4;
		(sbyte v1, long v2) t1 = (-1, 2);
		var t2 = (-1, 2);

		IComparable o = (x1: "a", x2: 1.ToString ());

		var arg = (x1: 1, x2: 1.ToString ());
		if (arg.x2 != "1")
			return;

		Foo ((x1: (oarg, 'v'), x2: 1.ToString ()));

		Test3 (ValueTuple.Create (1, "2"));

		(int v1, string v2) y = (1, null);

		(int v1, Action v2) y2 = (1, Main);
		(int v1, Action v2) y3 = (ValueTuple<int, Action>) (1, Main);

		(string v1, object v2) b = ("a", "b");

		(int v1, long v2)? x = null;

        var array = new [] {
            (name: "A", offset: 0),
            (name: "B", size: 4)
        };		
	}

	static void Foo<T> (T arg)
	{		
	}

	static void Test3 ((long a, object b) arg)
	{
	}
}

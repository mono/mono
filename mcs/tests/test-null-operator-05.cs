using System;

class CI
{
	public string this [string i] { set { } get { return ""; } }
	public int? this [int i] { set { } get { return 1; } }
}

class C
{
	static int TestArrayAccess ()
	{
		byte[] arr = null;
		var v = arr? [0];
		if (v != null)
			return 1;

		long?[] ar2 = null;
		var v2 = ar2? [-1];
		if (v2 != null)
			return 2;

		var v3 = arr? [0].GetHashCode () ?? 724;
		if (v3 != 724)
			return 3;

// TODO: Disabled for now?
//        arr? [0] += 2;
		return 0;
	}

	static int TestIndexerAccess ()
	{
		CI ci = null;
		var v = ci? ["x"];
		if (v != null)
			return 1;

		var v2 = ci? [0];
		if (v2 != null)
			return 2;

		var v3 = ci? [0].GetHashCode () ?? 724;
		if (v3 != 724)
			return 3;

// TODO: Disabled for now?
//       ci? [0] += 3;
		return 0;
	}

	static int Main ()
	{
		int res;
		res = TestArrayAccess ();
		if (res != 0)
			return 10 + res;

		res = TestIndexerAccess ();
		if (res != 0)
			return 20 + res;

		Console.WriteLine ("ok");
		return 0;
	}
}
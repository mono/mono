public class C
{
	static int Main ()
	{
		string x = null;
		var t1 = x?.ToString ().ToString ().ToString () ?? "t1";
		if (t1 != "t1")
			return 1;

		var t2 = x?.ToString ().ToString ()?.ToString () ?? "t2";
		if (t2 != "t2")
			return 2;

		var t3 = x?.ToString ()?.ToString ()?.ToString () ?? "t3";
		if (t3 != "t3")
			return 3;

		var t4 = x?.ToString ().GetHashCode () ?? 9;
		if (t4 != 9)
			return 4;

		var t5 = x?.ToString ()?.GetHashCode () ?? 8;
		if (t5 != 8)
			return 5;

		var t6 = x?.ToString().Length;
		if (t6 != null)
			return 6;

		return 0;
	}
}
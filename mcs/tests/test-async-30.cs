using System;
using System.Collections.Generic;
using System.Threading.Tasks;

class C
{
	private IEnumerable<string> Test (string s)
	{
		Func<Task<string>> a = async () => await Task.FromResult(s + "a");
		yield return a ().Result;
	}
	
	private IEnumerable<string> Test2 ()
	{
		var s = "bb";
		Func<Task<string>> a = async () => await Task.FromResult(s + "a");
		yield return a ().Result;
	}

	public static int Main ()
	{
		var c = new C ();
		string res = "";
		foreach (var e in c.Test ("tt"))
			res += e;
		
		if (res != "tta")
			return 1;
		
		res = "";
		foreach (var e in c.Test2 ())
			res += e;
		
		if (res != "bba")
			return 2;

		return 0;
	}
}

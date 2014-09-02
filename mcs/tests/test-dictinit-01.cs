using System;
using System.Collections.Generic;

class Program
{
	static int Main ()
	{
		var c1 = new C {
			["aaa"] = 12,
		};

		if (c1.Dict ["aaa"] != 12)
			return 1;

		var c2 = new C {
			["a1"] = 5,
			["a2"] = 10,
			Value = 20,
		};

		if (c2.Dict ["a1"] != 5)
			return 2;

		if (c2.Dict ["a2"] != 10)
			return 3;

		if (c2.Value != 20)
			return 4;

		return 0;
	}
}


class C
{
	public Dictionary<string, int> Dict = new Dictionary<string, int> ();

	public int Value;

	public int this [string arg] {
		get {
			return Dict [arg];
		}
		set {
			Dict [arg] = value;
		}
	}
}
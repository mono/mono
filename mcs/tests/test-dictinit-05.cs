using System.Collections.Generic;

class A {
	public A ()
	{
		Info = new Dictionary<string, int>();
	}

	public Dictionary<string, int> Info { get; set; }
}

class X
{
	public static void Main ()
	{
		var x = new A () {
			Info = { 
				["x"] = 1,
				["y"] = 2
			}
		};
	}
}
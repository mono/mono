using System;
using System.Collections.Generic;

class Program
{
	static int Main ()
	{
		var c = new C {
			["l1"] = new C {
				["l2"] = new C () {
					Value = 10
				}
			},
			["l5"] = {
				["51"] = new C () {
					Value = 100
				}
			}
		};

		if (c ["l1"]["l2"].Value != 10)
			return 1;

		if (c ["l5"]["51"].Value != 100)
			return 2;

		return 0;
	}
}


class C
{
	public Dictionary<string, C> Dict = new Dictionary<string, C> ();

	public int Value;

	public C this [string arg] {
		get {
			C c;
			if (!Dict.TryGetValue (arg, out c)) {
				c = new C ();
				Dict [arg] = c;
			}

			return c;
		}
		set {
			Dict [arg] = value;
		}
	}
}
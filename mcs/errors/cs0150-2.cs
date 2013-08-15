// CS0150: A constant value is expected
// Line : 14

using System;

public class Blah
{
	static readonly string Test;
	
	public static void Main ()
	{
		string s = null;
		switch (s) {
			case Blah.Test:
				break;
		}
	}
}

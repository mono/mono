using System;

public class Test
{
	byte Prop {
		get { return 4; }
		set { }
	}

	byte this [int arg] {
		get { return 2; }
		set { }
	}

	public static int Main ()
	{
		dynamic v = 'a';
		dynamic a = new Test ();

		string s = "-sdfas";
		
		// dynamic compound assignment with different result type
		v += s;

		if (v != "a-sdfas")
			return 1;
		
		dynamic d2 = null;
		d2 += "a";
		if (d2 != "a")
			return 2;

		byte b = 4;
		a.Prop *= b;
		a[4] ^= b;
		
		dynamic d = 1;
		b = byte.MaxValue;
		try {
			checked {
				b += d;
				return 1;
			}
		} catch (OverflowException) {
		}
		
		b += d;

		return 0;
	}
}
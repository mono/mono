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
	
	byte Byte = 200;
	static dynamic V;
	dynamic DynamicByte;
	byte[] ByteArray = { 1, 100 };

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
				return 3;
			}
		} catch (OverflowException) {
		}
			
		b += d;
		
		try {
			checked {
				a.Byte += 100;
				return 4;
			}
		} catch (OverflowException) {
		}
		
		a.Byte += 100;
		
		checked {
			d = byte.MaxValue;
			d += 100;
		}

		checked {
			V = byte.MaxValue;
			V -= 300;
		}

		var t = new Test ();
		t.DynamicByte = byte.MaxValue;
		d = t;
		checked {
			d.DynamicByte -= 500;
		}
		
		if (t.DynamicByte != -245)
			return 5;

		try {
			checked {
				d.ByteArray[1] += 200;
				return 6;
			}			
		} catch (OverflowException) {
		}

		return 0;
	}
}

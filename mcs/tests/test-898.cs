// Compiler options: -unsafe
 
using System;

class BoolArrayWithByteValues
{

	static int Foo (ref bool b)
	{
		bool b2 = true;
		bool r;
		r = b == true;
		if (!r)
			return 10;

		r = b == b2;
		if (!r)
			return 11;

		return 0;
	}

	static unsafe bool Ptr ()
	{
		bool rv;
	
		var arr = new byte [256];
		for (int i = 0; i < arr.Length; i++)
			arr [i] = (byte) i;
		fixed (byte* bptr = arr) {
			rv = true;
			for (int i = 0; i < arr.Length; i++) {
				bool* boptr = (bool*)(bptr + i);
				if (arr[i] > 0 && !*boptr)
					rv = false;
				System.Console.WriteLine ("#{0} = {1}", i, *boptr);
			}
		}

		return rv;
	}

	static int Main()
	{
		var a = new bool[1];
		Buffer.SetByte (a, 0, 5);

		var b = true;
		bool r;
		r = a [0];
		if (!r)
			return 1;

		r = a [0] == true;
		if (!r)
			return 2;

		r = a [0] == b;
		if (!r)
			return 3;

		r = a [0] != false;
		if (!r)
			return 4;

		r = a [0] != b;
		if (r)
			return 5;

		var res = Foo (ref a [0]);
		if (res != 0)
			return res;

		if (!Ptr ())
			return 6;

		return 0;
	}
}
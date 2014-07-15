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

		return 0;
	}
}
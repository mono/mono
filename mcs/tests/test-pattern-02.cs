// Compiler options: -langversion:experimental

using System;

class ConstantPattern
{
	public static int Main ()
	{
		bool b4 = false;
		b4 = !b4;
		object o = "x";
		bool r1 = o is "y";
		if (r1)
			return 1;

		r1 = o is "x";
		if (!r1)
			return 2;

		string s = "o";
		if (s is null)
			return 3;

		if (s is "oo")
			return 4;

		if (!(s is "o"))
			return 5;

		int? o3 = 4;
		bool r3 = o3 is null;
		if (r3)
			return 6;
		
		r3 = o3 is 4;
		if (!r3)
			return 7;

		object o4 = (byte?)255;
		if (!(o4 is +255))
			return 8;

		if (o4 is null)
			return 9;

		object o5 = (double)-255;
		if (!(o5 is -255))
			return 10;

		Console.WriteLine ("ok");
		return 0;
	}
}
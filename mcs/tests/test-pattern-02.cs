// Compiler options: -langversion:experimental

using System;

enum MyEnum : short
{
	V_4 = 4
}

class ConstantPattern
{
	static bool Generic<T> (T t) where T : class
	{
		return t is default (T);
	}

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
		var ggg = o4 is 255;
		if (ggg)
			return 8;

		if (o4 is null)
			return 9;

		object o5 = (double)-255;
		if (o5 is -byte.MaxValue)
			return 10;

		object o6 = MyEnum.V_4;
		bool r4 = o6 is 4;
		if (r4)
			return 11;

		r4 = o6 is MyEnum.V_4;
		if (!r4)
			return 12;

		ConstantPattern o7 = new ConstantPattern ();
		if (!(o7 is ConstantPattern))
			return 13;

		if (!(o7 is object))
			return 14;

		object o8 = true;
		if (o8 is false)
			return 15;

		if (!(o8 is true))
			return 16;

		if (Generic (""))
			return 17;

		if (!Generic<Delegate> (null))
			return 18;

		Console.WriteLine ("ok");
		return 0;
	}
}
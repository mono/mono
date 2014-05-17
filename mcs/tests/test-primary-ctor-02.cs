using System;

partial class Part
{
	public Part (string s)
		: this (5)
	{
		if (arg != 5)
			throw new ApplicationException ("1");

		if (Property != 12)
			throw new ApplicationException ("2");
	}
}

partial class Part(int arg)
{
	int field = 7;

	int Property {
		get {
			return arg + field;
		}
	}

	public static int Main ()
	{
		var p = new Part ("5");
		if (p.Property != 12)
			return 1;

		return 0;
	}
}
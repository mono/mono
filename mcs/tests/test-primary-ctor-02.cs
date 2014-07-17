using System;

partial class Part
{
	public Part (string s)
		: this (5)
	{
	}
}

partial class Part(int arg)
{
	static int field = 7;

	int Property { get; } = arg + field;

	{
		if (arg != 5)
			throw new ApplicationException ("1");

		if (Property != 12)
			throw new ApplicationException ("2");
	}

	public static int Main ()
	{
		var p = new Part ("5");
		if (p.Property != 12)
			return 1;

		return 0;
	}
}
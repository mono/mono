using System;

internal class ClassFormatError
{
	internal ClassFormatError(string msg, params object[] p)
	{
	}

	public static void Main ()
	{ }
}

internal class UnsupportedClassVersionError : ClassFormatError
{
	internal UnsupportedClassVersionError(string msg)
		: base(msg)
	{
	}
}

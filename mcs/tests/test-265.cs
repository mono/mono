using System;

internal class ClassFormatError
{
	internal ClassFormatError(string msg, params object[] p)
	{
	}

	static void Main ()
	{ }
}

internal class UnsupportedClassVersionError : ClassFormatError
{
	internal UnsupportedClassVersionError(string msg)
		: base(msg)
	{
	}
}

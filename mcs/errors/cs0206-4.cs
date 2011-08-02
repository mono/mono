// CS0206: A property, indexer or dynamic member access may not be passed as `ref' or `out' parameter
// Line: 16

using System;

public class Test
{
	public static void WriteOutData (out dynamic d)
	{
		d = 5.0;
	}

	public static void Main (string[] args)
	{
		dynamic d = null;
		WriteOutData (out d.Foo);
	}
}


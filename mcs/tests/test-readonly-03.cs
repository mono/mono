// Compiler options: -langversion:latest
using System;

readonly struct S
{
	static S shared = new S ();

	public S (int arg)
	{
		this = shared;
	}

	public static void Main ()
	{
	}
}
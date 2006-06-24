// Compiler options: -t:library

using System;

public class C
{
	public class CC {}

	public static string Print ()
	{
		return typeof (C).FullName;
	}
}
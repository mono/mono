// Compiler options: -warnaserror

using System;
using System.Collections.Generic;

[assembly: CLSCompliant (true)]

public class A
{
	public static void Main ()
	{
	}

	public void CLSCompliantMethod (dynamic[] parameter)
	{
	}

	public void CLSCompliantMethod (IEnumerable<dynamic> parameter)
	{
	}
}
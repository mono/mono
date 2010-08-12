using System;
using System.Collections.Generic;

static class Program
{
	public static IEnumerable<dynamic> D1 ()
	{
		yield break;
	}

	public static IEnumerable<Func<dynamic>> D2 ()
	{
		yield break;
	}

	static void Main ()
	{
	}
}
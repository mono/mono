using System;
using System.Collections.Generic;

public class Test
{
	public IEnumerable<int> TestMethod ()
	{
		try {

		} catch (Exception ex) {
			throw;
		}
		yield break;
	}

	public static void Main ()
	{
	}
}

using System;
using System.Collections.Generic;

public class Program
{
	public static int Main ()
	{
		var x = $@"({
				new Dictionary<int, object> {
					{ 1, "bbb" }
				}.Count
				});";

		if (x != "(1);")
			return 1;

		return 0;
	}
}
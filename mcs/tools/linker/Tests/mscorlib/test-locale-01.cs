using System;
using System.Globalization;
using System.Collections.Generic;
using System.Threading;

public class C
{
	public static unsafe int Main ()
	{
		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo ("cs-CZ");
		DictComparer ();

		CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo ("en-US");
		DictComparer ();

		return 0;
	}

	static void DictComparer ()
	{
		var n1 = "SEARCHFIELDS";
		var n2 = "Searchfields";

		if (!string.Equals (n1, n2, StringComparison.OrdinalIgnoreCase))
			throw new ApplicationException ("string equality");

		var dict = new Dictionary<string, string> (StringComparer.OrdinalIgnoreCase);
		dict [n1] = "test";

		string result;
		if (!dict.TryGetValue (n2, out result))
			throw new ApplicationException ("dictionary value");
	}
}
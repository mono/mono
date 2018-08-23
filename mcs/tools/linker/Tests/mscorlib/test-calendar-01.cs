using System;
using System.Globalization;

public class C
{
	public static unsafe int Main ()
	{
		var ci = CultureInfo.GetCultureInfo ("ar");

		// FIXME:
//		if (ci.Calendar.ToString () != "System.Globalization.UmAlQuraCalendar")
//			return 1;

		return 0;
	}
}
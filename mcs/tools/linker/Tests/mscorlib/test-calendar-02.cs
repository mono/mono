// Linker options has to be: -l none
using System;
using System.Globalization;

public class C
{
	public static unsafe int Main ()
	{
		var ci = CultureInfo.GetCultureInfo ("ps");

		// Should return System.Globalization.HijriCalendar without linker
		if (ci.Calendar.ToString () != "System.Globalization.GregorianCalendar")
			return 1;

		return 0;
	}
}
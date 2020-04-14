using System;
using System.Globalization;
namespace DebuggerTests {
	public class DateTimeTest {
		public static void LocaleTest (string locale)
		{
			CultureInfo.CurrentCulture = new CultureInfo (locale, false);
			Console.WriteLine("CurrentCulture is {0}", CultureInfo.CurrentCulture.Name);

			DateTime dt = new DateTime (2020, 1, 2, 3, 4, 5);
			Console.WriteLine("Current time is {0}", dt.ToString());
		}
	}
}
using System;

public class TestClass
{
	public static bool Test_1 ()
	{
		DayOfWeek? testEnum = DayOfWeek.Monday;
		switch (testEnum) {
			case DayOfWeek.Monday:
				return true;
		}
		return false;
	}
	
	public static bool Test_2 ()
	{
		DayOfWeek? testEnum = null;
		switch (testEnum) {
			case DayOfWeek.Monday:
				return false;
			case null:
				return true;
			default:
				return false;
		}
	}

	public static bool Test_3 ()
	{
		DayOfWeek? testEnum = null;
		switch (testEnum) {
			case DayOfWeek.Monday:
				return false;
			default:
				return true;
		}
	}

	public static bool Test_4 ()
	{
		DayOfWeek? testEnum = DayOfWeek.Monday;
		switch (testEnum) {
		}
		return true;
	}
	
	public static bool Test_5 ()
	{
		DayOfWeek? testEnum = DayOfWeek.Wednesday;
		switch (testEnum) {
			case DayOfWeek.Monday:
				return false;
			case DayOfWeek.Wednesday:
				goto case null;
			case null:
				return true;
			default:
				return false;
		}
	}
	
	static int Test_6 ()
	{
		switch ((int?)null){
		case 1:
			break;
		case null:
			return 1;
		default:
			return 3;
		}
		return 2;
	}
	
	public static int Main()
	{
		if (!Test_1 ())
			return 1;
		if (!Test_2 ())
			return 2;
		if (!Test_3 ())
			return 3;
		if (!Test_4 ())
			return 4;
		if (!Test_5 ())
			return 5;
		if (Test_6 () != 1)
			return 6;

		Console.WriteLine ("OK");
		return 0;
	}
}
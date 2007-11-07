using System;

class TestThing
{
	public int SetEnum (string a, Enum b)
	{
		return 0;
	}
	public int SetEnum (int a, Enum b)
	{
		return 1;
	}
}

class Test
{
	public static int Main (string [] args)
	{
		DayOfWeek? e = DayOfWeek.Monday;
		TestThing t = new TestThing ();
		return t.SetEnum ("hi", e);
	}
}
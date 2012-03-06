using System;
using System.Runtime.CompilerServices;

class CallerLineNumberTest
{
	object field = TraceStatic (6);
	
	CallerLineNumberTest ()
		: this (TraceStatic (9))
	{
	}
	
	CallerLineNumberTest (object arg)
	{
	}
	
	static void TraceStatic2([CallerLineNumber] double line = -1, [CallerLineNumber] decimal line2 = -1)
	{
	}
	
	public static object TraceStatic(int expected, [CallerLineNumber] int line = -1)
	{
		Console.WriteLine (line);
		
		if (expected != line)
			throw new ApplicationException (string.Format ("`{0}' !=  `{1}'", expected, line));
		
		return line;
	}
	
	public static void Main ()
	{
		var c = new CallerLineNumberTest ();
		TraceStatic (34);
		TraceStatic2 ();
		
		Action a = () => TraceStatic (37);
		a ();
	}
}
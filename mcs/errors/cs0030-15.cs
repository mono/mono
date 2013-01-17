// CS0030: Cannot convert type `long' to `System.DayOfWeek?'
// Line: 10

using System;

class C
{
    static void Main ()
    {
		var dow = (DayOfWeek?) long.MaxValue;
    }
}
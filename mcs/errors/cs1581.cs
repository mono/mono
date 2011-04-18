// CS1581: Invalid return type in XML comment cref attribute `explicit operator intp (Test)'
// Line: 7
// Compiler options: -doc:dummy.xml -warnaserror -warn:1

using System;
/// <seealso cref="explicit operator intp (Test)"/>
public class Test
{
	/// operator.
	public static explicit operator int (Test t)
	{
		return 0;
	}
}


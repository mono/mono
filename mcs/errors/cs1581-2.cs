// Compiler options: -doc:dummy.xml -warnaserror -warn:1
using System;
/// <seealso cref="explicit operator int (Test)"/>
public class Test
{
	/// operator.
	public static explicit operator int [] (Test t)
	{
		return new int [0];
	}
}


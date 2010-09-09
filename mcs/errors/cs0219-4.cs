// CS0219: The variable `e' is assigned but its value is never used
// Line: 12
// Compiler options: -warn:3 -warnaserror

using System;
public class ConsoleStub
{
	public static void Main()
	{
		try {
		} catch (Exception e) {
			e = null;
		}
	}
}


// CS0162: Unreachable code detected
// Line: 12
// Compiler options: -warnaserror

using System;

public class Driver
{
	public static void Main ()
	{
		int yyTop = 0;
		for (; ; ++yyTop) {
			if (yyTop > 0)
				break;
			else
				return;
		}
	}
}


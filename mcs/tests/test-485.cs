// Compiler options: -checked

using System;

public class MonoBUG
{
	public static int Main ()
	{
		long l = long.MaxValue;
		
		try {
			l *= 2;
			return 1;
		} catch (OverflowException) {
		}
		
		return 0;
	}
}

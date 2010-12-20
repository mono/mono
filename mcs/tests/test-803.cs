using System;

class A
{
	public static int Main ()
	{
		int a = 1;
		while (a < 2) {
			try {}
			finally {
				a++;
			}
		}
		
		return 0;
	}
}
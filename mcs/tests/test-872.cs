using System;

class X
{
	public static void Main ()
	{
		int x = 1;
		switch (x) {
		case 1:
			try {
				goto case 6;
			} catch {
			}
			break;
		case 6:
			try {
				goto default;
			} catch {
			}
			break;
		default:
			break;
		}
	}
}
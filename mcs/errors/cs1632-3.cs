// CS1632: Control cannot leave the body of an anonymous method
// Line: 14

using System;

class X
{
	public static void Main ()
	{
		int b = 0;
		switch (b) {
			case 1:
			Action a = () => {
				break;
			};
			
			break;
		}
	}
}

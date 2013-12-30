// CS1632: Control cannot leave the body of an anonymous method
// Line: 12

using System;

class X
{
	public static void Main ()
	{
		while (true) {
			Action a = () => {
				break;
			};
		}
	}
}

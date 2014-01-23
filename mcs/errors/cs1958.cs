// CS1958: Object and collection initializers cannot be used to instantiate a delegate 
// Line: 9
using System;

class Test
{
	public static void Main ()
	{
		var a = new Action (delegate { }) {
		};
	}
}

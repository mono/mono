using System;
using System.Reflection;

class C
{
	static Action f = new Action (
		delegate {
			Assembly[] aa = {
					typeof (object).Assembly,
				};
		});

	public static void Main ()
	{
	}
}
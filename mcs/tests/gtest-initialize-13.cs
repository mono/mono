using System;

public struct EventInitializerTest
{
	public event Action a;
	public event Action b;
	public event Action c;

	public static void Main()
	{
		Action d = null;
		var eit = new EventInitializerTest() {
			a = null,
			b = delegate {},
			c = d
		};
	}
}
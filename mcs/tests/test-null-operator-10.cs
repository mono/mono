using System;

class Event
{
	public string Name { get; set; }
	public string Foo { get; set; }
}

class X
{
	public static void Main ()
	{
		var evt = new Event();
		string str = (evt.Foo != null ? evt?.Name : "").Trim();
	}
}
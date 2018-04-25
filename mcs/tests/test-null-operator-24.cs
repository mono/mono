using System;
 
class X
{
	public int Field { get; set; }

	public int F3 { get; set; }
}

class App
{
	static void Main()
	{
		string s = null;
		var x = new X {
			Field = s?.ToString () == null ? 1 : 2
		}.F3;
	}
}
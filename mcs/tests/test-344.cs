using System;

delegate void Y ();

class X {
	public event Y y;
	public static void Main (string [] args)
	{
		X x = new X ();
		x.Foo ();
	}

	int a;
	
	void Foo ()
	{
		int x = 1;
		y += delegate {
			Console.WriteLine (x);
			Console.WriteLine (this.GetType ());
		};
		y ();
		
	}
}

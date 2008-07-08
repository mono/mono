using System;

class C
{
	public static int Main ()
	{
		if (new C ().Test () != 6)
			return 1;
		
		return 0;
	}
	
	public delegate void Cmd ();
	public delegate int Cmd2 ();

	int Test ()
	{
		int r = Foo2 (delegate () {
			int x = 4;
			Foo (delegate () {
				int y = 6;
				Foo (delegate () {
					x = y;
				});
			});
			return x;
		});
		
		Console.WriteLine (r);
		return r;
	}
	
	int Foo2 (Cmd2 cmd)
	{
		return cmd ();
	}
	
	void Foo (Cmd cmd)
	{
		cmd ();
	}
}
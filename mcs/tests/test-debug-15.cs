using System;

class Foo
{
    ~Foo()
	{
		Console.WriteLine("Finalize");
    }
	
    public static void Main ()
	{
		new Foo ();
	}
}

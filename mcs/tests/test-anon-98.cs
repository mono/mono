using System;

class Foo
{
    ~Foo()
	{
		int x = 1;
		Action a = () => Console.WriteLine("{0}", x);
    }
	
    public static void Main ()
	{
		new Foo ();
	}
}

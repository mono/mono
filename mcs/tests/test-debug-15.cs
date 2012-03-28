using System.IO;

class Foo
{
    ~Foo()
	{
		StreamWriter.Null.WriteLine("Finalize");
    }
	
    public static void Main ()
	{
		new Foo ();
	}
}

public class A
{
	public string Name { get; set; }
	
	public bool Matches (string s)
	{
		return Name == s;
	}
}

class M
{
	public static int Main ()
	{
		if (!new A () { Name = "Foo" }.Matches ("Foo"))
			return 1;
		
		return 0;
	}
}
// Compiler options: -warnaserror

partial class C
{
	int i;
	partial void Partial_A ();
	partial void Partial_A () { i += 1; }
		
	partial void Partial_B () { i += 3; }
	partial void Partial_B ();
	
	static byte s;
	static partial void Partial_S () { s += 5; }
	static partial void Partial_S ();	

	public static int Main ()
	{
		C c = new C ();
		c.Partial_A ();
		c.Partial_B ();
		
		if (c.i != 4)
			return 1;
			
		Partial_S ();
		if (s != 5)
			return 2;			
		
		return 0;
	}
}

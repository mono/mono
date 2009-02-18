class A
{
	public int X {
		get { 
			return 100;
		}
	}
}

class B : A
{
	public static int Main ()
	{
		return new B ().Test ();
	}
	
	int Test ()
	{
		var x = new { base.X };
		return x.X == 100 ? 0 : 1;
	}
}

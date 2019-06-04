class X
{
	int x;

	static void Main ()
	{
		var x = new X ();
		Foo (ref x.Wrap (1));
		Foo (ref x.Prop);
		Foo (ref x[""]);		
	}

	ref int Wrap (int arg)
	{
		return ref x;
	}

	ref int Prop {
		get {
			return ref x;
		}
	}

	ref int this [string arg] {
		get {
			return ref x;
		}
	}

	static void Foo (ref int arg)
	{
	}
}
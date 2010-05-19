interface I
{
	int this[int i] { get; set; }
}

class C : I
{
	int I.this[int i]
	{
		get { return i; }
		set { }
	}

	public int this[int i]
	{
		get { return i; }
		private set { }
	}

	public static void Main ()
	{
	}
}


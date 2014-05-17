interface I
{
	int P { get; }
}

class B : I
{
	int I.P { get { return 1; } }
}

class C : B
{
	public int get_P ()
	{
		return 1;
	}

	public static void Main ()
	{
	}
}
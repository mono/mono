class TestA
{
	public virtual string Method
	{
		get { return null; }
	}
}

class TestB : TestA
{
	private string Method
	{
		get { return null; }
	}

	public static void Main ()
	{
	}
}
abstract class A : I
{
	protected abstract void M ();
		
	void I.M ()
	{
	}
}

interface I
{
	void M ();
}

class C : A, I
{
	protected override void M ()
	{
	}
	
	public static void Main ()
	{
	}
}

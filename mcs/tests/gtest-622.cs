interface IX<TI>
{
	void M<TO> () where TO : TI;
}

interface IY
{
}

class CY : IY
{
}

class A : IX<IY>
{
	public void M<TO> () where TO : IY
	{
	}

	public static void Main ()
	{
		var a = new A ();
		a.M<CY> ();
	}
}
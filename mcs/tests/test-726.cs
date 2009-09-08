interface IFoo
{
	object Clone ();
}

class CS0102 : IFoo
{
	object IFoo.Clone()
	{
		return this;
	}

	public class Clone { }

	public static void Main ()
	{
	}
}

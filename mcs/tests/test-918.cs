public class Test
{
	Test x;

	void Foo ()
	{
		{
			string x = "dd";
		}

		{
			x = null;
		}

		x = new Test ();
	}

	public static void Main () { }
}
// CS0135: `x' conflicts with a declaration in a child block
// Line: 18

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
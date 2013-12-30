struct AStruct
{
	public object foo;

	public AStruct (int i)
		: this ()
	{
	}
}

public class Tests
{
	public static int Main ()
	{
		for (int i = 0; i < 100; ++i) {
			AStruct a;

			a = new AStruct (5);
			if (a.foo != null)
				return 1;

			a.foo = i + 1;
		}

		System.Console.WriteLine ("ok");
		return 0;
	}
}
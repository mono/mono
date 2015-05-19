public enum Foo { One, Two };

class MainClass
{
	public static int Main ()
	{
		const Foo foo = Foo.Two;
		int obj;

		switch (foo) {
			case Foo.One:
			case Foo.Two:
				obj = 2;
				break;
		}

		if (obj != 2)
			return 1;

		return 0;
	}
}

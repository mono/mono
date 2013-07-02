using System;

class Test
{
	int a;
	int b;

	Test (int i)
	{
		a = 10;
	}

	static Test Foo (dynamic d)
	{
		return new Test (d) {
			b = 20
		};
	}

	public static int Main ()
	{
		var t = Foo (44);
		if (t.a != 10)
			return 1;
		if (t.b != 20)
			return 2;
		
		return 0;
	}
}

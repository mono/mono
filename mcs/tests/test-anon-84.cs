using System;

public delegate C D (int i);

public class C
{
	private readonly D field = delegate {
		int x = 0;
		return null;
	};

	public C ()
	{
	}

	public C (D onMissing)
	{
	}

	public static int Main ()
	{
		new C ().field (3);
		return 0;
	}
}

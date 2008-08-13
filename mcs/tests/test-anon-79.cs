class Test
{
	delegate void D ();
	event D E;
	D proxy;

	event D Changed
	{
		add
		{
			if (proxy == null)
				proxy = delegate () { Foo (); };
		}
		remove
		{
		}
	}

	void Foo ()
	{
	}

	public static void Main ()
	{
	}
}
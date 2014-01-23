public delegate void A();

class B
{
	public static event A D;
	long[] d = new long [1];

	void C ()
	{
		int a = 0;
		int b = 0;

		A block = delegate {
			long c = 0;

			B.D += delegate {
				d [b] = c;
				F (c);
			};
		};
	}

	public void F (long i)
	{
	}

	public static void Main ()
	{
	}
}

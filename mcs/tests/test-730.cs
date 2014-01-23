class Demo
{
	public void Test (object arg)
	{
	}

	static int Test (int i)
	{
		return i;
	}

	delegate int D (int t);

	int GetPhones ()
	{
		D d = Test;
		return d (55);
	}

	public static int Main ()
	{
		int r = new Demo ().GetPhones ();
		if (r != 55)
			return 1;

		return 0;
	}
}

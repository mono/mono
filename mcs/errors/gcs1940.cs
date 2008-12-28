// CS1940: Ambiguous implementation of the query pattern `Select' for source type `Multiple'
// Line: 10

class Multiple
{
	delegate int D1 (int x);
	delegate int D2 (int x);

	int Select (D1 d)
	{
		return 0;
	}

	int Select (D2 d)
	{
		return 1;
	}

	public static void Main ()
	{
		var q = from x in new Multiple () select x;
	}
}

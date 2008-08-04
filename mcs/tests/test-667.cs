interface IA
{
	void M ();
}

class CA : IA
{
	void IA.M ()
	{
	}
}

class CC : CA, IA
{
	public static void Main ()
	{
	}
}
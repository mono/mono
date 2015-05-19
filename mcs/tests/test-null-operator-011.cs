static class Crash
{
	static string GetFoo ()
	{
		return null;
	}

	static void Main ()
	{
		(GetFoo ()?.ToLower ()).ToUpper ();
	}
}

class Test
{
	int? _state;
	public bool Working () => _state?.ToString () != "";

	public static int Main ()
	{
		var t = new Test ();
		if (!t.Working ())
			return 1;

		return 0;
	}
}

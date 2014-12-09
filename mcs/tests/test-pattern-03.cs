// Compiler options: -langversion:experimental

class WildcardPattern
{
	static int Main ()
	{
		long? o = 1;
		bool b = o is *;
		if (!b)
			return 1;

		return 0;
	}
}
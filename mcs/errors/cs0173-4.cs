// CS0173: Type of conditional expression cannot be determined because there is no implicit conversion between `null' and `null'
// Line: 8

class X
{
	static void Main (string[] args)
	{
		bool b = args.Length > 0 ? null : null;
	}
}

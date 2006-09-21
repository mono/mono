// CS0037: Cannot convert null to `bool' because it is a value type
// Line: 8

class X
{
	static void Main (string[] args)
	{
		bool b = args.Length > 0 ? null : null;
	}
}

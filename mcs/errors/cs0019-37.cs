// CS0019: Operator `==' cannot be applied to operands of type `bool' and `int'
// Line: 10

class C
{
	static bool HasSessionId (string path)
	{
		if (path == null || path.Length < 5)
			return false;

		return path.StartsWith ("/(") == 0;
	}
}

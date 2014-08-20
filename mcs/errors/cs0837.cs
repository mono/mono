// CS0837: The `is' operator cannot be applied to a lambda expression, anonymous method, or method group
// Line: 8

class X
{
	static void Main ()
	{
		if (delegate {} is int) {
			return;
		}
	}
}

// CS0221:  Constant value `-1' cannot be converted to a `char' (use `unchecked' syntax to override)
// Line: 10

class C
{
	static void Main ()
	{
		unchecked {
			checked {
				const char c = (char) -1;
			}
		}
	}
}

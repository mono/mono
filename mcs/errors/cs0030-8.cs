// CS0030: Cannot convert type `bool' to `int'
// Line: 9

class X
{
	static void Main ()
	{
		const bool b = true;
		int a = (int)(b ? true : false);
	}
}

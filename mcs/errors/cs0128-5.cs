// CS0128: A local variable named `xx' is already defined in this scope
// Line: 9

class X
{
	public static void Main ()
	{
		short xx;
		var (xx, yy) = (1, 'g');
	}
}
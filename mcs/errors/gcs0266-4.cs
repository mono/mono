// CS0266: Cannot implicitly convert type `object' to `bool'. An explicit conversion exists (are you missing a cast?)
// Line: 9

class X
{
	static void Main ()
	{
		object o = true;
		bool b = (o ?? string.Empty);
	}
}

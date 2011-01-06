// CS0266: Cannot implicitly convert type `bool?' to `bool'. An explicit conversion exists (are you missing a cast?)
// Line: 9

class X
{
	static void Main ()
	{
		bool? a = true;
		bool b = a & a;
	}
}

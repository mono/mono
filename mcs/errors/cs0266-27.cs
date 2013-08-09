// CS0266: Cannot implicitly convert type `E?' to `E'. An explicit conversion exists (are you missing a cast?)
// Line: 13

enum E
{
}

class C
{
	public static void Main ()
	{
		E e = 0;
		E r = e + null;
	}
}

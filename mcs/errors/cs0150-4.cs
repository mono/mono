// CS0150: A constant value is expected
// Line: 13

class Program
{
	static int Arg ()
	{
		return 4;
	}

	static void Main()
	{
		var s = $"{1,Arg()}";
	}
}
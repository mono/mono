// CS8327: The ref conditional expression types `int' and `byte' have to match
// Line: 12

class Program
{
	static int x;
	static byte y;

	public static void Main ()
	{
		bool b = false;
		ref int targetBucket = ref b ? ref x : ref y;
	}
}
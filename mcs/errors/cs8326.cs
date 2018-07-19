// CS8326: Both ref conditional operators must be ref values
// Line: 11

class Program
{
	static int x, y;

	public static void Main ()
	{
		bool b = false;
		ref int targetBucket = ref b ? x : y;
	}
}
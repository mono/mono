// CS1739: The best overloaded method match for `Program.M(int, int, int)' does not contain a parameter named `whatever'
// Line: 8

public class Program
{
	public static void Main ()
	{
		M (z: 1, whatever: 0);
	}

	void M (int x = 0, int y = 0, int z = 0)
	{
	}
}

public class A
{
	public static bool TryAssign (out int x)
	{
		x = 0;
		return true;
	}

	public static void Main ()
	{
		int x, y;
		if ((!TryAssign (out x) || x == 0) & (!TryAssign (out y) || y == 0)) {
		}
	}
}

// Compiler options: -warnaserror

public class Test
{
#pragma warning disable public
#pragma warning disable CS1685
#pragma warning disable CS1700, 1701

	public static void Main ()
	{
	}
#pragma warning restore CS1685
#pragma warning restore public, 1701

	public static void TestCS ()
	{
		return;
#pragma warning disable CS0162
		return;
#pragma warning restore
	}
}
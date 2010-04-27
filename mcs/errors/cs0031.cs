// cs0031.cs: Constant value `1022' cannot be converted to a `byte'
// Line: 9

public class Test
{
	public static void Main()
	{
		unchecked {
			byte b = 1024 - 2;
		}
	}
}
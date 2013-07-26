// CS0472: The result of comparing value type `byte' with null is always `false'
// Line: 9
// Compiler options: -warnaserror -warn:2

class C
{
	public static bool Test (byte value)
	{
		if (value == null)
			return false;

		return true;
	}
}

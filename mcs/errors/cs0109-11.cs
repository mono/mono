// CS0109: The member `Wrapper.DerivedClass.AnInt' does not hide an inherited member. The new keyword is not required
// Line: 18
// Compiler options: -warnaserror

public abstract class BaseClass
{
	private static readonly int AnInt = 1;

	public static void Main ()
	{
	}
}

public static class Wrapper
{
	public class DerivedClass : BaseClass
	{
		private new static readonly int AnInt = 2;
	}
}

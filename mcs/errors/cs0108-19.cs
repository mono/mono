// CS0108: `A.B.AnInt' hides inherited member `A.AnInt'. Use the new keyword if hiding was intended
// Line: 11
// Compiler options: -warnaserror

public abstract class A
{
	static readonly int AnInt = 2;

	public class B : A
	{
		static readonly int AnInt = 3;
	}
}

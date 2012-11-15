// CS0572: `Inner': cannot reference a type through an expression. Consider using `Outer.Inner' instead
// Line: 18

public class Outer
{
	public enum Inner
	{
		ONE,
		TWO
	}
}

public class C
{
	public static bool Test ()
	{
		Outer outer = null;
		return 0 == outer.Inner.ONE;
	}
}
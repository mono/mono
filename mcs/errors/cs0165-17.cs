// CS0165: Use of unassigned local variable `t'
// Line: 8

public class Foo<T>
{
	public static bool Test ()
	{
		T t;
		return t is int;
	}
}

// CS0246: The type or namespace name `B' could not be found. Are you missing an assembly reference?
// Line: 21

using static A;

class A : B
{
}

class P
{
	public class N<T>
	{
	}
}

class Test
{
	public static void Main ()
	{
		var n = default (N<int>);
	}
}
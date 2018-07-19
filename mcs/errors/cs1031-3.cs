// CS1031: Type expected
// Line: 16

public class B<Y>  where Y: B<Y>
{
}

public class A<X>: B<A<X>>
{
}

public class Repro
{
	public static void Main (string[] args)
	{
		var h = typeof (B<A<>>);
	}
}
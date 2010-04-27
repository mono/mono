public abstract class B<T> : A<T> { }
public abstract class A<T>
{
	internal sealed class C : B<T>
	{
	}
}

class M
{
	public static void Main ()
	{
	}
}

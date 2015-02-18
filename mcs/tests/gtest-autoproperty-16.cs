abstract class A
{
	public abstract int Foo { get; }
}

class B : A
{
	public override int Foo => 1;

	public static void Main ()
	{
	}
}
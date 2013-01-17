// CS0426: The nested type `Foo' does not exist in the type `C<int>'
// Line: 11

public abstract class B<T>
{
	public class Foo
	{
	}
}

public class C<T> : B<C<int>.Foo>
{
}

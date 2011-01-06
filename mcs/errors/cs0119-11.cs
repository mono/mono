// CS0119: Expression denotes a `type parameter', where a `variable', `value' or `type' was expected
// Line: 15

class A
{
	public class T
	{
	}
}

class B<T> : A
{
	void Foo ()
	{
		T.Equals (null, null);
	}
}

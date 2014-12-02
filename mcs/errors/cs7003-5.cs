// CS7003: Unbound generic name is not valid in this context
// Line: 14

class A<T>
{
	public class B
	{
		public int Foo;
	}
}

class X
{
	string s = nameof (A<>.B);
}

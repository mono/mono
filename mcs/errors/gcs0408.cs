// CS0408: `X<T>' cannot define overload members that may unify for some type parameter substitutions
// Line: 9
class X<T>
{
	public void Foo (T t)
	{ }

	public void Foo (int i)
	{ }
}

class X
{
	static void Main ()
	{ }
}

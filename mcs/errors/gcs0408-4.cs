// CS0408: `X<T>' cannot define overload members that may unify for some type parameter substitutions
// Line: 9
class X<T>
{
	void Foo (T t)
	{ }

	void Foo (int[] t)
	{ }
}

class X
{
	static void Main ()
	{ }
}

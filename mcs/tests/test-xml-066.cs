// Compiler options: -doc:xml-066.xml

static class C
{
	static void Foo<T, U> (this int a, T t, U[] u)
	{
	}
	
	/// <seealso cref="Foo{T0,U2}(int,T0,U2[])"/>
	static void Foo2<T> (this int a, T t)
	{
	}
	
	public static void Main ()
	{
	}
}
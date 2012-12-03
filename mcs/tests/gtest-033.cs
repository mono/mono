// Generic interfaces

interface Foo<R,S>
{
	void Hello (R r, S s);
}

interface Bar<T,U,V> : Foo<V,float>
{
	void Test (T t, U u, V v);
}

class X
{
	static void Test (Bar<long,int,string> bar)
	{
		bar.Hello ("Test", 3.14F);
		bar.Test (512, 7, "Hello");
	}

	public static void Main ()
	{ }
}

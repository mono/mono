// Compiler options: -t:library

public class Test<A,B>
{
	public void Foo<U> (U u)
	{ }

	public void Foo<V> (V[] v, V w)
	{ }

	public void Hello<V,W> (V v, W w, Test<V,W> x)
	{ }

	public void ArrayMethod<V> (params V[] args)
	{ }
}

// Compare this to gcs0695-*.cs: these are the allowed cases.

namespace A
{
	interface I<X>
	{ }

	interface J<X,Y> : I<X>
	{ }

	class A<X> : I<X>, I<A<X>>
	{ }

	class B<X> : I<B<X>>, I<X>, I<A<X>>
	{ }

	class C<X> : I<int>, I<A<X>>
	{ }

	class D<X> : I<A<float>>, I<B<X>>
	{ }

	class E<X,Y> : J<X,Y>, J<I<X>,I<Y>>
	{ }

	class F<X> : J<X,I<X>>, J<X,X>
	{ }
}

// bug #69057
namespace B
{
	struct KeyValuePair<K,V>
	{ }

	interface ITest<T>
	{ }

	interface ITest2<K,V> : ITest<KeyValuePair<K,V>>
	{ }

	class MyTest<K,V> : ITest2<K,V>, ITest<KeyValuePair<K,V>>
	{ }
}

// bug #58303
namespace C
{
	class S <K> { }

	interface Z<T> { }

	interface I<K> : Z<S<K>> { }

	class C <K> : I<K>, Z<S<K>> { }
}

class Test
{
	public static void Main ()
	{ }
}

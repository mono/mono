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

class Test
{
	static void Main ()
	{ }
}

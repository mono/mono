interface I<X>
{ }

class A<X,Y,Z> : I<X>, I<A<Y,Y,Z>>
{ }

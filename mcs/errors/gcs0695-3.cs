interface I<X>
{ }

interface K<X>
{ }

class C<X,Y> : I<X>, I<K<Y>>
{ }

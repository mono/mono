interface I<X>
{ }

interface K<X>
{ }

class C<X,Y> : I<K<Y>>, I<X>
{ }

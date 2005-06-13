// gcs0695.cs: `C<X,Y>' cannot implement both `I`1<K`1<Y>>' and `I`1<X>' because they may unify for some type parameter substitutions
// Line: 10

interface I<X>
{ }

interface K<X>
{ }

class C<X,Y> : I<X>, I<K<Y>>
{ }

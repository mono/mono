// gcs0695.cs: `C<X,Y>' cannot implement both `I`1<X>' and `I`1<K`1<Y>>' because they may unify for some type parameter substitutions
// Line: 10

interface I<X>
{ }

interface K<X>
{ }

class C<X,Y> : I<K<Y>>, I<X>
{ }

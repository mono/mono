// gcs0695.cs: `C<X,Y>' cannot implement both `I<X>' and `I<K<Y>>' because they may unify for some type parameter substitutions
// Line: 10

interface I<X>
{ }

interface K<X>
{ }

class C<X,Y> : I<K<Y>>, I<X>
{ }

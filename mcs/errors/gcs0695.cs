// gcs0695.cs: `A<X,Y>' cannot implement both `I`1<Y>' and `I`1<X>' because they may unify for some type parameter substitutions
// Line: 7

interface I<X>
{ }

class A<X,Y> : I<X>, I<Y>
{ }

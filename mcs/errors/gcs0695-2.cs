// gcs0695.cs: `A<X,Y,Z>' cannot implement both `I`1<A`3<Y,Y,Z>>' and `I`1<X>' because they may unify for some type parameter substitutions
// Line: 7

interface I<X>
{ }

class A<X,Y,Z> : I<X>, I<A<Y,Y,Z>>
{ }

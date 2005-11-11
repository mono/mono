// gcs0695.cs: `A<X,Y>' cannot implement both `I<Y>' and `I<X>' because they may unify for some type parameter substitutions
// Line: 7

interface I<X>
{ }

class A<X,Y> : I<X>, I<Y>
{ }

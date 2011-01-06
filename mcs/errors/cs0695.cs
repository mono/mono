// CS0695: `A<X,Y>' cannot implement both `I<X>' and `I<Y>' because they may unify for some type parameter substitutions
// Line: 7

interface I<X>
{ }

class A<X,Y> : I<X>, I<Y>
{ }

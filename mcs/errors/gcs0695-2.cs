// CS0695: `A<X,Y,Z>' cannot implement both `I<X>' and `I<A<Y,Y,Z>>' because they may unify for some type parameter substitutions
// Line: 7

interface I<X>
{ }

class A<X,Y,Z> : I<X>, I<A<Y,Y,Z>>
{ }

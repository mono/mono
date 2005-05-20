// cs0146.cs: Circular base class dependency involving 'C' and 'A'
// Line: 4

class A : B
{ }

class B : C
{ }

class C : A
{ }


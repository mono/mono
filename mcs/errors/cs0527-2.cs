// cs0146.cs: Circular base class dependency involving 'B' and 'A'
// Line: 7

struct A : B {
}

struct B : A {
}
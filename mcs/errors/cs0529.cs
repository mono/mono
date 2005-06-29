// cs0529.cs: Inherited interface `A' causes a cycle in the interface hierarchy of `C'
// Line: 10

interface A : B {
}

interface B : C {
}

interface C : A {
}
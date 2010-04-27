// CS0529: Inherited interface `C' causes a cycle in the interface hierarchy of `A'
// Line: 10

interface A : B {
}

interface B : C {
}

interface C : A {
}

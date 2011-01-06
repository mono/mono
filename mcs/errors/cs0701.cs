// CS0701: `A' is not a valid constraint. A constraint must be an interface, a non-sealed class or a type parameter
// Line: 6

sealed class A { }

class Foo<T> where T : A
{
}

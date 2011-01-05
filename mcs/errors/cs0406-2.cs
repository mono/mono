// CS0406: The class type constraint `A' must be listed before any other constraints. Consider moving type constraint to the beginning of the constraint list
// Line: 7

class A { }
interface I { }

class Foo<T> where T : I, A
{
}

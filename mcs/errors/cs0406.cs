// CS0406: The class type constraint `B' must be listed before any other constraints. Consider moving type constraint to the beginning of the constraint list
// Line: 8

class A { }
class B { }

class Foo<T>
	where T : A, B
{
}

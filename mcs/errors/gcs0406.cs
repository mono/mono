// gcs0406.cs: `B': the class constraint for `T' must come before any other constraints
// Line: 9

class A { }
class B { }

class Foo<T>
	where T : A, B
{
}

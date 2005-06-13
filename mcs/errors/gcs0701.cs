// gcs0701.cs: `A' is not a valid bound.  Bounds must be interfaces or non sealed classes
// Line: 8

sealed class A { }

class Foo<T>
	where T : A
{
}

// gcs0701.cs: `S' is not a valid constraint. A constraint must be an interface, a non-sealed class or a type parameter
// Line: 8

struct S
{
}

class Foo<T> where T : S
{
}

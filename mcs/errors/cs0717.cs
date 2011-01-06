// CS0717: `S' is not a valid constraint. Static classes cannot be used as constraints
// Line: 8

static class S
{
}

class Foo<T> where T : S
{
}

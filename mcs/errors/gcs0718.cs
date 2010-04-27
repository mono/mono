// CS0718: `S': static classes cannot be used as generic arguments
// Line: 14

static class S
{
}

class C<T>
{
}

class Test
{
	C<S> foo;
}
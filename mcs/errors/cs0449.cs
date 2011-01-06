// CS0449: The `class' or `struct' constraint must be the first constraint specified
// Line: 6

interface I
{
	void Foo<T> () where T : class, struct;
}

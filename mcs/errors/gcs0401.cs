// CS0401: The new() constraint must be the last constraint specified
// Line: 6

class Foo<T>
	where T : new (), new ()
{
}

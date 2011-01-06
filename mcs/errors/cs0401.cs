// CS0401: The `new()' constraint must be the last constraint specified
// Line: 4

class Foo<T> where T : new (), new ()
{
}

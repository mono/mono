// CS0405: Duplicate constraint `I' for type parameter `T'
// Line: 8

interface I { }

class Foo<T>
	where T : I, I
{
}

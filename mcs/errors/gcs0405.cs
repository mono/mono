interface I { }

class Foo<T>
	where T : I, I
{
}

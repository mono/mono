// CS1959: Type parameter `T' cannot be declared const
// Line: 10

class C
{
}

class C<T> where T : C
{
	const T t = null;
}

// CS1923: The covariant type parameter `T' must be invariantly valid on `B<T>.C(A<A<T>>)'
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<out T>
{
	void C(A<A<T>> a);
}

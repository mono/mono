// CS1961: The contravariant type parameter `T' must be invariantly valid on `B<T>.C(A<T>)'
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<in T>
{
	void C(A<T> a);
}

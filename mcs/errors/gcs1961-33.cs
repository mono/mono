// CS1961: The contravariant type parameter `T' must be invariantly valid on `A<T>.B(out T)'
// Line: 5
// Compiler options: -langversion:future

interface A<in T>
{
	void B(out T t);
}

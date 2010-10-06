// CS1961: The covariant type parameter `T' must be invariantly valid on `A<T>.B(out T)'
// Line: 7
// Compiler options: -langversion:future

interface A<out T>
{
	void B(out T t);
}

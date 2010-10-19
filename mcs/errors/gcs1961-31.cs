// CS1961: The covariant type parameter `T' must be invariantly valid on `B<T>'
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<out T> : A<T>
{
}

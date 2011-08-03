// CS1961: The covariant type parameter `T' must be invariantly valid on `B<T>'
// Line: 8

interface A<T>
{
}

interface B<out T> : A<T>
{
}

// CS1961: The covariant type parameter `T' must be contravariantly valid on `B<T>'
// Line: 8

interface A<in T>
{
}

interface B<out T> : A<T>
{
}

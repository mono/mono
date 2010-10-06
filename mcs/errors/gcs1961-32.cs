// CS1961: The covariant type parameter `T' must be contravariantly valid on `B<T>'
// Line: 9
// Compiler options: -langversion:future

interface A<in T>
{
}

interface B<out T> : A<T>
{
}

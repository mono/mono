// CS8039: Contravariant type parameters cannot be used as arguments in interface inheritance
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

interface B<in T> : A<T>
{
}

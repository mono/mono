// CS8035: Covariant type parameters can only be used as type arguments in covariant positions
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

delegate void B<out T> (A<T> a);

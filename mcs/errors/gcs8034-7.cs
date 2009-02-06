// CS8034: Contravariant type parameters can only be used as type arguments in contravariant positions
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

delegate A<T> B<in T> ();

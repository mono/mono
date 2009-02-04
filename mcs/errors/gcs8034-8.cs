// CS8034: Contravariant type parameters can only be used as input arguments to a method
// Line: 9
// Compiler options: -langversion:future

interface A<T>
{
}

delegate A<A<T>> B<in T> ();

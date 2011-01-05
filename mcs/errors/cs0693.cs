// CS0693: Type parameter `T' has the same name as the type parameter from outer type `A<T>'
// Line: 7
// Compiler options: -warnaserror -warn:3

class A<T>
{
    interface I<T>
    {
    }
}
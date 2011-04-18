// CS0402: `C.Main<T,U>()': an entry point cannot be generic or in a generic type
// Line: 7
// Compiler options: -warnaserror -warn:4

class C
{
    public static void Main<T, U> ()
    {
    }
}
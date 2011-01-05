// gcs0402.cs: `TestClass<T>.Main()': an entry point cannot be generic or in a generic type
// Line: 7
// Compiler options: -warnaserror -warn:4

class TestClass<T>
{
    public static void Main ()
    {
    }
}
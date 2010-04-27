// CS1720: Expression will always cause a `System.NullReferenceException'
// Line: 9
// Compiler options: -warnaserror -warn:1

public class Tester 
{
    public static void GenericClass<T>(T t) where T : class 
    {
        string s = default(T).ToString();
    }
}
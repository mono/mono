// CS0209: The type of locals declared in a fixed statement must be a pointer type
// Line: 9
// Compiler options: -unsafe

public class A
{
        unsafe static void Main ()
        {
                fixed (string s = null)
                {
                }
        }
}



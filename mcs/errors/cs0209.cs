// cs0209.cs: variable in a fixed statement must be a pointer
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



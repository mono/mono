// cs0209.cs: variable in a fixed statement must be a pointer
// Line: 7
public class A
{
        unsafe static void Main ()
        {
                fixed (string s = null)
                {
                }
        }
}



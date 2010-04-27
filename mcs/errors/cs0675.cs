// CS0675: The operator `|' used on the sign-extended type `int'. Consider casting to a smaller unsigned type first
// Line: 11
// Compiler options: -warnaserror -warn:3

public class C
{
   public static void Main()
   {
      int x = 1;
      int y = 1;
      long value = (((long)x) << 32) | y;
   }
}


// CS0229: Ambiguity between `TestLib.FOO()' and `TestLib.FOO'
// Line: 9
// Compiler options: -r:CS0229-4-lib.dll

public class Test
{
   public static void Main()
   {
      System.Console.WriteLine(TestLib.FOO);
   }
}

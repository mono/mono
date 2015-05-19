// Compiler options: -warn:4
// This test should print only: warning CS1633: Unrecognized #pragma directive

#pragma xxx some unrecognized text

public class C
{
  public static void Main () {}
}

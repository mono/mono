// CS0429: Unreachable expression code detected
// Line: 11
// Compiler options: -warn:4 -warnaserror

using System;

class Main
{
   public void Method (int i)
   {
       if (5 == 5 || i > 10)
	   Console.WriteLine ("TEST");
   }
}

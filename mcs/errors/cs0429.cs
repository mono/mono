// CS0429: Unreachable expression code detected
// Line: 9
// Compiler options: -warn:4 -warnaserror

class Main
{
   public void Method (int i)
   {
       if (false && i > 10)
	   return;
   }
}

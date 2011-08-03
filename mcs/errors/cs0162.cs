// CS0162: Unreachable code detected
// Line: 12
// Compiler options: -warnaserror -warn:2

using System;

class E
{
   public void Method (int i)
   {
       throw new ArgumentNullException ();
       Console.WriteLine ("Once upon a time..");
   }
}

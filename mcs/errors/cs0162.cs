// cs0162.cs: Unreachable code detected
// Line: 12
// Compiler options: -t:library

using System;

class E
{
   public void Method (int i)
   {
       throw new ArgumentNullException ();
       Console.WriteLine ("Once upon a time..");
   }
}

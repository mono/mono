// CS0162: Unreachable code detected
// Line: 12
// Compiler options: -warnaserror -warn:2

using System;

class E
{
   public void Method<T> () where T : class
   {
      if (default (T) != null)
         throw new ArgumentNullException ();
   }
}

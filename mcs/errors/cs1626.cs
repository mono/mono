// CS1626: Cannot yield a value in the body of a try block with a catch clause
// Line: 11

using System.Collections;

class C: IEnumerable
{
   public IEnumerator GetEnumerator ()
   {
	   try {
		   yield return this;
	   }
	   catch {
	   }
   }
}

// CS01627: Expression expected after yield return
// Line: 10

using System.Collections;

class C: IEnumerable
{
   public IEnumerator GetEnumerator ()
   {
	   yield return;
   }
}

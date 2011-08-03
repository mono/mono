// CS01621: The yield statement cannot be used inside anonymous method blocks
// Line: 13

using System.Collections;

delegate object D ();

class C: IEnumerable
{
   public IEnumerator GetEnumerator ()
   {
      D d = delegate {
		yield return this;
	  };
   }
}

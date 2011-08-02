// CS0117: `System.Runtime.CompilerServices.IndexerNameAttribute' does not contain a definition for `errorarg'
// Line: 

using System.Runtime.CompilerServices;

public class E
{
   [IndexerName("xxx", errorarg = "")]
   public int this[int index] {
      get {
         return 0;
      }
   }
}

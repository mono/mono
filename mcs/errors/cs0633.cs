public class MonthDays {
   [System.Runtime.CompilerServices.IndexerName ("buggypo for you")]
   public int this [int a] {
      get {
         return 0;
      }
   }

   public static void Main ()
   {
	int i = new MonthDays () [1];
   }
}



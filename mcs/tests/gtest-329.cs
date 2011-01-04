using System;

public class NullableInt
{
         public static int Main()
         {
                 object x = null;

                 int? y = x as int?;  /* Causes CS0077 */

                 Console.WriteLine("y: '{0}'", y);
                 Console.WriteLine("y.HasValue: '{0}'", y.HasValue);
			 
			 int? b = 1 as int?;
			 if (b != 1)
				 return 1;
			 
			 return 0;
         }
}

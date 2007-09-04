using System;

public class NullableInt
{
         public static void Main()
         {
                 object x = null;

                 int? y = x as int?;  /* Causes CS0077 */

                 Console.WriteLine("y: '{0}'", y);
                 Console.WriteLine("y.HasValue: '{0}'", y.HasValue);
         }
}

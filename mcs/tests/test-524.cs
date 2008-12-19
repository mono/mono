using System;
public class Foo {
   public static int Main()
   {
	   try {
		   lock (null) {
		   }
	   }
	   catch (ArgumentNullException) {
		   return 0;
	   }
	   return 1;
   }
}
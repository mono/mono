// error CS0234: The type or namespace name `Enum' could not be found in namespace `A.B.System'
// Line: 8

using System;
namespace A.B.System {
	public class Test { 
		public static void Main () {
			Console.WriteLine (typeof (System.Enum));
		} 
	} 
}

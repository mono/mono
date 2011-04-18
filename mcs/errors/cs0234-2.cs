// CS0234: The type or namespace name `Enum' does not exist in the namespace `A.B.System'. Are you missing an assembly reference?
// Line: 8

using System;
namespace A.B.System {
	public class Test { 
		public static void Main () {
			Console.WriteLine (typeof (System.Enum));
		} 
	} 
}

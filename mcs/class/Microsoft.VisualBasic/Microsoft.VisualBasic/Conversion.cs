//
// Conversion.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class Conversion {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.String ErrorToString () { return "";}
		public static System.String ErrorToString (System.Int32 ErrorNumber) { return "";}
		public static System.Int16 Fix (System.Int16 Number) { return 0;}
		public static System.Int32 Fix (System.Int32 Number) { return 0;}
		public static System.Int64 Fix (System.Int64 Number) { return 0;}
		public static System.Double Fix (System.Double Number) { return 0;}
		public static System.Single Fix (System.Single Number) { return 0;}
		public static System.Decimal Fix (System.Decimal Number) { return 0;}
		public static System.Object Fix (System.Object Number) { return null;}
		public static System.Int16 Int (System.Int16 Number) { return 0;}
		public static System.Int32 Int (System.Int32 Number) { return 0;}
		public static System.Int64 Int (System.Int64 Number) { return 0;}
		public static System.Double Int (System.Double Number) { return 0;}
		public static System.Single Int (System.Single Number) { return 0;}
		public static System.Decimal Int (System.Decimal Number) { return 0;}
		public static System.Object Int (System.Object Number) { return null;}
		public static System.String Hex (System.Int16 Number) { return "";}
		public static System.String Hex (System.Byte Number) { return "";}
		public static System.String Hex (System.Int32 Number) { return "";}
		public static System.String Hex (System.Int64 Number) { return "";}
		public static System.String Hex (System.Object Number) { return "";}
		public static System.String Oct (System.Int16 Number) { return "";}
		public static System.String Oct (System.Byte Number) { return "";}
		public static System.String Oct (System.Int32 Number) { return "";}
		public static System.String Oct (System.Int64 Number) { return "";}
		public static System.String Oct (System.Object Number) { return "";}
		public static System.String Str (System.Object Number) { return "";}
		public static System.Double Val (System.String InputStr) { return 0;}
		public static System.Int32 Val (System.Char Expression) { return 0;}
		public static System.Double Val (System.Object Expression) { return 0;}
		// Events
	};
}

//
// DecimalType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic.CompilerServices {
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class DecimalType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Decimal FromBoolean (System.Boolean Value) { return 0;}
		public static System.Decimal FromString (System.String Value) { return 0;}
		public static System.Decimal FromString (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) { return 0;}
		public static System.Decimal FromObject (System.Object Value) { return 0;}
		public static System.Decimal FromObject (System.Object Value, System.Globalization.NumberFormatInfo NumberFormat) { return 0;}
		public static System.Decimal Parse (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) { return 0;}
		// Events
	};
}

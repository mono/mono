//
// StringType.cs
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
	sealed public class StringType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.String FromBoolean (System.Boolean Value) { return "";}
		public static System.String FromByte (System.Byte Value) { return "";}
		public static System.String FromChar (System.Char Value) { return "";}
		public static System.String FromShort (System.Int16 Value) { return "";}
		public static System.String FromInteger (System.Int32 Value) { return "";}
		public static System.String FromLong (System.Int64 Value) { return "";}
		public static System.String FromSingle (System.Single Value) { return "";}
		public static System.String FromDouble (System.Double Value) { return "";}
		public static System.String FromSingle (System.Single Value, System.Globalization.NumberFormatInfo NumberFormat) { return "";}
		public static System.String FromDouble (System.Double Value, System.Globalization.NumberFormatInfo NumberFormat) { return "";}
		public static System.String FromDate (System.DateTime Value) { return "";}
		public static System.String FromDecimal (System.Decimal Value) { return "";}
		public static System.String FromDecimal (System.Decimal Value, System.Globalization.NumberFormatInfo NumberFormat) { return "";}
		public static System.String FromObject (System.Object Value) { return "";}
		public static System.Int32 StrCmp (System.String sLeft, System.String sRight, System.Boolean TextCompare) { return 0;}
		public static System.Boolean StrLike (System.String Source, System.String Pattern, Microsoft.VisualBasic.CompareMethod CompareOption) { return false;}
		public static System.Boolean StrLikeBinary (System.String Source, System.String Pattern) { return false;}
		public static System.Boolean StrLikeText (System.String Source, System.String Pattern) { return false;}
		public static void MidStmtStr (ref System.String sDest, ref System.Int32 StartPosition, ref System.Int32 MaxInsertLength, ref System.String sInsert) { }
		// Events
	};
}

//
// Information.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.Runtime.InteropServices.StructLayoutAttribute(System.Runtime.InteropServices.LayoutKind.Auto)] 
	sealed public class Information {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static Microsoft.VisualBasic.ErrObject Err () { return null;}
		[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
		public static System.Int32 Erl () { return 0;}
		public static System.Boolean IsArray (System.Object VarName) { return false;}
		public static System.Boolean IsDate (System.Object Expression) { return false;}
		public static System.Boolean IsDBNull (System.Object Expression) { return false;}
		public static System.Boolean IsNothing (System.Object Expression) { return false;}
		public static System.Boolean IsError (System.Object Expression) { return false;}
		public static System.Boolean IsReference (System.Object Expression) { return false;}
		public static System.Boolean IsNumeric (System.Object Expression) { return false;}
		public static System.Int32 LBound (System.Array Array, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] System.Int32 Rank) { return 0;}
		public static System.Int32 UBound (System.Array Array, [System.Runtime.InteropServices.Optional] [System.ComponentModel.DefaultValue(1)] System.Int32 Rank) { return 0;}
		public static System.String TypeName (System.Object VarName) { return "";}
		public static System.String SystemTypeName (System.String VbName) { return "";}
		public static System.String VbTypeName (System.String UrtName) { return "";}
		public static System.Int32 QBColor (System.Int32 Color) { return 0;}
		public static System.Int32 RGB (System.Int32 Red, System.Int32 Green, System.Int32 Blue) { return 0;}
		public static Microsoft.VisualBasic.VariantType VarType (System.Object VarName) { return 0;}
		// Events
	};
}

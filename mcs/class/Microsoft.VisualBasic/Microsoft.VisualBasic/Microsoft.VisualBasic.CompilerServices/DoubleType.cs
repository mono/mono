//
// DoubleType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	sealed public class DoubleType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Double FromString (System.String Value) { return System.Double.Parse(Value); }
		[MonoTODO]
		public static System.Double FromString (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double FromObject (System.Object Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double FromObject (System.Object Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		public static System.Double Parse (System.String Value) { return System.Double.Parse(Value); }
		[MonoTODO]
		public static System.Double Parse (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		// Events
	};
}

//
// DecimalType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class DecimalType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		public static System.Decimal FromBoolean (System.Boolean Value) { throw new NotImplementedException (); }
		public static System.Decimal FromString (System.String Value) { return System.Decimal.Parse(Value); }
		[MonoTODO]
		public static System.Decimal FromString (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Decimal FromObject (System.Object Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Decimal FromObject (System.Object Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Decimal Parse (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		// Events
	};
}

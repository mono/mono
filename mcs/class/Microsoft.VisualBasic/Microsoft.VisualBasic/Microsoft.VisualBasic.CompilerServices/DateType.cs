//
// DateType.cs
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
	sealed public class DateType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.DateTime FromString (System.String Value) { return System.DateTime.Parse(Value); }
		[MonoTODO]
		public static System.DateTime FromString (System.String Value, System.Globalization.CultureInfo culture) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.DateTime FromObject (System.Object Value) { throw new NotImplementedException (); }
		// Events
	};
}

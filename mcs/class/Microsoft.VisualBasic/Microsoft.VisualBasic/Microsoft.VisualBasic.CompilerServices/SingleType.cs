//
// SingleType.cs
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
	sealed public class SingleType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Single FromString (System.String Value) { return System.Single.Parse(Value); }
		[MonoTODO]
		public static System.Single FromString (System.String Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Single FromObject (System.Object Value) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Single FromObject (System.Object Value, System.Globalization.NumberFormatInfo NumberFormat) { throw new NotImplementedException (); }
		// Events
	};
}

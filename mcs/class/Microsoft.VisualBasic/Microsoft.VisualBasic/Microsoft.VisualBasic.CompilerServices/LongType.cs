//
// LongType.cs
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
	sealed public class LongType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Int64 FromString (System.String Value) { return System.Int64.Parse(Value); }
		[MonoTODO]
		public static System.Int64 FromObject (System.Object Value) { throw new NotImplementedException (); }
		// Events
	};
}

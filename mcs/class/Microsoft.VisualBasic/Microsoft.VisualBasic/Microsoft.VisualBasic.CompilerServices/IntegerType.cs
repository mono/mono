//
// IntegerType.cs
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
	sealed public class IntegerType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Int32 FromString (System.String Value) { return System.Int32.Parse(Value); }
		[MonoTODO]
		public static System.Int32 FromObject (System.Object Value) { throw new NotImplementedException (); }
		// Events
	};
}

//
// ShortType.cs
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
	sealed public class ShortType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Int16 FromString (System.String Value) { return System.Int16.Parse(Value); }
		[MonoTODO]
		public static System.Int16 FromObject (System.Object Value) { throw new NotImplementedException (); }
		// Events
	};
}

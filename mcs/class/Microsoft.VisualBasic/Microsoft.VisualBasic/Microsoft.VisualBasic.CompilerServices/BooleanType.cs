//
// BooleanType.cs
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
	sealed public class BooleanType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Boolean FromString (System.String Value) { return System.Boolean.Parse(Value); }
		[MonoTODO]
		public static System.Boolean FromObject (System.Object Value) { throw new NotImplementedException(); }
		// Events
	};
}

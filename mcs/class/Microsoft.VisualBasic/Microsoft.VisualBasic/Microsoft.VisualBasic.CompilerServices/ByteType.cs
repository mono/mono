//
// ByteType.cs
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
	sealed public class ByteType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Byte FromString (System.String Value) { return System.Byte.Parse(Value); }
		[MonoTODO]
		public static System.Byte FromObject (System.Object Value) { throw new NotImplementedException(); }
		// Events
	};
}

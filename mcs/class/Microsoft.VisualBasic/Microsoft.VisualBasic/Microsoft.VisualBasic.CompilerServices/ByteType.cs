//
// ByteType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Chris J Breisch
//     2002 Tipic, Inc (http://www.tipic.com)
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

		public static System.Byte FromObject (System.Object Value) 
		{
			if ((object)Value == null) return 0;
			else return System.Convert.ToByte(Value);
		}

		// Events
	};
}

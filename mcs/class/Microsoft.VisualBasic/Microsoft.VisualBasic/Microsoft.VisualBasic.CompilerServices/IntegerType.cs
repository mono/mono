//
// IntegerType.cs
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
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	sealed public class IntegerType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Int32 FromString (System.String Value) { return System.Int32.Parse(Value); }

		public static System.Int32 FromObject (System.Object Value) 
		{ 
			if ((object)Value==null)
				return 0;
			else return System.Convert.ToInt32(Value);
		}
		// Events
	};
}

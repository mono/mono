//
// BooleanType.cs
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
	sealed public class BooleanType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Boolean FromString (System.String Value) { return System.Boolean.Parse(Value); }
		public static System.Boolean FromObject (System.Object Value) 
		{
			if ((object)Value == null) return false;
			//if (Value.GetType() == typeof(string)) return FromString((string)Value);
			else return System.Convert.ToBoolean(Value);
		}
		// Events
	};
}

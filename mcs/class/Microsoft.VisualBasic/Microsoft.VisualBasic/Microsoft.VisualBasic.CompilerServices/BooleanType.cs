//
// BooleanType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//   Joerg Rosenkranz (JoergR@voelcker.com)
//
// (C) 2002 Chris J Breisch
//     2002 Tipic, Inc (http://www.tipic.com)
//     2004 Joerg Rosenkranz
//

using System;
using System.Globalization;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class BooleanType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.Boolean FromString (System.String Value) 
		{
			if (string.Compare(Value, bool.TrueString, true) == 0)
				return true;
			
			if (string.Compare(Value, bool.FalseString, true) == 0)
				return false;
			
			double conv;
			if (double.TryParse(Value, NumberStyles.Any, NumberFormatInfo.InvariantInfo, out conv))
				return (conv != 0);
			
			throw new InvalidCastException (
				string.Format (
					"Cast from string \"{0}\" to type 'Boolean' is not valid.",
					Value));
		}
		
		public static System.Boolean FromObject (System.Object Value) 
		{
			if ((object)Value == null) return false;
			if (Value is string) 
				return FromString((string)Value);
			else 
				return System.Convert.ToBoolean(Value);
		}
		// Events
	};
}

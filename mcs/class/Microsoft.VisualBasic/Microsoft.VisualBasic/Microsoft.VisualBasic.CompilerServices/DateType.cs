//
// DateType.cs
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
	sealed public class DateType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.DateTime FromString (System.String Value) { return System.DateTime.Parse(Value); }
		public static System.DateTime FromString (System.String Value, System.Globalization.CultureInfo culture) 
		{ 
			return System.DateTime.Parse (Value,culture);
		}
		public static System.DateTime FromObject (System.Object Value) { 
			if ((object)Value ==null)
				return new DateTime(1,1,1);
			//if (Value.GetType() == typeof(string)) return FromString((string)Value);
			else return System.Convert.ToDateTime(Value);
		}
		// Events
	};
}

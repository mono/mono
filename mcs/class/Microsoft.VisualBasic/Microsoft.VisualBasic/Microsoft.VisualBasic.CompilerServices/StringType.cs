//
// StringType.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Francesco Delfino (pluto@tipic.com)
//
// (C) 2002 Chris J Breisch
//     2002 Tipic, Inc. (http://www.tipic.com)
//

using System;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class StringType {
		// Declarations
		// Constructors
		// Properties
		// Methods
		public static System.String FromBoolean (System.Boolean Value) { 
			return Convert.ToString(Value);
		}
		public static System.String FromByte (System.Byte Value) 
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromChar (System.Char Value) 
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromShort (System.Int16 Value) 
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromInteger (System.Int32 Value) 
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromLong (System.Int64 Value) 
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromSingle (System.Single Value) 
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromDouble (System.Double Value) 
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromSingle (System.Single Value, System.Globalization.NumberFormatInfo NumberFormat) 
		{ 
			return Convert.ToString(Value,NumberFormat);
		}
		public static System.String FromDouble (System.Double Value, System.Globalization.NumberFormatInfo NumberFormat) 
		{ 
			return Convert.ToString(Value,NumberFormat);
		}
		public static System.String FromDate (System.DateTime Value)
		{ 
			return Convert.ToString(Value);
		}
		public static System.String FromDecimal (System.Decimal Value) 
		{	 
			return Convert.ToString(Value);
		}
		public static System.String FromDecimal (System.Decimal Value, System.Globalization.NumberFormatInfo NumberFormat)
		{ 
			return Convert.ToString(Value,NumberFormat);
		}
		public static System.String FromObject (System.Object Value) 
		{
			if ((object)Value==null) return "";
			else return Convert.ToString(Value);
		}
		[MonoTODO("Last boolean parameter ignored")]
		public static System.Int32 StrCmp (System.String sLeft, System.String sRight, System.Boolean TextCompare) 
		{ 
			return sLeft.CompareTo(sRight);
		}
		[MonoTODO]
		public static System.Boolean StrLike (System.String Source, System.String Pattern, Microsoft.VisualBasic.CompareMethod CompareOption) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean StrLikeBinary (System.String Source, System.String Pattern) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Boolean StrLikeText (System.String Source, System.String Pattern) { throw new NotImplementedException (); }
		[MonoTODO]
		public static void MidStmtStr (ref System.String sDest, ref System.Int32 StartPosition, ref System.Int32 MaxInsertLength, ref System.String sInsert) { throw new NotImplementedException (); }
		// Events
	};
}

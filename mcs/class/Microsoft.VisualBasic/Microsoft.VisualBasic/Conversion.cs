//
// Conversion.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//
// (C) 2002 Chris J Breisch
//

using System;

namespace Microsoft.VisualBasic {
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class Conversion {
		// Declarations
		// Constructors
		// Properties
		// Methods
		[MonoTODO]
		public static System.String ErrorToString () { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String ErrorToString (System.Int32 ErrorNumber) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int16 Fix (System.Int16 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Fix (System.Int32 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int64 Fix (System.Int64 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double Fix (System.Double Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Single Fix (System.Single Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Decimal Fix (System.Decimal Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object Fix (System.Object Number) { throw new NotImplementedException (); }
		public static System.Int16 Int (System.Int16 Number) { return (System.Int16)Math.Floor(Number); }
		public static System.Int32 Int (System.Int32 Number) { return (System.Int16)Math.Floor(Number); }
		public static System.Int64 Int (System.Int64 Number) { return (System.Int16)Math.Floor(Number); }
		public static System.Double Int (System.Double Number) { return (System.Int16)Math.Floor(Number); }
		public static System.Single Int (System.Single Number) { return (System.Int16)Math.Floor(Number); }
		[MonoTODO]
		public static System.Decimal Int (System.Decimal Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Object Int (System.Object Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Hex (System.Int16 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Hex (System.Byte Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Hex (System.Int32 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Hex (System.Int64 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Hex (System.Object Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Oct (System.Int16 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Oct (System.Byte Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Oct (System.Int32 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Oct (System.Int64 Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Oct (System.Object Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.String Str (System.Object Number) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double Val (System.String InputStr) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Int32 Val (System.Char Expression) { throw new NotImplementedException (); }
		[MonoTODO]
		public static System.Double Val (System.Object Expression) { throw new NotImplementedException (); }
		// Events
	};
}

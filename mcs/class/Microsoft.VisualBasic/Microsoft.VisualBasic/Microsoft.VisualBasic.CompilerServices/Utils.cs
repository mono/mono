//
// Utils.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Rafael Teixeira (rafaelteixeirabr@hotmail.com)
//
// (C) 2002 Chris J Breisch
// (C) 2004 Rafael Teixeira
//

using System;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	[Microsoft.VisualBasic.CompilerServices.StandardModuleAttribute] 
	sealed public class Utils {

		static Utils() {
		}

		[MonoTODO]
		public static void ThrowException (System.Int32 hr) { 
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public static System.Object SetCultureInfo (System.Globalization.CultureInfo Culture) { 
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public static System.Array CopyArray (System.Array SourceArray, System.Array DestinationArray) { 
#if NET_1_1
			long SourceLength = SourceArray.LongLength;
			long DestinationLength = DestinationArray.LongLength;
			long LengthToCopy = (SourceLength < DestinationLength) ? SourceLength : DestinationLength;
			Array.Copy(SourceArray, DestinationArray, LengthToCopy); 
#else
			int SourceLength = SourceArray.Length;
			int DestinationLength = DestinationArray.Length;
			int LengthToCopy = (SourceLength < DestinationLength) ? SourceLength : DestinationLength;
			Array.Copy(SourceArray, DestinationArray, LengthToCopy); 
#endif
			return DestinationArray;
		}

		[MonoTODO]
		public static System.String MethodToString (System.Reflection.MethodBase Method) {
			 throw new NotImplementedException (); 
		}
		// Events
	};
}

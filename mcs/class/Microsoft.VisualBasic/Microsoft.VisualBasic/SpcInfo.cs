//
// SpcInfo.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//

using System.ComponentModel;

namespace Microsoft.VisualBasic {
	[EditorBrowsableAttribute(EditorBrowsableState.Never)] 
	public struct SpcInfo {
		// Declarations
		public short Count;

		internal SpcInfo(short value)
		{
			Count = value;
		}
	};
}

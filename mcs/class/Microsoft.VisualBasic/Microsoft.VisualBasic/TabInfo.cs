//
// TabInfo.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//

using System.Runtime.InteropServices;
using System.ComponentModel;

namespace Microsoft.VisualBasic {
	[EditorBrowsable(EditorBrowsableState.Never)] 
	public struct TabInfo {
		// Declarations
		public short Column;

		internal TabInfo(short value)
		{
			Column = value;
		}
	};
}

//
// HostServices.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net) 
//   Dennis Hayes (dennish@raytek.com)
//
// (C) 2002 Chris J Breisch
//     2004 Novell

using System;

namespace Microsoft.VisualBasic.CompilerServices 
{
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	sealed public class HostServices {
		private static IVbHost host = null;
		// Properties
		public static IVbHost VBHost {
			get {
				return host;
			}
			set {
				host = value;
			}
		}
	};
}

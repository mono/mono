//
// IVbHost.cs
//
// Author:
//   Chris J Breisch (cjbreisch@altavista.net)
//
// (C) 2002 Chris J Breisch
//
using System;
namespace Microsoft.VisualBasic.CompilerServices {
	[System.ComponentModel.EditorBrowsableAttribute(System.ComponentModel.EditorBrowsableState.Never)] 
	public interface IVbHost {
		// Methods
		string GetWindowTitle ();
		// IWin32Window GetParentWindow ();
	};
}

//
// System.Runtime.InteropServices.IDLFLAG.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

using System;

namespace System.Runtime.InteropServices
{
	[Flags, ComVisible (false)]
	public enum IDLFLAG
	{
		IDLFLAG_NONE = 0,
		IDLFLAG_FIN = 1,
		IDLFLAG_FOUT = 2,
		IDLFLAG_FLCID = 4,
		IDLFLAG_FRETVAL = 8
	}
}

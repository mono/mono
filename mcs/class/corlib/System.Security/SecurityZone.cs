//
// System.Security.SecurityZone.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

namespace System.Security {

	[Serializable]
	public enum SecurityZone {
		MyComputer = 0x0,
		Intranet,
		Trusted,
		Internet,
		Untrusted,
		NoZone = -1,
	}
}

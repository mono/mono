//
// System.Security.PolicyLevelType.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

namespace System.Security {

	[Serializable]
	public enum PolicyLevelType {
		User = 0x0,
		Machine,
		Enterprise,
		AppDomain,
	}
}

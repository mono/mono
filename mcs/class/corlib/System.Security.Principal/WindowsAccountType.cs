//
// System.Security.Principal.WindowsAccountType.cs
//
// Author:
//   Miguel de Icaza (miguel@ximian.com)
//
// (C) Ximian, Inc.  http://www.ximian.com
//

namespace System.Security.Principal {

	[Serializable]
	public enum WindowsAccountType {
		Normal,
		Guest,
		System,
		Anonymous
	}
}

//
// System.UriHostNameType.cs
//
// Author:
//    Miguel de Icaza (miguel@ximian.com)
//
// (C) 2001 Ximian, Inc.
//

using System;

namespace System {

	public enum UriHostNameType {
		Basic,
		Dns,
		IPv4,
		IPv6,
		Unknown
	};
}

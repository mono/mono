//
// System.Net.SecurityProtocolType.cs
//
// Authors
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//

namespace System.Net {

	[Flags]
	[Serializable]
#if NET_1_0
	internal
#else
	public
#endif
	enum SecurityProtocolType {

#if NET_1_2
		Default = -1073741824,
		Ssl2 = 12,
#endif
		Ssl3 = 48,
		Tls = 192
	}
}

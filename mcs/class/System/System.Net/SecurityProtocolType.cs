//
// System.Net.SecurityProtocolType.cs
//
// Author:
//   Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//

#if NET_1_1

namespace System.Net 
{
	[Flags]
	public enum SecurityProtocolType
	{
		Ssl3 = 48,
		Tls = 192
	}
}

#endif
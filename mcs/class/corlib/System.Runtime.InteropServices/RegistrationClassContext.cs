//
// System.Runtime.InteropServices.RegistrationClassContext
//
// Author:
//   Kazuki Oikawa  (kazuki@panicode.com)
//

#if NET_2_0

using System;

namespace System.Runtime.InteropServices
{
	[ComVisible (false)]
	public enum RegistrationClassContext
	{
		DisableActivateAsActivator = 32768,
		EnableActivateAsActivator = 65536,
		EnableCodeDownload = 8192,
		FromDefaultContext = 131072,
		InprocHandler = 2,
		InprocHandler16 = 32,
		InprocServer = 1,
		InprocServer16 = 8,
		LocalServer = 4,
		NoCodeDownload = 1024,
		NoCustomMarshal = 4096,
		NoFailureLog = 16384,
		RemoteServer = 16,
		Reserved1 = 64,
		Reserved2 = 128,
		Reserved3 = 256,
		Reserved4 = 512,
		Reserved5 = 2048
	}
}
#endif
using System;

namespace Novell.Directory.Ldap
{
	public enum AuthenticationTypes
	{
		Anonymous = 16,
		Delegation = 256,
	    Encryption = 2,
	    FastBind = 32,
	    None = 0,
	    ReadonlyServer = 4,
	    Sealing = 128,
	    Secure = 1,
	    SecureSocketsLayer = 2,
	    ServerBind = 512,
	    Signing = 64
	}
}

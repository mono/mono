//
// System.Management.AuthenticationLevel
//
// Authors:
//	Gonzalo Paniagua Javier (gonzalo@ximian.com)
//
// (C) 2003 Ximian, Inc (http://www.ximian.com)
//
namespace System.Management
{
	public enum AuthenticationLevel
	{
		Unchanged = -1,
		Default = 0,
		None = 1,
		Connect = 2,
		Call = 3,
		Packet = 4,
		PacketIntegrity = 5,
		PacketPrivacy = 6
	}
}


// System.Security.Policy.IIdentityPermissionFactory
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

namespace System.Security.Policy
{
	public interface IIdentityPermissionFactory
	{
		IPermission CreateIdentityPermission(Evidence evidence);
	}
}

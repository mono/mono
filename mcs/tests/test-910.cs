using System.Security;
using System.Security.Permissions;

[HostProtection]
delegate void D ();

[HostProtection]
class X
{
	public static void Main ()
	{
	}
}

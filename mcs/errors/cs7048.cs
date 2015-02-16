// CS7048: First argument of a security attribute `CustomSecurityAttribute' must be a valid SecurityAction
// Line: 20

using System.Security;
using System.Security.Permissions;

public class CustomSecurityAttribute : CodeAccessSecurityAttribute
{
	public CustomSecurityAttribute ()
		: base (SecurityAction.Demand)
	{
	}

	public override IPermission CreatePermission()
	{
		return null;
	}
}

[CustomSecurity]
class X
{
	public static void Main ()
	{
	}
}

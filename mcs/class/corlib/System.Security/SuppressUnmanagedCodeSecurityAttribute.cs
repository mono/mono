//
// System.Security.SuppressUnmanagedCodeSecurityAttribute.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

namespace System.Security {

	[AttributeUsage (AttributeTargets.Class | AttributeTargets.Method |
			 AttributeTargets.Interface)]
	public sealed class SuppressUnmanagedCodeSecurityAttribute : Attribute {}
}

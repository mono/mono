// System.Security.Policy.IMembershipCondition.cs
//
// Nick Drochak (ndrochak@gol.com)
//
// (C) 2001 Nick Drochak

namespace System.Security.Policy
{
	public interface IMembershipCondition : ISecurityEncodable,
		ISecurityPolicyEncodable
		{
			bool Check(Evidence evidence);
			IMembershipCondition Copy();
			string ToString();
		}
}


//
// System.Security.ISecurityPolicyEncodable.cs
//
// Author:
//   Nick Drochak(ndrochak@gol.com)
//
// (C) Nick Drochak
//

using System.Security.Policy;

namespace System.Security {

	interface ISecurityPolicyEncodable {

		void FromXml (SecurityElement e, PolicyLevel level);

		SecurityElement ToXml (PolicyLevel level);
	}
}

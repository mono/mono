// System.Security.Policy.ApplicationDirectoryMembershipCondition
//
// Author(s):
//   Nick Drochak (ndrochak@gol.com)
//
// (C) 2002 Nick Drochak, All rights reserved.

using System.Security;

namespace System.Security.Policy
{

	// FIXME: This class is mostly just method stubs.

	[Serializable]
	public sealed class ApplicationDirectoryMembershipCondition :
		IMembershipCondition, 
		ISecurityEncodable, 
		ISecurityPolicyEncodable
	{
		// Methods
		[MonoTODO]
		public bool Check(Evidence evidence) { 
			throw new NotImplementedException (); 
		}

		[MonoTODO]
		public IMembershipCondition Copy() { 
			throw new NotImplementedException (); 
		}
		
		public override bool Equals(object o) { 
			return o is ApplicationDirectoryMembershipCondition; 
		}
		
		[MonoTODO]
		public void FromXml(SecurityElement e) { 
			throw new NotImplementedException (); 
		}
		
		[MonoTODO]
		public void FromXml(SecurityElement e, PolicyLevel level) { 
			throw new NotImplementedException (); 
		}
		
		[MonoTODO]
		public override int GetHashCode() { 
			throw new NotImplementedException (); 
		}
		
		[MonoTODO]
		public override string ToString() { 
			throw new NotImplementedException (); 
		}
		
		[MonoTODO]
		public SecurityElement ToXml() { 
			throw new NotImplementedException (); 
		}
		
		[MonoTODO]
		public SecurityElement ToXml(PolicyLevel level) { 
			throw new NotImplementedException (); 
		}
	}
}
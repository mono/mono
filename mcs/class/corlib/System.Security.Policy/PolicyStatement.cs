//
// System.Security.Policy.PolicyStatement
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//

namespace System.Security.Policy {

	[Serializable]
	public sealed class PolicyStatement : ISecurityEncodable, ISecurityPolicyEncodable {
		public PolicyStatement (PermissionSet perms) :
			this (perms, PolicyStatementAttribute.Nothing)
		{
		}

		public PolicyStatement (PermissionSet perms, PolicyStatementAttribute attrs) {
			this.perms = perms;
			this.attrs = attrs;
		}
		
		public PermissionSet PermissionSet {
			get { return perms; }
			set { perms = value; }
		}
		
		public PolicyStatementAttribute Attributes {
			get { return attrs; }
			set { attrs = value; }
		}

		public string AttributeString {
			get { return attrs.ToString ("F"); }
		}

		public PolicyStatement Copy ()
		{
			return new PolicyStatement (perms, attrs);
		}

		// ISecurityEncodable

		[MonoTODO]
		public void FromXml (SecurityElement e) {
		}

		[MonoTODO]
		public void FromXml (SecurityElement e, PolicyLevel level) {
		}
		
		[MonoTODO]
		public SecurityElement ToXml () {
			return null;
		}

		[MonoTODO]
		public SecurityElement ToXml (PolicyLevel level) {
			return null;
		}

		private PermissionSet perms;
		private PolicyStatementAttribute attrs;
	}
}

//
// System.Security.Policy.PolicyStatement
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//

using System.Security.Permissions;

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

		public void FromXml (SecurityElement e)
		{
			FromXml (e, null);
		}

		public void FromXml (SecurityElement e, PolicyLevel level)
		{
			SecurityElement permissions = e.SearchForChildByTag ("PermissionSet");

			string attributes = e.Attribute ("Attributes");

			if (attributes != null)
				attrs = (PolicyStatementAttribute) Enum.Parse (
					typeof (PolicyStatementAttribute), attributes);
				
			perms = new PermissionSet (PermissionState.None);
			perms.FromXml (permissions);
		}
		
		public SecurityElement ToXml ()
		{
			return ToXml (null);
		}

		public SecurityElement ToXml (PolicyLevel level)
		{
			SecurityElement element = new SecurityElement ("PolicyStatement");
			element.AddAttribute ("version", "1");

			if (attrs != PolicyStatementAttribute.Nothing)
				element.AddAttribute ("Attributes", attrs.ToString ());
			
			element.AddChild (perms.ToXml ());

			return element;
		}

		private PermissionSet perms;
		private PolicyStatementAttribute attrs;
	}
}

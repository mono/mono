//
// System.Security.Policy.PermissionRequestEvidence.cs
//
// Authors:
//      Nick Drochak (ndrochak@gol.com)
//
// (C) 2003 Nick Drochak
//

using System.Text;

namespace System.Security.Policy
{
	[Serializable]
	public sealed class PermissionRequestEvidence {
		PermissionSet requested, optional, denied;

		public PermissionRequestEvidence(PermissionSet requested, 
				PermissionSet optional, PermissionSet denied) {
			this.requested = requested;
			this.optional = optional;
			this.denied = denied;
		}

		public PermissionSet DeniedPermissions {
			get {return denied;}
		}

		public PermissionSet OptionalPermissions {
			get {return optional;}
		}

		public PermissionSet RequestedPermissions {
			get {return requested;}
		}

		public PermissionRequestEvidence Copy() {
			return new PermissionRequestEvidence (requested, optional, denied);
		}

		public override string ToString() {
			// Cannot use XML classes in corlib, so do it by hand
			StringBuilder sb = new StringBuilder ();

			sb.Append ("<System.Security.Policy.PermissionRequestEvidence version=\"1\">");
			sb.Append ("<Request>");
			sb.Append ("<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\"");
			if (requested.IsUnrestricted ())
				sb.Append (" Unrestricted=\"true\"");
			sb.Append (@"/>");  
			sb.Append (@"</Request>");

			sb.Append ("<Optional>");
			sb.Append ("<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\"");
			if (optional.IsUnrestricted ())
				sb.Append (" Unrestricted=\"true\"");
			sb.Append (@"/>");  
			sb.Append (@"</Optional>");

			sb.Append ("<Denied>");
			sb.Append ("<PermissionSet class=\"System.Security.PermissionSet\" version=\"1\"");
			if (denied.IsUnrestricted ())
				sb.Append (" Unrestricted=\"true\"");
			sb.Append (@"/>");  
			sb.Append (@"</Denied>");

			sb.Append ("</System.Security.Policy.PermissionRequestEvidence>");

			return sb.ToString ();
		}

	}
}
//
// System.Security.Policy.PermissionRequestEvidence.cs
//
// Authors:
//      Nick Drochak (ndrochak@gol.com)
//
// (C) 2003 Nick Drochak
//

using System.Security;
using System.Text;

namespace System.Security.Policy
{
	[Serializable]
	public sealed class PermissionRequestEvidence : IBuiltInEvidence {
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

		public PermissionRequestEvidence Copy ()
		{
			return new PermissionRequestEvidence (requested, optional, denied);
		}

		public override string ToString () 
		{
			SecurityElement se = new SecurityElement ("System.Security.Policy.PermissionRequestEvidence");
			se.AddAttribute ("version", "1");

			if (requested != null) {
				SecurityElement requestElement = new SecurityElement ("Request");
				requestElement.AddChild (requested.ToXml ());
				se.AddChild (requestElement);
			}
			if (optional != null) {
				SecurityElement optionalElement = new SecurityElement ("Optional");
				optionalElement.AddChild (optional.ToXml ());
				se.AddChild (optionalElement);
			}
			if (denied != null) {
				SecurityElement deniedElement = new SecurityElement ("Denied");
				deniedElement.AddChild (denied.ToXml ());
				se.AddChild (deniedElement);
			}
			return se.ToString ();
		}

		// interface IBuiltInEvidence

		[MonoTODO]
		int IBuiltInEvidence.GetRequiredSize (bool verbose) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.InitFromBuffer (char [] buffer, int position) 
		{
			return 0;
		}

		[MonoTODO]
		int IBuiltInEvidence.OutputToBuffer (char [] buffer, int position, bool verbose) 
		{
			return 0;
		}

	}
}

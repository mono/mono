//
// System.Security.NamedPermissionSet
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot (spouliot@motus.com)
//
// (C) 2002
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
//

using System;
using System.Security.Permissions;

namespace System.Security {
	
	[Serializable]
	public sealed class NamedPermissionSet : PermissionSet {

		public NamedPermissionSet (string name, PermissionSet set) : base (set) 
		{
			Name = name;
		}

		public NamedPermissionSet (string name, PermissionState state) : base (state) 
		{
			Name = name;
		}

		public NamedPermissionSet (NamedPermissionSet set) : this (set.name, set) {}

		public NamedPermissionSet (string name) : this (name, PermissionState.None) {}

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public string Name {
			get { return name; }
			set { 
				if ((name == null) || (name == String.Empty)) 
					throw new ArgumentException ("invalid name");
				name = value; 
			}
		}

		public override PermissionSet Copy () 
		{
			return new NamedPermissionSet (this);
		}

		public NamedPermissionSet Copy (string name) 
		{
			NamedPermissionSet nps = new NamedPermissionSet (this);
			nps.Name = name;
			return nps;
		}

		public override void FromXml (SecurityElement e) 
		{
			FromXml (e, "System.Security.NamedPermissionSet");
			Name = (e.Attributes ["Name"] as string);
			Description = (e.Attributes ["Description"] as string);
		}

		public override SecurityElement ToXml () 
		{
			SecurityElement se = base.ToXml ();
			if (name != null)
				se.AddAttribute ("Name", name);
			if (description != null)
				se.AddAttribute ("Description", description);
			return se;
		}

		// private

		private string name;
		private string description;
	}
}

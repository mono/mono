//
// System.Security.NamedPermissionSet
//
// Author:
//   Dan Lewis (dihlewis@yahoo.co.uk)
//
// (C) 2002
//
// Stubbed.
//

using System;
using System.Security.Permissions;

namespace System.Security {
	
	[MonoTODO]
	public sealed class NamedPermissionSet : PermissionSet {
		public NamedPermissionSet (string name, PermissionSet set) : base (set) {
			this.name = name;
			this.description = "";
		}

		public NamedPermissionSet (string name, PermissionState state) : base (state) {
			this.name = name;
			this.description = "";
		}

		public NamedPermissionSet (NamedPermissionSet set) : this (set.name, set) {
		}

		public NamedPermissionSet (string name) : this (name, PermissionState.None) {
		}

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public string Name {
			get { return name; }
			set { name = value; }
		}

		public override PermissionSet Copy () {
			return null;
		}

		public NamedPermissionSet Copy (string name) {
			return null;
		}

		public override void FromXml (SecurityElement e) {
		}

		public override SecurityElement ToXml () {
			return null;
		}

		// private

		private string name;
		private string description;
	}
}

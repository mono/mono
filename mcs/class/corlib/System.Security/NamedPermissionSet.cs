//
// System.Security.NamedPermissionSet
//
// Authors:
//	Dan Lewis (dihlewis@yahoo.co.uk)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002
// Portions (C) 2003, 2004 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004 Novell, Inc (http://www.novell.com)
//
// Permission is hereby granted, free of charge, to any person obtaining
// a copy of this software and associated documentation files (the
// "Software"), to deal in the Software without restriction, including
// without limitation the rights to use, copy, modify, merge, publish,
// distribute, sublicense, and/or sell copies of the Software, and to
// permit persons to whom the Software is furnished to do so, subject to
// the following conditions:
// 
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF
// MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE
// LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION
// OF CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION
// WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
//

using System.Security.Permissions;

namespace System.Security {
	
	[Serializable]
	public sealed class NamedPermissionSet : PermissionSet {

		private string name;
		private string description;

		// for PolicyLevel (to avoid validation duplication)
		internal NamedPermissionSet ()
			: base ()
		{
		}

		public NamedPermissionSet (string name, PermissionSet set) 
			: base (set) 
		{
			Name = name;
		}

		public NamedPermissionSet (string name, PermissionState state) 
			: base (state) 
		{
			Name = name;
		}

		public NamedPermissionSet (NamedPermissionSet set) 
			: base (set)
		{
			name = set.name; // name can be null here
			description = set.description;
		}

		public NamedPermissionSet (string name) 
			: this (name, PermissionState.None)
		{
		}

		// properties

		public string Description {
			get { return description; }
			set { description = value; }
		}

		public string Name {
			get { return name; }
			set { 
				if ((value == null) || (value == String.Empty)) {
					throw new ArgumentException (Locale.GetText ("invalid name"));
				}
				name = value; 
			}
		}

		// methods

		public override PermissionSet Copy () 
		{
			return new NamedPermissionSet (this);
		}

		public NamedPermissionSet Copy (string name) 
		{
			NamedPermissionSet nps = new NamedPermissionSet (this);
			nps.Name = name;		// get the new name
			return nps;
		}

		public override void FromXml (SecurityElement e) 
		{
			FromXml (e, "NamedPermissionSet");
			// strangely it can import a null Name (bypassing property setter)
			name = e.Attribute ("Name");
			description = e.Attribute ("Description");
			if (description == null)
				description = String.Empty;
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

#if NET_2_0
		public override bool Equals (object obj)
		{
			if (obj == null)
				return false;
			NamedPermissionSet nps = (obj as NamedPermissionSet);
			if (nps == null)
				return false;
			// description isn't part of the comparaison
			return ((name == nps.Name) && base.Equals (obj));
		}

		public override int GetHashCode ()
		{
			int hc = base.GetHashCode ();
			// name is part of the hash code (except when null)
			if (name != null)
				hc ^= name.GetHashCode ();
			// description is never part of the hash code
			return hc;
		}
#endif
	}
}

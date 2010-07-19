//
// System.Web.UI.WebControls.RoleGroupCollection class
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

#if NET_2_0

using System.Collections;
using System.ComponentModel;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.Web.UI.WebControls
{
	// CAS (no InheritanceDemand for sealed class)
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	// attributes
	[Editor ("System.Web.UI.Design.WebControls.RoleGroupCollectionEditor, " + Consts.AssemblySystem_Design, "System.Drawing.Design.UITypeEditor, " + Consts.AssemblySystem_Drawing)]
	public sealed class RoleGroupCollection : CollectionBase
	{
		public RoleGroupCollection ()
		{
		}


		public RoleGroup this [int index] {
			get { return (RoleGroup) List [index]; }
		}


		public void Add (RoleGroup group)
		{
			List.Add (group);
		}

		public bool Contains (RoleGroup group)
		{
			return List.Contains (group);
		}

		public void CopyTo (RoleGroup[] array, int index)
		{
			if (array == null)
				throw new ArgumentNullException ("array");
			if (index < 0)
				throw new ArgumentException (Locale.GetText ("Negative index."), "index");
			if (this.Count <= array.Length - index)
				throw new ArgumentException (Locale.GetText ("Destination isn't large enough to copy collection."), "array");

			for (int i=0; i < Count; i++)
				array [i + index] = this [i];
		}

		public RoleGroup GetMatchingRoleGroup (IPrincipal user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			if (Count > 0) {
				foreach (RoleGroup rg in this) {
					if (rg.ContainsUser (user))
						return rg;
				}
			}
			return null;
		}

		public int IndexOf (RoleGroup group)
		{
			return List.IndexOf (group);
		}

		public void Insert (int index, RoleGroup group)
		{
			List.Insert (index, group);
		}

		protected override void OnValidate (object value)
		{
			// LAMESPEC: undocumented
			//
			// What do we validate here?
			base.OnValidate (value);
		}
		
		public void Remove (RoleGroup group)
		{
			// note: checks required or we'll throw more exceptions :(
			if (group != null) {
				if (Contains (group))
					List.Remove (group);
			}
		}
	}
}

#endif

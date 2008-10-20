//
// System.Web.UI.WebControls.RoleGroup class
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

using System.ComponentModel;
using System.Security.Permissions;
using System.Security.Principal;

namespace System.Web.UI.WebControls {

	// CAS (no InheritanceDemand for sealed class)
	[AspNetHostingPermission (SecurityAction.LinkDemand, Level = AspNetHostingPermissionLevel.Minimal)]
	public sealed class RoleGroup 
	{
		ITemplate contentTemplate;
		string[] roles;

		public RoleGroup ()
		{
		}

		[Browsable (false)]
		[DefaultValue (null)]
		[PersistenceMode (PersistenceMode.InnerProperty)]
		[TemplateContainer (typeof (LoginView))]
		public ITemplate ContentTemplate {
			get { return contentTemplate; }
			set { contentTemplate = value; }
		}

		// LAMESPEC (beta2) : default value isn't null
		[TypeConverter (typeof (StringArrayConverter))]
		public string[] Roles {
			get {
				if (roles == null)
					roles = new string [0];
				return roles;
			}
			set { roles = value; }
		}


		public bool ContainsUser (IPrincipal user)
		{
			if (user == null)
				throw new ArgumentNullException ("user");

			if (roles != null) {
				foreach (string role in roles) {
					if (user.IsInRole (role))
						return true;
				}
			}
			return false;
		}

		public override string ToString ()
		{
			if ((roles == null) || (roles.Length == 0)) {
				return String.Empty;
			} else if (roles.Length == 1) {
				return roles [0];
			} else {
				return String.Join (",", roles);
			}
		}
	}
}

#endif

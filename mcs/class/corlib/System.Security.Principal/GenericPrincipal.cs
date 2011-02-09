//
// System.Security.Principal.GenericPrincipal.cs
//
// Authors:
//	Miguel de Icaza (miguel@ximian.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) Ximian, Inc.  http://www.ximian.com
// Copyright (C) 2004-2006 Novell, Inc (http://www.novell.com)
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

using System.Runtime.InteropServices;

namespace System.Security.Principal {

	[Serializable]
	[ComVisible (true)]
	public class GenericPrincipal : IPrincipal {

		// field names are serialization compatible with .net
		private IIdentity m_identity;
		private string[] m_roles;
		
		public GenericPrincipal (IIdentity identity, string [] roles)
		{
			if (identity == null)
				throw new ArgumentNullException ("identity");

			m_identity = identity;
			if (roles != null) {
				// make our own (unchangeable) copy of the roles
				m_roles = new string [roles.Length];
				for (int i=0; i < roles.Length; i++)
					m_roles [i] = roles [i];
			}
		}

		internal string [] Roles {
			get { return m_roles; }
		}

		public virtual IIdentity Identity {
			get { return m_identity; }
		}

		public virtual bool IsInRole (string role)
		{
			if (m_roles == null)
				return false;

			int l = role.Length;
			foreach (string r in m_roles) {
				if ((r != null) && (l == r.Length)) {
					if (String.Compare (role, 0, r, 0, l, true) == 0)
						return true;
				}
			}
			return false;
		}
	}
}

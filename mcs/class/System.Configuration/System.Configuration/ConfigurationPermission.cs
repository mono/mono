//
// System.Configuration.ConfigurationPermission.cs
//
// Author:
//      Chris Toshok <toshok@ximian.com>
//
//
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

#if NET_2_0

using System.Security;
using System.Security.Permissions;

namespace System.Configuration {

	[Serializable]
	public sealed class ConfigurationPermission : CodeAccessPermission, IUnrestrictedPermission
	{
		public ConfigurationPermission (PermissionState state)
		{
                        unrestricted = (state == PermissionState.Unrestricted);
		}

		[MonoTODO]
		public override IPermission Copy ()
		{
			throw new NotImplementedException ();
		}

		public override void FromXml (SecurityElement securityElement)
		{
			if (securityElement == null)
				throw new ArgumentNullException ("securityElement");

                        // LAMESPEC: it says to throw an ArgumentNullException in this case
                        if (securityElement.Tag != "IPermission")
                                throw new ArgumentException ("securityElement");

                        string unrestricted = securityElement.Attribute ("Unrestricted");
                        if (unrestricted != null) {
                                this.unrestricted = (String.Compare (unrestricted, "true", true) == 0);
                        }
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target)
		{
			throw new NotImplementedException ();
		}

		public bool IsUnrestricted ()
		{
			return unrestricted;
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement root = new SecurityElement ("IPermission");
			root.AddAttribute ("class", this.GetType().AssemblyQualifiedName);
			root.AddAttribute ("version", "1");
			if (unrestricted) {
				root.AddAttribute ("Unrestricted", "true");
			}
			return root;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target)
		{
			throw new NotImplementedException ();
		}

		bool unrestricted;
	}

}
#endif

//
// System.Net.DnsPermission.cs
//
// Authors:
//	Lawrence Pit (loz@cable.a2000.nl)
//	Sebastien Pouliot  <sebastien@ximian.com>
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

using System.Collections;
using System.Security;
using System.Security.Permissions;

namespace System.Net {

	[Serializable]
	public sealed class DnsPermission : CodeAccessPermission, IUnrestrictedPermission {

		private const int version = 1;

		// Fields
		bool m_noRestriction;
		
		// Constructors
		public DnsPermission (PermissionState state)
			: base () 
		{						
			m_noRestriction = (state == PermissionState.Unrestricted);
		}
		
		// Methods
				
		public override IPermission Copy ()
		{
			return new DnsPermission (m_noRestriction ? PermissionState.Unrestricted : PermissionState.None);		
		}
		
		public override IPermission Intersect (IPermission target)
		{
			DnsPermission dp = Cast (target);
			if (dp == null)
				return null;
			if (IsUnrestricted () && dp.IsUnrestricted ())
				return new DnsPermission (PermissionState.Unrestricted);
			return null;
		}
		
		public override bool IsSubsetOf (IPermission target) 
		{
			DnsPermission dp = Cast (target);
			if (dp == null)
				return IsEmpty ();

			return (dp.IsUnrestricted () || (m_noRestriction == dp.m_noRestriction));
		}

		public bool IsUnrestricted () 
		{
			return this.m_noRestriction;
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = PermissionHelper.Element (typeof (DnsPermission), version);
			if (m_noRestriction)
				se.AddAttribute ("Unrestricted", "true");				
			return se;
		}
		
		public override void FromXml (SecurityElement securityElement)
		{
			PermissionHelper.CheckSecurityElement (securityElement, "securityElement", version, version);
		
			// LAMESPEC: it says to throw an ArgumentNullException in this case				
			if (securityElement.Tag != "IPermission")
				throw new ArgumentException ("securityElement");
				
			this.m_noRestriction = PermissionHelper.IsUnrestricted (securityElement);
		}		
		
		public override IPermission Union (IPermission target) 
		{
			DnsPermission dp = Cast (target);
			if (dp == null)
				return Copy ();
			if (IsUnrestricted () || dp.IsUnrestricted ())
				return new DnsPermission (PermissionState.Unrestricted);
			else
				return new DnsPermission (PermissionState.None);
		}

		// Internal helpers methods

		private bool IsEmpty ()
		{
			return !m_noRestriction;
		}

		private DnsPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			DnsPermission dp = (target as DnsPermission);
			if (dp == null) {
				PermissionHelper.ThrowInvalidPermission (target, typeof (DnsPermission));
			}

			return dp;
		}
	}
}

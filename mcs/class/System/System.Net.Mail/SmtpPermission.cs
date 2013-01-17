//
// System.Net.Mail.SmtpPermission
//
// Authors:
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// Copyright (C) 2006 Novell, Inc (http://www.novell.com)
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


using System.Security;
using System.Security.Permissions;

namespace System.Net.Mail {

	[Serializable]
	public sealed class SmtpPermission : CodeAccessPermission, IUnrestrictedPermission {

		private const int version = 1;

		private bool unrestricted;
		private SmtpAccess access;


		public SmtpPermission (bool unrestricted)
			: base ()
		{
			this.unrestricted = unrestricted;
			access = unrestricted ? SmtpAccess.ConnectToUnrestrictedPort : SmtpAccess.None;
		}

		public SmtpPermission (PermissionState state)
			: base ()
		{
			unrestricted = (state == PermissionState.Unrestricted);
			access =  unrestricted ? SmtpAccess.ConnectToUnrestrictedPort : SmtpAccess.None;
		}

		public SmtpPermission (SmtpAccess access)
			: base ()
		{
			// this ctor can accept invalid enum values
			this.access = access;
		}
		

		public SmtpAccess Access {
			get { return access; }
		}


		public void AddPermission (SmtpAccess access)
		{
			if (!unrestricted && (access > this.access)) {
				this.access = access;
			}
		}

		public override IPermission Copy ()
		{
			if (unrestricted) {
				return new SmtpPermission (true);
			} else {
				return new SmtpPermission (access);
			}
		}

		public override IPermission Intersect (IPermission target)
		{
			SmtpPermission sp = Cast (target);
			if (sp == null)
				return null;

			if (unrestricted && sp.unrestricted)
				return new SmtpPermission (true);
			else if (access > sp.access)
				return new SmtpPermission (sp.access);
			else
				return new SmtpPermission (access);
		}
		
		public override bool IsSubsetOf (IPermission target) 
		{
			SmtpPermission sp = Cast (target);
			if (sp == null)
				return IsEmpty ();

			if (unrestricted) {
				return sp.unrestricted;
			} else {
				return (access <= sp.access);
			}
		}

		public bool IsUnrestricted () 
		{
			return unrestricted;
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement se = PermissionHelper.Element (typeof (SmtpPermission), version);
			if (unrestricted) {
				se.AddAttribute ("Unrestricted", "true");
			} else {
				switch (access) {
				case SmtpAccess.ConnectToUnrestrictedPort:
					se.AddAttribute ("Access", "ConnectToUnrestrictedPort");
					break;
				case SmtpAccess.Connect:
					se.AddAttribute ("Access", "Connect");
					break;
				// note: SmtpAccess.None and invalid values aren't serialized to XML
				}
			}
			return se;
		}
		
		public override void FromXml (SecurityElement securityElement)
		{
			PermissionHelper.CheckSecurityElement (securityElement, "securityElement", version, version);
		
			// LAMESPEC: it says to throw an ArgumentNullException in this case				
			if (securityElement.Tag != "IPermission")
				throw new ArgumentException ("securityElement");
				
			if (PermissionHelper.IsUnrestricted (securityElement))
				access = SmtpAccess.Connect;
			else
				access = SmtpAccess.None;
		}		
		
		public override IPermission Union (IPermission target) 
		{
			SmtpPermission sp = Cast (target);
			if (sp == null)
				return Copy ();

			if (unrestricted || sp.unrestricted)
				return new SmtpPermission (true);
			else if (access > sp.access)
				return new SmtpPermission (access);
			else
				return new SmtpPermission (sp.access);
		}

		// Internal helpers methods

		private bool IsEmpty ()
		{
			return (!unrestricted && (access == SmtpAccess.None));
		}

		private SmtpPermission Cast (IPermission target)
		{
			if (target == null)
				return null;

			SmtpPermission sp = (target as SmtpPermission);
			if (sp == null) {
				PermissionHelper.ThrowInvalidPermission (target, typeof (SmtpPermission));
			}

			return sp;
		}
	}
}


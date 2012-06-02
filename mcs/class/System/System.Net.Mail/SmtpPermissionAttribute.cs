//
// System.Net.Mail.SmtpPermissionAttribute
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

	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct 
		| AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	[Serializable]
	public sealed class SmtpPermissionAttribute : CodeAccessSecurityAttribute {

		private string access;

		
		public SmtpPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}


		public string Access {
			get { return access; }
			set { access = value; }
		}


		private SmtpAccess GetSmtpAccess ()
		{
			if (access == null)
				return SmtpAccess.None;

			switch (access.ToLowerInvariant ()) {
			case "connecttounrestrictedport":
				return SmtpAccess.ConnectToUnrestrictedPort;
			case "connect":
				return SmtpAccess.Connect;
			case "none":
				return SmtpAccess.None;
			default:
				string s = Locale.GetText ("Invalid Access='{0}' value.", access);
				throw new ArgumentException ("Access", s);
			}
		}

		public override IPermission CreatePermission ()
		{
			if (Unrestricted)
				return new SmtpPermission (true);

			return new SmtpPermission (GetSmtpAccess ());
		}
	}
}


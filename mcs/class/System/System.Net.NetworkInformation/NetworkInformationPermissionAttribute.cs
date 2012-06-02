//
// System.Net.NetworkInformation.NetworkInformationPermissionAttribute
//
// Author:
//	Gonzalo Paniagua Javier (gonzalo@novell.com)
//
// Copyright (c) 2006 Novell, Inc. (http://www.novell.com)
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
using System;
using System.Security;
using System.Security.Permissions;

namespace System.Net.NetworkInformation {
	[Serializable]
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Struct | AttributeTargets.Constructor | AttributeTargets.Method, AllowMultiple = true, Inherited = false)]
	public sealed class NetworkInformationPermissionAttribute : System.Security.Permissions.CodeAccessSecurityAttribute {
		string access;

		public NetworkInformationPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}

		[MonoTODO ("verify implementation")]
		public override IPermission CreatePermission ()
		{
			NetworkInformationAccess a = NetworkInformationAccess.None;
			switch (Access) {
			case "Read":
				a = NetworkInformationAccess.Read;
				break;
			case "Full":
				a = NetworkInformationAccess.Read | NetworkInformationAccess.Ping;
				break;
			}
			return new NetworkInformationPermission (a);
		}

		public string Access {
			get { return access; }
			set {
				switch (access) {
				case "Read":
				case "Full":
				case "None":
					break;
				default:
					throw new ArgumentException ("Only 'Read', 'Full' and 'None' are allowed");
				}
				access = value; 
			}
		}
	}
}


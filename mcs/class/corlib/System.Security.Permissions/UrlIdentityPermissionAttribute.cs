//
// System.Security.Permissions.UrlIdentityPermissionAttribute.cs
//
// Authors:
//	Duncan Mak <duncan@ximian.com>
//	Sebastien Pouliot  <sebastien@ximian.com>
//
// (C) 2002 Ximian, Inc. http://www.ximian.com
// Portions (C) 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2004-2005 Novell, Inc (http://www.novell.com)
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

namespace System.Security.Permissions {

#if NET_2_0
	[ComVisible (true)]
#endif
	[AttributeUsage (AttributeTargets.Assembly | AttributeTargets.Class |
			 AttributeTargets.Struct | AttributeTargets.Constructor |
			 AttributeTargets.Method, AllowMultiple=true, Inherited=false)]
	[Serializable]
	public sealed class UrlIdentityPermissionAttribute : CodeAccessSecurityAttribute {

		// Fields
		private string url;
		
		// Constructor
		public UrlIdentityPermissionAttribute (SecurityAction action)
			: base (action)
		{
		}
		
		// Properties
		public string Url {
			get { return url; }
			set { url = value; }
		}
		
		// Methods
		public override IPermission CreatePermission ()
		{
			if (this.Unrestricted)
				return new UrlIdentityPermission (PermissionState.Unrestricted);
			// Note: It is possible to create a permission with a 
			// null URL but not to create a UrlIdentityPermission (null)
			else if (url == null)
				return new UrlIdentityPermission (PermissionState.None);
			else
				return new UrlIdentityPermission (url);
		}
	}
}

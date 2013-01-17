//
// System.Web.AspNetHostingPermissionAttribute.cs
//
// Authors:
//	Andreas Nahr (ClassDevelopment@A-SoftTech.com)
//	Sebastien Pouliot  <sebastien@ximian.com>
//
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

using System.Security;
using System.Security.Permissions;

namespace System.Web {

	[Serializable]
	[AttributeUsage (AttributeTargets.All, AllowMultiple = true, Inherited = false)]
	public sealed class AspNetHostingPermissionAttribute : CodeAccessSecurityAttribute {

		private AspNetHostingPermissionLevel _level;

		public AspNetHostingPermissionAttribute (SecurityAction action)
			: base (action)
		{
			// LAMESPEC: seems to initialize to None
			_level = AspNetHostingPermissionLevel.None;
		}

		public override IPermission CreatePermission ()
		{
			if (base.Unrestricted)
				return new AspNetHostingPermission (PermissionState.Unrestricted);
			else
				return new AspNetHostingPermission (_level);
		}

		public AspNetHostingPermissionLevel Level {
			get { return _level; }
			set {
				if ((value < AspNetHostingPermissionLevel.None) || (value > AspNetHostingPermissionLevel.Unrestricted)) {
					string msg = Locale.GetText ("Invalid enum {0}.");
					throw new ArgumentException (String.Format (msg, value), "Level");
				}
				_level = value;
			}
		}
	}
}

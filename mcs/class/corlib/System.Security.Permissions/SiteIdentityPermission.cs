//
// System.Security.Permissions.SiteIdentityPermission.cs
//
// Author
//	Sebastien Pouliot  <spouliot@motus.com>
//
// Copyright (C) 2003 Motus Technologies. http://www.motus.com
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

using System;
using System.Globalization;

namespace System.Security.Permissions {

	[Serializable]
	public sealed class SiteIdentityPermission : CodeAccessPermission, IBuiltInPermission {

		private string _site;

		// Constructors

		public SiteIdentityPermission (PermissionState state) 
		{
			if (state == PermissionState.Unrestricted)
				throw new ArgumentException ("Unsupported PermissionState.Unrestricted for Identity Permission");
		}

		public SiteIdentityPermission (string site) 
		{
			_site = site;
		}

		// Properties

		public string Site {
			get { 
				if (_site == null)
					throw new NullReferenceException ("Site");
				return _site; 
			}
			set { _site = value; }
		}

		// Methods

		public override IPermission Copy () 
		{
			return new SiteIdentityPermission (_site);
		}

		public override void FromXml (SecurityElement esd) 
		{
			if (esd == null)
				throw new ArgumentNullException (
					Locale.GetText ("The argument is null."));
			
			if (esd.Attribute ("class") != GetType ().AssemblyQualifiedName)
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			if (esd.Attribute ("version") != "1")
				throw new ArgumentException (
					Locale.GetText ("The argument is not valid"));

			this.Site = esd.Attribute ("Site");
		}

		[MonoTODO]
		public override IPermission Intersect (IPermission target)
		{
			return null;
		}

		[MonoTODO]
		public override bool IsSubsetOf (IPermission target) 
		{
			return false;
		}

		public override SecurityElement ToXml ()
		{
			SecurityElement e = new SecurityElement ("IPermission");
			e.AddAttribute ("class", GetType ().AssemblyQualifiedName);
			e.AddAttribute ("version", "1");

			e.AddAttribute ("Site", _site);

                        return e;
		}

		[MonoTODO]
		public override IPermission Union (IPermission target) 
		{
			return null;
		}

		// IBuiltInPermission
		int IBuiltInPermission.GetTokenIndex ()
		{
			return 10;
		}
	}
}

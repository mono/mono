
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
#if NET_1_0
#endif
#if NET_1_1
//
// System.Xml.XmlSecureResolver.cs
//
// Author: Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) 2003 Atsushi Enomoto
//
using System;
using System.Net;
using System.Security;
using System.Security.Policy;
using System.Security.Permissions;

namespace System.Xml
{
	public class XmlSecureResolver : XmlResolver
	{

#region Static Members

		public static Evidence CreateEvidenceForUrl (string securityUrl)
		{
			Evidence e = new Evidence ();
			Url url = null;
			Zone zone = null;
			Site site = null;

			if (securityUrl != null) {
				try {
					if (securityUrl.Length > 0)
						url = new Url (securityUrl);
				} catch (ArgumentException) {
				}

				try {
					zone = Zone.CreateFromUrl (securityUrl);
				} catch (ArgumentException) {
				}

				try {
					if (securityUrl.Length > 0)
						site = Site.CreateFromUrl (securityUrl);
				} catch (ArgumentException) {
				}
			}

			if (url != null)
				e.AddHost (url);
			if (zone != null)
				e.AddHost (zone);
			if (site != null)
				e.AddHost (site);

			return e;
		}
#endregion

		XmlResolver resolver;
		PermissionSet permissionSet;
//		Evidence evidence;

#region .ctor and Finalizer

		public XmlSecureResolver (
			XmlResolver resolver, Evidence evidence)
		{
			this.resolver = resolver;
//			this.evidence = evidence;
			this.permissionSet = SecurityManager.ResolvePolicy (evidence);
		}

		public XmlSecureResolver (
			XmlResolver resolver, PermissionSet permissionSet)
		{
			this.resolver = resolver;
			this.permissionSet = permissionSet;
		}

		public XmlSecureResolver (
			XmlResolver resolver, string securityUrl)
			: this (resolver, CreateEvidenceForUrl (securityUrl))
		{
		}
#endregion

#region Property

		public override ICredentials Credentials {
			set { resolver.Credentials = value; }
		}

#endregion

#region Methods

		public override object GetEntity (
			Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			permissionSet.PermitOnly ();
			return resolver.GetEntity (absoluteUri, role, ofObjectToReturn);
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			return resolver.ResolveUri (baseUri, relativeUri);
		}
#endregion

	}
}
#endif

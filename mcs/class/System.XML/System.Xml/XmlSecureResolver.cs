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

			try {
				url = new Url (securityUrl);
			} catch (ArgumentException) {
			}

			try {
				zone = Zone.CreateFromUrl (securityUrl);
			} catch (ArgumentException) {
			}

			try {
				site = Site.CreateFromUrl (securityUrl);
			} catch (ArgumentException) {
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

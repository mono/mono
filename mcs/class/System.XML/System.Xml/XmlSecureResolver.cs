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

namespace System.Xml
{
	public class XmlSecureResolver : XmlResolver
	{

#region Static Members

		[MonoTODO]
		public static Evidence CreateEvidenceForUrl (string securityUrl)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public static new bool Equals (object objA, object objB)
		{
			throw new NotImplementedException ();
		}
#endregion

#region .ctor and Finalizer

		[MonoTODO]
		public XmlSecureResolver (
			XmlResolver resolver, Evidence evidence)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlSecureResolver (
			XmlResolver resolver, PermissionSet permissionSet)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public XmlSecureResolver (
			XmlResolver resolver, string securityUrl)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		~XmlSecureResolver ()
		{
			// What is expected here, not in Dispose() ?
		}
#endregion

#region Property

		[MonoTODO]
		public override ICredentials Credentials {
			set { throw new NotImplementedException (); }
		}

#endregion

#region Methods

		[MonoTODO]
		public override object GetEntity (
			Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			throw new NotImplementedException ();
		}

		[MonoTODO]
		public override Uri ResolveUri ( Uri baseUri, string relativeUri)
		{
			throw new NotImplementedException ();
		}

#endregion

	}
}
#endif

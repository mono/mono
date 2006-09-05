//
// XmlStoredResolver.cs
//
// Author: Atsushi Enomoto <atsushi@ximian.com>
//
// This code is too short to have "creativity". (thus, there must be no 
// copyright on this code). Feel free to use anywhere.
//
// Use like this:
//
//	XmlDocument doc = new XmlDocument ();
//	XmlStoredResolver r = new XmlStoredResolver (new XmlUrlResolver ());
//	r.Add ("http://www.w3.org/TR/2001/REC-SVG-20010904/DTD/svg10.dtd",
//		"svg10.dtd");
//	doc.XmlResolver = r;
//
using System;
using System.Collections;
using System.Net;
using System.Xml;

namespace Mono.Xml
{
	public class XmlStoredResolver : XmlResolver
	{
		XmlResolver external;
		XmlResolver local;
		IDictionary uriTable;

		public XmlStoredResolver (XmlResolver resolver)
			: this (resolver, resolver, new Hashtable ())
		{
		}

		public XmlStoredResolver (XmlResolver resolver, IDictionary uriTable)
			: this (resolver, resolver, uriTable)
		{
		}

		public XmlStoredResolver (XmlResolver external, XmlResolver local)
			: this (external, local, new Hashtable ())
		{
		}
		
		public XmlStoredResolver (XmlResolver external, XmlResolver local, IDictionary uriTable)
		{
			this.external = external;
			this.local = local;
			this.uriTable = uriTable;
		}

		public override ICredentials Credentials {
			set {
				external.Credentials = value;
				if (local != external)
					local.Credentials = value;
			}
		}

		public IDictionary Mapping {
			get { return uriTable; }
		}

		public void Add (string nominalUri, string actualLocation)
		{
			uriTable.Add (
				external.ResolveUri (null, nominalUri).ToString (),
				local.ResolveUri (null, actualLocation).ToString ());
		}

		public override object GetEntity (Uri uri, string role, Type returnType)
		{
			string uriString = uri.ToString ();
			string actualLocation = (string) uriTable [uriString];
			if (actualLocation == null)
				return external.GetEntity (uri, role, returnType);
			else
				return local.GetEntity (local.ResolveUri (null, actualLocation), role, returnType);
		}
	}
}

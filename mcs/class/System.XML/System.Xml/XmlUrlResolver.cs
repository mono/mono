// System.Xml.XmlUrlResolver.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System.Net;

namespace System.Xml
{
	public class XmlUrlResolver : XmlResolver
	{
		// Field
		ICredentials credential;
		
		// Constructor
		public XmlUrlResolver ()
			: base ()
		{
		}

		// Properties		
		public override ICredentials Credentials
		{
			set { credential = value; }
		}
		
		// Methods
		[MonoTODO]
		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			return null;
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			return new Uri (baseUri, relativeUri);
		}
	}       
}

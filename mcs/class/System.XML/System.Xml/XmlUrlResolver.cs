// System.Xml.XmlUrlResolver.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//	   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
//

using System.Net;
using System.IO;

namespace System.Xml
{
	public class XmlUrlResolver : XmlResolver
	{
		// Field
		ICredentials credential;
		WebClient webClientInternal;
		WebClient webClient {
			get {
				if (webClientInternal == null)
					webClientInternal = new WebClient ();
				return webClientInternal;
			}
		}

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
		[MonoTODO("This implementation is bad because the spec explicitly forbids parameter Uri representing non-absolute.")]
		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			// (MS documentation says) parameter role isn't used yet.
			Stream s = null;
			webClient.Credentials = credential;
			s = new XmlInputStream (webClient.OpenRead (absoluteUri.ToString ()));
			if (s.GetType ().IsSubclassOf (ofObjectToReturn))
				return s;
			s.Close ();
			return null;
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			return new Uri (baseUri, relativeUri);
		}
	}
}

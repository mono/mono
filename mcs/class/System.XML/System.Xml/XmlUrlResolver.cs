// System.Xml.XmlUrlResolver.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//	   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
//

using System.Net;
using System.IO;
using System.Text;
using Mono.Xml.Native;

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
		[MonoTODO("Use Credentials; Uri must be absolute.")]
		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			// (MS documentation says) parameter role isn't used yet.
			Stream s = null;
			using (s) {
				WebClient wc = new WebClient ();
//				wc.Credentials = credential;
				s = wc.OpenRead (absoluteUri.ToString ());
				if (s.GetType ().IsSubclassOf (ofObjectToReturn))
					return s;
				wc.Dispose ();
			}
			return null;
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			// Don't expect baseUri is not null here.
//			if (relativeUri == null)
//				return baseUri;

			if (baseUri == null) {
				// Don't ignore such case that relativeUri is in fact absolute uri.
//				return new Uri (Path.GetFullPath (relativeUri));

				// extraneous "/a" is required because current Uri stuff 
				// seems ignorant of difference between "." and "./". 
				// I'd be appleciate if it is fixed with better solution.
				return new Uri (new Uri (Path.GetFullPath ("./a")), relativeUri);
			}

			// Do not expect relativeUri.Length > 2.
//			if (relativeUri.IndexOf ("://") >= 0)
//				return new Uri (relativeUri);

			if (relativeUri == null)
				return baseUri;

			return new Uri (baseUri, relativeUri);
		}
	}
}

//
// System.Xml.XmlResolver.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
// (C) 2004 Novell Inc.
//

using System;
using System.IO;
using System.Net;

namespace System.Xml
{
	public abstract class XmlResolver
	{
		public abstract ICredentials Credentials { set; }

		public abstract object GetEntity (
			Uri absoluteUri,
			string role,
			Type type);


		public virtual Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			if (baseUri == null) {
				if (relativeUri == null)
					throw new NullReferenceException ("Either baseUri or relativeUri are required.");
				// Don't ignore such case that relativeUri is in fact absolute uri (e.g. ResolveUri (null, "http://foo.com")).
				if (relativeUri.StartsWith ("http:") ||
					relativeUri.StartsWith ("https:") ||
					relativeUri.StartsWith ("file:"))
					return new Uri (relativeUri);
				else
					// extraneous "/a" is required because current Uri stuff 
					// seems ignorant of difference between "." and "./". 
					// I'd be appleciate if it is fixed with better solution.
					return new Uri (Path.GetFullPath (relativeUri));
//					return new Uri (new Uri (Path.GetFullPath ("./a")), EscapeRelativeUriBody (relativeUri));
			}

			if (relativeUri == null)
				return baseUri;

			return new Uri (baseUri, EscapeRelativeUriBody (relativeUri));
		}

		// see also XmlUrlResolver.UnescapeRelativeUriBody().
		private string EscapeRelativeUriBody (string src)
		{
			return src.Replace ("<", "%3C")
				.Replace (">", "%3E")
				.Replace ("#", "%23")
				.Replace ("%", "%25")
				.Replace ("\"", "%22");
		}
	}
}

//
// System.Xml.XmlResolver.cs
//
// Author:
//   Jason Diamond (jason@injektilo.org)
//   Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2001 Jason Diamond  http://injektilo.org/
// Copyright (C) 2004,2009 Novell, Inc (http://www.novell.com)
//

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

using System.IO;
using System.Net;
using System.Security.Permissions;

namespace System.Xml
{
	public abstract class XmlResolver
	{
#if !MOONLIGHT
		public abstract ICredentials Credentials { set; }
#endif

		public abstract object GetEntity (
			Uri absoluteUri,
			string role,
			Type type);

		[PermissionSet (SecurityAction.InheritanceDemand, Unrestricted = true)]
		public virtual Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			if (baseUri == null) {
				if (relativeUri == null)
					throw new ArgumentNullException ("Either baseUri or relativeUri are required.");
#if MOONLIGHT
				return new Uri (relativeUri, UriKind.RelativeOrAbsolute);
#else
				// Don't ignore such case that relativeUri is in fact absolute uri (e.g. ResolveUri (null, "http://foo.com")).
				if (relativeUri.StartsWith ("http:") ||
					relativeUri.StartsWith ("https:") ||
					relativeUri.StartsWith ("ftp:") ||
					relativeUri.StartsWith ("file:"))
					return new Uri (relativeUri);
				else
					return new Uri (Path.GetFullPath (relativeUri));
#endif
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
#if MOONLIGHT
		public virtual bool SupportsType (Uri absoluteUri, Type type)
		{
			if (absoluteUri == null)
				throw new ArgumentNullException ("absoluteUri");
			return ((type == null) || (type == typeof (Stream)));
		}
#endif
	}
}

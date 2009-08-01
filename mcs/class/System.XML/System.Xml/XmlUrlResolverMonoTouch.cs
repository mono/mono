// System.Xml.XmlUrlResolver.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//	   Atsushi Enomoto (ginga@kit.hi-ho.ne.jp)
//
// (C) Ximian, Inc.
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

using System.Net;
using System.IO;
using System.Text;

namespace System.Xml
{
	public class XmlUrlResolver : XmlResolver
	{
		// Constructor
		public XmlUrlResolver ()
			: base ()
		{
		}

		// Methods
		public override object GetEntity (Uri absoluteUri, string role, Type ofObjectToReturn)
		{
			if (ofObjectToReturn == null)
				ofObjectToReturn = typeof (Stream);
			if (ofObjectToReturn != typeof (Stream))
				throw new XmlException ("This object type is not supported.");

			if (!absoluteUri.IsAbsoluteUri)
				throw new ArgumentException ("uri must be absolute.", "absoluteUri");

			if (absoluteUri.Scheme == "file") {
				if (absoluteUri.AbsolutePath == String.Empty)
					throw new ArgumentException ("uri must be absolute.", "absoluteUri");
				return new FileStream (UnescapeRelativeUriBody (absoluteUri.LocalPath), FileMode.Open, FileAccess.Read, FileShare.Read);
			}

			// (MS documentation says) parameter role isn't used yet.
			//WebRequest req = WebRequest.Create (absoluteUri);
			//return req.GetResponse().GetResponseStream();

			return "";
		}

		public override Uri ResolveUri (Uri baseUri, string relativeUri)
		{
			return base.ResolveUri (baseUri, relativeUri);
		}

		// see also XmlResolver.EscapeRelativeUriBody().
		private string UnescapeRelativeUriBody (string src)
		{
			return src.Replace ("%3C", "<")
				.Replace ("%3E", ">")
				.Replace ("%23", "#")
				.Replace ("%22", "\"")
				.Replace ("%20", " ")
				.Replace ("%25", "%");
		}
	}
}

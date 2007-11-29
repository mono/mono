//
// Authors:
//   Atsushi Enomoto
//
// Copyright 2007 Novell (http://www.novell.com)
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

using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml;

namespace System.Xml.Linq
{
	public class XDeclaration
	{
		string encoding, standalone, version;

		public XDeclaration (string version, string encoding, string standalone)
		{
			this.version = version;
			this.encoding = encoding;
			this.standalone = standalone;
		}

		public XDeclaration (XDeclaration other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			this.version = other.version;
			this.encoding = other.encoding;
			this.standalone = other.standalone;
		}

		public string Encoding {
			get { return encoding; }
			set { encoding = value; }
		}

		public string Standalone {
			get { return standalone; }
			set { standalone = value; }
		}

		public string Version {
			get { return version; }
			set { version = value; }
		}

		public override string ToString ()
		{
			return String.Concat ("<?xml",
				version != null ? " version=\"" : null,
				version != null ?  version : null,
				version != null ? "\"" : null,
				encoding != null ? " encoding=\"" : null,
				encoding != null ?  encoding : null,
				encoding != null ? "\"" : null,
				standalone != null ? " standalone=\"" : null,
				standalone != null ?  standalone : null,
				standalone != null ? "\"" : null,
				"?>");
		}

		/*
		public override void WriteTo (XmlWriter w)
		{
			StringBuilder sb = new StringBuilder ();
			sb.AppendFormat ("version=\"{0}\"", version);
			if (encoding != null)
				sb.AppendFormat (" encoding=\"{0}\"", encoding);
			if (standalone != null)
				sb.AppendFormat (" standalone=\"{0}\"", standalone);
			// "xml" is not allowed PI, but because of nasty
			// XmlWriter API design it must pass.
			w.WriteProcessingInstruction ("xml", sb.ToString ());
		}
		*/
	}
}

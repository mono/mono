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
	public class XDocumentType : XNode
	{
		string name, pubid, sysid, intSubset;

		public XDocumentType (string name, string publicId, string systemId, string internalSubset)
		{
			this.name = name;
			pubid = publicId;
			sysid = systemId;
			intSubset = internalSubset;
		}

		public XDocumentType (XDocumentType other)
		{
			if (other == null)
				throw new ArgumentNullException ("other");
			name = other.name;
			pubid = other.pubid;
			sysid = other.sysid;
			intSubset = other.intSubset;
		}

		public string Name {
			get { return name; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				name = value;
			}
		}

		public string PublicId {
			get { return pubid; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				pubid = value;
			}
		}

		public string SystemId {
			get { return sysid; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				sysid = value;
			}
		}

		public string InternalSubset {
			get { return intSubset; }
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				intSubset = value;
			}
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.DocumentType; }
		}

		public override void WriteTo (XmlWriter writer)
		{
			XDocument doc = Document;
			XElement root = doc.Root;
			if (root != null)
				writer.WriteDocType (root.Name.LocalName, pubid, sysid, intSubset);
		}
	}
}

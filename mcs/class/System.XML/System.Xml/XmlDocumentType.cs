//
// System.Xml.XmlDocumentType.cs
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
using System;
using System.IO;
using System.Collections;
using Mono.Xml;

#if NET_2_0
using XmlTextReaderImpl = Mono.Xml2.XmlTextReader;
#else
using XmlTextReaderImpl = System.Xml.XmlTextReader;
#endif

namespace System.Xml
{
	public class XmlDocumentType  : XmlLinkedNode
	{
		// Fields
		internal XmlNamedNodeMap entities;
		internal XmlNamedNodeMap notations;
		DTDObjectModel dtd;

		// Constructor
		protected internal XmlDocumentType (string name, string publicId,
						    string systemId, string internalSubset,
						    XmlDocument doc)
			: base (doc)
		{
			XmlTextReaderImpl xtr = new XmlTextReaderImpl (BaseURI, new StringReader (""), doc.NameTable);
			xtr.XmlResolver = doc.Resolver;
			xtr.GenerateDTDObjectModel (name, publicId, systemId, internalSubset);
			this.dtd = xtr.DTD;

			ImportFromDTD ();
		}

		internal XmlDocumentType (DTDObjectModel dtd, XmlDocument doc)
			: base (doc)
		{
			this.dtd = dtd;
			ImportFromDTD ();
		}

		private void ImportFromDTD ()
		{
			entities = new XmlNamedNodeMap (this);
			notations = new XmlNamedNodeMap (this);

			foreach (DTDEntityDeclaration decl in DTD.EntityDecls.Values) {
				XmlNode n = new XmlEntity (decl.Name, decl.NotationName,
					decl.PublicId, decl.SystemId, OwnerDocument);
				entities.SetNamedItem (n);
			}
			foreach (DTDNotationDeclaration decl in DTD.NotationDecls.Values) {
				XmlNode n = new XmlNotation (decl.LocalName, decl.Prefix,
					decl.PublicId, decl.SystemId, OwnerDocument);
				notations.SetNamedItem (n);
			}
		}

		// Properties
		internal DTDObjectModel DTD {
			get { return dtd; }
		}

		public XmlNamedNodeMap Entities
		{
			get { return entities; }
		}
			
		public string InternalSubset
		{
			get { return dtd.InternalSubset; }
		}

		public override bool IsReadOnly
		{
			get { return true; } // always return true
		}

		public override string LocalName
		{
			get { return dtd.Name; }
		}

		public override string Name
		{
			get { return dtd.Name; }
		}

		public override XmlNodeType NodeType
		{
			get { return XmlNodeType.DocumentType; }
		}

		public XmlNamedNodeMap Notations
		{
			get { return notations; }
		}

		public string PublicId
		{
			get { return dtd.PublicId; }
		}

		public string SystemId
		{
			get { return dtd.SystemId; }
		}

		// Methods
		public override XmlNode CloneNode (bool deep)
		{
			// deep is ignored
			return new XmlDocumentType (dtd, OwnerDocument);
		}
		
		public override void WriteContentTo (XmlWriter w)
		{
			// No effect
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteDocType (Name, PublicId, SystemId, InternalSubset);
		}
	}
}

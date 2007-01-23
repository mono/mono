//
// DataObject.cs - DataObject implementation for XML Signature
// http://www.w3.org/2000/09/xmldsig#Object
//
// Author:
//	Sebastien Pouliot  <sebastien@ximian.com>
//	Atsushi Enomoto (atsushi@ximian.com)
//
// (C) 2002, 2003 Motus Technologies Inc. (http://www.motus.com)
// Copyright (C) 2005 Novell, Inc (http://www.novell.com)
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

using System.Xml;

namespace System.Security.Cryptography.Xml {

	// XmlElement part of the signature
	// Note: Looks like KeyInfoNode (but the later is XmlElement inside KeyInfo)
	// required for "enveloping signatures"
	public class DataObject {

		private XmlElement element;
		private bool propertyModified;

		public DataObject ()
		{
			Build (null, null, null, null);
		}

		public DataObject (string id, string mimeType, string encoding, XmlElement data) 
		{
			if (data == null)
				throw new ArgumentNullException ("data");

			Build (id, mimeType, encoding, data);
		}

		// this one accept a null "data" parameter
		private void Build (string id, string mimeType, string encoding, XmlElement data) 
		{
			XmlDocument document = new XmlDocument ();
			XmlElement xel = document.CreateElement (XmlSignature.ElementNames.Object, XmlSignature.NamespaceURI);
			if (id != null) {
				xel.SetAttribute (XmlSignature.AttributeNames.Id, id);
			}
			if (mimeType != null) {
				xel.SetAttribute (XmlSignature.AttributeNames.MimeType, mimeType);
			}
			if (encoding != null) {
				xel.SetAttribute (XmlSignature.AttributeNames.Encoding, encoding);
			}
			if (data != null) {
				XmlNode newNode = document.ImportNode (data, true);
				xel.AppendChild (newNode);
			}
			element = xel;
		}

		// why is data a XmlNodeList instead of a XmlElement ?
		public XmlNodeList Data {
			get { 
				return element.ChildNodes;
			}
			set {
				if (value == null)
					throw new ArgumentNullException ("value");
				XmlDocument doc = new XmlDocument ();
				XmlElement el = (XmlElement) doc.ImportNode (element, true);
				while (el.LastChild != null)
					el.RemoveChild (el.LastChild);
				foreach (XmlNode n in value)
					el.AppendChild (doc.ImportNode (n, true));
				element = el;
				propertyModified = true;
			}
		}

		// default to null - no encoding
		public string Encoding {
			get { return GetField (XmlSignature.AttributeNames.Encoding); }
			set { SetField (XmlSignature.AttributeNames.Encoding, value); }
		}

		// default to null
		public string Id {
			get { return GetField (XmlSignature.AttributeNames.Id); }
			set { SetField (XmlSignature.AttributeNames.Id, value); }
		}

		// default to null
		public string MimeType {
			get { return GetField (XmlSignature.AttributeNames.MimeType); }
			set { SetField (XmlSignature.AttributeNames.MimeType, value); }
		}
		
		private string GetField (string attribute)
		{
			XmlNode attr = element.Attributes [attribute];
			return attr != null ? attr.Value : null;
		}

		private void SetField (string attribute, string value)
		{
			// MS-BUGS: it never cleans attribute value up.
			if (value == null)
				return;

			if (propertyModified)
				element.SetAttribute (attribute, value);
			else {
				XmlDocument document = new XmlDocument ();
				XmlElement el = document.ImportNode (element, true) as XmlElement;
				el.SetAttribute (attribute, value);
				element = el;
				propertyModified = true;
			}
		}

		public XmlElement GetXml () 
		{
			if (propertyModified) {
				// It looks MS.NET returns element which comes from new XmlDocument every time
				XmlElement oldElement = element;
				XmlDocument doc = new XmlDocument ();
				element = doc.CreateElement (XmlSignature.ElementNames.Object, XmlSignature.NamespaceURI);
				foreach (XmlAttribute attribute in oldElement.Attributes) {
					switch (attribute.Name) {
					case XmlSignature.AttributeNames.Id:
					case XmlSignature.AttributeNames.Encoding:
					case XmlSignature.AttributeNames.MimeType:
						element.SetAttribute (attribute.Name, attribute.Value);
						break;
					}
				}
				foreach (XmlNode n in oldElement.ChildNodes)
					element.AppendChild (doc.ImportNode (n, true));
			}
			return element;
		}

		public void LoadXml (XmlElement value) 
		{
			if (value == null)
				throw new ArgumentNullException ("value");
			element = value;
			propertyModified = false;
		}
	}
}

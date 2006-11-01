//
// System.Xml.XmlEntityReference.cs
// Author:
//	Duncan Mak  (duncan@ximian.com)
//	Atsushi Enomoto  (atsushi@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
// (C) 2004 Novell inc.
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
using System.Xml.XPath;
using Mono.Xml;

namespace System.Xml
{
	public class XmlEntityReference : XmlLinkedNode, IHasXmlChildNode
	{
		string entityName;
		XmlLinkedNode lastLinkedChild;

		// Constructor
		protected internal XmlEntityReference (string name, XmlDocument doc)
			: base (doc)
		{
			// LAMESPEC: MS CreateNode() allows null node name.
			XmlConvert.VerifyName (name);
			entityName = doc.NameTable.Add (name);
		}

		// Properties

		XmlLinkedNode IHasXmlChildNode.LastLinkedChild {
			get { return lastLinkedChild; }
			set { lastLinkedChild = value; }
		}

		public override string BaseURI {
			get { return base.BaseURI; }
		}

		private XmlEntity Entity {
			get {
				XmlDocumentType doctype = OwnerDocument.DocumentType;
				if (doctype == null)
					return null;

				if (doctype.Entities == null)
					return null;

				return doctype.Entities.GetNamedItem (Name) as XmlEntity;
			}
		}

		internal override string ChildrenBaseURI {
			get {
				XmlEntity ent = Entity;
				if (ent == null)
					return string.Empty;

				if (ent.SystemId == null || ent.SystemId.Length == 0)
					return ent.BaseURI;

				if (ent.BaseURI == null || ent.BaseURI.Length == 0)
					return ent.SystemId;
				
				Uri baseUri = null;
				try {
					baseUri = new Uri (ent.BaseURI);
				} catch (UriFormatException) {
				}

				XmlResolver resolver = OwnerDocument.Resolver;
				if (resolver != null)
					return resolver.ResolveUri (baseUri, ent.SystemId).ToString ();

				return new Uri (baseUri, ent.SystemId).ToString ();
			}
		}

		public override bool IsReadOnly {
			get { return true; } 
		}

		public override string LocalName {
			get { return entityName; } // name of the entity referenced.
		}

		public override string Name {
			get { return entityName; } // name of the entity referenced.
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.EntityReference; }
		}

		public override string Value {
			get { return null; } // always return null here.
			set {
				throw new XmlException ("entity reference cannot be set value.");
			}
		}

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Text;
			}
		}


		// Methods
		public override XmlNode CloneNode (bool deep)
		{
			
			// API docs: "The replacement text is not included." XmlNode.CloneNode
			// "The replacement text is set when node is inserted." XmlEntityReference.CloneNode
			//
			return new XmlEntityReference (Name, OwnerDocument);
		}

		public override void WriteContentTo (XmlWriter w)
		{
			for (int i = 0; i < ChildNodes.Count; i++)
				ChildNodes [i].WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteRaw ("&");
			w.WriteName (Name);
			w.WriteRaw (";");
		}

		internal void SetReferencedEntityContent ()
		{
			if (FirstChild != null)
				return;

			if (OwnerDocument.DocumentType == null)
				return;

			XmlEntity ent = Entity;
			if (ent == null)
				InsertBefore (OwnerDocument.CreateTextNode (String.Empty), null, false, true);
			else {
				for (int i = 0; i < ent.ChildNodes.Count; i++)
					InsertBefore (ent.ChildNodes [i].CloneNode (true), null, false, true);
			}
		}
	}
}

//
// System.Xml.XmlEntityReference.cs
// Author:
//	Duncan Mak  (duncan@ximian.com)
//	Atsushi Enomoto  (atsushi@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
// (C) 2004 Novell inc.
//
using System;
using Mono.Xml;

namespace System.Xml
{
	public class XmlEntityReference : XmlLinkedNode
	{
		string entityName;
		
		// Constructor
		protected internal XmlEntityReference (string name, XmlDocument doc)
			: base (doc)
		{
			entityName = doc.NameTable.Add (name);
		}

		// Properties
		public override string BaseURI {
			get { return base.BaseURI; }
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

		// Methods
		public override XmlNode CloneNode (bool deep)
		{
			
			// API docs: "The replacement text is not included." XmlNode.CloneNode
			// "The replacement text is set when node is inserted." XmlEntityReference.CloneNode
			//
			return new XmlEntityReference ("", OwnerDocument);
		}

		public override void WriteContentTo (XmlWriter w)
		{
			foreach (XmlNode n in ChildNodes)
				n.WriteTo (w);
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteRaw("&");
			w.WriteName(Name);
			w.WriteRaw(";");
		}

		internal void SetReferencedEntityContent ()
		{
			if (FirstChild != null)
				return;

			XmlDocumentType doctype = OwnerDocument.DocumentType;
			if (doctype == null)
				return;

			XmlEntity ent = doctype.Entities.GetNamedItem (Name) as XmlEntity;
			if (ent == null)
				InsertBefore (OwnerDocument.CreateTextNode (String.Empty), null, false, true);
			else {
				ent.SetEntityContent ();
				for (int i = 0; i < ent.ChildNodes.Count; i++)
					InsertBefore (ent.ChildNodes [i].CloneNode (true), null, false, true);
			}
		}
	}
}

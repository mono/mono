//
// System.Xml.XmlEntityReference.cs
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Xml
{
	public class XmlEntityReference : XmlLinkedNode
	{
		string entityName;
		
		// Constructor
		protected internal XmlEntityReference (string name, XmlDocument doc)
			: base (doc)
		{
			entityName = name;
		}

		// Properties
		[MonoTODO]
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
			// nothing to write for this object.
		}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteRaw("&");
			w.WriteName(Name);
			w.WriteRaw(";");
		}
	}
}

//
// System.Xml.XmlNotation.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

using System;
using System.Xml;

namespace System.Xml
{
	public class XmlNotation : XmlNode
	{
		#region Fields
		
		string localName;
		string publicId;
		string systemId;
		string prefix;
		XmlLinkedNode lastChild;
		
		#endregion
		
		#region Constructor
		
		internal XmlNotation (string localName, string prefix, string publicId,
				      string systemId, XmlDocument doc)
			: base (doc)
		{
			this.localName = localName;
			this.prefix = prefix;
			this.publicId = publicId;
			this.systemId = systemId;
		}

		#endregion

		#region Properties
		
		public override string InnerXml {
			get { return String.Empty; }
			set { throw new InvalidOperationException ("This operation is not allowed."); }
		}

		public override bool IsReadOnly {
			get { return true; } // Notation nodes are always read-only
		}

		internal protected override XmlLinkedNode LastLinkedChild {
			get { return lastChild; }

			set { lastChild = value; }
		}

		public override string LocalName {
			get { return localName; }
		}

		public override string Name {
			get { return prefix + ":" + localName; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Notation; }
		}

		public override string OuterXml {
			get { return String.Empty; }
		}

		public string PublicId {
			get {
				if (publicId != null)
					return publicId;
				else
					return null;
			}
		}

		public string SystemId {
			get {
				if (systemId != null)
					return systemId;
				else
					return null;
			}
		}

		#endregion

		#region Methods
		
		public override XmlNode CloneNode (bool deep)
		{
			throw new InvalidOperationException ("This operation is not allowed.");
		}

		public override void WriteContentTo (XmlWriter w) {	} // has no effect.

		public override void WriteTo (XmlWriter w) {	} // has no effect.
		       
		#endregion
	}
}

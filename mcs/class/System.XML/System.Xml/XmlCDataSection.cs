//
// System.Xml.XmlCDataSection.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
//

namespace System.Xml
{
	public class XmlCDataSection : XmlCharacterData
	{
		// Constructor
		protected internal XmlCDataSection (string data, XmlDocument doc)
			: base (data, doc)
		{
		}

        // Properties
		public override string LocalName
		{
			get { return "#cdata-section"; }
		}

		public override string Name
		{
			get { return "#cdata-section"; }
		}

		public override XmlNodeType NodeType
		{
			get { return XmlNodeType.CDATA; }
		}

		// Methods
		public override XmlNode CloneNode (bool deep)
		{
			return null;
		}

		public override void WriteContentTo (XmlWriter w)
		{
			// CDATA nodes have no children, WriteContentTo has no effect.
		}

		public override void WriteTo (XmlWriter w)
		{
		}
	}
}

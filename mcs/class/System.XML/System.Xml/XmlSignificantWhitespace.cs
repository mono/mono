//
// System.Xml.XmlSignificantWhitespace.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;

namespace System.Xml
{
	public class XmlSignificantWhitespace : XmlCharacterData
	{
		// Constructor
		protected internal XmlSignificantWhitespace (string strData, XmlDocument doc)
			: base (strData, doc)
		{
		}
		
		// Properties
		public override string LocalName {
			get { return "#significant-whitespace"; }
		}

		public override string Name {
			get { return "#significant-whitespace"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.SignificantWhitespace; }
		}

		public override string Value {
			get { return Data; }
			set {}
		}

		// Methods
		public override XmlNode CloneNode (bool deep)
		{
			return new XmlSignificantWhitespace (Data, OwnerDocument);
		}

		[MonoTODO]
		public override void WriteContentTo (XmlWriter w)
		{			
		}

		[MonoTODO]
		public override void WriteTo (XmlWriter w)
		{			
		}
	}
}

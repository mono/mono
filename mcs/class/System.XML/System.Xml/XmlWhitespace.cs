//
// System.Xml.XmlWhitespace.cs
//
// Author:
//	Duncan Mak  (duncan@ximian.com)
//
// (C) Ximian, Inc. http://www.ximian.com
//

using System;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlWhitespace : XmlCharacterData
	{
		// Constructor
		protected internal XmlWhitespace (string strData, XmlDocument doc)
			: base (strData, doc)
		{
		}
		
		// Properties
		public override string LocalName {
			get { return "#whitespace"; }
		}

		public override string Name {
			get { return "#whitespace"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Whitespace; }
		}

		internal override XPathNodeType XPathNodeType {
			get { return XPathNodeType.Whitespace; }
		}

		public override string Value {
			get { return Data; }
			[MonoTODO]
			set {
				if (IsValidWhitespaceChar (value) == false)
					throw new ArgumentException ("Invalid whitespace characters.");
			}
		}

		// Methods
		public override XmlNode CloneNode (bool deep)
		{
			// always return the data value
			return new XmlWhitespace (Data, OwnerDocument); 
		}

		public override void WriteContentTo (XmlWriter w) {}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteWhitespace (Data);
		}

		private bool IsValidWhitespaceChar (string text)
		{
			foreach (char c in text)
				if ((c != ' ') && (c != '\r') && (c != '\n') && (c != '\t'))
					return false;
			return true;
		}
	}
}

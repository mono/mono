//
// System.Xml.XmlText
//
// Author:
//   Jason Diamond <jason@injektilo.org>
//
// (C) 2002 Jason Diamond  http://injektilo.org/
//

using System;
using System.Xml.XPath;

namespace System.Xml
{
	public class XmlText : XmlCharacterData
	{
		#region Constructor

		protected internal XmlText (string strData, XmlDocument doc) : base(strData, doc)
		{
		}

		#endregion

		#region Properties

		public override string LocalName 
		{
			get { return "#text"; }
		}

		public override string Name {
			get { return "#text"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Text; }
		}

		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Text;
			}
		}
		
		public override string Value {
			get { return Data; }
			set { Data = value; }
		}

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			XmlText newText = OwnerDocument.CreateTextNode(Data);
			if(deep)
			{
				foreach(XmlNode child in ChildNodes)
					newText.AppendChild(child.CloneNode(deep));
			}
			return newText;
		}

		public virtual XmlText SplitText (int offset)
		{
			XmlText next = OwnerDocument.CreateTextNode(this.Data.Substring(offset));
			DeleteData(offset, Data.Length - offset);
			this.ParentNode.InsertAfter(next, this);
			return next;
		}

		public override void WriteContentTo (XmlWriter w) {}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteString (Data);
		}

		#endregion
	}
}

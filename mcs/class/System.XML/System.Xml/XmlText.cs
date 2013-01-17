//
// System.Xml.XmlText
//
// Author:
//   Jason Diamond <jason@injektilo.org>
//
// (C) 2002 Jason Diamond  http://injektilo.org/
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

		public override XmlNode ParentNode {
			get { return base.ParentNode; }
		}

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			XmlText newText = OwnerDocument.CreateTextNode(Data);
			return newText;
		}

		public virtual XmlText SplitText (int offset)
		{
			XmlText next = OwnerDocument.CreateTextNode (this.Data.Substring (offset));
			DeleteData (offset, Data.Length - offset);
			this.ParentNode.InsertAfter (next, this);
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

//
// System.Xml.XmlCDataSection.cs
//
// Author: Duncan Mak (duncan@ximian.com)
//
// (C) Ximian, Inc.
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

namespace System.Xml
{
	public class XmlCDataSection : XmlCharacterData
	{
		#region Constructors

		protected internal XmlCDataSection (string data, XmlDocument doc)
			: base (data, doc)
		{
		}

		#endregion

		#region Properties

		public override string LocalName {
			get { return "#cdata-section"; }
		}

		public override string Name	{
			get { return "#cdata-section"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.CDATA; }
		}

#if NET_2_0
		public override XmlNode ParentNode {
			get { return base.ParentNode; }
		}
#endif

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			XmlNode n = new XmlCDataSection (Data, OwnerDocument); // CDATA nodes have no children.
			return n;
		}

		public override void WriteContentTo (XmlWriter w) {	}

		public override void WriteTo (XmlWriter w)
		{
			w.WriteCData (Data);
		}

		#endregion
	}
}

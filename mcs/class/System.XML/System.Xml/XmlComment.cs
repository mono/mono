//
// System.Xml.XmlComment
//
// Author:
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Kral Ferch
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
	public class XmlComment : XmlCharacterData
	{
		#region Constructors

		protected internal XmlComment (string comment, XmlDocument doc)
			: base (comment, doc)
		{
		}

		#endregion

		#region Properties

		public override string LocalName {
			get { return "#comment"; }
		}

		public override string Name {
			get { return "#comment"; }
		}

		public override XmlNodeType NodeType {
			get { return XmlNodeType.Comment; }
		}
		
		internal override XPathNodeType XPathNodeType {
			get {
				return XPathNodeType.Comment;
			}
		}

		#endregion

		#region Methods

		public override XmlNode CloneNode (bool deep)
		{
			// discard deep because Comments have no children.
			XmlNode n = new XmlComment(Value, OwnerDocument); 
			return n;
		}

		public override void WriteContentTo (XmlWriter w) { }

		public override void WriteTo (XmlWriter w)
		{
			w.WriteComment (Data);
		}

		#endregion
	}
}

//
// System.Xml.XmlLinkedNode
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Jason Diamond, Kral Ferch
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

namespace System.Xml
{
	public abstract class XmlLinkedNode : XmlNode
	{
		#region Fields

		XmlLinkedNode nextSibling;

		#endregion

		#region Constructors
		internal XmlLinkedNode(XmlDocument doc) : base(doc) { }

		#endregion

		#region Properties
		internal bool IsRooted {
			get {
				for (XmlNode n = ParentNode; n != null; n = n.ParentNode)
					if (n.NodeType == XmlNodeType.Document)
						return true;
				return false;
			}
		}

		public override XmlNode NextSibling
		{
			get { return ParentNode == null || ParentNode.LastChild == this ? null : nextSibling; }
		}

		internal XmlLinkedNode NextLinkedSibling
		{
			get { return nextSibling; }
			set { nextSibling = value; }
		}

		public override XmlNode PreviousSibling
		{
			get {
				if (ParentNode != null) {
					XmlNode node = ParentNode.FirstChild;
					if (node != this) {
						do {
							if (node.NextSibling == this)
								return node;
						} while ((node = node.NextSibling) != null);
					}					
				}
				return null;
			}
		}

		#endregion
	}
}

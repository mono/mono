//
// System.Xml.XmlLinkedNode
//
// Authors:
//   Jason Diamond <jason@injektilo.org>
//   Kral Ferch <kral_ferch@hotmail.com>
//
// (C) 2002 Jason Diamond, Kral Ferch
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

		public override XmlNode NextSibling
		{
			get {
				if(ParentNode == null) {
					return null;
				}
				else if (Object.ReferenceEquals(nextSibling, ParentNode.LastLinkedChild.NextLinkedSibling) == false) {
					return nextSibling;
				}
				else {
					return null;
				}
			}
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
